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
        private DelegateCommand _openProjectCommand;
        private ICommand _saveProjectCommand;
        private ICommand _saveAsProjectCommand;
        private ICommand _createSetForInstallationCommand;
        private ICommand _createSetForUpdatesCommand;
        private ICommand _moveUpScriptCommand;
        private ICommand _descriptionAsTargetFileNameCommand;
        private ICommand _openFolderInExplorerCommand;
        private ICommand _openFolderInEditorCommand;

        public ICommand NewProjectCommand
        {
            get
            {
                return _newProjectCommand ?? new DelegateCommand(() =>
                {
                    ProjectFileName = string.Empty;
                    Scripts.Clear();
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Открыть проект".
        /// </summary>
        public DelegateCommand OpenProjectCommand
        {
            get
            {
                return _openProjectCommand ?? new DelegateCommand(() =>
                {
                    OpenFileDialog openFileDialog =
                        new OpenFileDialog { Filter = @"Проекты (*.ssip)|*.ssip|Все файлы (*.*)|*.*" };
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Scripts.Clear();

                        XDocument xmlDocument = XDocument.Parse(File.ReadAllText(openFileDialog.FileName));

                        string pathName = Path.GetDirectoryName(openFileDialog.FileName);

                        foreach (XElement scriptXmlElement in xmlDocument.Root.Element("scripts").Elements())
                        {
                            ScriptListItem scriptListItem =
                                new ScriptListItem(
                                    Path.Combine(
                                        pathName,
                                        scriptXmlElement.Element("fileName").Value))
                                {
                                    Description = scriptXmlElement.Element("description").Value
                                };

                            Scripts.Add(scriptListItem);
                        }

                        ApplicationVersion = xmlDocument.Root.Element("project")?.Element("version")?.Value ??
                                             string.Empty;

                        StartIndex = Convert.ToInt32(xmlDocument.Root.Element("project")?.Element("startIndex")?.Value ?? "0");

                        ProjectFileName = openFileDialog.FileName;
                    }
                }, () => true);
            }
        }

        /// <summary>
        /// Команда "Открыть выходной файл".
        /// </summary>
        public ICommand OpenOutputFileCommand
        {
            get
            {
                return _openOutputFileCommand ?? new DelegateCommand(() =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog()
                    {
                        Filter = @"Выходной файл (*.sql)|*.sql|Все файлы (*.*)|*.*"
                    };

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        OpenOutputFile(openFileDialog.FileName);
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
                return _saveProjectCommand ?? new DelegateCommand(() =>
                {
                    SaveProjectFile(ProjectFileName, false);
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
                return _saveAsProjectCommand ?? new DelegateCommand(() =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = @"Проекты (*.ssip)|*.ssip|Все файлы (*.*)|*.*",
                        DefaultExt = "ssip"
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SaveProjectFile(saveFileDialog.FileName, false);
                    }
                }, () => Scripts.Count > 0);
            }
        }

        /// <summary>
        /// Команда "Создать набор скриптов для инсталляции БД".
        /// </summary>
        public ICommand CreateSetForInstallationCommand
        {
            get
            {
                return _createSetForInstallationCommand ?? new DelegateCommand(() =>
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
                            File.Copy(script.SourceFileName,
                                Path.Combine(folderBrowserDialog.SelectedPath, script.TargetOnlyFileName));

                            stringBuilder.AppendLine(InstallationTemplate
                                .Replace("{description}", script.Description)
                                .Replace("{fileName}", script.TargetOnlyFileName));
                        }

                        File.WriteAllText(Path.Combine(folderBrowserDialog.SelectedPath, "run.sql"),
                            stringBuilder.ToString());
                    }
                }, () => Scripts.Count > 0);
            }
        }

        /// <summary>
        /// Команда "Создать набор скриптов для обновления БД".
        /// </summary>
        public ICommand CreateSetForUpdatesCommand
        {
            get
            {
                return _createSetForUpdatesCommand ?? new DelegateCommand(() =>
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        CreateSetForUpdates(Path.GetDirectoryName(saveFileDialog.FileName));

                        SaveProjectFile(saveFileDialog.FileName, true);
                    }
                }, () => Scripts.Count > 0);
            }
        }

        /// <summary>
        /// Команда "Добавить скрипт".
        /// </summary>
        public ICommand AddScriptCommand
        {
            get
            {
                return _addScriptCommand ?? new DelegateCommand(() =>
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
                return _addBeforeCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            AddScript(openFileDialog.FileName, ScriptPosition.Before);
                        }
                    }
                }, () => SelectedScript != null);
            }
        }

        /// <summary>
        /// Команда "Добавить скрипт после".
        /// </summary>
        public ICommand AddAfterScriptCommand
        {
            get
            {
                return _addAfterCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            AddScript(openFileDialog.FileName, ScriptPosition.After);
                        }
                    }
                }, () => SelectedScript != null);
            }
        }

        /// <summary>
        /// Команда "Заменить скрипт".
        /// </summary>
        public ICommand ReplaceScriptCommand
        {
            get
            {
                return _replaceScriptCommand ?? new DelegateCommand(() =>
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
                }, () => SelectedScript != null);
            }
        }

        public ICommand MoveUpScriptCommand => _moveUpScriptCommand ?? new DelegateCommand(() =>
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
        }, () => SelectedScript != null);

        /// <summary>
        /// Команда "Переместить скрипт вниз".
        /// </summary>
        public ICommand MoveDownScriptCommand => _moveDownScriptCommand ?? new DelegateCommand(() =>
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
        }, () => SelectedScript != null);

        /// <summary>
        /// Команда "Удалить скрипт".
        /// </summary>
        public ICommand DeleteScriptCommand
        {
            get
            {
                return _deleteScriptCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        if (MessageBox.Show($@"Удалить скрипт ""{SelectedScript.SourceOnlyFileName}""?", QTResources.ApplicationName, MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            Scripts.Remove(this.SelectedScript);

                            UpdateScriptsParameters();

                            RefreshDataGrid();
                        }
                    }
                }, () => SelectedScript != null);
            }
        }

        public ICommand DescriptionAsTargetFileNameCommand
        {
            get
            {
                return _descriptionAsTargetFileNameCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        SelectedScript.Description = SelectedScript.TargetOnlyFileName;

                        RefreshDataGrid();
                    }
                }, () => SelectedScript != null);
            }
        }

        public ICommand OpenFolderInExplorerCommand
        {
            get
            {
                return _openFolderInExplorerCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        Process.Start("explorer.exe", Path.GetDirectoryName(SelectedScript.SourceFileName));
                    }
                }, () => SelectedScript != null);
            }
        }

        public ICommand OpenScriptInEditorCommand
        {
            get
            {
                return _openFolderInEditorCommand ?? new DelegateCommand(() =>
                {
                    if (SelectedScript != null)
                    {
                        Process.Start(SelectedScript.SourceFileName);
                    }
                }, () => SelectedScript != null);
            }
        }
    }
}