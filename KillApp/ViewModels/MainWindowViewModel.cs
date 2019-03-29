using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using KillApp.Annotations;

namespace KillApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Process> _processes;
        private int _numberOfProcesses;

        public ObservableCollection<Process> Processes
        {
            get => _processes;
            set
            {
                _processes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Processes)));
            }
        }

        public ICommand RowClick { get; }

        public ICommand Refresh { get; }

        public Process SelectedProcess { get; set; }

        public int NumberOfProcesses
        {
            get => _numberOfProcesses;
            set
            {
                _numberOfProcesses = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfProcesses)));
            }
        }

        public MainWindowViewModel()
        {
            RowClick = new DelegateCommand(KillProcess);
            Refresh = new DelegateCommand(LoadProcesses);

            Processes = new ObservableCollection<Process>(); 

            LoadProcesses();
        }

        private void KillProcess()
        {
            if (SelectedProcess != null)
            {
                try
                {
                    Processes.Where(x => x.ProcessName == SelectedProcess.ProcessName).ToList().ForEach(x => x.Kill());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Thread.Sleep(100);

                LoadProcesses();
            }
        }

        private void LoadProcesses()
        {
            Processes = new ObservableCollection<Process>(Process.GetProcesses().OrderBy(x => x.ProcessName).ToList());
            NumberOfProcesses = Processes.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
