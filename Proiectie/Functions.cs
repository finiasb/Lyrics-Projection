using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

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

                string coloanaCautare = window.rbContinut.IsChecked == true ? "versuri" : "titlu";

                SqliteCommand command = new SqliteCommand($"SELECT id, titlu, versuri FROM Cantece WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER({coloanaCautare}), '-', ' '), ',', ' '), 'ă', 'a'), 'ţ', 't'), 'ş', 's'), 'â', 'a'), 'î', 'i'), 'ș', 's'), 'ț', 't') LIKE @filtru LIMIT 46", connection);
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

    }
}
