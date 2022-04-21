using System;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library.External
{
    public static class ExternalReferenceBuilder
    {
        public static ExternalReference Build(IncludeMode includeMode, string rawPath, ExternalReference owner = null)
        {
            var reference = Prefabs.New<ExternalReference>();
            reference.Id = Guid.NewGuid().ToString();
            reference.Mode = includeMode;
            reference.SourceFile = rawPath;
            reference.IncludedBy = owner?.Id ?? "";
            reference.IsTransitive = owner != null;

            return reference;
        }
    }
}