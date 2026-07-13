using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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

            double savedClockFontSize = Properties.Settings.Default.clockFontSize;
            if (savedClockFontSize > 0)
            {
                txtClock.FontSize = savedClockFontSize;
            }
        }

        public void SetTextCuStil(string textStrofa, bool esteUltimaStrofa, bool trackerActiv)
        {
            txtMain.Inlines.Clear();
            if (string.IsNullOrEmpty(textStrofa)) return;

            txtMain.FontSize = CalculateBestFontSize(textStrofa);

            var matchPrefix = Regex.Match(textStrofa, @"^([0-9]{1,4}\s*[.:\-]?\s*)");
            if (matchPrefix.Success && esteUltimaStrofa && trackerActiv)
            {
                string prefix = matchPrefix.Groups[1].Value;
                string restulTextului = textStrofa.Substring(prefix.Length);

                string savedHex = Properties.Settings.Default.HexNumber;
                System.Windows.Media.Brush customBrush;
                try
                {
                    if (!string.IsNullOrEmpty(savedHex))
                    {
                        if (!savedHex.StartsWith("#")) savedHex = "#" + savedHex;
                        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(savedHex);
                        customBrush = new SolidColorBrush(color);
                    }
                    else
                    {
                        customBrush = System.Windows.Media.Brushes.Orange;
                    }
                }
                catch
                {
                    customBrush = System.Windows.Media.Brushes.Orange;
                }

                Run runPrefix = new Run(prefix);
                runPrefix.Foreground = customBrush;

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

        private double CalculateBestFontSize(string text)
        {
            double maxFont = Properties.Settings.Default.txtFontSize;
            double minFont = 34;

            double sigurantaMargin = 20;

            double availableWidth = ActualWidth - txtMain.Margin.Left - txtMain.Margin.Right - (sigurantaMargin * 2);
            double availableHeight = ActualHeight - txtMain.Margin.Top - txtMain.Margin.Bottom - (sigurantaMargin * 2);

            if (availableWidth <= 0 || availableHeight <= 0)
                return maxFont > 0 ? maxFont : minFont;

            Typeface typeface = new Typeface(txtMain.FontFamily, txtMain.FontStyle, txtMain.FontWeight, txtMain.FontStretch);

            for (double size = maxFont; size >= minFont; size -= 2)
            {
                FormattedText formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    size,
                    System.Windows.Media.Brushes.White,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );

                formattedText.MaxTextWidth = availableWidth;

                if (formattedText.Height <= availableHeight)
                {
                    return size;
                }
            }

            return minFont;
        }

        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() =>
            {
                txtMain.FontSize = CalculateBestFontSize(textCurent);
                txtMain.Text = textCurent;
            });
        }
    }
}