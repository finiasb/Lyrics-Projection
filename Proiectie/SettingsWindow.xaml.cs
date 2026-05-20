using System.Windows;
using System.Windows.Media;

namespace Proiectie
{
    public partial class SettingsWindow : Window
    {
        public static List<string> SelectedMonitorIds = new List<string>();
        MainWindow mw;
        public SettingsWindow(MainWindow window)
        {
            InitializeComponent();
            Functions.LoadMonitors(this);
            mw = window;
            cmbFonts.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            txtImagePath.Text = Properties.Settings.Default.BackgroundImagePath;
            chkShowTracker.IsChecked = Properties.Settings.Default.ShowVerseTracker;
            string savedFont = Properties.Settings.Default.SelectedFont;
            if (!string.IsNullOrEmpty(savedFont))
            {
                cmbFonts.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == savedFont);
            }
            else
            {
                cmbFonts.SelectedIndex = 0; 
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var selected = (List<MonitorOption>)MonitorsList.ItemsSource;
            var collection = new System.Collections.Specialized.StringCollection();

            SelectedMonitorIds.Clear();

            foreach (var item in selected.Where(x => x.IsSelected))
            {
                collection.Add(item.ScreenId);
                SelectedMonitorIds.Add(item.ScreenId); 
            }
            if (cmbFonts.SelectedItem is System.Windows.Media.FontFamily selectedFont)
            {
                Properties.Settings.Default.SelectedFont = selectedFont.Source;
            }
            Properties.Settings.Default.BackgroundImagePath = txtImagePath.Text;
            Properties.Settings.Default.SavedMonitors = collection;
            Properties.Settings.Default.ShowVerseTracker = chkShowTracker.IsChecked == true;

            Properties.Settings.Default.Save();

            string fontName = Properties.Settings.Default.SelectedFont;
            mw.txtLiveClockPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);
            mw.txtLiveTextPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);

            this.DialogResult = true; 
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
