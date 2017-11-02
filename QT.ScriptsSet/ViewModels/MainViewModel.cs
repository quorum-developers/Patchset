using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using QT.Common;
using QT.ScriptsSet.Core;
using QT.ScriptsSet.Core.Commands;
using QT.ScriptsSet.Core.Enums;
using QT.ScriptsSet.Views;

namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        private string _applicationVersion;
        private int _startIndex = 1;        
        private SimpleCommand _openRunSqlFileCommand;
        private SimpleCommand _addScriptCommand;
        private SimpleCommand _addBeforeCommand;
        private SimpleCommand _addAfterCommand;
        private SimpleCommand _replaceScriptCommand;
        private SimpleCommand _moveUpCommand;
        private SimpleCommand _moveDownScriptCommand;
        private SimpleCommand _deleteScriptCommand;
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

        public void Load(string pathName)
        {
            foreach (string fileName in Directory.GetFiles(pathName, "*.*"))
            {
                AddScript(fileName, ScriptPosition.End);
            }

            UpdateScriptsParameters();

            RefreshDataGrid();
        }

        public string GetRunSql()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (ScriptListItem script in Scripts)
            {
                stringBuilder.AppendLine(UpdateTemplate
                    .Replace("{description}", script.Description)
                    .Replace("{fileName}", script.TargetOnlyFileName)
                    .Replace("{version}", ApplicationVersion)).AppendLine();
            }

            return stringBuilder.ToString();
        }

        private void CreateSetForUpdates(string pathName)
        {
            foreach (ScriptListItem script in Scripts)
            {
                Directory.CreateDirectory(pathName);

                File.Copy(Path.Combine(script.PathName, script.SourceOnlyFileName),
                    Path.Combine(pathName, script.TargetOnlyFileName));
            }

            File.WriteAllText(Path.Combine(pathName, "patches.sql"), GetRunSql());
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

        private void AddScript(string fileName, ScriptPosition scriptPosition)
        {
            int index;

            switch (scriptPosition)
            {
                case ScriptPosition.Before:
                    index = Scripts.IndexOf(SelectedScript);
                    break;
                case ScriptPosition.After:
                    index = Scripts.IndexOf(SelectedScript) + 1;
                    if (index > Scripts.Count)
                    {
                        index = Scripts.Count;
                    }
                    break;
                case ScriptPosition.End:
                    index = Scripts.Count;
                    break;
                default:
                    throw new IndexOutOfRangeException($"Неизвестное значение \"{(int)scriptPosition}\" позиции скрипта.");
            }
            
            Scripts.Insert(index, new ScriptListItem(fileName));

            UpdateScriptsParameters();
        }

        private void ReplaceScript(ScriptListItem script)
        {
            Scripts[Scripts.IndexOf(SelectedScript)] = script;

            UpdateScriptsParameters();

            RefreshDataGrid();
        }

        private void SaveProjectFile(string fileName)
        {
            XDocument xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            xmlDocument.Add(new XElement("data"));

            xmlDocument.Root.Add(new XElement("header"));

            xmlDocument.Root.Element("header").Add(new XElement("version", "1.0.0"));

            xmlDocument.Root.Add(new XElement("scripts"));

            foreach (var script in Scripts)
            {
                XElement scriptXmlElement = new XElement("script");
                scriptXmlElement.Add(new XElement("fileName", script.TargetOnlyFileName));
                scriptXmlElement.Add(new XElement("description", script.Description));

                xmlDocument.Root.Element("scripts").Add(scriptXmlElement);
            }
            //DefaultExt = "ssip"
            xmlDocument.Save(fileName);
        }
    }
}