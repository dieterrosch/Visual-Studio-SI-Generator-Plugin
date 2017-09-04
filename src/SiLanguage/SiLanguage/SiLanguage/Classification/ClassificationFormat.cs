using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiLanguage
{
    #region Format definition

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiComments)]
    [Name(ClassificationTypeName.SiComments)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiComments : ClassificationFormatDefinition
    {
        public SiComments()
        {
            DisplayName = "SI Comments";
            ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiDatatypes)]
    [Name(ClassificationTypeName.SiDatatypes)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiDatatypes : ClassificationFormatDefinition
    {
        public SiDatatypes()
        {
            DisplayName = "SI Datatypes";
            ForegroundColor = Colors.Teal;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiIdents)]
    [Name(ClassificationTypeName.SiIdents)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiIdents : ClassificationFormatDefinition
    {
        public SiIdents()
        {
            DisplayName = "SI Idents";
            ForegroundColor = Colors.LightGray;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiKeywords)]
    [Name(ClassificationTypeName.SiKeywords)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiKeywords : ClassificationFormatDefinition
    {
        public SiKeywords()
        {
            DisplayName = "SI Keywords";
            ForegroundColor = Colors.CornflowerBlue;
            IsBold = true;
            FontTypeface = new Typeface("Courier New");
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiNumbers)]
    [Name(ClassificationTypeName.SiNumbers)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiNumbers : ClassificationFormatDefinition
    {
        public SiNumbers()
        {
            DisplayName = "SI Numbers";
            ForegroundColor = Colors.Orange;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiOthers)]
    [Name(ClassificationTypeName.SiOthers)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiOthers : ClassificationFormatDefinition
    {
        public SiOthers()
        {
            DisplayName = "SI Others";
            ForegroundColor = Colors.Yellow;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiPlaceholders)]
    [Name(ClassificationTypeName.SiPlaceholders)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiPlaceholders : ClassificationFormatDefinition
    {
        public SiPlaceholders()
        {
            DisplayName = "SI Placeholders";
            ForegroundColor = Colors.DarkOrange;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiPunctuations)]
    [Name(ClassificationTypeName.SiPunctuations)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiPunctuations : ClassificationFormatDefinition
    {
        public SiPunctuations()
        {
            DisplayName = "SI Punctuations";
            ForegroundColor = Colors.Gray;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiSqlFunction)]
    [Name(ClassificationTypeName.SiSqlFunction)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiSqlFunction : ClassificationFormatDefinition
    {
        public SiSqlFunction()
        {
            DisplayName = "SI SQL Function";
            ForegroundColor = Colors.Magenta;
            IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiSqlKeywords)]
    [Name(ClassificationTypeName.SiSqlKeywords)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiSqlKeywords : ClassificationFormatDefinition
    {
        public SiSqlKeywords()
        {
            DisplayName = "SI SQL Keywords";
            ForegroundColor = Colors.CornflowerBlue;
            IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiSqlOperator)]
    [Name(ClassificationTypeName.SiSqlOperator)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiSqlOperator : ClassificationFormatDefinition
    {
        public SiSqlOperator()
        {
            DisplayName = "SI SQL Operator";
            ForegroundColor = Colors.Gray;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiStrings)]
    [Name(ClassificationTypeName.SiStrings)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiStrings : ClassificationFormatDefinition
    {
        public SiStrings()
        {
            DisplayName = "SI Strings";
            ForegroundColor = Colors.PaleGoldenrod;
            IsItalic = true;
            IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypeName.SiSqlStrings)]
    [Name(ClassificationTypeName.SiSqlStrings)]
    [UserVisible(true)]
    [Order(After = Priority.Default, Before = Priority.High)]
    public sealed class SiSqlStrings : ClassificationFormatDefinition
    {
        public SiSqlStrings()
        {
            DisplayName = "SI SQL Strings";
            ForegroundColor = Colors.Olive;
        }
    }

    #endregion
}
