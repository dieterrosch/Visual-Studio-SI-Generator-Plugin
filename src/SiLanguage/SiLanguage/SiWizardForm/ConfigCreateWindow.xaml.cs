using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SiWizardForm
{
    /// <summary>
    /// Interaction logic for ConfigCreateWindow.xaml
    /// </summary>
    public partial class ConfigCreateWindow : Window
    {
        private Dictionary<string, string> generators = new Dictionary<string, string>()
        {
            ["AdoCCode"] = "AdoCCode - Generate ADO (OleDB) C++ Code",
            ["AdoCSCode"] = "AdoCSCode - Generate C# Code for ADO.NET via IDbConnection Version 2",
            ["AdoPythonCode"] = "AdoPythonCode - Generate ADO (OleDB) Python Code",
            ["BCPCode"] = "BCPCode - Generate bulk loader for MS SQL Server Code",
            ["BinCode"] = "BinCode - Generate IDL Code for Binu 3 Tier Access",
            ["BinCppCode"] = "BinCppCode - Generate IDL Code for Binu 3 Tier Access",
            ["BinCSCode"] = "BinCSCode - Generate IDL Code for Binu 3 Tier Access",
            ["BinJavaCode"] = "BinJavaCode - Generate IDL Code for Binu 3 Tier Access",
            ["CliCCode"] = "CliCCode - Generate CLI C++ Code for DB2",
            ["CliJCode"] = "CliJCode - Generate Client Java Code",
            ["CSIdl2Code"] = "CSIdl2Code - Generate C# IDL Code for 3 Tier Access",
            ["CSNetCode"] = "CSNetCode - Generate C# Code for ADO.NET via IDbConnection",
            ["Db2DDL"] = "Db2DDL - Generate DB2 DDL",
            ["DBPortalSI"] = "DBPortalSI - Generate JPortal SI",
            ["DBPyCode"] = "DBPyCode - Generate DB Python Code",
            ["IdlCode"] = "IdlCode - Generate IDL Code for 3 Tier Access",
            ["IdlJCode"] = "IdlJCode - Generate IDL Code for 3 Tier Access",
            ["IdlJRWCode"] = "IdlJRWCode - Generate IDL Code for 3 Tier Access",
            ["IdlObjCCode"] = "IdlObjCCode - Generate IDL Objective-C Code for 3 Tier Access",
            ["JavaCode"] = "JavaCode - Generate Java Code",
            ["JavaCSCode"] = "JavaCSCode - Generate Java CS Wrapper Code",
            ["JavaIdlCode"] = "JavaIdlCode - Generate Java IDL Code for 3 Tier Access",
            ["JavaRWCode"] = "JavaRWCode - Generate Java RW Code",
            ["JavaRWiiCode"] = "JavaRWiiCode - Generate Java IDL Code for 3 Tier Access",
            ["Lite3CCode"] = "Lite3CCode - Generate Lite3 C Code",
            ["Lite3DDL"] = "Lite3DDL - Generate Lite3 DDL",
            ["Lite3PyCode"] = "Lite3PyCode - Generate Lite3 Code for Python",
            ["MSSqlDDL"] = "MSSqlDDL - Generate MsSQL DDL",
            ["MySqlCCode"] = "MySqlCCode - Generate MySQL C Code",
            ["MySqlDDL"] = "MySqlDDL - Generate MySQL DDL",
            ["OciCCode"] = "OciCCode - Generate OCI C++ Code",
            ["OciSHCode"] = "OciSHCode - Generate OCI C++ SH Code",
            ["OracleDDL"] = "OracleDDL - Generate Oracle DDL",
            ["PostgreCCode"] = "PostgreCCode - Generate C++ Code for PostgreSQL",
            ["PostgreDDL"] = "PostgreDDL - Generate PostgreSQL DDL",
            ["PyParamCode"] = "PyParamCode - Generate Param Python Code",
            ["PythonCliCode"] = "PythonCliCode - Generate CLI Python Code",
            ["PythonCode"] = "PythonCode - Generate ADO (OleDB) Python Code",
            ["PythonTreeCode"] = "PythonTreeCode - Generate Python Tree Code",
            ["QueryCode"] = "QueryCode - Generate SQL Query Code",
            ["XsdCode"] = "XsdCode - Generate XSD Code for NET"
        };

        private string createPath;

        public ConfigCreateWindow(string createPath)
        {
            InitializeComponent();
            this.createPath = createPath;
            this.GenCombo.ItemsSource = generators;
        }

        private void SelectJPortalDirectory(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "jportal.jar";
            openFileDialog.Filter = "JPortal|jportal.jar";
            if (openFileDialog.ShowDialog() == true)
            {
                this.tbJPortal.Text = openFileDialog.FileName;
            }
        }

        private void AddGenerator(object sender, RoutedEventArgs e)
        {
            if (this.GenCombo.SelectedValue != null)
            {
                if (this.GenList.ItemsSource == null)
                    this.GenList.ItemsSource = new ObservableCollection<string>();

                (this.GenList.ItemsSource as ObservableCollection<string>).Add(this.GenCombo.SelectedValue.ToString());
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(this.tbJPortal.Text))
            {
                MessageBox.Show("Please capture a valid JPortal directory");
                return;
            }

            var genContent = new List<string>() { this.tbJPortal.Text };
            if (this.GenList.ItemsSource != null)
            {
                foreach (string gen in this.GenList.ItemsSource)
                {
                    genContent.Add(gen);
                    this.generators.Remove(gen);
                }
            }

            foreach (var gen in generators)
            {
                genContent.Add("//" + gen.Key);
            }

            File.WriteAllLines(createPath + "\\gen.config", genContent);
            this.DialogResult = true;
            this.Close();
        }

        private void DeleteGenerator(object sender, RoutedEventArgs e)
        {
            if (this.GenList.SelectedItem != null)
            {
                (this.GenList.ItemsSource as ObservableCollection<string>).Remove(this.GenList.SelectedItem.ToString());
            }
        }

        private void ClearList(object sender, RoutedEventArgs e)
        {
            if (this.GenList.ItemsSource != null)
            {
                (this.GenList.ItemsSource as ObservableCollection<string>).Clear();
            }
        }
    }
}
