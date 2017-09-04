using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TemplateWizard;

namespace SiWizardForm
{
    class SiWizard : IWizard
    {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {

        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            var genPath = Path.GetDirectoryName(projectItem.Properties.Item("FullPath").Value.ToString());
            var configPath = Path.Combine(genPath, "gen.config");
            if (!File.Exists(configPath))
            {
                var genForm = new ConfigCreateWindow(genPath);
                genForm.ShowDialog();
            }

            var project = projectItem.ContainingProject;
            project.ProjectItems.AddFromFile(configPath);
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            //  Create the form.
            var form = new MainWindow();

            form.tbTableName.Text = replacementsDictionary["$safeitemname$"];
            form.tbPackage.Text = replacementsDictionary["$rootnamespace$"];
            replacementsDictionary["$safeitemname$"] = replacementsDictionary["$safeitemname$"].ToLower();

            //  Show the form.
            form.ShowDialog();

            //  Add the options to the replacementsDictionary.
            replacementsDictionary.Add("$TableName$", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(form.tbTableName.Text));
            replacementsDictionary.Add("$Package$", form.tbPackage.Text);
            replacementsDictionary.Add("$Database$", form.tbDatabase.Text);
            if (!string.IsNullOrWhiteSpace(form.tbSchema.Text))
            {
                replacementsDictionary.Add("$Schema$", "\r\nSCHEMA " + form.tbSchema.Text);
            }

            replacementsDictionary.Add("$Flags$", "");

            if (form.cbNoDatatables.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"no datatables\"";
            if (form.cbGenerics.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use generics\"";
            if (form.cbYields.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use yields\"";
            if (form.cbFunc.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use func\"";
            if (form.cbSeperate.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use separate\"";
            if (form.cbTriggers.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use triggers\"";
            if (form.cbNotify.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"use notify\"";
            if (form.cbAudit.IsChecked == true)
                replacementsDictionary["$Flags$"] += " \"audit triggers\"";
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

    }
}
