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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransparentInkCanvas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IStylus
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool isDrawing = false;
        public bool IsDrawing()
        {
            return this.isDrawing;
        }

        public void StylusHover()
        {
            this.inkCanvas.Background = new SolidColorBrush(Color.FromArgb(10, 255, 155, 155));
        }

        private void InkCanvas_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            //this.inkCanvas.Background = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
            if (!this.isDrawing)
            {
                this.isDrawing = true;
                Console.WriteLine("I drawing true");
            }
        }

        private void inkCanvas_StylusLeave(object sender, StylusEventArgs e)
        {
            //this.inkCanvas.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            this.isDrawing = false;
            Console.WriteLine("I drawing false");
        }

        void IStylus.MouseMove()
        {

            if (this.inkCanvas.Background != Brushes.Transparent)
                this.inkCanvas.Background = Brushes.Transparent; // new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
        }
    }
}
