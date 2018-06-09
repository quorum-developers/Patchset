using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QT.ScriptsSet.Core.Commands;

namespace QT.ScriptsSet.ViewModels
{
    public partial class MainViewModel
    {
        private ICommand _clearCommand;

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? new DelegateCommand(() =>
                {
                    Scripts.Clear();
                }, () => true);
            }
        }
    }
}
