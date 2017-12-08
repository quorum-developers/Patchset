using System.Windows;
using System.Windows.Input;
using QT.Common;
using QT.ScriptsSet.Core;
using QT.ScriptsSet.Core.Commands;

namespace QT.ScriptsSet.ViewModels
{
    public class ReplaceScriptWindowViewModel : ViewModelBase
    {
        private Window _window;
        private string _fileName;
        private string _description;
        private DelegateCommand _saveCommand;

        public ReplaceScriptWindowViewModel(Window window, ScriptListItem script)
        {
            _window = window;
            FileName = script.SourceFileName;
            Description = script.Description;
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? new DelegateCommand(() =>
                {
                    _window.DialogResult = true;
                }, () => true);
            }
        }
    }
}
