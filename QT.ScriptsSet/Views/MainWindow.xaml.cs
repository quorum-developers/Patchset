using System.Windows;
using QT.ScriptsSet.ViewModels;

namespace QT.ScriptsSet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = _viewModel = new MainViewModel(ScriptDataGrid);
        }
    }
}
