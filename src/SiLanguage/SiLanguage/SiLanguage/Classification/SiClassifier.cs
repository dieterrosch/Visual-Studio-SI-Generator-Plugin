using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;


namespace SiLanguage
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("si")]
    [TagType(typeof(ClassificationTag))]
    public sealed class SiClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("si")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition SiContentType = null;

        [Export]
        [FileExtension(".si")]
        [ContentType("si")]
        public static FileExtensionToContentTypeDefinition SiFileType = null;

        [Import]
        public IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        public IBufferTagAggregatorFactoryService AggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<SiTokenTag> siTagAggregator = AggregatorFactory.CreateTagAggregator<SiTokenTag>(buffer);
            return new SiClassifier(buffer, siTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    public sealed class SiClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITextBuffer _SiTokenTag;
        ITagAggregator<SiTokenTag> _aggregator;
        IDictionary<SiTokenTypes, IClassificationType> types;
        public SiClassifier(ITextBuffer siBuffer, ITagAggregator<SiTokenTag> siAggregator, IClassificationTypeRegistryService typeService)
        {
            _buffer = siBuffer;
            _aggregator = siAggregator;
            types = new Dictionary<SiTokenTypes, IClassificationType>();
            types[SiTokenTypes.siComment] = typeService.GetClassificationType(ClassificationTypeName.SiComments);
            types[SiTokenTypes.siDatatype] = typeService.GetClassificationType(ClassificationTypeName.SiDatatypes);
            types[SiTokenTypes.siIdent] = typeService.GetClassificationType(ClassificationTypeName.SiIdents);
            types[SiTokenTypes.siKeyword] = typeService.GetClassificationType(ClassificationTypeName.SiKeywords);
            types[SiTokenTypes.siSqlKeyword] = typeService.GetClassificationType(ClassificationTypeName.SiSqlKeywords);
            types[SiTokenTypes.siSqlFuntion] = typeService.GetClassificationType(ClassificationTypeName.SiSqlFunction);
            types[SiTokenTypes.siSqlOperator] = typeService.GetClassificationType(ClassificationTypeName.SiSqlOperator);
            types[SiTokenTypes.siNumber] = typeService.GetClassificationType(ClassificationTypeName.SiNumbers);
            types[SiTokenTypes.siOther] = typeService.GetClassificationType(ClassificationTypeName.SiOthers);
            types[SiTokenTypes.siPunctuation] = typeService.GetClassificationType(ClassificationTypeName.SiPunctuations);
            types[SiTokenTypes.siPlaceholder] = typeService.GetClassificationType(ClassificationTypeName.SiPlaceholders);
            types[SiTokenTypes.siString] = typeService.GetClassificationType(ClassificationTypeName.SiStrings);
            types[SiTokenTypes.siSqlString] = typeService.GetClassificationType(ClassificationTypeName.SiSqlStrings);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(types[tagSpan.Tag.Type]));
            }
        }
    }
}