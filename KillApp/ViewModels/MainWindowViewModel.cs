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
        #region Properties

        private ObservableCollection<Process> _processes;
        private int _numberOfProcesses;
        private Process _selectedProcess;
        private bool _isSingleProcess;

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

        public Process SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProcess)));
            }
        }

        public int NumberOfProcesses
        {
            get => _numberOfProcesses;
            set
            {
                _numberOfProcesses = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfProcesses)));
            }
        }

        public bool IsSingleProcess
        {
            get => _isSingleProcess;
            set
            {
                _isSingleProcess = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSingleProcess)));
            }
        }

        #endregion

        #region CTOR

        public MainWindowViewModel()
        {
            RowClick = new DelegateCommand(KillProcess);
            Refresh = new DelegateCommand(LoadProcesses);

            LoadProcesses();

            //new System.Timers.Timer
            //{
            //    Interval = 5000,
            //    AutoReset = true,
            //    Enabled = true
            //}.Elapsed += (a, b) => LoadProcesses();
        }
        #endregion


        private void KillProcess()
        {
            if (SelectedProcess != null)
            {
                try
                {
                    if (IsSingleProcess)
                    {
                        SelectedProcess.Kill();
                    }
                    else
                    {
                        Processes
                            .Where(x => x.ProcessName == SelectedProcess.ProcessName)
                            .ToList()
                            .ForEach(x => x.Kill());
                    }
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
            var selectedProcess = SelectedProcess;

            Processes = new ObservableCollection<Process>(Process.GetProcesses().OrderBy(x => x.ProcessName).ToList());

            if (selectedProcess != null)
            {
                SelectedProcess = Processes.FirstOrDefault(x => x.Id == selectedProcess.Id);
            }

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
