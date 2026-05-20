using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Proiectie
{
    public static class Functions
    {
        public static string NormalizeazaText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = text.ToLower();

            text = text.Replace("ă", "a").Replace("â", "a");
            text = text.Replace("î", "i");
            text = text.Replace("ș", "s").Replace("ş", "s");
            text = text.Replace("ț", "t").Replace("ţ", "t");

            text = text.Replace("-", " ");
            text = Regex.Replace(text, @"[^a-z0-9]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }
        public static void LoadSongs(MainWindow window, string filtru = "")
        {
            using (var connection = new SqliteConnection(window._connectionString))
            {
                connection.Open();

                string filtruCurat = NormalizeazaText(filtru);
                string filtruSql = "%" + filtruCurat.Replace(" ", "%") + "%";

                string sqlQuery;

                if (window.rbContinut.IsChecked == true)
                {
                    sqlQuery = $@"SELECT id, titlu, versuri, autor FROM Cantece WHERE versuriNormalizate LIKE @filtru LIMIT 50";
                }
                else
                {
                    sqlQuery = $@"SELECT id, titlu, versuri, autor FROM Cantece WHERE titluNormalizat LIKE @filtru OR SUBSTR(versuriNormalizate, 1, 60) LIKE @filtru LIMIT 50";
                }
                SqliteCommand command = new SqliteCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@filtru", filtruSql);

                List<Song> lista = new List<Song>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1) != "Titlu necunoscut")
                            lista.Add(new Song
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Verses = reader.GetString(2),
                                Author = reader.GetString(3)
                            });
                    }
                }
                window.lstCantari.ItemsSource = lista;
            }
        }

        public static void LoadSongDetails(MainWindow window, Song selected)
        {
            window.lblSelectedAuthor.Text = "Autor: " + (string.IsNullOrWhiteSpace(selected.Author) ? "Anonim" : selected.Author);
            window.lblSelectedTitle.Text = selected.Title;

            string cleanedVerses = Regex.Replace(selected.Verses, @"^(?:I:|O:).*$", "", RegexOptions.Multiline).Trim();

            var strofe = Regex.Split(cleanedVerses, @"(?=\d+\.)|(?:\r?\n){2,}")
                              .Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Where(s => s.Length > 5)
                              .Where(s => !s.Contains("Text:"))
                              .Where(s => !s.Contains("Compozitor:"))
                              .Where(s => s.Any(char.IsLetter))
                              .Where(s => !Regex.IsMatch(s, @"^\d{4}$"))
                              .ToList();
            window.lstStrofe.ItemsSource = strofe;
        }

        public static void SendLive(MainWindow window, List<ProjectionWindow> _projectionWindows, Song lastSongSelected, ObservableCollection<Song> _listaIstoric, object selectedItem)
        {
            if (selectedItem == null) return;

            string textStrofa = selectedItem.ToString();
            window.txtLiveTextPreview.Text = textStrofa;

            foreach (var _projWindow in _projectionWindows)
            {
                if (_projWindow.IsLoaded)
                {
                    _projWindow.SetText(textStrofa);
                }
            }

            if (_projectionWindows.Count >= 1 && lastSongSelected != null)
            {
                if (!_listaIstoric.Any(c => c.Id == lastSongSelected.Id))
                {
                    _listaIstoric.Add(lastSongSelected);
                }
            }
        }

        public static void LoadMonitors(SettingsWindow window)
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

            window.MonitorsList.ItemsSource = options;
        }
        public static Song GetSongByTitle(MainWindow window, string title)
        {
            Song foundSong = null;

            using (SQLiteConnection conn = new SQLiteConnection(window._connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Cantece WHERE titlu = @title LIMIT 1";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            foundSong = new Song
                            {
                                Title = reader["titlu"].ToString(),
                                Verses = reader["versuri"].ToString(),
                                Author = reader["autor"] != DBNull.Value ? reader["autor"].ToString() : "-"
                            };
                        }
                    }
                }
            }
            return foundSong;
        }
        public static void UpdateBackground(System.Windows.Controls.Image controlImagine)
        {
            if (controlImagine == null) return;

            string path = Properties.Settings.Default.BackgroundImagePath;

            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    controlImagine.Source = bitmap;
                }
                catch
                {
                    controlImagine.Source = null;
                }
            }
            else
            {
                controlImagine.Source = null;
            }
        }
        public static string SetClock(string hour, string minute)
        {
            string txtClock = "";
            if (hour == "" && minute == "")
                txtClock = "";
            else
            {
                if (int.Parse(hour) <= 9)
                    txtClock += $"0{hour}";
                else
                    txtClock += hour;

                txtClock += ":";

                if (int.Parse(minute) <= 9)
                    txtClock += $"0{minute}";
                else
                    txtClock += minute;
            }
            return txtClock;
        }
    }
}
