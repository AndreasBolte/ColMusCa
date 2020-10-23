using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ColMusCa
{
    public class PaletteGradient
    {
        //public System.Windows.Media.SolidColorBrush BrushError = System.Windows.Media.Brushes.Red;
        //public System.Windows.Media.SolidColorBrush BrushNoError = System.Windows.Media.Brushes.White;

        // For the Image in PaletteWindow
        public BitmapImage BitmapImagePalGrad;

        public Bitmap BitmapPalGrad; // For pixel accces

        private string pathBitmapImage;
        private string pathBitmap;

        // Temporer Data File
        private string pathTempBitmapImage;

        private string pathTempBitmap;

        public System.Windows.Media.Color StartColor;
        public System.Windows.Media.Color EndColor;
        public System.Drawing.Color RGB_StartColor;
        public System.Drawing.Color RGB_EndColor;
        public System.Drawing.Color RGB_NewColor;

        public double[] LabStartColor;
        public double[] LabEndColor;
        public double[] LabStepColor;
        public double[] LabNewColor;

        public double[] HsvStartColor;
        public double[] HsvEndColor;
        public double[] HsvStepColor;
        public double[] HsvNewColor;

        private int pixelCounter;
        private int hsvCounter0;
        private int hsvCounter1;
        private int hsvCounter2;

        public int PixelCounter
        {
            get
            {
                return pixelCounter;
            }
            set
            {
                if (value > 0)
                {
                    pixelCounter = value;
                }
                else
                {
                    MessageBox.Show("unzulässige Eingabe: TxtPixelCounter < 1");
                }
            }
        }

        public int HsvCounter0
        {
            get
            {
                return hsvCounter0;
            }
            set
            {
                if (value > 0)
                {
                    hsvCounter0 = value;
                }
                else
                {
                    MessageBox.Show("unzulässige Eingabe: hsvCounter0 < 1");
                }
            }
        }

        public int HsvCounter1
        {
            get
            {
                return hsvCounter1;
            }
            set
            {
                if (value > 0)
                {
                    hsvCounter1 = value;
                }
                else
                {
                    MessageBox.Show("unzulässige Eingabe: hsvCounter1 < 1");
                }
            }
        }

        public int HsvCounter2
        {
            get
            {
                return hsvCounter2;
            }
            set
            {
                if (value > 0)
                {
                    hsvCounter2 = value;
                }
                else
                {
                    MessageBox.Show("unzulässige Eingabe: hsvCounter2 < 1");
                }
            }
        }

        public string PathBitmapImage { get => pathBitmapImage; set => pathBitmapImage = value; }
        public string PathBitmap { get => pathBitmap; set => pathBitmap = value; }
        public string PathTempBitmapImage { get => pathTempBitmapImage; set => pathTempBitmapImage = value; }
        public string PathTempBitmap { get => pathTempBitmap; set => pathTempBitmap = value; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PaletteGradient()
        {
            LabStartColor = new double[3];
            LabEndColor = new double[3];
            LabStepColor = new double[3];
            LabNewColor = new double[3];

            HsvStartColor = new double[3];
            HsvEndColor = new double[3];
            HsvStepColor = new double[3];
            HsvNewColor = new double[3];
        }
    }
}