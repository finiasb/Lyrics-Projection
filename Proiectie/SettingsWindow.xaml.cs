using System.Windows;

namespace Proiectie
{
    public partial class SettingsWindow : Window
    {
        public static List<string> SelectedMonitorIds = new List<string>();

        public SettingsWindow()
        {
            InitializeComponent();
            Functions.LoadMonitors(this);

            txtImagePath.Text = Properties.Settings.Default.BackgroundImagePath;
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
        private void DecreaseFont_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtFontSize.Text, out double currentSize))
            {
                if (currentSize > 10) 
                    txtFontSize.Text = (currentSize - 2).ToString();
            }
        }

        private void IncreaseFont_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtFontSize.Text, out double currentSize))
            {
                if (currentSize < 300) 
                    txtFontSize.Text = (currentSize + 2).ToString();
            }
        }
    }
}
