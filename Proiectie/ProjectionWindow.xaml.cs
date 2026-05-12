using System.Windows;
using System.Windows.Media.Imaging;

namespace Proiectie
{
    public partial class ProjectionWindow : Window
    {
        public ProjectionWindow()
        {
            InitializeComponent();
            Functions.UpdateBackground(this.imgBackground);

            string fontName = Properties.Settings.Default.SelectedFont;
            if (!string.IsNullOrEmpty(fontName))
            {
                txtClock.FontFamily = new System.Windows.Media.FontFamily(fontName);
                txtMain.FontFamily = new System.Windows.Media.FontFamily(fontName);
            }
        }
        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() => {
                txtMain.Text = textCurent;
            });
        }
    }
}
