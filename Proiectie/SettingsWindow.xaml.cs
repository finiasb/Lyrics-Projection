using System.Windows;
using System.Windows.Media;

namespace Proiectie
{
    public partial class SettingsWindow : Window
    {
        public static List<string> SelectedMonitorIds = new List<string>();
        MainWindow mw;
        private List<ProjectionWindow> _projectionWindows = new List<ProjectionWindow>();

        public SettingsWindow(MainWindow window, List<ProjectionWindow> _projectionWindows)
        {
            InitializeComponent();
            Functions.LoadMonitors(this);
            mw = window;
            this._projectionWindows = _projectionWindows;


            var lista = new[] { "Arial", "Book Antiqua", "Calibri", "Century Gothic", "Consolas", "Courier New", "Franklin Gothic", "Georgia", "Impact", "JetBrains Mono", "Sagoe UI", "Times New Roman", "Arial", };

            var fonturiSigureFiltrate = Fonts.SystemFontFamilies.Where(f => f.Source != null && lista.Any(dorit => f.Source.Equals(dorit, StringComparison.OrdinalIgnoreCase))).OrderBy(f => f.Source).ToList();

            cmbFonts.ItemsSource = fonturiSigureFiltrate;

            txtImagePath.Text = Properties.Settings.Default.BackgroundImagePath;
            chkShowTracker.IsChecked = Properties.Settings.Default.ShowVerseTracker;
            txtFontSize.Text = Properties.Settings.Default.txtFontSize.ToString();
            txtClockFontSize.Text = Properties.Settings.Default.clockFontSize.ToString();

            string savedFont = Properties.Settings.Default.SelectedFont;
            var fontPotrivit = fonturiSigureFiltrate.FirstOrDefault(f => f.Source == savedFont);

            if (fontPotrivit != null)
            {
                cmbFonts.SelectedItem = fontPotrivit;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtFontSize.Text, out double customFontSize) || customFontSize <= 0)
            {
                System.Windows.MessageBox.Show("Te rog introdu o dimensiune numerică validă și mai mare decât 0 (ex: 48).", "Eroare validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!double.TryParse(txtClockFontSize.Text, out double customClockFontSize) || customClockFontSize <= 0)
            {
                System.Windows.MessageBox.Show("Te rog introdu o dimensiune numerică validă și mai mare decât 0 (ex: 48).", "Eroare validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            Properties.Settings.Default.txtFontSize = customFontSize;
            Properties.Settings.Default.clockFontSize = customClockFontSize;
            Properties.Settings.Default.Save();

            string fontName = Properties.Settings.Default.SelectedFont;
            mw.txtLiveClockPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);
            mw.txtLiveTextPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);

            foreach(var window in _projectionWindows)
            {
                window.txtMain.FontSize = customFontSize;
                window.txtClock.FontSize = customClockFontSize;
            }

            this.DialogResult = true; 
            this.Close();
        }

        private void Shortcuts_Click(object sender, RoutedEventArgs e)
        {
            string mesajShortcuts = "Scurtături Tastatură Aplicație:\n\n" +
                                    "F1\t\t-> Pune cursorul în caseta de căutare\n" +
                                    "F2\t\t-> Afișează prima cântare din listă\n" +
                                    "F3\t\t-> Împarte strofele în două părți(>= 6)\n" + 
                                    "F12\t\t-> Afișează / Ascunde ceasul pe proiecție\n" +
                                    "Enter (în căutare)\t-> Trece focusul pe lista de cântări\n" +
                                    "Enter (în cântări)\t-> Selectează prima strofă din cântare\n" +
                                    "Enter (în strofe)\t-> Trimite strofa selectată live pe ecran\n" +
                                    "Escape\t\t-> Șterge textul de pe ecran";

            System.Windows.MessageBox.Show(
                mesajShortcuts,
                "Ghid Scurtături",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
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
