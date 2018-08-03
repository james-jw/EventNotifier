using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IComponentConnector
    {
        private System.Windows.Forms.NotifyIcon icon;

        private Notifications notificationManager;

        public readonly static DependencyProperty LastMessageProperty;

        public readonly static DependencyProperty NotificationDurationProperty;

        public readonly static DependencyProperty NotificationScaleProperty;

        public readonly static DependencyProperty EventLogMonitorsProperty;

        private SettingsManager settingsManager;

        private string _eventLogMonitorList;


        public ObservableCollection<EventLogMonitor> EventLogMonitors {
            get {
                return (ObservableCollection<EventLogMonitor>)base.GetValue(MainWindow.EventLogMonitorsProperty);
            }
            set {
                base.SetValue(MainWindow.EventLogMonitorsProperty, value);
            }
        }

        public string LastMessage {
            get {
                return (string)base.GetValue(MainWindow.LastMessageProperty);
            }
            set {
                base.SetValue(MainWindow.LastMessageProperty, value);
            }
        }

        public int NotificationDuration {
            get {
                return (int)base.GetValue(MainWindow.NotificationDurationProperty);
            }
            set {
                base.SetValue(MainWindow.NotificationDurationProperty, value);
            }
        }

        public double NotificationScale {
            get {
                return (double)base.GetValue(MainWindow.NotificationScaleProperty);
            }
            set {
                base.SetValue(MainWindow.NotificationScaleProperty, value);
            }
        }

        static MainWindow()
        {
            MainWindow.LastMessageProperty = DependencyProperty.Register("LastMessage", typeof(string), typeof(MainWindow));
            MainWindow.NotificationDurationProperty = DependencyProperty.Register("NotificationDuration", typeof(int), typeof(MainWindow));
            MainWindow.NotificationScaleProperty = DependencyProperty.Register("NotificationScale", typeof(double), typeof(MainWindow));
            MainWindow.EventLogMonitorsProperty = DependencyProperty.Register("EventLogMonitors", typeof(ObservableCollection<EventLogMonitor>), typeof(MainWindow));
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.settingsManager = new SettingsManager("EventNotifier.exe.config");
            this.notificationManager = new Notifications();
            this.notificationManager.Show();
            this.NotificationScale = Convert.ToDouble(this.settingsManager.GetValue("NotificationScale", "1"));
            this.NotificationDuration = Convert.ToInt32(this.settingsManager.GetValue("NotificationDuration", "7"));
            this.notificationManager.NotificationDuration = (long)this.NotificationDuration;
            this._eventLogMonitorList = this.settingsManager.GetValue("EventLogMonitorList", "Miner");
            this.EventLogMonitors = new ObservableCollection<EventLogMonitor>();
            string[] strArrays = this._eventLogMonitorList.Split(new char[] { ',' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str = strArrays[i];
                if (str != "")
                {
                    EventLogMonitor eventLogMonitor = new EventLogMonitor(str, this.notificationManager, this.settingsManager);
                    eventLogMonitor.InitializeMonitor();
                    this.EventLogMonitors.Add(eventLogMonitor);
                }
            }
            this.icon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("EventNotifier.Images.TrayInformation.ico")),
                Visible = true
            };

            this.icon.Click += new EventHandler((object sender, EventArgs args) => {
                if (base.Visibility == Visibility.Visible)
                {
                    base.Hide();
                    return;
                }
                base.Activate();
                base.Show();
            });

            base.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
            base.Activated += new EventHandler(this.MainWindow_Activated);
            base.Closing += new CancelEventHandler(this.MainWindow_Closing);
            base.Deactivated += new EventHandler(this.MainWindow_Deactivated);
            base.Show();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EventLogMonitor eventLogMonitor = new EventLogMonitor("Enter Log Name Here", this.notificationManager, this.settingsManager);
            this.EventLogMonitors.Insert(0, eventLogMonitor);
            this.lstEventLogMonitors.SelectedItem = eventLogMonitor;
        }

        private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            base.Visibility = Visibility.Collapsed;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            string[] newLine = new string[] { "Are you sure you want to exit the Event Log Notifier?", Environment.NewLine, "This will shut down the application.", Environment.NewLine, "Would you still like to continue?" };
            if (MessageBox.Show(string.Concat(newLine), "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.EventLogMonitors.Remove((EventLogMonitor)this.lstEventLogMonitors.SelectedItem);
            }
            catch
            {
                MessageBox.Show("Error removing item.");
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            Rect workArea = SystemParameters.WorkArea;
            base.Left = workArea.Right - base.ActualWidth - 5;
            Rect rect = SystemParameters.WorkArea;
            base.Top = rect.Bottom - base.ActualHeight - 5;
            base.Hide();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string str = "";
            foreach (EventLogMonitor eventLogMonitor in this.EventLogMonitors)
            {
                eventLogMonitor.SaveSettings();
                str = string.Concat(str, eventLogMonitor.EventLogName, ",");
            }
            str.Remove(str.LastIndexOf(','));
            this.settingsManager.SetValue("EventLogMonitorList", str);
            this.settingsManager.SetValue("NotificationDuration", Convert.ToString(this.NotificationDuration));
            this.settingsManager.SetValue("NotificationScale", Convert.ToString(this.NotificationScale));
            this.settingsManager.Save();
            this.icon.Visible = false;
            this.notificationManager.Close();
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            base.Visibility = Visibility.Hidden;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Rect workArea = SystemParameters.WorkArea;
            base.Left = workArea.Right - base.ActualWidth - 5;
            Rect rect = SystemParameters.WorkArea;
            base.Top = rect.Bottom - base.ActualHeight - 5;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == MainWindow.NotificationDurationProperty)
            {
                this.notificationManager.NotificationDuration = (long)this.NotificationDuration;
                return;
            }
            if (e.Property == MainWindow.NotificationScaleProperty)
            {
                this.notificationManager.NotificationScale = this.NotificationScale;
            }
        }
    }
}