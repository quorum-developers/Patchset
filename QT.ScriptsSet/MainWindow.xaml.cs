using System.Windows;
using System.Windows.Forms;
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
                
        private void CreatePatches_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.CreatePatches(folderBrowserDialog.SelectedPath);
            }
        }        
    }
}
