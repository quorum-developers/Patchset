using System.Windows;
using ScriptList.ViewModels;
using System.Windows.Forms;

namespace ScriptList
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

        private void OpenMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.Parse(openFileDialog.FileName);
            }
        }
        
        private void CreatePatches_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.CreatePatches(folderBrowserDialog.SelectedPath);
            }
        }

        private void LoadFromFolder_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.Load(folderBrowserDialog.SelectedPath);
            }
        }
    }
}
