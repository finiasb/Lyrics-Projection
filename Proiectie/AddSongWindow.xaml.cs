using System;
using System.Data.SQLite;
using System.Windows;

namespace Proiectie
{
    public partial class AddSongWindow : Window
    {
        public string _connectionString = $"Data Source={System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cantece.db")};Version=3;";

        public AddSongWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewTitle.Text))
            {
                System.Windows.Forms.MessageBox.Show("Te rugăm să introduci un titlu!");
                return;
            }

            string titlu = txtNewTitle.Text;
            string autor = string.IsNullOrWhiteSpace(txtNewAuthor.Text) ? "Anonim" : txtNewAuthor.Text;
            string versuri = txtNewContent.Text;

            string titluNorm = Functions.NormalizeazaText(titlu);
            string versuriNorm = Functions.NormalizeazaText(versuri);

                int noulId = GetNextId();

                InsertSongIntoDatabase(noulId, titlu, versuri, autor, titluNorm, versuriNorm);
                this.DialogResult = true;
                this.Close();
            
        }

        private int GetNextId()
        {
            int nextId = 1;

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT MAX(Id) FROM Cantece";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(result) + 1;
                    }
                }
            }
            return nextId;
        }

        private void InsertSongIntoDatabase(int id, string titlu, string versuri, string autor, string titluNorm, string versuriNorm)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = @"INSERT INTO Cantece (id, titlu, versuri, autor, titluNormalizat, versuriNormalizate, url) 
                       VALUES (@id, @titlu, @versuri, @autor, @titluNorm, @versuriNorm, @url)";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@titlu", titlu);
                        command.Parameters.AddWithValue("@versuri", versuri);
                        command.Parameters.AddWithValue("@autor", autor);
                        command.Parameters.AddWithValue("@titluNorm", titluNorm);
                        command.Parameters.AddWithValue("@versuriNorm", versuriNorm);
                        command.Parameters.AddWithValue("@url", "manual_entry_" + id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception e) 
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
    }
}