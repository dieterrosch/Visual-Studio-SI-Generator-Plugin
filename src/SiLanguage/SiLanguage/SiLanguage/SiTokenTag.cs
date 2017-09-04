// Copyright (c) Microsoft Corporation
// All rights reserved
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using SiLanguage.BraceMatching;
using SiLanguage.Outlining;


namespace SiLanguage
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("si")]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            //create a single tagger for each buffer.
            Func<ITagger<T>> sc = delegate () { return new OutliningTagger(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        }
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("si")]
    [TagType(typeof(TextMarkerTag))]
    internal class BraceMatchingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
                return null;

            //provide highlighting only on the top-level buffer
            if (textView.TextBuffer != buffer)
                return null;

            return new BraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType("si")]
    [TagType(typeof(SiTokenTag))]
    internal sealed class SiTokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag => new SiTokenTagger(buffer) as ITagger<T>;
    }
    public class SiTokenTag : ITag
    {
        public SiTokenTypes Type { get; private set; }
        public SiTokenTag(SiTokenTypes type)
        {
            Type = type;
        }
    }
    internal sealed class SiTokenTagger : ITagger<SiTokenTag>
    {
        ITextBuffer buffer;
        SiTokenizer tokenizer;
        internal SiTokenTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            tokenizer = new SiTokenizer();
        }
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
        public IEnumerable<ITagSpan<SiTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int position = containingLine.Start.Position;
                var tokens = new List<SiToken>();
                tokenizer.getTokens(tokens, containingLine.GetText().ToLower(), position, IsSqlSection(containingLine));
                foreach (SiToken token in tokens)
                {
                    int pos = position + (token.position - token.value.Length);
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(pos, token.value.Length));
                    var tokenTag = new SiTokenTag(token.tokenType);
                    if (tokenSpan.IntersectsWith(curSpan))
                        yield return new TagSpan<SiTokenTag>(tokenSpan, tokenTag);
                }
            }
        }

        private bool IsSqlSection(ITextSnapshotLine containingLine)
        {
            if (containingLine.LineNumber == 0)
                return false;

            for (int i = containingLine.LineNumber; i > 0; i--)
            {
                var lineText = containingLine.Snapshot.Lines.ElementAt(i).GetText();
                if (lineText.ToUpper().StartsWith("KEY") || lineText.ToUpper().StartsWith("LINK") || lineText.ToUpper().StartsWith("PROC")
                    || lineText.ToUpper().StartsWith("OUTPUT") || lineText.ToUpper().StartsWith("INPUT") || lineText.ToUpper().StartsWith("ENDCODE"))
                {
                    return false;
                }

                if (lineText.ToUpper().StartsWith("SQLCODE") || lineText.ToUpper().StartsWith("SQLDATA"))
                    return true;
            }

            return false;

        }
    }
}
