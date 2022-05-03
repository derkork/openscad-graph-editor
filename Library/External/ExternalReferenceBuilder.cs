using System;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library.External
{
    public static class ExternalReferenceBuilder
    {
        public static ExternalReference Build(IncludeMode includeMode, string sourceFile, ExternalReference owner = null)
        {
            var reference = new ExternalReference
            {
                Id = Guid.NewGuid().ToString(),
                Mode = includeMode,
                IncludePath = sourceFile,
                IncludedBy = owner?.Id ?? ""
            };

            return reference;
        }
        public static ExternalReference BuildEmptyCopy(ExternalReference original)
        {
            var reference = new ExternalReference
            {
                Id = Guid.NewGuid().ToString(),
                Mode = original.Mode,
                IncludePath = original.IncludePath,
                IncludedBy = original.IncludedBy
            };

            return reference;
        }
    }
}