﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SiGenerator
{
    public static class VsHelper
    {
        private const int S_OK = 0;

        public static IVsHierarchy GetCurrentHierarchy(IServiceProvider provider)
        {
            DTE vs = (DTE)provider.GetService(typeof(DTE));
            if (vs == null)
                throw new InvalidOperationException("DTE not found.");
            return ToHierarchy(vs.SelectedItems.Item(1).ProjectItem.ContainingProject);
        }

        public static IVsHierarchy ToHierarchy(EnvDTE.Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            string projectGuid = null;        // DTE does not expose the project GUID that exists at in the msbuild project file.        // Cannot use MSBuild object model because it uses a static instance of the Engine,         // and using the Project will cause it to be unloaded from the engine when the         // GC collects the variable that we declare.       
            using (XmlReader projectReader = XmlReader.Create(project.FileName))
            {
                projectReader.MoveToContent();
                object nodeName = projectReader.NameTable.Add("ProjectGuid");
                while (projectReader.Read())
                {
                    if (Object.Equals(projectReader.LocalName, nodeName))
                    {
                        projectGuid = (String)projectReader.ReadElementContentAsString();
                        break;
                    }
                }
            }
            //Debug.Assert(!String.IsNullOrEmpty(projectGuid));
            if (string.IsNullOrEmpty(projectGuid))
            {
                throw new Exception("Project Type GUID not found for this type of project. Project type not currently supported");
            }
            IServiceProvider serviceProvider = new ServiceProvider(project.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            return VsShellUtilities.GetHierarchy(serviceProvider, new Guid(projectGuid));
        }

        public static IVsProject ToVsProject(EnvDTE.Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            IVsProject vsProject = ToHierarchy(project) as IVsProject;
            if (vsProject == null)
            {
                throw new ArgumentException("Project is not a VS project.");
            }
            return vsProject;
        }

        public static EnvDTE.Project ToDteProject(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException("hierarchy");
            object prjObject = null;
            if (hierarchy.GetProperty(0xfffffffe, -2027, out prjObject) >= 0)
            {
                return (EnvDTE.Project)prjObject;
            }
            else
            {
                throw new ArgumentException("Hierarchy is not a project.");
            }
        }

        public static EnvDTE.Project ToDteProject(IVsProject project)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            return ToDteProject(project as IVsHierarchy);
        }

        public static EnvDTE.ProjectItem FindProjectItem(EnvDTE.Project project, string file)
        {
            return FindProjectItem(project.ProjectItems, file);
        }

        public static EnvDTE.ProjectItem FindProjectItem(EnvDTE.ProjectItems items, string file)
        {
            string atom = file.Substring(0, file.IndexOf("\\") + 1);
            foreach (EnvDTE.ProjectItem item in items)
            {
                //if ( item
                //if (item.ProjectItems.Count > 0)
                if (atom.StartsWith(item.Name))
                {
                    // then step in
                    EnvDTE.ProjectItem ritem = FindProjectItem(item.ProjectItems, file.Substring(file.IndexOf("\\") + 1));
                    if (ritem != null)
                        return ritem;
                }
                if (Regex.IsMatch(item.Name, file))
                {
                    return item;
                }
                if (item.ProjectItems.Count > 0)
                {
                    EnvDTE.ProjectItem ritem = FindProjectItem(item.ProjectItems, file.Substring(file.IndexOf("\\") + 1));
                    if (ritem != null)
                        return ritem;
                }
            }
            return null;
        }

        public static List<EnvDTE.ProjectItem> FindProjectItems(EnvDTE.ProjectItems items, string match)
        {
            List<EnvDTE.ProjectItem> values = new List<EnvDTE.ProjectItem>();

            foreach (EnvDTE.ProjectItem item in items)
            {
                if (Regex.IsMatch(item.Name, match))
                {
                    values.Add(item);
                }
                if (item.ProjectItems.Count > 0)
                {
                    values.AddRange(FindProjectItems(item.ProjectItems, match));
                }
            }
            return values;
        }

        #region New CPS projects
        
        public static IVsProject ToNewVsProject(EnvDTE.Project project, Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            try
            {
                IVsSolution solutionService = GetSolutionService(serviceProvider, typeof(SVsSolution), typeof(IVsSolution));
                return ToNewVsHierarchy(solutionService, project) as IVsProject;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static IVsSolution GetSolutionService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider, System.Type serviceType, System.Type interfaceType)
        {
            IVsSolution service = null;
            IntPtr servicePointer;
            int hResult = 0;
            Guid serviceGuid;
            Guid interfaceGuid;

            serviceGuid = serviceType.GUID;
            interfaceGuid = interfaceType.GUID;

            hResult = serviceProvider.QueryService(ref serviceGuid, ref interfaceGuid, out servicePointer);
            if (hResult != S_OK)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hResult);
            }
            else if (servicePointer != IntPtr.Zero)
            {
                service = (IVsSolution)System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(servicePointer);
                System.Runtime.InteropServices.Marshal.Release(servicePointer);
            }
            return service;
        }

        private static IVsHierarchy ToNewVsHierarchy(IVsSolution solutionService, EnvDTE.Project project)
        {
            IVsHierarchy projectHierarchy = null;

            if (solutionService.GetProjectOfUniqueName(project.UniqueName, out projectHierarchy) == S_OK)
            {
                if (projectHierarchy != null)
                {
                    return projectHierarchy;
                }
            }
            return null;
        }

        #endregion
    }
}