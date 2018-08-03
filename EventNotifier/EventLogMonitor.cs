using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace EventNotifier
{
    public class EventLogMonitor : INotifyPropertyChanged
    {
        private string _eventLogName;

        private bool _raiseMessages;

        private bool _raiseWarnings;

        private bool _raiseErrors;

        private int _eventsLogged;

        private string _eventStatus;

        private string _lastEventTimeStamp;

        private EventLog _eventLogger;

        private Notifications _notificationManager;

        private SettingsManager _settingsManager;

        public string EventLogName {
            get {
                return this._eventLogName;
            }
            set {
                this._eventLogName = value;
                if (!this.Initialized)
                {
                    this.InitializeMonitor();
                    this.OnPropertyChanged("EventLogName");
                }
            }
        }

        public string EventLogStatus {
            get {
                return this._eventStatus;
            }
            private set {
                this._eventStatus = value;
                this.OnPropertyChanged("EventLogStatus");
            }
        }

        public int EventsLogged {
            get {
                return this._eventsLogged;
            }
            private set {
                this._eventsLogged = value;
                this.OnPropertyChanged("EventsLogged");
            }
        }

        public bool Initialized {
            get;
            private set;
        }

        public string LastEventTimeStamp {
            get {
                return this._lastEventTimeStamp;
            }
            set {
                this._lastEventTimeStamp = value;
                this.OnPropertyChanged("LastEventTimeStamp");
            }
        }

        public int NotificationDuration {
            get;
            set;
        }

        public ICommand OpenEventViewer {
            get;
            private set;
        }

        public bool RaiseErrors {
            get {
                return this._raiseErrors;
            }
            set {
                this._raiseErrors = value;
                this.OnPropertyChanged("RaiseErrors");
            }
        }

        public bool RaiseMessages {
            get {
                return this._raiseMessages;
            }
            set {
                this._raiseMessages = value;
                this.OnPropertyChanged("RaiseMessages");
            }
        }

        public bool RaiseWarnings {
            get {
                return this._raiseWarnings;
            }
            set {
                this._raiseWarnings = value;
                this.OnPropertyChanged("RaiseWarnings");
            }
        }

        public EventLogMonitor(string eventLogName, Notifications notificationManager, SettingsManager settingsManager)
        {
            this._eventLogName = eventLogName;
            this._notificationManager = notificationManager;
            this._settingsManager = settingsManager;
            int num = 0;
            bool flag = num == 0 ? false : true;
            this._raiseWarnings = num == 0 ? false : true;
            bool flag1 = flag;
            bool flag2 = flag1;
            this._raiseMessages = flag1;
            this._raiseErrors = flag2;
            this.Initialized = false;
            this.OpenEventViewer = new RelayCommand(new Action<object>(this.OpenEventViewerCommand));
        }

        private void eventLogger_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            try
            {
                EventLogEntry entry = e.Entry;
                string[] eventLogName = new string[] { "New ", this.EventLogName, " ", e.Entry.EntryType.ToString(), " Logged!" };
                string str = string.Concat(eventLogName);
                string str1 = e.Entry.EntryType.ToString();
                string str2 = str1;
                if (str1 != null)
                {
                    if (str2 == "Information")
                    {
                        if (this.RaiseMessages)
                        {
                            this._notificationManager.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this._notificationManager.Notify(str, entry.Message, entry.TimeGenerated.ToShortTimeString(), false, (Notifications.NotificationType)Enum.Parse(typeof(Notifications.NotificationType), "Information"), this.EventLogName)));
                        }
                    }
                    else if (str2 != "Warning")
                    {
                        if (str2 == "Error")
                        {
                            if (this.RaiseErrors)
                            {
                                this._notificationManager.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this._notificationManager.Notify(str, entry.Message, entry.TimeGenerated.ToShortTimeString(), false, (Notifications.NotificationType)Enum.Parse(typeof(Notifications.NotificationType), "Error"), this.EventLogName)));
                            }
                        }
                    }
                    else if (this.RaiseWarnings)
                    {
                        this._notificationManager.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this._notificationManager.Notify(str, entry.Message, entry.TimeGenerated.ToShortTimeString(), false, (Notifications.NotificationType)Enum.Parse(typeof(Notifications.NotificationType), "Warning"), this.EventLogName)));
                    }
                }
                if (this.EventLogged != null)
                {
                    this.EventLogged(this, null);
                }
                EventLogMonitor eventsLogged = this;
                eventsLogged.EventsLogged = eventsLogged.EventsLogged + 1;
                this.LastEventTimeStamp = DateTime.Now.ToLongTimeString();
                this.OnPropertyChanged("EventLogged");
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                // TODO Add proper logging back
                //GUErrorManager.Instance.LogError(string.Concat("Error creating event log notification! Error: ", exception.ToString()));
            }
        }

        public void InitializeMonitor()
        {
            try
            {
                this.EventsLogged = 0;
                this._eventLogger = new EventLog(this.EventLogName)
                {
                    EnableRaisingEvents = true
                };
                this._eventLogger.EntryWritten += new EntryWrittenEventHandler(this.eventLogger_EntryWritten);
                this.RaiseMessages = Convert.ToBoolean(this._settingsManager.GetValue(string.Concat(this.EventLogName, ".RaiseMessages"), "True"));
                this.RaiseWarnings = Convert.ToBoolean(this._settingsManager.GetValue(string.Concat(this.EventLogName, ".RaiseWarnings"), "True"));
                this.RaiseErrors = Convert.ToBoolean(this._settingsManager.GetValue(string.Concat(this.EventLogName, ".RaiseErrors"), "True"));
                this.Initialized = true;
                this.OnPropertyChanged("Initialized");
            }
            catch
            {
                this.EventLogStatus = "Invalid event log entered!";
            }
        }

        private void OnPropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public void OpenEventViewerCommand(object parameter)
        {
            try
            {
                Process.Start("eventvwr.exe", string.Concat("/c:", this.EventLogName));
            }
            catch
            {
            }
        }

        public void Remove()
        {
            this._settingsManager.Remove(string.Concat(this.EventLogName, ".RaiseMessages"));
            this._settingsManager.Remove(string.Concat(this.EventLogName, ".RaiseWarnings"));
            this._settingsManager.Remove(string.Concat(this.EventLogName, ".RaiseErrors"));
        }

        public void SaveSettings()
        {
            this._settingsManager.SetValue(string.Concat(this.EventLogName, ".RaiseMessages"), Convert.ToString(this.RaiseMessages));
            this._settingsManager.SetValue(string.Concat(this.EventLogName, ".RaiseWarnings"), Convert.ToString(this.RaiseWarnings));
            this._settingsManager.SetValue(string.Concat(this.EventLogName, ".RaiseErrors"), Convert.ToString(this.RaiseErrors));
        }

        public event EventHandler EventLogged;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
