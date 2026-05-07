using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
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
        public static void IncarcaCantari(MainWindow window, string filtru = "")
        {
            using (var connection = new SqliteConnection(window._connectionString))
            {
                connection.Open();

                string filtruCurat = NormalizeazaText(filtru);
                string filtruSql = "%" + filtruCurat.Replace(" ", "%") + "%";

                string sqlQuery;

                if (window.rbContinut.IsChecked == true)
                {
                    sqlQuery = $@"SELECT id, titlu, versuri FROM Cantece WHERE {CreateSqlFiltre("versuri")} LIKE @filtru LIMIT 46";
                }
                else
                {
                    sqlQuery = $@"SELECT id, titlu, versuri FROM Cantece WHERE {CreateSqlFiltre("titlu")} LIKE @filtru OR {CreateSqlFiltre("SUBSTR(versuri, 1, 60)")} LIKE @filtru LIMIT 46";
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
                                Verses = reader.GetString(2)
                            });
                    }
                }
                window.lstCantari.ItemsSource = lista;
            }
        }

        private static string CreateSqlFiltre(string column)
        {
            return $"REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER({column}), '-', ' '), ',', ' '), 'ă', 'a'), 'ţ', 't'), 'ş', 's'), 'â', 'a'), 'î', 'i'), 'ș', 's'), 'ț', 't')";
        }
        public static void IncarcaDetaliiCantare(MainWindow window, Song selectata)
        {
            window.lblSelectedTitle.Text = selectata.Title;
            var strofe = Regex.Split(selectata.Verses, @"(?=\d+\.)|(?:\r?\n){2,}")
                              .Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Where(s => !s.StartsWith("I:"))
                              .Where(s => !s.StartsWith("O:"))
                              .ToList();
            window.itemsStrofe.ItemsSource = strofe;
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
    }
}
