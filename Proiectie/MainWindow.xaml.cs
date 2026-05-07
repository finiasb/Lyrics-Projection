using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging; 
namespace Proiectie
{
    public partial class MainWindow : Window
    {
        private ProjectionWindow _projectionWindow;
        private List<Song> _listaFavorite = new List<Song>();
        private List<ProjectionWindow> _projectionWindows = new List<ProjectionWindow>();

        public string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")}";
        public MainWindow()
        {
            InitializeComponent();

            Functions.UpdateBackground(this.imgPreviewBackground);

            if (Properties.Settings.Default.SavedMonitors != null)
            {
                SettingsWindow.SelectedMonitorIds = Properties.Settings.Default.SavedMonitors.Cast<string>().ToList();
            }

            Functions.IncarcaCantari(this);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            this.Closing += (s, e) => {
                foreach (var window in _projectionWindows)
                {
                    window.Close();
                }
            };
        }

        private void SearchType_Changed(object sender, RoutedEventArgs e)
        {
            if (txtSearch != null)
                Functions.IncarcaCantari(this, txtSearch.Text);
        }
        private void btnToggleProjection_Click(object sender, RoutedEventArgs e)
        {
            if (_projectionWindows.Count == 0)
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                var targetIds = SettingsWindow.SelectedMonitorIds;

                foreach (var screen in screens)
                {
                    bool shouldProject = targetIds.Count > 0 ? targetIds.Contains(screen.DeviceName): !screen.Primary; 

                    if (shouldProject)
                    {
                        ProjectionWindow window = new ProjectionWindow();
                        window.Left = screen.Bounds.Left;
                        window.Top = screen.Bounds.Top;
                        window.Width = screen.Bounds.Width;
                        window.Height = screen.Bounds.Height;

                        window.Show();
                        window.WindowState = WindowState.Maximized;
                        _projectionWindows.Add(window);

                        Functions.UpdateBackground(window.imgBackground);
                        if(txtLivePreview.Text != "- MUTE -" && txtLivePreview.Text != "PROIECȚIE ACTIVĂ" && txtLivePreview.Text != "PROIECȚIE OPRITĂ" && txtLivePreview.Text != "-")
                            window.SetText(txtLivePreview.Text);
                    }
                }

                if (_projectionWindows.Count > 0)
                {
                    btnToggleProjection.Content = "OPREȘTE PROIECȚIA";
                    btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
                    if(txtLivePreview.Text == "-")
                        txtLivePreview.Text = "PROIECȚIE ACTIVĂ";
                }
            }
            else
            {
                _projectionWindows.ForEach(w => w.Close());
                _projectionWindows.Clear();

                btnToggleProjection.Content = "DESCHIDE PROIECȚIA";
                btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#28A745"));
                txtLivePreview.Text = "PROIECȚIE OPRITĂ";
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.IncarcaCantari(this, txtSearch.Text);
        }
        private void BtnStergeFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (lstFavorite.SelectedItem is Song selectata)
            {
                _listaFavorite.Remove(selectata);
                lstFavorite.ItemsSource = null;
                lstFavorite.ItemsSource = _listaFavorite;
            }
        }

        private void lstCantari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selectata)
                Functions.IncarcaDetaliiCantare(this, selectata);
        }

        private void Strofa_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            if (button != null)
            {
                string textStrofa = button.DataContext.ToString();
                txtLivePreview.Text = textStrofa;

                foreach (var window in _projectionWindows)
                {
                    if (window.IsLoaded)
                    {
                        window.SetText(textStrofa);
                    }
                }
            }
        }
        private void Blackout_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in _projectionWindows)
            {
                window.SetText("");
            }
            txtLivePreview.Text = "- MUTE -";
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selectata)
            {
                if (!_listaFavorite.Any(c => c.Id == selectata.Id))
                {
                    _listaFavorite.Add(selectata);
                    lstFavorite.ItemsSource = null;
                    lstFavorite.ItemsSource = _listaFavorite;
                }
            }
        }
        private void lstFavorite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFavorite.SelectedItem is Song selectata)
                Functions.IncarcaDetaliiCantare(this, selectata);
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this;
            if (settings.ShowDialog() == true || settings.IsActive == false)
            {
                Functions.UpdateBackground(this.imgPreviewBackground);

                foreach (var window in _projectionWindows)
                {
                    Functions.UpdateBackground(window.imgBackground);
                }
            }
        }
        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (lstCantari.Items.Count > 0)
                {
                    lstCantari.Focus();
                    lstCantari.SelectedIndex = 0;
                    if (lstCantari.SelectedItem is Song selectata)
                    {
                        Functions.IncarcaDetaliiCantare(this, selectata);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}