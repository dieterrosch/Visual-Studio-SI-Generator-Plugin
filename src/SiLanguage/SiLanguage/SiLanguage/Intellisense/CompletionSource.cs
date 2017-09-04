using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace SiLanguage
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("si")]
    [Name("Completion Source")]
    class SiCompletionSourceProvider : ICompletionSourceProvider
    {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) => new SiCompletionSource(textBuffer);
    }

    class SiCompletionSource : ICompletionSource
    {
        private static Dictionary<string, KeyValuePair<DateTime,List<string>>> _externalCompletion;
        private static Dictionary<string, KeyValuePair<string, string>> _siFiles;

        static SiCompletionSource()
        {
            _externalCompletion = new Dictionary<string, KeyValuePair<DateTime, List<string>>>();
            _siFiles = new Dictionary<string, KeyValuePair<string, string>>();

            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

            foreach (EnvDTE.Project project in dte.ActiveSolutionProjects as Array)
            {
                BuildInitialCache(project);
            }
        }

        private static void BuildInitialCache(EnvDTE.Project project)
        {
            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
            {
                if (item.Name.EndsWith(".si"))
                {
                    var path = item.Properties.Item("FullPath").Value as string;

                    var lines = File.ReadAllLines(path);

                    var add = false;
                    var fields = new List<string>();
                    var tableName = "";
                    foreach (var l in lines)
                    {
                        if (l.StartsWith("TABLE"))
                        {
                            add = true;
                            var comps = l.Split();
                            if (comps.Count() > 1)
                            {
                                tableName = comps[1];
                            }
                            continue;
                        }

                        if (add && string.IsNullOrWhiteSpace(l) || l.StartsWith("KEY") || l.StartsWith("PROC"))
                            break;

                        if (add)
                        {
                            fields.Add(l.Trim().Split()[0].Replace("l'", "").Replace("'", ""));
                        }
                    }
                    _siFiles.Add(item.Name.Split('.')[0].ToLower(), new KeyValuePair<string, string>(item.Properties.Item("FullPath").Value as string, tableName));
                    _externalCompletion.Add(item.Name.Split('.')[0].ToLower(), new KeyValuePair<DateTime, List<string>>(File.GetLastWriteTime(path), fields));
                }
                else if (item.ProjectItems.Count > 0)
                {
                    BuildInitialCache(item);
                }
            }
        }

        private static void BuildInitialCache(EnvDTE.ProjectItem project)
        {
            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
            {
                if (item.Name.EndsWith(".si"))
                {
                    var path = item.Properties.Item("FullPath").Value as string;

                    var lines = File.ReadAllLines(path);

                    var add = false;
                    var fields = new List<string>();
                    var tableName = "";
                    foreach (var l in lines)
                    {
                        if (l.StartsWith("TABLE"))
                        {
                            add = true;
                            var comps = l.Split();
                            if (comps.Count() > 1)
                            {
                                tableName = comps[1];
                            }
                            continue;
                        }

                        if (add && string.IsNullOrWhiteSpace(l) || l.StartsWith("KEY") || l.StartsWith("PROC"))
                            break;

                        if (add)
                        {
                            fields.Add(l.Trim().Split()[0].Replace("l'", "").Replace("'", ""));
                        }
                    }
                    _siFiles.Add(item.Name.Split('.')[0].ToLower(), new KeyValuePair<string, string>(item.Properties.Item("FullPath").Value as string, tableName));
                    _externalCompletion.Add(item.Name.Split('.')[0].ToLower(), new KeyValuePair<DateTime, List<string>>(File.GetLastWriteTime(path), fields));
                }
                else if (item.ProjectItems.Count > 0)
                {
                    BuildInitialCache(item);
                }
            }
        }

        private ITextBuffer _buffer;
        private bool _isDisposed = false;

        public SiCompletionSource(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        private int Compare(Completion a, Completion b) => a.DisplayText.CompareTo(b.DisplayText);

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("SiCompletionSource");
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(snapshot);
            if (triggerPoint == null)
                return;
            var line = triggerPoint.GetContainingLine();
            SnapshotPoint start = triggerPoint;
            while (start > line.Start && (!char.IsWhiteSpace((start - 1).GetChar()) && !(new char[] { ',', '.', '(', ')', '[', ']' }.Contains((start - 1).GetChar()))))
            {
                start -= 1;
            }
            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint), SpanTrackingMode.EdgeInclusive);

            if (start.Position == start.Snapshot.Length)
                return;

            if ((start - 1).GetChar() == '.')
            {
                var alias = "";
                while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
                {
                    start -= 1;
                    alias = start.GetChar() + alias;
                }

                alias = alias.Replace(".", "");

                if (alias.Length > 0)
                {
                    alias = " " + alias + " ";
                    bool found = false;
                    var table = "";
                    var idx = line.LineNumber;
                    while (!found)
                    {
                        if (idx < 0)
                            break;

                        var currentLine = line.Snapshot.Lines.ElementAt(idx).GetText();

                        if (currentLine.Contains(alias))
                        {
                            found = true;
                            table = currentLine.Substring(0, currentLine.LastIndexOf(alias)).Trim().Split().Last();
                        }
                        else if (currentLine.EndsWith(alias.TrimEnd()))
                        {
                            found = true;
                            table = currentLine.Substring(0, currentLine.LastIndexOf(alias.TrimEnd())).Trim().Split().Last();
                        }
                        else if (currentLine.StartsWith("PROC") || currentLine.StartsWith("ENDCODE"))
                        {
                            break;
                        }
                        idx--;
                    }

                    idx = line.LineNumber;
                    while (!found)
                    {
                        if (idx < 0)
                            break;

                        var currentLine = line.Snapshot.Lines.ElementAt(idx).GetText();

                        if (currentLine.Contains(alias))
                        {
                            found = true;
                            table = currentLine.Substring(0, currentLine.LastIndexOf(alias)).Trim().Split().Last();
                        }
                        else if (currentLine.EndsWith(alias.TrimEnd()))
                        {
                            found = true;
                            table = currentLine.Substring(0, currentLine.LastIndexOf(alias.TrimEnd())).Trim().Split().Last();
                        }
                        else if (currentLine.StartsWith("PROC") || currentLine.StartsWith("ENDCODE"))
                        {
                            break;
                        }
                        idx++;
                    }

                    if (string.IsNullOrWhiteSpace(table))
                    {
                        found = false;
                    }

                    var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                    if (!found)
                    {
                        if (_siFiles.ContainsKey(alias.Trim().ToLower()))
                        {
                            found = true;
                            table = alias.Trim().ToLower();
                        }
                        else
                        {
                            if (File.Exists(Path.Combine(dte.ActiveDocument.Path, alias.Trim().ToLower() + ".si")))
                            {
                                found = true;
                                table = alias.Trim().ToLower();
                            }
                        }
                    }

                    if (found)
                    {
                        string siFile = "";

                        if (_siFiles.ContainsKey(table.ToLower()))
                        {
                            siFile = _siFiles[table.ToLower()].Key;
                        }
                        else
                        {
                            siFile = Path.Combine(dte.ActiveDocument.Path, table.ToLower() + ".si");
                        }

                        if (!_externalCompletion.ContainsKey(table.ToLower()) || _externalCompletion[table.ToLower()].Key < File.GetLastWriteTime(siFile))
                        {
                            if (File.Exists(siFile))
                            {
                                var lines = File.ReadAllLines(siFile);

                                bool add = false;
                                var fields = new List<string>();
                                var tableName = "";
                                foreach (var l in lines)
                                {
                                    if (l.StartsWith("TABLE"))
                                    {
                                        add = true;
                                        var comps = l.Split();
                                        if (comps.Count() > 1)
                                        {
                                            tableName = comps[1];
                                        }
                                        continue;
                                    }

                                    if (add && string.IsNullOrWhiteSpace(l) || l.StartsWith("KEY") || l.StartsWith("PROC"))
                                        break;

                                    if (add)
                                    {
                                        fields.Add(l.Trim().Split()[0].Replace("l'", "").Replace("'", ""));
                                    }
                                }

                                if (_externalCompletion.ContainsKey(table.ToLower()))
                                    _externalCompletion.Remove(table.ToLower());

                                _externalCompletion.Add(table.ToLower(), new KeyValuePair<DateTime, List<string>>(File.GetLastWriteTime(siFile), fields));

                                if (!_siFiles.ContainsKey(table.ToLower()))
                                    _siFiles.Add(table.ToLower(), new KeyValuePair<string, string>(siFile, tableName));
                            }
                        }

                        if (_externalCompletion.ContainsKey(table.ToLower()))
                        {
                            var completions = new List<Completion>();
                            _externalCompletion[table.ToLower()].Value.ForEach(x => completions.Add(new Completion(x)));
                            completions.Sort(Compare);
                            completionSets.Add(new CompletionSet("External", "External", applicableTo, completions, Enumerable.Empty<Completion>()));
                            return;
                        }
                    }
                }
            }

            if (line.GetText().ToUpper().StartsWith("LINK"))
            {
                var compList = new List<Completion>();
                var add = false;
                foreach (var cline in _buffer.CurrentSnapshot.Lines)
                {
                    var lineText = cline.GetText();

                    if (lineText.ToUpper().StartsWith("TABLE"))
                    {
                        compList.Add(new Completion(lineText.Split()[1]));
                        add = true;
                        continue;
                    }

                    if (lineText.ToUpper().StartsWith("KEY") || lineText.ToUpper().StartsWith("LINK") || lineText.ToUpper().StartsWith("PROC"))
                    {
                        break;
                    }

                    if (add)
                    {
                        if (!string.IsNullOrEmpty(lineText.Trim()) && !lineText.Trim().StartsWith("/"))
                        {
                            if (!lineText.Trim().Split()[0].Contains("="))
                                compList.Add(new Completion(lineText.Trim().Split()[0]));
                        }
                    }
                }

                compList.AddRange(_siFiles.Values.Select(x => x.Value).Select(y => new Completion(y)));
                compList.Sort(Compare);
                completionSets.Add(new CompletionSet("Links", "Links", applicableTo, compList, Enumerable.Empty<Completion>()));
            }
            else if (start.GetChar() == ':')
            {
                int inputIndex = 0;
                var compList = new List<Completion>();
                var lineObj = triggerPoint.GetContainingLine();
                if (!lineObj.GetText().ToUpper().StartsWith("SQLDATA"))
                {
                    for (int i = lineObj.LineNumber; i > 0; i--)
                    {
                        var lineText = triggerPoint.Snapshot.Lines.ElementAt(i).GetText();
                        if (lineText.ToUpper().StartsWith("INPUT"))
                        {
                            inputIndex = i + 1;
                            break;
                        }

                    }
                }

                if (inputIndex > 0)
                {
                    while (true)
                    {
                        var inLine = triggerPoint.Snapshot.Lines.ElementAt(inputIndex).GetText();

                        if (inLine.ToUpper().StartsWith("OUTPUT") || inLine.ToUpper().StartsWith("SQLCODE"))
                        {
                            break;
                        }

                        compList.Add(new Completion(":" + inLine.Trim().Split()[0]));
                        inputIndex++;
                    }
                }

                compList.Sort(Compare);

                completionSets.Add(new CompletionSet("Local", "Local", applicableTo, compList, Enumerable.Empty<Completion>()));
            }
            else if (IsSqlSection(triggerPoint))
            {
                completionSets.Add(new CompletionSet("Local", "Local", applicableTo, GetSqlCodeCompletion(triggerPoint), Enumerable.Empty<Completion>()));
            }
            else
            {
                var completions = new List<Completion>();

                string[] keywords = SiTokenizer.Keywords.Substring(1, SiTokenizer.Keywords.Length - 2).Split(':');
                string[] dataTypes = SiTokenizer.DataTypes.Substring(1, SiTokenizer.DataTypes.Length - 2).Split(':');

                foreach (string word in keywords)
                {
                    var comp = new Completion(SiTokenizer.SiKeywordMap[word]);
                    comp.InsertionText = word;
                    if (word == "UpdateBy")
                    {
                        comp.InsertionText = "UpdateBy   FOR";
                    }
                    if (word == "Select")
                    {
                        comp.InsertionText = "Select\nINPUT\n   \nOUTPUT\n  \nFROM\n    \"\"\nWHERE\n    \"\"\n";
                    }

                    if (word == "SQLCODE")
                    {
                        comp.InsertionText = "SQLCODE\n\nENDCODE";
                    }
                    if (word == "SQLDATA")
                    {
                        comp.InsertionText = "SQLDATA\n\nENDDATA\n";
                    }
                    completions.Add(comp);
                }

                foreach (string word in dataTypes) completions.Add(new Completion(SiTokenizer.SiDatatypeMap[word]) { InsertionText = word });

                bool startSearch = false;
                foreach (var lineItem in _buffer.CurrentSnapshot.Lines)
                {
                    var lineText = lineItem.GetText();

                    if (lineText.ToUpper().StartsWith("TABLE"))
                    {
                        startSearch = true;
                        continue;
                    }

                    if (lineText.ToUpper().StartsWith("KEY") || lineText.ToUpper().StartsWith("LINK") || lineText.ToUpper().StartsWith("PROC"))
                    {
                        break;
                    }

                    if (startSearch)
                    {
                        if (!string.IsNullOrEmpty(lineText.Trim()) && !lineText.Trim().StartsWith("/"))
                        {
                            if (!lineText.Trim().Split()[0].Contains("="))
                                completions.Add(new Completion(lineText.Trim().Split()[0]));
                        }
                    }
                }

                completions.Sort(Compare);

                completionSets.Add(new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Completion>()));
            }
        }

        private bool IsSqlSection(SnapshotPoint triggerPoint)
        {
            var line = triggerPoint.GetContainingLine();

            if (line.LineNumber == 0)
                return false;

            for (int i = line.LineNumber; i > 0; i--)
            {
                var lineText = triggerPoint.Snapshot.Lines.ElementAt(i).GetText();
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

        private List<Completion> GetSqlCodeCompletion(SnapshotPoint triggerPoint)
        {
            var compList = new List<Completion>();

            var start = false;
            foreach (var line in _buffer.CurrentSnapshot.Lines)
            {
                var lineText = line.GetText();

                if (lineText.ToUpper().StartsWith("TABLE"))
                {
                    compList.Add(new Completion(lineText.Split()[1]));
                    start = true;
                    continue;
                }

                if (lineText.ToUpper().StartsWith("KEY") || lineText.ToUpper().StartsWith("LINK") || lineText.ToUpper().StartsWith("PROC"))
                {
                    break;
                }

                if (start)
                {
                    if (!string.IsNullOrEmpty(lineText.Trim()) && !lineText.Trim().StartsWith("/"))
                    {
                        if (!lineText.Trim().Split()[0].Contains("="))
                            compList.Add(new Completion(lineText.Trim().Split()[0]));
                    }
                }
            }
            string[] sqlKeywords = SiTokenizer.SqlKeywords.Substring(1, SiTokenizer.SqlKeywords.Length - 2).Split(':');
            string[] sqlFunctions = SiTokenizer.SqlFunctions.Substring(1, SiTokenizer.SqlFunctions.Length - 2).Split(':');
            string[] sqlOperators = SiTokenizer.SqlOperators.Substring(1, SiTokenizer.SqlOperators.Length - 2).Split(':');

            foreach (var word in sqlKeywords) compList.Add(new Completion(SiTokenizer.SqlKeywordMap[word]) { InsertionText = word });
            foreach (var word in sqlFunctions) compList.Add(new Completion(SiTokenizer.SqlFunctionMap[word]) { InsertionText = word });
            foreach (var word in sqlOperators) compList.Add(new Completion(SiTokenizer.SqlOperatorMap[word]) { InsertionText = word });

            int inputIndex = 0;
            var lineObj = triggerPoint.GetContainingLine();
            if (!lineObj.GetText().ToUpper().StartsWith("SQLDATA"))
            {
                for (int i = lineObj.LineNumber; i > 0; i--)
                {
                    var lineText = triggerPoint.Snapshot.Lines.ElementAt(i).GetText();
                    if (lineText.ToUpper().StartsWith("INPUT"))
                    {
                        inputIndex = i + 1;
                        break;
                    }

                }
            }

            if (inputIndex > 0)
            {
                while (true)
                {
                    var inLine = triggerPoint.Snapshot.Lines.ElementAt(inputIndex).GetText();

                    if (inLine.ToUpper().StartsWith("OUTPUT") || inLine.ToUpper().StartsWith("SQLCODE"))
                    {
                        break;
                    }

                    compList.Add(new Completion(":" + inLine.Trim().Split()[0]));
                    inputIndex++;
                }
            }

            foreach (var item in _siFiles)
            {
                compList.Add(new Completion(item.Value.Value));
            }

            compList.Sort(Compare);

            return compList;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}


