using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data.SQLite;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Proiectie
{
    public partial class EditSongWindow : Window
    {
        Song song;
        MainWindow window;
        private List<ProjectionWindow> _projectionWindows = new List<ProjectionWindow>();
        private ObservableCollection<Song> _listaIstoric = new ObservableCollection<Song>();
        object strofa; 
        public EditSongWindow(MainWindow window, List<ProjectionWindow> _projectionWindowsm, ObservableCollection<Song> _listaIstoric, Song song, object strofa)
        {
            InitializeComponent();
            this.song = song;
            this.strofa = strofa;
            this._listaIstoric = _listaIstoric;
            this.window = window;
            this._projectionWindows = _projectionWindowsm;
            if(song != null )
            {
                txtNewAuthor.Text = song.Author;
                txtNewTitle.Text = song.Title;
                txtNewContent.Text = song.Verses;
            }
            else
            {
                this.ContentRendered += (s, e) => {
                    System.Windows.Forms.MessageBox.Show("Trebuie să selectați o cântare întâi!");
                    this.Close();
                };
            }
        }
        private void SaveLocal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string textNouComplet = txtNewContent.Text.Trim();

                Song cantarelocal = new Song
                {
                    Id = song != null ? song.Id : 0,
                    Title = txtNewTitle.Text.Trim(),
                    Verses = textNouComplet,
                    Author = txtNewAuthor.Text.Trim()
                };

                object strofaDeTrimisLive = strofa; 

                if (strofa != null && !string.IsNullOrWhiteSpace(textNouComplet))
                {
                    string textStrofaVeche = strofa.ToString().Trim();

                    var strofeNoi = System.Text.RegularExpressions.Regex.Split(textNouComplet, @"(?=\d+\.)|(?:\r?\n){2,}")
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrWhiteSpace(s))
                                        .Where(s => s.Length > 5)
                                        .Where(s => !s.Contains("Text:"))
                                        .Where(s => !s.Contains("Compozitor:"))
                                        .Where(s => s.Any(char.IsLetter))
                                        .Where(s => !System.Text.RegularExpressions.Regex.IsMatch(s, @"^\d{4}$"))
                                        .ToList();

                    int indexStrofaVeche = window.lstStrofe.Items.IndexOf(strofa);

                    if (indexStrofaVeche >= 0 && indexStrofaVeche < strofeNoi.Count)
                    {
                        strofaDeTrimisLive = strofeNoi[indexStrofaVeche];
                    }
                }

                Functions.LoadSongDetails(window, cantarelocal);

                if (window.lstStrofe.Items.Count > 0 && window.lstStrofe.Items.Contains(strofaDeTrimisLive))
                {
                    window.lstStrofe.SelectedItem = strofaDeTrimisLive;
                }

                Functions.SendLive(window, _projectionWindows, cantarelocal, _listaIstoric, strofaDeTrimisLive);

                System.Windows.Forms.MessageBox.Show("Cântare actualizată live cu succes!");

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Eroare la salvarea locală: " + ex.Message);
            }
        }

        private void SaveGlobal_Click(object sender, RoutedEventArgs e)
        {
            string titluNou = txtNewTitle.Text.Trim();
            string autorNou = txtNewAuthor.Text.Trim();
            string versuriNoi = txtNewContent.Text.Trim();

            if (string.IsNullOrWhiteSpace(titluNou) || string.IsNullOrWhiteSpace(versuriNoi))
            {
                System.Windows.Forms.MessageBox.Show("Titlul și conținutul nu pot fi goale!");
                return;
            }

            string titluNorm = Functions.NormalizeazaText(titluNou);
            string versuriNorm = Functions.NormalizeazaText(versuriNoi);

            try
            {
                UpdateSongInDatabase(song.Id, titluNou, versuriNoi, autorNou, titluNorm, versuriNorm);

                object strofaDeTrimisLive = strofa;
                if (strofa != null)
                {
                    var strofeNoi = System.Text.RegularExpressions.Regex.Split(versuriNoi, @"(?=\d+\.)|(?:\r?\n){2,}")
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrWhiteSpace(s))
                                        .Where(s => s.Length > 5)
                                        .Where(s => !s.Contains("Text:"))
                                        .Where(s => !s.Contains("Compozitor:"))
                                        .Where(s => s.Any(char.IsLetter))
                                        .Where(s => !System.Text.RegularExpressions.Regex.IsMatch(s, @"^\d{4}$"))
                                        .ToList();

                    int indexStrofaVeche = window.lstStrofe.Items.IndexOf(strofa);

                    if (indexStrofaVeche >= 0 && indexStrofaVeche < strofeNoi.Count)
                    {
                        strofaDeTrimisLive = strofeNoi[indexStrofaVeche];
                    }
                }

                song.Title = titluNou;
                song.Verses = versuriNoi;
                song.Author = autorNou;

                Functions.LoadSongDetails(window, song);

                if (window.lstStrofe.Items.Count > 0 && window.lstStrofe.Items.Contains(strofaDeTrimisLive))
                {
                    window.lstStrofe.SelectedItem = strofaDeTrimisLive;
                }

                Functions.SendLive(window, _projectionWindows, song, _listaIstoric, strofaDeTrimisLive);

                System.Windows.Forms.MessageBox.Show("Cântare salvată în baza de date și actualizată live!");

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Eroare la salvare: " + ex.Message);
            }
        }

        private void UpdateSongInDatabase(int id, string titlu, string versuri, string autor, string titluNorm, string versuriNorm)
        {
            string connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")}";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Cantece 
                       SET titlu = @titlu, 
                           versuri = @versuri, 
                           autor = @autor, 
                           titluNormalizat = @titluNorm, 
                           versuriNormalizate = @versuriNorm 
                       WHERE id = @id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@titlu", titlu);
                    command.Parameters.AddWithValue("@versuri", versuri);
                    command.Parameters.AddWithValue("@autor", autor);
                    command.Parameters.AddWithValue("@titluNorm", titluNorm);
                    command.Parameters.AddWithValue("@versuriNorm", versuriNorm);
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
