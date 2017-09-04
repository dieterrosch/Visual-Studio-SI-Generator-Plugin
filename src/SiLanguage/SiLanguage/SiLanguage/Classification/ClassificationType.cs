using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiLanguage
{
    internal static class OrdinaryClassificationDefinition
    {
        [Export]
        [Name("si")]
        internal static ClassificationTypeDefinition SiClassificationDefinition = null;

        [Export]
        [Name(ClassificationTypeName.SiComments)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Comments = null;

        [Export]
        [Name(ClassificationTypeName.SiDatatypes)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Datatypes = null;

        [Export]
        [Name(ClassificationTypeName.SiIdents)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Idents = null;

        [Export]
        [Name(ClassificationTypeName.SiKeywords)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Keywords = null;

        [Export]
        [Name(ClassificationTypeName.SiSqlKeywords)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition SqlKeywords = null;

        [Export]
        [Name(ClassificationTypeName.SiSqlFunction)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition SqlFunction = null;

        [Export]
        [Name(ClassificationTypeName.SiSqlOperator)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition SqlOperator = null;

        [Export]
        [Name(ClassificationTypeName.SiNumbers)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Numbers = null;

        [Export]
        [Name(ClassificationTypeName.SiOthers)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Others = null;

        [Export]
        [Name(ClassificationTypeName.SiPunctuations)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Punctuations = null;

        [Export]
        [Name(ClassificationTypeName.SiPlaceholders)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Placeholders = null;

        [Export]
        [Name(ClassificationTypeName.SiStrings)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Strings = null;

        [Export]
        [Name(ClassificationTypeName.SiSqlStrings)]
        [BaseDefinition("si")]
        internal static ClassificationTypeDefinition Sqlstrings = null;
    }
}
