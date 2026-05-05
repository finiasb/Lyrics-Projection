using System.Windows;

namespace Proiectie
{
    public partial class SettingsWindow : Window
    {
        public static List<string> SelectedMonitorIds = new List<string>();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadMonitors();

            txtImagePath.Text = Properties.Settings.Default.BackgroundImagePath;
        }

        private void LoadMonitors()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            var options = new List<MonitorOption>();

            var savedIds = Properties.Settings.Default.SavedMonitors ?? new System.Collections.Specialized.StringCollection();
            foreach (var s in screens)
            {
                options.Add(new MonitorOption
                {
                    DeviceName = $"Monitor {(s.Primary ? "(Principal) " : "")}- {s.Bounds.Width}x{s.Bounds.Height}",
                    ScreenId = s.DeviceName,
                    IsSelected = savedIds.Contains(s.DeviceName)
                });
            }

            MonitorsList.ItemsSource = options;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var selected = (List<MonitorOption>)MonitorsList.ItemsSource;
            var collection = new System.Collections.Specialized.StringCollection();

            foreach (var item in selected.Where(x => x.IsSelected))
            {
                collection.Add(item.ScreenId);
                SelectedMonitorIds.Add(item.ScreenId);
            }
            Properties.Settings.Default.BackgroundImagePath = txtImagePath.Text;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.SavedMonitors = collection;
            Properties.Settings.Default.Save();

            this.Close();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Imagini (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                txtImagePath.Text = openFileDialog.FileName;
            }
        }

        private void ClearImage_Click(Object sender, RoutedEventArgs e)
        {
            txtImagePath.Text = string.Empty;
        }
    }
}
