using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using SiLanguage.Navigation;

namespace SiLanguage.Outlining
{
    internal sealed class OutliningTagger : ITagger<IOutliningRegionTag>
    {
        Dictionary<string, string> _regiondefs = new Dictionary<string, string> { { "PROC", "ENDCODE" }, { "SQLCODE", "ENDCODE" }, { "SQLDATA", "ENDDATA" }, { "TABLE", "{empty}" }, { "DATABASE", "{empty}" }, { "/*", "*/" }, { "INPUT", "OUTPUT+SQLCODE+{empty}+(-1)" }, { "OUTPUT", "SQLCODE+{empty}+(-1)" } };
        ITextBuffer _buffer;
        ITextSnapshot _snapshot;
        List<Region> _regions;

        public OutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            _regions = new List<Region>();
            ReParse();
            _buffer.Changed += BufferChanged;

            var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = _regions;
            ITextSnapshot currentSnapshot = _snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;
            foreach (var region in currentRegions)
            {
                if (region.StartLine <= endLineNumber &&
                    region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    //the region starts at the beginning of the "PROC", and goes until the *end* of the line that contains the "ENDCODE".
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(startLine.Start + region.StartOffset,
                        endLine.End),
                        new OutliningRegionTag(false, false, region.CollapsedForm, region.HoverText));
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            if (e.After != _buffer.CurrentSnapshot)
                return;
            ReParse();
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = _buffer.CurrentSnapshot;
            var newRegions = new List<Region>();

            //keep the current (deepest) partial region, which will have
            // references to any parent partial regions.

            if (Navigation.Navigation.NavItems != null)
            {
                Navigation.Navigation.NavItems.Clear();
            }
            else
            {
                Navigation.Navigation.NavItems = new Dictionary<string,List<NavItem>>();
            }
            var navItems = new List<NavItem>();

            var nameLine = newSnapshot.Lines.FirstOrDefault(x => x.GetText().Contains("Name:"));
            var name = "";
            if (nameLine != null)
                name = nameLine.GetText().Split(':')[1].Split('.')[0];

            if (!string.IsNullOrEmpty(name))
                Navigation.Navigation.NavItems.Add(name, null);

            foreach (var item in _regiondefs)
            {
                PartialRegion currentRegion = null;

                var end = item.Value.Split('+');

                foreach (var line in newSnapshot.Lines)
                {
                    int regionStart = -1;
                    string text = line.GetText();

                    //lines that contain a "PROC" denote the start of a new region.
                    if ((regionStart = text.ToUpper().IndexOf(item.Key, StringComparison.Ordinal)) != -1 && text.StartsWith(item.Key))
                    {
                        if (line.LineNumber < 10 && item.Key == "OUTPUT")
                        {
                            continue;
                        }

                        if (text.ToUpper().StartsWith("PROC") && text.Split().Count() > 1 && Navigation.Navigation.NavItems != null)
                        {
                            navItems.Add(new NavItem() { Name = text.Split()[1], Line = line.LineNumber + 1, Snapshot = line });
                        }

                        int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                        int newLevel;
                        if (!TryGetLevel(text, regionStart, out newLevel))
                            newLevel = currentLevel + 1;

                        //levels are the same and we have an existing region;
                        //end the current region and start the next
                        if (currentLevel == newLevel && currentRegion != null)
                        {
                            newRegions.Add(new Region()
                            {
                                CollapsedForm = newSnapshot.Lines.ElementAt(currentRegion.StartLine).GetText(),
                                HoverText = string.Join("\n", newSnapshot.Lines.Skip(currentRegion.StartLine).Take(line.LineNumber - (currentRegion.StartLine - 1)).Select(x => x.GetText())),
                                Level = currentRegion.Level,
                                StartLine = currentRegion.StartLine,
                                StartOffset = currentRegion.StartOffset,
                                EndLine = line.LineNumber
                            });

                            currentRegion = new PartialRegion()
                            {
                                Level = newLevel,
                                StartLine = line.LineNumber,
                                StartOffset = regionStart,
                                PartialParent = currentRegion.PartialParent
                            };
                        }
                        //this is a new (sub)region
                        else
                        {
                            currentRegion = new PartialRegion()
                            {
                                Level = newLevel,
                                StartLine = line.LineNumber,
                                StartOffset = regionStart,
                                PartialParent = currentRegion
                            };
                        }
                    }
                    //lines that contain "ENDCODE" denote the end of a region
                    else if (end.Any(x => text.Trim().EndsWith(x) || text.Trim().StartsWith(x)) 
                        && (regionStart = text.ToUpper().IndexOf(end.First(x => text.Trim().EndsWith(x) 
                            || text.Trim().StartsWith(x)), StringComparison.Ordinal)) != -1 
                        || (currentRegion != null && string.IsNullOrWhiteSpace(text) && item.Value.Contains("{empty}")))
                    {

                        int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                        int closingLevel;
                        int endLine = line.LineNumber;

                        if ((string.IsNullOrWhiteSpace(text) && item.Value.Contains("{empty}")) || end.Any(x => x == "(-1)"))
                        {
                            endLine -= 1;
                            regionStart = 0;
                        }

                        if (!TryGetLevel(text, regionStart, out closingLevel))
                            closingLevel = currentLevel;

                        //the regions match
                        if (currentRegion != null &&
                            currentLevel == closingLevel)
                        {
                            newRegions.Add(new Region()
                            {
                                CollapsedForm = newSnapshot.Lines.ElementAt(currentRegion.StartLine).GetText(),
                                HoverText = string.Join("\n", newSnapshot.Lines.Skip(currentRegion.StartLine).Take(line.LineNumber - (currentRegion.StartLine - 1)).Select(x => x.GetText())),
                                Level = currentLevel,
                                StartLine = currentRegion.StartLine,
                                StartOffset = currentRegion.StartOffset,
                                EndLine = endLine
                            });

                            currentRegion = currentRegion.PartialParent;
                        }

                        if (string.IsNullOrWhiteSpace(text) && item.Value == "{empty}")
                        {
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                navItems.Sort((x, y) => x.Name.ToString().CompareTo(y.Name.ToString()));
                Navigation.Navigation.NavItems[name] = navItems;
            }

            //determine the changed span, and send a changed event with the new spans
            var oldSpans =
                new List<Span>(_regions.Select(r => AsSnapshotSpan(r, _snapshot)
                    .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
                    .Span));
            var newSpans =
                    new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

            var oldSpanCollection = new NormalizedSpanCollection(oldSpans);
            var newSpanCollection = new NormalizedSpanCollection(newSpans);

            //the changed regions are regions that appear in one set or the other, but not both.
            var removed = NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

            int changeStart = int.MaxValue;
            int changeEnd = -1;

            if (removed.Count > 0)
            {
                changeStart = removed[0].Start;
                changeEnd = removed[removed.Count - 1].End;
            }

            if (newSpans.Count > 0)
            {
                changeStart = Math.Min(changeStart, newSpans[0].Start);
                changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
            }

            _snapshot = newSnapshot;
            _regions = newRegions;

            if (changeStart <= changeEnd)
            {
                ITextSnapshot snap = _snapshot;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_snapshot, Span.FromBounds(changeStart, changeEnd))));
            }
        }

        static bool TryGetLevel(string text, int startIndex, out int level)
        {
            level = -1;
            if (text.Length > startIndex + 3)
            {
                if (int.TryParse(text.Substring(startIndex + 1), out level))
                    return true;
            }

            return false;
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
            var endLine = (region.StartLine == region.EndLine) ? startLine : snapshot.GetLineFromLineNumber(region.EndLine);
            return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        }
    }

    class PartialRegion
    {
        public int StartLine { get; set; }
        public int StartOffset { get; set; }
        public int Level { get; set; }
        public PartialRegion PartialParent { get; set; }
    }

    class Region : PartialRegion
    {
        public string CollapsedForm { get; set; }
        public string HoverText { get; set; }

        public int EndLine { get; set; }
    } 
}
