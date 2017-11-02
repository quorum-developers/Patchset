using System.Windows;
using QT.ScriptsSet.Core;
using QT.ScriptsSet.ViewModels;

namespace QT.ScriptsSet.Views
{
    /// <summary>
    /// Логика взаимодействия для ReplaceScriptWindow.xaml
    /// </summary>
    public partial class ReplaceScriptWindow : Window
    {
        private readonly ReplaceScriptWindowViewModel _viewModel;

        public ReplaceScriptWindow(ScriptListItem script)
        {
            InitializeComponent();

            DataContext = _viewModel = new ReplaceScriptWindowViewModel(this, script);
        }

        public ScriptListItem GetData()
        {
            return new ScriptListItem(_viewModel.FileName, _viewModel.Description);
        }
    }
}
