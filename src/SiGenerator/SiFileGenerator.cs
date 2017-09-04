using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SiGenerator
{
    [ComVisible(true)]
    [Guid("52B316AA-1997-4c81-9969-83604C09EEB4")]
    //[Guid("E809C61C-BABF-4103-A27A-DDB5787FDB51")]
    /* .NET FX C# Class library   */    [CodeGeneratorRegistration(typeof(SiFileGenerator), "Si Code Generator", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
    /* .NET Core C# Class library */    [CodeGeneratorRegistration(typeof(SiFileGenerator), "Si Code Generator", "{FAE04EC0-301F-11d3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
    /* .NET CPS Support           */    [CodeGeneratorRegistration(typeof(SiFileGenerator), "Si Code Generator", "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}", GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(SiFileGenerator))]
    public class SiFileGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        #pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "SiFileGenerator";
        #pragma warning restore 0414

        #region Visual Studio Specific Fields
        private object site;
        private ServiceProvider _serviceProvider;
        #endregion

        #region Our Fields
        //private string _bstrInputFileContents;
        //private string _wszInputFilePath;
        private EnvDTE.Project _project;

        private List<string> _newFileNames;
        #endregion

        private IVsGeneratorProgress _codeGeneratorProgress;
        internal IVsGeneratorProgress CodeGeneratorProgress
        {
            get
            {
                return _codeGeneratorProgress;
            }
        }

        protected EnvDTE.Project Project
        {
            get
            {
                return _project;
            }
        }

        protected string InputFileContents { get; private set; }

        protected string InputFilePath { get; private set; }

        private ServiceProvider SiteServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleServiceProvider = site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                    _serviceProvider = new ServiceProvider(oleServiceProvider);
                }
                return _serviceProvider;
            }
        }

        public SiFileGenerator()
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            Array ary = dte.ActiveSolutionProjects as Array;
            if (ary.Length > 0)
            {
                _project = (EnvDTE.Project)ary.GetValue(0);

            }
            _newFileNames = new List<string>();
        }

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ""; // the extension must include the leading period
            return VSConstants.E_NOTIMPL; // signal successful completion
        }


        public byte[] GenerateSummaryContent()
        {
            // Im not going to put anything in here...
            return new byte[0];
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            try
            {
                bool oldVsProject = site.GetType().IsCOMObject;
                InputFileContents = bstrInputFileContents;
                InputFilePath = wszInputFilePath;
                _codeGeneratorProgress = pGenerateProgress;
                _newFileNames.Clear();

                int iFound = 0;
                uint itemId = 0;
                EnvDTE.ProjectItem item;
                Microsoft.VisualStudio.Shell.Interop.VSDOCUMENTPRIORITY[] pdwPriority = new Microsoft.VisualStudio.Shell.Interop.VSDOCUMENTPRIORITY[1];

                // obtain a reference to the current project as an IVsProject type
                //Microsoft.VisualStudio.Shell.Interop.IVsProject VsProject = VsHelper.ToVsProject(_project);
                Microsoft.VisualStudio.Shell.Interop.IVsProject VsProject;
                if (oldVsProject)
                    VsProject = VsHelper.ToVsProject(_project);
                else
                    VsProject = VsHelper.ToNewVsProject(_project, (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_project.DTE);
                // this locates, and returns a handle to our source file, as a ProjectItem
                VsProject.IsDocumentInProject(InputFilePath, out iFound, pdwPriority, out itemId);


                // if our source file was found in the project (which it should have been)
                if (iFound != 0 && itemId != 0)
                {
                    VsProject.GetItemContext(itemId, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleSp);
                    if (oleSp != null)
                    {
                        var sp = new ServiceProvider(oleSp);
                        // convert our handle to a ProjectItem
                        item = sp.GetService(typeof(EnvDTE.ProjectItem)) as EnvDTE.ProjectItem;
                    }
                    else
                        throw new ApplicationException("Unable to retrieve Visual Studio ProjectItem");
                }
                else
                    throw new ApplicationException("Unable to retrieve Visual Studio ProjectItem");

                var generatePath = Path.GetDirectoryName(wszInputFilePath);
                var configPath = Path.Combine(generatePath, "gen.config");
                var genTypes = (from line in File.ReadAllLines(configPath)
                                where !line.StartsWith("//", StringComparison.Ordinal)
                                where !string.IsNullOrWhiteSpace(line)
                                select line).ToArray();
                var siFileName = Path.GetFileName(wszInputFilePath);

                Debug.WriteLine("Start Custom Tool.");
                int lineCount = bstrInputFileContents.Split('\n').Length;
                byte[] bytes = Encoding.UTF8.GetBytes(lineCount.ToString() + " LOC");
                int length = bytes.Length;
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], length);
                pcbOutput = (uint)length;
                // Use ProcessStartInfo class.
                var startInfo = new ProcessStartInfo
                {
                    FileName = "java.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Path.GetDirectoryName(_project.FullName)
                };

                bool isfirst = true;
                var args = new StringBuilder();
                foreach (var type in genTypes)
                {
                    if (isfirst)
                    {
                        args.Append($"-jar \"{type}\" \"{wszInputFilePath}\"");
                        isfirst = false;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(type) && !type.StartsWith("//"))
                        {
                            var genComps = type.Split();
                            if (genComps.Length == 1)
                            {
                                args.Append($" -o \"{generatePath}\" {type}");
                            }
                            else
                            {
                                args.Append(" -o \"" + generatePath + type.Substring(type.IndexOf(" ")).Trim() + "\" " + genComps[0]);
                                Directory.CreateDirectory(generatePath + type.Substring(type.IndexOf(" ")).Trim());
                            }
                        }
                    }
                }
                startInfo.Arguments = args.ToString();

                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using-statement will close.
                    using (var exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();

                        //var output = "";
                        //while (!exeProcess.StandardOutput.EndOfStream)
                        //{
                        //    output += exeProcess.StandardOutput.ReadLine() + "\n";
                        //}
                        var sb = new StringBuilder();
                        while (!exeProcess.StandardOutput.EndOfStream)
                        {
                            sb.AppendLine(exeProcess.StandardOutput.ReadLine());
                        }
                        var output = sb.ToString();

                        bool start = false;
                        foreach (var oline in Regex.Split(output, "\r\n|\r|\n"))
                        {
                            if (genTypes.Contains(oline) || genTypes.Any(x => x.StartsWith(oline)))
                                start = true;

                            if (start)
                            {
                                if (!string.IsNullOrEmpty(oline) && !oline.Trim().StartsWith("(") && !genTypes.Contains(oline))
                                {
                                    _newFileNames.Add(oline.Substring(oline.IndexOf(":") + 2).Trim());
                                }
                            }
                        }

                        if (output.Contains("ParseException"))
                        {
                            var exception = output.Substring(output.IndexOf("ParseException"));
                            exception = exception.Substring(0, exception.IndexOf('\n'));

                            uint line = uint.Parse(exception.Split(new string[] { " line " }, StringSplitOptions.None)[1].Split(',')[0]) - 1;
                            uint column = uint.Parse(exception.Split(new string[] { " column " }, StringSplitOptions.None)[1].Split('.')[0]) - 1;

                            GeneratorError(4, output.Replace("\n", " "), line, column);

                            foreach (EnvDTE.ProjectItem childItem in item.ProjectItems)
                            {
                                childItem.Delete();
                            }
                            return VSConstants.E_FAIL;
                        }
                        else if (output.Contains("bbd.jportal.TokenMgrError"))
                        {
                            var exception = output.Substring(output.IndexOf("bbd.jportal.TokenMgrError"));
                            exception = exception.Substring(0, exception.IndexOf('\n'));

                            uint line = uint.Parse(exception.Split(new string[] { " line " }, StringSplitOptions.None)[1].Split(',')[0]) - 1;
                            uint column = uint.Parse(exception.Split(new string[] { " column " }, StringSplitOptions.None)[1].Split('.')[0]) - 1;

                            GeneratorError(4, output.Replace("\n", " "), line, column);

                            foreach (EnvDTE.ProjectItem childItem in item.ProjectItems)
                            {
                                childItem.Delete();
                            }
                            return VSConstants.E_FAIL;
                        }
                    }
                }
                catch (Exception ex)
                {
                    pcbOutput = 0;
                    GeneratorError(4, ex.ToString(), 0, 0);
                    File.WriteAllText("sifilegenerator.log", "Si file generation failed for: " + Directory.GetCurrentDirectory() + "\r\n");
                    File.AppendAllText("sifilegenerator.log", ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.InnerException);
                    File.AppendAllText("sifilegenerator.log", Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                    return VSConstants.E_FAIL;
                }

                foreach (var pfile in _newFileNames.ToList())
                {
                    try
                    {
                        if (File.Exists(pfile) && Directory.GetParent(pfile).FullName == generatePath)
                        {
                            EnvDTE.ProjectItem itm = item.ProjectItems.AddFromFile(pfile);
                        }
                        else
                        {
                            _newFileNames.Remove(pfile);
                        }
                    }
                    catch (COMException) { }
                }

                /*
                 * Here you may wish to perform some addition logic
                 * such as, setting a custom tool for the target file if it
                 * is intented to perform its own generation process.
                 * Or, set the target file as an 'Embedded Resource' so that
                 * it is embedded into the final Assembly.
             
                EnvDTE.Property prop = itm.Properties.Item("CustomTool");
                //// set to embedded resource
                itm.Properties.Item("BuildAction").Value = 3;
                if (String.IsNullOrEmpty((string)prop.Value) || !String.Equals((string)prop.Value, typeof(AnotherCustomTool).Name))
                {
                    prop.Value = typeof(AnotherCustomTool).Name;
                            }
                            */

                // perform some clean-up, making sure we delete any old (stale) target-files
                foreach (EnvDTE.ProjectItem childItem in item.ProjectItems)
                {
                    if (!_newFileNames.Select(x => x.Substring(x.LastIndexOf("\\") + 1)).Contains(childItem.Name))
                    {
                        // then delete it
                        childItem.Delete();
                    }
                }

                // generate our summary content for our 'single' file
                byte[] summaryData = GenerateSummaryContent();

                if (summaryData == null)
                {
                    rgbOutputFileContents[0] = IntPtr.Zero;
                    pcbOutput = 0;
                }
                else
                {
                    // return our summary data, so that Visual Studio may write it to disk.
                    rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(summaryData.Length);
                    Marshal.Copy(summaryData, 0, rgbOutputFileContents[0], summaryData.Length);
                    pcbOutput = (uint)summaryData.Length;
                }
            }
            catch (Exception e)
            {
                pcbOutput = 0;
                GeneratorError(4, e.ToString(), 0, 0);
                var errorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SiFileGenerator");
                if (!Directory.Exists(errorPath))
                    Directory.CreateDirectory(errorPath);
                var filePath = Path.Combine(errorPath, "Error.txt");

                using (var writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(e);
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }

                return VSConstants.E_FAIL;
            }
            
            return VSConstants.S_OK;
        }

        protected virtual void GeneratorError(uint level, string message, uint line, uint column)
        {
            IVsGeneratorProgress progress = CodeGeneratorProgress;
            if (progress != null)
            {
                progress.GeneratorError(0, level, message, line, column);
            }
        }


        #region IObjectWithSite Members

        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (site == null)
            {
                throw new Win32Exception(-2147467259);
            }

            IntPtr objectPointer = Marshal.GetIUnknownForObject(site);

            try
            {
                Marshal.QueryInterface(objectPointer, ref riid, out ppvSite);
                if (ppvSite == IntPtr.Zero)
                {
                    throw new Win32Exception(-2147467262);
                }
            }
            finally
            {
                if (objectPointer != IntPtr.Zero)
                {
                    Marshal.Release(objectPointer);
                    objectPointer = IntPtr.Zero;
                }
            }
        }

        public void SetSite(object pUnkSite)
        {
            site = pUnkSite;
        }

        #endregion

    }
}

