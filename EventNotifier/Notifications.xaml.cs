using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EventNotifier
{
    /// <summary>
    /// Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications : Window, IComponentConnector
    {
        public readonly static DependencyProperty NotificationsListProperty;
        public readonly static DependencyProperty NotificationDurationProperty;
        public readonly static DependencyProperty NotificationScaleProperty;

        private double _initialMaxWidth;

        public long NotificationDuration {
            get {
                return (long)base.GetValue(Notifications.NotificationDurationProperty);
            }
            set {
                base.SetValue(Notifications.NotificationDurationProperty, value);
            }
        }

        public double NotificationScale {
            get {
                return (double)base.GetValue(Notifications.NotificationScaleProperty);
            }
            set {
                base.SetValue(Notifications.NotificationScaleProperty, value);
                this.gridScaleTransform.ScaleX = value;
                this.gridScaleTransform.ScaleY = value;
                base.MaxWidth = this._initialMaxWidth * value;
            }
        }

        public ObservableCollection<Notifications.Notification> NotificationsList {
            get {
                return (ObservableCollection<Notifications.Notification>)base.GetValue(Notifications.NotificationsListProperty);
            }
            set {
                base.SetValue(Notifications.NotificationsListProperty, value);
            }
        }

        static Notifications()
        {
            Notifications.NotificationsListProperty = DependencyProperty.Register("NotificationsList", typeof(ObservableCollection<Notifications.Notification>), typeof(Notifications));
            Notifications.NotificationDurationProperty = DependencyProperty.Register("NotificationDuration", typeof(long), typeof(Notifications));
            Notifications.NotificationScaleProperty = DependencyProperty.Register("NotificationScale", typeof(double), typeof(Notifications));
        }

        public Notifications()
        {
            this.InitializeComponent();
            this.NotificationsList = new ObservableCollection<Notifications.Notification>();
            base.Loaded += new RoutedEventHandler(this.Notifications_Loaded);
            base.SizeChanged += new SizeChangedEventHandler(this.Notifications_SizeChanged);
            this.NotificationDuration = (long)7;
            this._initialMaxWidth = base.MaxWidth;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void newNotificationTimer_Tick(object sender, EventArgs e)
        {
            base.Dispatcher.BeginInvoke(new Action(() => {
                DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
                Notifications.Notification tag = (Notifications.Notification)dispatcherTimer.Tag;
                if (!tag.StayOpen)
                {
                    this.NotificationsList.Remove(tag);
                    dispatcherTimer.Stop();
                }
            }), new object[0]);
        }

        private void Notifications_Loaded(object sender, RoutedEventArgs e)
        {
            Rect workArea = SystemParameters.WorkArea;
            base.Left = workArea.Right - base.ActualWidth - 5;
            Rect rect = SystemParameters.WorkArea;
            base.Top = rect.Bottom - base.ActualHeight - 5;
        }

        private void Notifications_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rect workArea = SystemParameters.WorkArea;
            base.Top = workArea.Bottom - base.ActualHeight - 5;
            Rect rect = SystemParameters.WorkArea;
            base.Left = rect.Right - base.ActualWidth - 5;
        }

        public void Notify(string Title, string Message, string TimeStamp, bool InputRequired, Notifications.NotificationType type, string logName)
        {
            Notifications.Notification notification = new Notifications.Notification(new Duration(new TimeSpan(this.NotificationDuration * (long)10000000)), this.NotificationsList)
            {
                Title = Title,
                Message = Message,
                TimeStamp = TimeStamp,
                RequiresInput = InputRequired,
                Type = type,
                LogName = logName
            };
            this.NotificationsList.Add(notification);
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(this.newNotificationTimer_Tick);
            dispatcherTimer.Tag = notification;
            dispatcherTimer.Interval = new TimeSpan(0, 0, Convert.ToInt32(this.NotificationDuration));
            dispatcherTimer.Start();
        }

        public class Notification
        {
            private ObservableCollection<Notifications.Notification> ParentList;

            public ICommand CopyToClipboard {
                get;
                private set;
            }

            public Duration DisplayDuration {
                get;
                set;
            }

            public BitmapImage Icon {
                get {
                    BitmapImage bitmapImage;
                    try
                    {
                        Uri uri = new Uri(string.Concat("/EventNotifier;component/Images/", Enum.GetName(typeof(Notifications.NotificationType), this.Type), ".ico"), UriKind.Relative);
                        return new BitmapImage(uri);
                    }
                    catch (Exception exception)
                    {
                        bitmapImage = new BitmapImage();
                    }
                    return bitmapImage;
                }
            }

            public string LogName {
                get;
                set;
            }

            public string Message {
                get;
                set;
            }

            public ICommand OpenEventViewer {
                get;
                private set;
            }

            public ICommand Remove {
                get;
                private set;
            }

            public bool RequiresInput {
                get;
                set;
            }

            public bool StayOpen {
                get;
                set;
            }

            public string TimeStamp {
                get;
                set;
            }

            public string Title {
                get;
                set;
            }

            public Notifications.NotificationType Type {
                get;
                set;
            }

            public Notification(Duration displayDuration, ObservableCollection<Notifications.Notification> parentList)
            {
                this.ParentList = parentList;
                this.DisplayDuration = displayDuration;
                this.CopyToClipboard = new RelayCommand(new Action<object>(this.CopyToClipboardCommand));
                this.Remove = new RelayCommand(new Action<object>(this.RemoveCommand));
                this.OpenEventViewer = new RelayCommand(new Action<object>(this.OpenEventViewerCommand));
            }

            public void CopyToClipboardCommand(object parameter)
            {
                Clipboard.SetText(string.Concat(this.Message, Environment.NewLine, this.TimeStamp));
            }

            public void OpenEventViewerCommand(object parameter)
            {
                try
                {
                    Process.Start("eventvwr.exe", string.Concat("/c:", this.LogName));
                }
                catch
                {
                }
            }

            public void RemoveCommand(object parameter)
            {
                if (this.ParentList != null)
                {
                    this.ParentList.Remove(this);
                }
            }
        }

        public enum NotificationType
        {
            Information,
            Warning,
            Error
        }
    }
}