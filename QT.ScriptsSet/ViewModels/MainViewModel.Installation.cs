using System;
using System.IO;
using System.Linq;
using QT.ScriptsSet.Core.Enums;

namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel
    {
        public string _installationTemplate = @"prompt {description}" + Environment.NewLine + "@@{fileName}" + Environment.NewLine + "prompt";

        public string _updateTemplate = @"prompt {description}" + Environment.NewLine + "@@{fileName}" +
                                        Environment.NewLine +
                                        "insert into own_patch_log(version, note) values('{version}', '{fileName}');" +
                                        Environment.NewLine + "commit;";        
        


        public string InstallationTemplate
        {
            get => _installationTemplate;
            set
            {
                _installationTemplate = value;
                OnPropertyChanged(nameof(InstallationTemplate));
            }
        }

        public string UpdateTemplate
        {
            get => _updateTemplate;
            set
            {
                _updateTemplate = value;
                OnPropertyChanged(nameof(UpdateTemplate));
            }
        }

        private void OpenOutputFile(string fileName)
        {
            Scripts.Clear();

            string[] scriptFileNames = File.ReadAllLines(fileName)
                .Where(v => v.Trim().StartsWith("@@", StringComparison.OrdinalIgnoreCase)).Select(v => v.Substring(2))
                .ToArray();

            foreach (string scriptFileName in scriptFileNames)
            {
                AddScript(Path.Combine(Path.GetDirectoryName(fileName), scriptFileName), ScriptPosition.End);
            }

            UpdateScriptsParameters();

            RefreshDataGrid();
        }
    }
}