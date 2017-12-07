using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using QT.ScriptsSet.Core;
using QT.ScriptsSet.Core.Commands;
using QT.ScriptsSet.Core.Enums;
using QT.ScriptsSet.Views;

namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel
    {
        private ICommand _newProjectCommand;
        private ICommand _openProjectCommand;
        private ICommand _saveProjectCommand;
        private ICommand _saveAsProjectCommand;
        private ICommand _createSetForInstallationCommand;
        private ICommand _createSetForUpdatesCommand;
        private ICommand _moveUpScriptCommand;
        private ICommand _descriptionAsTargetFileNameCommand;
        private ICommand _openFolderInExplorerCommand;

        public ICommand NewProjectCommand
        {
            get
            {
                return _newProjectCommand ?? new SimpleCommand(() =>
                {
                    _projectFileName = string.Empty;
                    Scripts.Clear();
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Открыть проект".
        /// </summary>
        public ICommand OpenProjectCommand
        {
            get
            {
                return _openProjectCommand ?? new SimpleCommand(() =>
                {
                    OpenFileDialog openFileDialog =
                        new OpenFileDialog { Filter = "Проекты (*.ssip)|*.ssip|Все файлы (*.*)|*.*" };
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Scripts.Clear();

                        XDocument xmlDocument = XDocument.Parse(File.ReadAllText(openFileDialog.FileName));

                        ApplicationVersion = xmlDocument.Root.Element("project")?.Element("version")?.Value ??
                                             string.Empty;

                        StartIndex = Convert.ToInt32(xmlDocument.Root.Element("project")?.Element("startIndex")?.Value ?? "0");

                        foreach (XElement scriptXmlElement in xmlDocument.Root.Element("scripts").Elements())
                        {
                            ScriptListItem scriptListItem =
                                new ScriptListItem(
                                    Path.Combine(
                                        Path.GetDirectoryName(openFileDialog.FileName),
                                        scriptXmlElement.Element("fileName").Value))
                                {
                                    Description = scriptXmlElement.Element("description").Value
                                };

                            Scripts.Add(scriptListItem);
                        }

                        _projectFileName = openFileDialog.FileName;
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Сохранить проект".
        /// </summary>
        public ICommand SaveProjectCommand
        {
            get
            {
                return _saveProjectCommand ?? new SimpleCommand(() =>
                {
                    SaveProjectFile(_projectFileName);
                }, () => !string.IsNullOrWhiteSpace(ProjectFileName));
            }
        }

        /// <summary>
        /// Команда "Сохранить файл проекта как".
        /// </summary>
        public ICommand SaveAsProjectCommand
        {
            get
            {
                return _saveAsProjectCommand ?? new SimpleCommand(() =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Проекты (*.ssip)|*.ssip|Все файлы (*.*)|*.*",
                        DefaultExt = "ssip"
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SaveProjectFile(saveFileDialog.FileName);
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Создать набор скриптов для инсталляции БД".
        /// </summary>
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
                            File.Copy(Path.Combine(script.PathName, script.SourceOnlyFileName),
                                Path.Combine(folderBrowserDialog.SelectedPath, script.TargetOnlyFileName));

                            stringBuilder.AppendLine(InstallationTemplate
                                .Replace("{description}", script.Description)
                                .Replace("{fileName}", script.TargetOnlyFileName));
                        }

                        File.WriteAllText(Path.Combine(folderBrowserDialog.SelectedPath, "run.sql"),
                            stringBuilder.ToString());
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Создать набор скриптов для обновления БД".
        /// </summary>
        public ICommand CreateSetForUpdatesCommand
        {
            get
            {
                return _createSetForUpdatesCommand ?? new SimpleCommand(() =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        CreateSetForUpdates(Path.GetDirectoryName(saveFileDialog.FileName));

                        SaveProjectFile(saveFileDialog.FileName);
                    }
                }, () => true);
            }
        }

        public ICommand OpenRunSqlFileCommand
        {
            get
            {
                return _openRunSqlFileCommand ?? new SimpleCommand(() =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        OpenInstallationScript(openFileDialog.FileName);
                    }

                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Добавить скрипт".
        /// </summary>
        public ICommand AddScriptCommand
        {
            get
            {
                return _addScriptCommand ?? new SimpleCommand(() =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var fileName in openFileDialog.FileNames)
                        {
                            AddScript(fileName, ScriptPosition.End);
                        }
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Добавить скрипт перед".
        /// </summary>
        public ICommand AddBeforeScriptCommand
        {
            get
            {
                return _addBeforeCommand ?? new SimpleCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            AddScript(openFileDialog.FileName, ScriptPosition.Before);
                        }
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Добавить скрипт после".
        /// </summary>
        public ICommand AddAfterScriptCommand
        {
            get
            {
                return _addAfterCommand ?? new SimpleCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            AddScript(openFileDialog.FileName, ScriptPosition.After);
                        }
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Заменить скрипт".
        /// </summary>
        public ICommand ReplaceScriptCommand
        {
            get
            {
                return _replaceScriptCommand ?? new SimpleCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            ReplaceScriptWindow replaceScriptWindow = new ReplaceScriptWindow(
                                new ScriptListItem(openFileDialog.FileName, SelectedScript.Description));

                            if (replaceScriptWindow.ShowDialog().Value)
                            {
                                ReplaceScript(replaceScriptWindow.GetData());
                            }
                        }
                    }
                }, () => true);
            }
        }

        public ICommand MoveUpScriptCommand => _moveUpScriptCommand ?? new SimpleCommand(() =>
        {
            if (SelectedScript != null)
            {
                int index = Scripts.Select((v, i) => new
                {
                    Scrip = v,
                    Index = i
                }).First(v => v.Scrip.Id == SelectedScript.Id).Index;

                if (index > 0)
                {
                    Scripts.Move(index, index - 1);

                    UpdateScriptsParameters();

                    RefreshDataGrid();

                    SelectedScript = Scripts[index - 1];

                    _dataGrid.Focus();
                }
            }
        }, () => true);

        /// <summary>
        /// Команда "Переместить скрипт вниз".
        /// </summary>
        public ICommand MoveDownScriptCommand => _moveDownScriptCommand ?? new SimpleCommand(() =>
        {
            if (SelectedScript != null)
            {
                int index = Scripts.Select((v, i) => new
                {
                    Scrip = v,
                    Index = i
                }).First(v => v.Scrip.Id == SelectedScript.Id).Index;

                if (index < Scripts.Count - 1)
                {
                    Scripts.Move(index, index + 1);

                    UpdateScriptsParameters();

                    RefreshDataGrid();

                    SelectedScript = Scripts[index + 1];

                    _dataGrid.Focus();
                }
            }
        }, () => true);

        /// <summary>
        /// Команда "Удалить скрипт".
        /// </summary>
        public ICommand DeleteScriptCommand
        {
            get
            {
                return _deleteScriptCommand ?? new SimpleCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        Scripts.Remove(this.SelectedScript);

                        UpdateScriptsParameters();

                        RefreshDataGrid();
                    }
                }, () => true);
            }
        }

        public ICommand DescriptionAsTargetFileNameCommand
        {
            get
            {
                return _descriptionAsTargetFileNameCommand ?? new SimpleCommand(() =>
                {
                    foreach (var script in Scripts)
                    {
                        script.Description = script.TargetOnlyFileName;
                    }

                    RefreshDataGrid();
                }, () => true);
            }
        }

        public ICommand OpenFolderInExplorerCommand
        {
            get
            {
                return _openFolderInExplorerCommand ?? new SimpleCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        Process.Start("explorer.exe", SelectedScript.PathName);
                    }
                }, () => true);
            }           
        }
    }
}