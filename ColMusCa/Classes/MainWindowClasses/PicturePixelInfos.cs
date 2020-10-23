using System.Drawing;

namespace ColMusCa
{
    /// <summary>
    /// Color for the palette from the picture
    /// </summary>
    public class PicturePixelInfos
    {
        ///Constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="col">Drawing Color from the pixel</param>
        public PicturePixelInfos(Color col)
        {
            Pix = col;
            Counter = 1;
            Hsv = new double[3];
            Lab = new double[3];

            Hsv = ColorSpace.RGB2HSV(Pix);
            Lab = ColorSpace.RGB2Lab(Pix);
            DistanceToWhite = ColorSpace.ColorDistance2(this.Lab, ColorSpace.RGB2Lab(Color.White));
        }

        private int counter;

        private Color pix;

        /// <summary>
        /// RGB Palette, Alpha, R, G,B
        /// </summary>
        public Color Pix
        {
            get
            {
                return pix;
            }

            set
            {
                pix = value;
            }
        }

        private double distanceToWhite;

        private double[] hsv;

        /// <summary>
        /// HSV Palette
        /// </summary>
        /// /// <param name="hsv">
        /// Ein 3 dimensionaler Vektor mit den HSV-Farbkomponenten mit
        /// h in [0,360], s in [0,1] und v in [0,1].
        /// </param>
        public double[] Hsv
        {
            get
            {
                return hsv;
            }

            set
            {
                hsv = value;
            }
        }

        private double[] lab;

        /// <summary>
        /// Lab2 Palette
        /// </summary>
        /// /// <param name="Lab">
        /// 3D-Vektor mit den Lab-Komponenten der Farbe.
        /// </param>
        public double[] Lab
        {
            get
            {
                return lab;
            }

            set
            {
                lab = value;
            }
        }

        /// <summary>
        /// Counter from the pixels with the same color in the picture
        /// </summary>
        public int Counter { get => counter; set => counter = value; }

        /// <summary>
        /// Distance to white
        /// </summary>
        public double DistanceToWhite { get => distanceToWhite; set => distanceToWhite = value; }
    }
}