using System;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library.External
{
    public static class ExternalReferenceBuilder
    {
        public static ExternalReference Build(ExternalFilePathMode pathMode, IncludeMode includeMode,
            string rawPath)
        {
            var reference = Prefabs.New<ExternalReference>();
            reference.Id = Guid.NewGuid().ToString();
            reference.Mode = includeMode;
            reference.SourceFile = PathResolver.Encode(rawPath, pathMode);

            return reference;
        }
    }
}