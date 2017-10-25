﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using QT.ScriptsSet.Core;
using QT.ScriptsSet.Core.Commands;

namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel
    {
        private ICommand _createSetForInstallationCommand;
        private ICommand _openInstallationProjectCommand;
        private ICommand _saveInstallationProjectCommand;

        public ICommand CreateSetForInstallationCommand
        {
            get
            {
                return _createSetForInstallationCommand ?? new SimpleCommand(() =>
                {
                    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder stringBuilder = new StringBuilder();

                        stringBuilder.AppendLine("set define off");
                        stringBuilder.AppendLine("spool run.log");
                        stringBuilder.AppendLine();

                        foreach (var script in Scripts)
                        {
                            File.Copy(Path.Combine(script.PathName, script.VirtualFileName), Path.Combine(folderBrowserDialog.SelectedPath, script.VirtualFileName));
                            stringBuilder.AppendLine($"prompt {script.Description}");                            
                            stringBuilder.AppendLine($"@@{script.VirtualFileName}");
                            stringBuilder.AppendLine("prompt");
                        }

                        File.WriteAllText(Path.Combine(folderBrowserDialog.SelectedPath, "run.sql"), stringBuilder.ToString());
                    }
                }, () => true);
            }
        }

        public ICommand OpenInstallationProjectCommand
        {
            get
            {
                return _openInstallationProjectCommand ?? new SimpleCommand(() =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Проекты (*.ssip)|*.ssip|Все файлы (*.*)|*.*" };
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Scripts.Clear();

                        XDocument xmlDocument = XDocument.Parse(File.ReadAllText(openFileDialog.FileName));

                        foreach (XElement scriptXmlElement in xmlDocument.Root.Element("scripts").Elements())
                        {
                            ScriptListItem scriptListItem = new ScriptListItem()
                            {
                                SourceFileName = Path.Combine(scriptXmlElement.Element("fileName").Value),
                                PathName = Path.GetDirectoryName(openFileDialog.FileName),
                                Description = scriptXmlElement.Element("description").Value
                            };

                            Scripts.Add(scriptListItem);
                        }
                    }
                }, () => true);
            }
        }

        public ICommand SaveInstallationProjectCommand
        {
            get
            {
                return _saveInstallationProjectCommand ?? new SimpleCommand(() =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog { DefaultExt = "ssip" };
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

                        xmlDocument.Add(new XElement("data"));
                        xmlDocument.Root.Add(new XElement("scripts"));

                        foreach (var script in Scripts)
                        {
                            XElement scriptXmlElement = new XElement("script");
                            scriptXmlElement.Add(new XElement("fileName", script.SourceFileName));
                            scriptXmlElement.Add(new XElement("description", script.Description));

                            xmlDocument.Root.Element("scripts").Add(scriptXmlElement);
                        }
                        xmlDocument.Save(saveFileDialog.FileName);
                    }
                }, () => true);
            }
        }

        private void OpenInstallationScript(string fileName)
        {
            Scripts.Clear();

            string[] scriptFileNames = File.ReadAllLines(fileName)
                .Where(v => v.Trim().StartsWith("@@", StringComparison.OrdinalIgnoreCase)).Select(v => v.Substring(2))
                .ToArray();

            foreach (string scriptFileName in scriptFileNames)
            {
                AddScript(Path.Combine(Path.GetDirectoryName(fileName), scriptFileName));
            }

            UpdateScriptsParameters();

            RefreshDataGrid();
        }
    }
}