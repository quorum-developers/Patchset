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

                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title
        {
            get
            {
                string title = "Набор скриптов 13.06.2018";

                if (!string.IsNullOrWhiteSpace(ProjectFileName))
                {
                    title = $"{title} - \"{ ProjectFileName}\"";
                }

                return title;
            }
        }
    }
}
