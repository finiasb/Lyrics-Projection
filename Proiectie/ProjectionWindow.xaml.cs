using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
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

        public void SetTextCuStil(string textStrofa, bool esteUltimaStrofa, bool trackerActiv)
        {
            txtMain.Inlines.Clear(); 

            if (string.IsNullOrEmpty(textStrofa)) return;

            var matchPrefix = Regex.Match(textStrofa, @"^([0-9]{1,4}\s*[.:\-]?\s*)");
            if (matchPrefix.Success && esteUltimaStrofa && trackerActiv)
            {
                string prefix = matchPrefix.Groups[1].Value;
                string restulTextului = textStrofa.Substring(prefix.Length);

                Run runPrefix = new Run(prefix);
                runPrefix.Foreground = System.Windows.Media.Brushes.Orange; 

                Run runRest = new Run(restulTextului);
                runRest.Foreground = System.Windows.Media.Brushes.White;

                txtMain.Inlines.Add(runPrefix);
                txtMain.Inlines.Add(runRest);
            }
            else
            {
                Run runNormal = new Run(textStrofa);
                runNormal.Foreground = System.Windows.Media.Brushes.White;
                txtMain.Inlines.Add(runNormal);
            }
        }

        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() =>
            {
                txtMain.Text = textCurent;
            });
        }
    }
}
