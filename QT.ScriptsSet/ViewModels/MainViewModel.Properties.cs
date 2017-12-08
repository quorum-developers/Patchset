namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel
    {
        private string _projectFileName;

        public string ProjectFileName
        {
            get => _projectFileName;
            set
            {
                _projectFileName = value;


                OnPropertyChanged(nameof(ProjectFileName));                             
            }
        }
    }
}
