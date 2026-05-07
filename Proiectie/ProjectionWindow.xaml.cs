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
        }
        public void SetText(string textCurent)
        {
            Dispatcher.Invoke(() => {
                txtMain.Text = textCurent;
            });
        }
    }
}
