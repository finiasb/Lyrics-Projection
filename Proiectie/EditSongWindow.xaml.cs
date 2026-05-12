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

namespace Proiectie
{
    /// <summary>
    /// Interaction logic for EditSongWindow.xaml
    /// </summary>
    public partial class EditSongWindow : Window
    {
        Song song;
        public EditSongWindow(Song song)
        {
            InitializeComponent();
            this.song = song;
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

        private void Save_Click(object sender, RoutedEventArgs e)
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

                System.Windows.Forms.MessageBox.Show("Cântare actualizată cu succes!");

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
