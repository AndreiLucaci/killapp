using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private DispatcherTimer _autoRefreshTimer;
        private DispatcherTimer _atSecondUpdateTimer;
        private DispatcherTimer _systemInfoTimer;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;

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

        public string NumberOfProcesses
        {
            get => $"Number of processes: {_numberOfProcesses}";
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

        public bool AutoRefresh
        {
            get => _autoRefreshTimer.IsEnabled;
            set
            {
                if (value)
                {
                    StartAutoRefreshBackgroundProcess();
                }
                else
                {
                    StopAutoRefresh();
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoRefresh)));
            }
        }

        public string CurrentSystemTime => $"System time: {DateTime.Now.ToString("HH:mm:ss")}";
        public string CPUUsage => $"CPU Usage: {_cpuCounter.NextValue()}%";
        public string MemoryUsage => $"Memory Usage: {_ramCounter.NextValue()}MB";

        #endregion

        #region CTOR

        public MainWindowViewModel()
        {
            RowClick = new DelegateCommand(KillProcess);
            Refresh = new DelegateCommand(LoadProcesses);

            LoadProcesses();

            InitializeAutoRefreshTimer();
            InitializeAtSecondTimer();
            InitializeSystemInformation();

            StartAutoRefreshBackgroundProcess();

            //new System.Timers.Timer
            //{
            //    Interval = 5000,
            //    AutoReset = true,
            //    Enabled = true
            //}.Elapsed += (a, b) => LoadProcesses();
        }
        #endregion

        private void InitializeAutoRefreshTimer()
        {
            if (_autoRefreshTimer == null)
            {
                _autoRefreshTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1),
                };

                _autoRefreshTimer.Tick += (sender, args) =>
                {
                    LoadProcesses();
                };
            }
        }

        private void InitializeSystemInformation()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            if (_systemInfoTimer == null)
            {
                _systemInfoTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };

                _systemInfoTimer.Tick += (sender, args) =>
                {
                    OnPropertyChanged(nameof(CPUUsage));
                    OnPropertyChanged(nameof(MemoryUsage));
                };
            }

            _systemInfoTimer.Start();
        }

        private void InitializeAtSecondTimer()
        {
            if (_atSecondUpdateTimer == null)
            {
                _atSecondUpdateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };

                _atSecondUpdateTimer.Tick += (sender, args) =>
                {
                    UpdateSystemInformation();
                };
            }
            _atSecondUpdateTimer.Start();
        }

        private void UpdateSystemInformation()
        {
            OnPropertyChanged(nameof(CurrentSystemTime));
        }

        private void StartAutoRefreshBackgroundProcess()
        {
            if (!_autoRefreshTimer.IsEnabled)
            {
                _autoRefreshTimer.Start();
            }
        }

        private void StopAutoRefresh()
        {
            if (_autoRefreshTimer.IsEnabled)
            {
                _autoRefreshTimer.Stop();
            }
        }


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

            var processesFromTheSystem = Process.GetProcesses().ToList();
            var orderedProcesses = processesFromTheSystem.OrderBy(x => x.ProcessName).ToList();

            Processes = new ObservableCollection<Process>(orderedProcesses);

            if (selectedProcess != null)
            {
                SelectedProcess = Processes.FirstOrDefault(x => x.Id == selectedProcess.Id);
            }

            _numberOfProcesses = Processes.Count;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfProcesses)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
