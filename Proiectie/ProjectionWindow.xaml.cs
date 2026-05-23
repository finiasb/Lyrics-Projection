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

            double savedFontSize = Properties.Settings.Default.txtFontSize;
            double savedClockFontSize = Properties.Settings.Default.clockFontSize;
            if (savedFontSize > 0 && savedClockFontSize > 0)
            {
                txtMain.FontSize = savedFontSize;
                txtClock.FontSize = savedClockFontSize;
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
