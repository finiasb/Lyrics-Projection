using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data.SQLite;
namespace Proiectie
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Song> _listaFavorite = new ObservableCollection<Song>();
        private ObservableCollection<Song> _listaIstoric = new ObservableCollection<Song>();
        private List<ProjectionWindow> _projectionWindows = new List<ProjectionWindow>();
        Song lastSongSelected;
        public string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")}";
        public MainWindow()
        {
            InitializeComponent();

            Functions.UpdateBackground(this.imgPreviewBackground);

            if (Properties.Settings.Default.SavedMonitors != null)
            {
                SettingsWindow.SelectedMonitorIds = Properties.Settings.Default.SavedMonitors.Cast<string>().ToList();
            }

            Functions.LoadSongs(this);

            Song cantareCor = Functions.GetSongByTitle(this, "Cantare cor mixt");

            if (cantareCor != null)
            {
                if (!_listaFavorite.Any(s => s.Title.Equals("Cantare cor mixt", StringComparison.OrdinalIgnoreCase)))
                {
                    _listaFavorite.Add(cantareCor);
                }
            }
            lstFavOrIstoric.ItemsSource = _listaFavorite;

            this.Closing += (s, e) =>
            {
                foreach (var window in _projectionWindows)
                {
                    window.Close();
                }
            };

            string fontName = Properties.Settings.Default.SelectedFont;
            if (!string.IsNullOrEmpty(fontName))
            {
                txtLiveClockPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);
                txtLiveTextPreview.FontFamily = new System.Windows.Media.FontFamily(fontName);
            }
        }

        private void SearchType_Changed(object sender, RoutedEventArgs e)
        {
            if (txtSearch != null)
                Functions.LoadSongs(this, txtSearch.Text);
        }

        private void ListType_Changed(object sender, RoutedEventArgs e)
        {
            if (BtnDeleteFromList == null || lstFavOrIstoric == null) return;

            if (rbFavorite.IsChecked == true)
            {
                BtnDeleteFromList.Content = "Șterge din favorite";
                lstFavOrIstoric.ItemsSource = _listaFavorite;
            }
            else
            {
                BtnDeleteFromList.Content = "Șterge din istoric";
                lstFavOrIstoric.ItemsSource = _listaIstoric;
            }
        }
        private void btnToggleProjection_Click(object sender, RoutedEventArgs e)
        {
            if (_projectionWindows.Count == 0)
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                var targetIds = SettingsWindow.SelectedMonitorIds;

                foreach (var screen in screens)
                {
                    bool shouldProject;
                    if (targetIds != null && targetIds.Count > 0)
                        shouldProject = targetIds.Contains(screen.DeviceName);
                    else
                        shouldProject = !screen.Primary;

                    if (shouldProject)
                    {
                        ProjectionWindow window = new ProjectionWindow();
                        window.Left = screen.Bounds.Left;
                        window.Top = screen.Bounds.Top;
                        window.Width = screen.Bounds.Width;
                        window.Height = screen.Bounds.Height;

                        window.Show();
                        window.WindowState = WindowState.Maximized;
                        window.txtClock.Text = txtLiveClockPreview.Text;
                        _projectionWindows.Add(window);

                        Functions.UpdateBackground(window.imgBackground);
                        if (txtLiveTextPreview.Text != "- MUTE -" && txtLiveTextPreview.Text != "PROIECȚIE ACTIVĂ" && txtLiveTextPreview.Text != "PROIECȚIE OPRITĂ" && txtLiveTextPreview.Text != "-")
                            window.SetText(txtLiveTextPreview.Text);
                    }
                }

                if (_projectionWindows.Count > 0)
                {
                    btnToggleProjection.Content = "OPREȘTE PROIECȚIA";
                    btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0392B"));
                    if (txtLiveTextPreview.Text == "-")
                        txtLiveTextPreview.Text = "PROIECȚIE ACTIVĂ";
                }
            }
            else
            {
                _projectionWindows.ForEach(w => w.Close());
                _projectionWindows.Clear();

                btnToggleProjection.Content = "DESCHIDE PROIECȚIA";
                btnToggleProjection.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#28A745"));
                txtLiveTextPreview.Text = "PROIECȚIE OPRITĂ";
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.LoadSongs(this, txtSearch.Text);
        }
        private void BtnStergeLista_Click(object sender, RoutedEventArgs e)
        {
            if (BtnDeleteFromList.Content == "Șterge din favorite")
            {
                if (lstFavOrIstoric.SelectedItem is Song selected)
                {
                    _listaFavorite.Remove(selected);
                    lstFavOrIstoric.ItemsSource = null;
                    lstFavOrIstoric.ItemsSource = _listaFavorite;
                }
            }
            else
            {
                if (lstFavOrIstoric.SelectedItem is Song selected)
                {
                    _listaIstoric.Remove(selected);
                    lstFavOrIstoric.ItemsSource = null;
                    lstFavOrIstoric.ItemsSource = _listaIstoric;
                }
            }
        }
        private void lstCantari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selected)
            {
                Functions.LoadSongDetails(this, selected);
                lastSongSelected = selected;
            }
        }

        private void Blackout_Click(object sender, RoutedEventArgs e)
        {
            foreach (var window in _projectionWindows)
            {
                window.SetText("");
            }
            txtLiveTextPreview.Text = "- MUTE -";
        }

        private void lstStrofe_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (lstStrofe.SelectedItem != null)
                {
                    Functions.SendLive(this, _projectionWindows, lastSongSelected, _listaIstoric, lstStrofe.SelectedItem);
                }
                e.Handled = true; 
            }
            else if (e.Key == Key.Escape)
            {
                txtLiveTextPreview.Text = "";
                Functions.SendLive(this, _projectionWindows, lastSongSelected, _listaIstoric, lstStrofe.SelectedItem);
            }
        }
        private void lstStrofe_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lstStrofe, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                item.IsSelected = true;
                Functions.SendLive(this, _projectionWindows, lastSongSelected, _listaIstoric, item.Content);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e) 
        {
            if (lstCantari.SelectedItem is Song selectata)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show($"Ești sigur că vrei să ștergi definitiv cântarea: {selectata.Title}?", "Confirmare Ștergere", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        DeleteSongFromDatabase(selectata.Id);

                        Functions.LoadSongs(this, txtSearch.Text);
                        
                        lblSelectedTitle.Text = "Cântare ștearsă";
                        lstStrofe.ItemsSource = null;

                        System.Windows.Forms.MessageBox.Show("Cântarea a fost ștearsă cu succes.");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("Eroare la ștergere: " + ex.Message);
                    }
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Te rugăm să selectezi o cântare din listă pentru a o șterge.");
            }
        }
        private void DeleteSongFromDatabase(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM Cantece WHERE id = @id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void lstStrofe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (lstCantari.SelectedItem is Song selectata)
            {
                if (!_listaFavorite.Any(c => c.Id == selectata.Id))
                {
                    _listaFavorite.Add(selectata);
                }
            }
        }
        private void lstFavorite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFavOrIstoric.SelectedItem is Song selectata)
                Functions.LoadSongDetails(this, selectata);
        }
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow(this);
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
                        Functions.LoadSongDetails(this, selectata);
                    }
                    e.Handled = true;
                }
            }
        }
        private void lstCantari_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (lstStrofe.Items.Count > 0)
                {
                    lstStrofe.SelectedIndex = 0;

                    var item = lstStrofe.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                    item?.Focus();
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T) return (T)child;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                string clockTime;
                if (txtLiveClockPreview.Text == "")
                    clockTime = Functions.SetClock(DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString());
                else
                    clockTime = Functions.SetClock("", "");

                foreach (var window in _projectionWindows)
                {
                    if (window.IsLoaded)
                    {
                        window.txtClock.Text = clockTime;
                    }
                }

                txtLiveClockPreview.Text = clockTime;
            }
            if (e.Key == Key.Escape)
            {
                foreach (var window in _projectionWindows)
                {
                    if (window.IsLoaded)
                    {
                        window.SetText(""); 
                    }
                }

                txtLiveTextPreview.Text = "-";


                e.Handled = true;
            }
        }
        private void AddSong_Click(object sender, RoutedEventArgs e)
        {
            AddSongWindow AddSong = new AddSongWindow();
            AddSong.Owner = this;
            if (AddSong.ShowDialog() == true)
            {
                Functions.LoadSongs(this);
            }
        }

        private void OpenEditPage_Click(object sender, RoutedEventArgs e)
        {
            EditSongWindow EditSong = new EditSongWindow(lastSongSelected);
            EditSong.Owner = this;
            if(EditSong.ShowDialog() == true)
            {
                txtSearch.Text = "";
                lstStrofe.ItemsSource = null;
                Functions.LoadSongs(this);
            }
        }
    }
}