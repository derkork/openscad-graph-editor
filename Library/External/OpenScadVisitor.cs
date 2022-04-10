using System.Linq;
using Antlr4.Runtime;
using Godot;

namespace OpenScadGraphEditor.Library.External
{
    public class OpenScadVisitor : OpenScadParserBaseVisitor<object>
    {
        private readonly ExternalReference _externalReference;
        private readonly string _sourceFileHash;


        public OpenScadVisitor(ExternalReference externalReference)
        {
            _externalReference = externalReference;
            _sourceFileHash = _externalReference.SourceFile.SHA256Text();
        }


        private string MakeId(string type, string name)
        {
            return $"{_sourceFileHash}_{type}_{name}";
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
                var variable = VariableBuilder.NewVariable(variableName, MakeId("variable", variableName));
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
                // for now we treat any parameter that is from an external function as PortType.ANY
                // same goes for the return type.
                
                var functionName = context.identifier().IDENTIFIER().GetText();
                var builder = FunctionBuilder.NewFunction(functionName, MakeId("function", functionName));
                
                // now find all the parameters
                var parameterDeclarations = context.parameterList().parameterDeclaration();
                foreach (var parameter in parameterDeclarations)
                {
                    var name = parameter.identifier().IDENTIFIER().GetText();
                    builder.WithParameter(name);
                }
                
                _externalReference.Functions.Add(builder.Build());
            }
            
            // and walk the rest of the tree
            return base.VisitFunctionDeclaration(context);
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
                    builder.WithParameter(name);
                }
                
                _externalReference.Modules.Add(builder.Build());
            }
            
            // and walk the rest of the tree
            return base.VisitModuleDeclaration(context);
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