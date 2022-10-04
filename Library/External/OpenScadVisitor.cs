using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library.External
{
    public class OpenScadVisitor : OpenScadParserBaseVisitor<object>
    {
        private readonly CommonTokenStream _commonTokenStream;
        private readonly ExternalReference _externalReference;
        private readonly string _sourceFileHash;


        public OpenScadVisitor(CommonTokenStream commonTokenStream, ExternalReference externalReference)
        {
            _commonTokenStream = commonTokenStream;
            _externalReference = externalReference;
            _sourceFileHash = _externalReference.IncludePath.SHA256Text();
        }


        /// <summary>
        /// Makes an identifier for a thing in an openSCAD file. We want to have things with the same
        /// name inside the same file to get the same ID, so we can later have less trouble finding stuff
        /// that has changed. 
        /// </summary>
        private string MakeId(string type, string name)
        {
            return $"{_sourceFileHash}_{type}_{name}";
        }


        public override object VisitIncludeDeclaration(OpenScadParser.IncludeDeclarationContext context)
        {
            var includePath = context.PATH_STRING().GetText();
            _externalReference.References.Add(includePath);
            return base.VisitIncludeDeclaration(context);
        }

        public override object VisitVariableDeclaration(OpenScadParser.VariableDeclarationContext context)
        {
            var variableName = context.identifier().IDENTIFIER().GetText();
            // in OpenScad variables take the value of the last assignment no matter in which scope, so all
            // variables are effectively global. Therefore we may see more than one declaration of the same
            // variable.
            // check if we already know this variable
            
            // ReSharper disable once SimplifyLinqExpressionUseAll
            if (!_externalReference.Variables.Any(it => it.Name == variableName))
            {
                // TODO: we may want to infer the type from the value, but this could also lead to problems
                // when the variable holds multiple different types over its lifetime, so for now we keep this
                // as "Any".
                var variable = VariableBuilder.NewVariable(variableName, MakeId("variable", variableName)).Build();
                _externalReference.Variables.Add(variable);
            }

            // and walk the rest of the tree
            return base.VisitVariableDeclaration(context);
        }

        
        public override object VisitFunctionDeclaration(OpenScadParser.FunctionDeclarationContext context)
        {
            // first check if this function is not inside of any module
            if (IsNotInsideModule(context))
            {


                var functionName = context.identifier().IDENTIFIER().GetText();
                // return type of a function is always inferred as PortType.ANY unless overwritten by
                // the documentation comment
                var builder = FunctionBuilder.NewFunction(functionName, MakeId("function", functionName));
                
                // now find all the parameters
                var parameterDeclarations = context.parameterList().parameterDeclaration();
                foreach (var parameter in parameterDeclarations)
                {
                    var name = parameter.identifier().IDENTIFIER().GetText();
                    // try to infer the parameter type.
                    var portType = InferParameterType(parameter);
                    builder.WithParameter(name, portType);
                }
                
                var comment = ParseDocumentationComment(context.FUNCTION());

                var functionDescription = builder.Build();
                // apply any documentation comment
                comment.ApplyTo(functionDescription);
                
                _externalReference.Functions.Add(functionDescription);
            }
            
            // and walk the rest of the tree
            return base.VisitFunctionDeclaration(context);
        }

        private PortType InferParameterType(OpenScadParser.ParameterDeclarationContext parameterDeclarationContext)
        {
            var expression = parameterDeclarationContext.expression();
            if (expression == null)
            {
                return PortType.Any;
            }

            OpenScadParser.ExpressionContext StripParentheses(OpenScadParser.ExpressionContext outer)
            {
                while(outer.parenthesizedExpression() != null)
                {
                    outer = outer.parenthesizedExpression().expression();
                }

                return outer;
            }

            expression = StripParentheses(expression);
            
            // is it a simple expression? 
            var simpleExpression = expression.simpleExpression();
            if (simpleExpression?.NUMBER() != null)
            {
                return PortType.Number;
            }

            if (simpleExpression?.STRING() != null)
            {
                return PortType.String;
            }

            if (simpleExpression?.BOOLEAN() != null)
            {
                return PortType.Boolean;
            } 
            
            // is it a vector expression?
            var vectorExpression = expression.vectorExpression();
            if (vectorExpression != null)
            {
                // [ VectorInner, VectorInner, VectorInner ]
                // count how many vectorInner we have
                var vectorContents = vectorExpression.children.OfType<OpenScadParser.VectorInnerContext>().ToList();
                if (vectorContents.Count > 3)
                {
                    // we don't care what is inside, this is an array
                    return PortType.Vector;
                }
                
                // now check that all vector contents are ultimately numbers.
                foreach (var vectorInner in vectorContents)
                {
                    // [ VectorInner { ParenthesizedVectorInner { ... expression ... } } ]
                    var innerExpression = vectorInner;
                    while (innerExpression.parenthesizedVectorInner() != null)
                    {
                        innerExpression = vectorInner.parenthesizedVectorInner().vectorInner();
                    }
                    
                    // now we should have all parenthesized vectorInner removed.
                    var innermostExpression  = StripParentheses(innerExpression.expression());
                    if (innermostExpression.simpleExpression()?.NUMBER() == null)
                    {
                        // if the expression is anything but a number, we infer as "Vector"
                        return PortType.Vector;
                    }
                }
                
                // if we get here, we have a vector of numbers
                if (vectorContents.Count == 2)
                {
                    return PortType.Vector2;
                }

                return PortType.Vector3;
            }
            
            // anything else, we don't recognize -> Any
            return PortType.Any;
        }
        
        private DocumentationComment ParseDocumentationComment(ISyntaxTree invokableDeclarationStart)
        {
          
            var hiddenTokensToLeft = _commonTokenStream
                .GetHiddenTokensToLeft(invokableDeclarationStart.SourceInterval.a, 2 /* channel 2 == comments */);
            
            // that is a really bad API, a list should never return null but apparently from the source code
            // it does.
            if (hiddenTokensToLeft == null)
            {
                // return an empty comment
                return new DocumentationComment();
            }

            string CleanUpCommentText(string text)
            {
                // remove any starting /** and whitespace or newlines
                text = text.TrimStart('/', '*', ' ', '\n', '\r', '\t');
                // remove any ending */ and whitespace or newlines
                text = text.TrimEnd('/', '*', ' ', '\n', '\r', '\t');
                
                // for every line in the text, remove leading * and all whitespace before the *.
                // do not remove whitespace after the *, because that is intended indentation.
                var lines = text.Split('\n');
                var result = new StringBuilder();
                foreach (var line in lines)
                {
                    var trimmedLine = line.TrimStart('*', ' ', '\t');
                    // we deliberately do not use AppendLine here as we always use \n as line separator internally. 
                    result.Append(trimmedLine).Append("\n");
                }
                
                // remove the last newline
                result.Length--;
                
                return result.ToString();
            }
            
            // check the returned list backwards and find the first documentation comment (e.g. the one that is closest
            // to the invokable declaration)
            foreach(var token in hiddenTokensToLeft.Reverse())
            {
                if (token.Type != OpenScadParser.BLOCK_COMMENT || !token.Text.StartsWith("/**"))
                {
                    continue; // not a documentation comment
                }
                
                // a documentation comment starts with the text, and then there may be @param and @return tags
                // in any order. The text after each tag belongs in full to the preceding tag. So we need to
                // split the text into the tags and then parse the tags separately.

                const int paramTag = 1;
                const int returnTag = 2;
                // we declare the regexes here to avoid the overhead of creating them every time we need them
                var paramRegex = new Regex(@"@param\s+(?<name>\w+)\s*(?<type>\[\w+\])?\s+(?<description>.*)", RegexOptions.Singleline);
                var returnRegex = new Regex(@"@return\s+(?<type>\[\w+\])?\s+(?<description>.*)", RegexOptions.Singleline);
    

                // Helper function that finds the next tag in the text and its type.
                bool HasNextTag(string text, int startIndex, out int resultIndex, out int tagType)
                {
                    resultIndex = text.IndexOf("@param", startIndex, StringComparison.Ordinal);
                    if (resultIndex == -1)
                    {
                        resultIndex = text.IndexOf("@return", startIndex, StringComparison.Ordinal);
                        if (resultIndex == -1)
                        {
                            tagType = default;
                            return false;
                        }
                        tagType = returnTag;
                        return true;
                    }

                    tagType = paramTag;
                    return true;
                }
                
                // helper function that converts a type string into a PortType
                PortType GetPortType(string typeString)
                {
                    if (typeString == null )
                    {
                        return PortType.None;
                    }

                    switch (typeString)
                    {
                        case "[number]":
                            return PortType.Number;
                        case "[string]":
                            return PortType.String;
                        case "[any]":
                            return PortType.Any;
                        case "[boolean]":
                            return PortType.Boolean;
                        case "[vector2]":
                            return PortType.Vector2;
                        case "[vector3]":
                            return PortType.Vector3;
                        case "[vector]":
                            return PortType.Vector;
                    }

                    // anything else or something we don't recognize
                    return PortType.None;
                }
                
                if (!HasNextTag(token.Text, 0, out var nextIndex, out var nextTagType)) {
                    // there is no next tag, so we can use the whole text
                    return new DocumentationComment {Summary = CleanUpCommentText(token.Text)};
                }
                
                // everything until now is the summary
                var result = new DocumentationComment
                {
                    Summary = CleanUpCommentText(token.Text.Substring(0, nextIndex))
                };

                do
                {
                    var hasNext = HasNextTag(token.Text, nextIndex + 1, out var endIndex, out var followingTagType);
                    if (!hasNext)
                    {
                        endIndex = token.Text.Length - 1;
                    }
                    
                    // now extract the tag text
                    var tagText = token.Text.Substring(nextIndex, endIndex - nextIndex);
                    
                    // check the tag type and try to parse it.
                    switch (nextTagType)
                    {
                        case paramTag:
                            var paramMatch = paramRegex.Match(tagText);
                            if (paramMatch.Success)
                            {
                                result.AddParameter(
                                    paramMatch.Groups["name"]?.Value ?? "",
                                    CleanUpCommentText(paramMatch.Groups["description"]?.Value ?? ""),
                                    GetPortType(paramMatch.Groups["type"]?.Value)
                                );
                            }
                            // if we can't parse it properly, ignore it.
                            break;
                        case returnTag:
                            var returnMatch = returnRegex.Match(tagText);
                            if (returnMatch.Success)
                            {
                                result.ReturnValueTypeHint = GetPortType(returnMatch.Groups["type"]?.Value);
                                result.ReturnValueDescription = CleanUpCommentText(returnMatch.Groups["description"]?.Value ?? "");
                            }
                            // if we can't parse it properly, ignore it.
                            break;
                    }
                    
                    if (!hasNext)
                    {
                        return result;
                    }
                    
                    nextIndex = endIndex;
                    nextTagType = followingTagType;
                } while (true);
            }
            
            // no documentation comment found at all, return an empty one
            return new DocumentationComment();
        }


        public override object VisitModuleDeclaration(OpenScadParser.ModuleDeclarationContext context)
        {
            // modules work very similar to functions
            if (IsNotInsideModule(context))
            {
                var moduleName = context.identifier().IDENTIFIER().GetText();
                var builder = ModuleBuilder.NewModule(moduleName, MakeId("module", moduleName));
                
                // now find all the parameters
                var parameterDeclarations = context.parameterList().parameterDeclaration();
                foreach (var parameter in parameterDeclarations)
                {
                    var name = parameter.identifier().IDENTIFIER().GetText();
                    var portType = InferParameterType(parameter);
                    builder.WithParameter(name, portType);
                }
                // check if this module supports children
                if (SupportsChildren(context))
                {
                    builder.WithChildren();
                }

                var moduleDescription = builder.Build();
                // parse any documentation comment
                var documentationComment = ParseDocumentationComment(context.MODULE());
                documentationComment.ApplyTo(moduleDescription);
                _externalReference.Modules.Add(moduleDescription);
            }
            
            // and walk the rest of the tree
            return base.VisitModuleDeclaration(context);
        }
        
        /// <summary>
        /// Checks if the given module declaration supports children. It supports children if it has any
        /// module invocation in it that calls to the `children` module.
        /// </summary>
        private static bool SupportsChildren(IParseTree context)
        {
            var contextChildCount = context.ChildCount;
            for (var i = 0; i < contextChildCount; i++)
            {
                var child = context.GetChild(i);
                if (child is OpenScadParser.ModuleInvocationContext invocation)
                {
                    if (invocation.identifier().IDENTIFIER().GetText() == "children")
                    {
                        return true;
                    }
                }
                // recursively call this method on the child
                if (SupportsChildren(child))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether no parent context of the given context is a ModuleDeclarationContext.
        /// </summary>
        private static bool IsNotInsideModule(RuleContext context)
        {
            while (true)
            {
                switch (context.Parent)
                {
                    case null:
                        return true;
                    case OpenScadParser.ModuleDeclarationContext _:
                        return false;
                }

                context = context.Parent;
            }
        }

    }
}