using System.Windows;
using System.Windows.Media.Imaging;

namespace Proiectie
{
    public partial class ProjectionWindow : Window
    {
        public ProjectionWindow()
        {
            InitializeComponent();
            UpdateBackground();
        }
        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() => {
                txtMain.Text = textCurent;
            });
        }

        public void UpdateBackground()
        {
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

                    imgBackground.Source = bitmap;
                }
                catch
                {
                    imgBackground.Source = null;
                }
            }
            else
            {
                imgBackground.Source = null;
            }
        }
    }
}
