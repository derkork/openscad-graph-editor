using Antlr4.Runtime;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// This parser parses OpenScad code into our internal project representation, so that we can reference it in our UI.
    /// </summary>
    public static class ExternalFileParser
    {
        public static void Parse(string text, ExternalReference externalReference)
        {
            var inputStream = new AntlrInputStream(text);
            var lexer = new OpenScadLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new OpenScadParser(tokenStream);

            var root = parser.scadFile();
            var visitor = new OpenScadVisitor(externalReference);
            visitor.Visit(root);

            externalReference.IsLoaded = true;
        }
    }
}