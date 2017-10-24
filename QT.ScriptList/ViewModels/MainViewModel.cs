using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;
using QT.Common;
using ScriptList.Core;
using ScriptList.Core.Commands;

namespace ScriptList.ViewModels
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        private string _applicationVersion;
        private int _startIndex = 1;
        private SimpleCommand _addScriptCommand;
        private SimpleCommand _moveUpCommand;
        private SimpleCommand _moveDownCommand;
        private SimpleCommand _deleteCommand;
        private ObservableCollection<ScriptListItem> _scripts = new ObservableCollection<ScriptListItem>();
        private DataGrid _dataGrid;

        public MainViewModel(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
        }

        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            set
            {
                _applicationVersion = value;

                UpdateScriptsParameters();

                RefreshDataGrid();

                OnPropertyChanged("ApplicationVersion");
            }
        }

        public int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;

                UpdateScriptsParameters();

                RefreshDataGrid();

                OnPropertyChanged("StartIndex");
            }
        }

        private ScriptListItem _selectedScript;

        public ScriptListItem SelectedScript
        {
            get { return _selectedScript; }
            set
            {
                _selectedScript = value;
                OnPropertyChanged("SelectedScript");
            }
        }

        public ObservableCollection<ScriptListItem> Scripts
        {
            get => _scripts;
            set
            {
                _scripts = value;
                OnPropertyChanged(nameof(Scripts));
            }
        }

        public void Parse(string fileName)
        {
            try
            {
                Scripts.Clear();

                XDocument xDocument = XDocument.Parse(File.ReadAllText(fileName));

                foreach (XElement scriptElement in xDocument.Root.Element("scripts").Elements())
                {
                    ScriptListItem scriptListItem = new ScriptListItem()
                    {
                        SourceFileName = Path.Combine(Path.GetDirectoryName(fileName),
                            scriptElement.Element("fileName").Value),
                        //FileName = ExtractFileName(scriptElement.Element("fileName").Value),
                        Description = scriptElement.Element("description").Value
                    };

                    Scripts.Add(scriptListItem);
                }

                OnPropertyChanged("ScriptList");
            }
            catch
            {

            }
        }

        public void Load(string pathName)
        {
            foreach (string fileName in Directory.GetFiles(pathName, "*.*"))
            {
                AddScript(fileName);
            }

            UpdateScriptsParameters();

            RefreshDataGrid();
        }

        public string GetRunSql()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (ScriptListItem scriptListItem in Scripts)
            {
                stringBuilder.AppendLine($"prompt @{scriptListItem.VirtualFileName}");
                stringBuilder.AppendLine($"@{scriptListItem.VirtualFileName}");
                stringBuilder.AppendLine(
                    $"insert into own_patch_log(version, note) values('{ApplicationVersion}', '{scriptListItem.VirtualFileName}');");
                stringBuilder.AppendLine("commit;");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public void CreatePatches(string pathName)
        {
            foreach (ScriptListItem script in Scripts)
            {
                Directory.CreateDirectory(pathName);

                File.Copy(Path.Combine(script.PathName, script.SourceFileName), Path.Combine(pathName, script.VirtualFileName));
            }

            File.WriteAllText(Path.Combine(pathName, "patches.sql"), GetRunSql());
        }

        public ICommand AddScriptCommand
        {
            get
            {
                return _addScriptCommand ?? new SimpleCommand(() =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog().Value)
                    {
                        AddScript(openFileDialog.FileName);
                    }
                }, () => true);
            }
        }

        public ICommand MoveUp
        {
            get
            {
                return _moveUpCommand ?? new SimpleCommand(() =>
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
            }
        }

        public ICommand MoveDown
        {
            get
            {
                return _moveDownCommand ?? new SimpleCommand(() =>
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
            }
        }

        public ICommand Delete
        {
            get
            {
                return _deleteCommand ?? new SimpleCommand(() =>
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

        public ICommand ExitCommand
        {
            get
            {
                return new SimpleCommand(() => Application.Current.Shutdown(), () => true);
            }
        }

        public string Error
        {
            get { throw new System.NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                switch (columnName)
                {
                    case nameof(ApplicationVersion):
                        if (string.IsNullOrWhiteSpace(ApplicationVersion))
                        {
                            error = "Введите значение";
                        }
                        break;
                    case nameof(StartIndex):
                        if (StartIndex < 1)
                        {
                            error = "Введите значение больше 0";
                        }
                        break;
                }

                return error;
            }
        }

        private void UpdateScriptsParameters()
        {
            int index = StartIndex;

            foreach (ScriptListItem script in Scripts)
            {
                script.Version = ApplicationVersion;
                script.Index = index.ToString();
                index++;
            }
        }

        private void RefreshDataGrid()
        {
            _dataGrid.ItemsSource = null;
            _dataGrid.ItemsSource = Scripts;
        }

        private void AddScript(string fileName)
        {
            Scripts.Add(new ScriptListItem()
            {
                PathName = Path.GetDirectoryName(fileName),
                SourceFileName = Path.GetFileName(fileName),
                Description = string.Empty
            });
        }
    }
}