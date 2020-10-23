using System.Drawing;

namespace ColMusCa
{
    public class HSV_Color
    {
        public Color Pix;
        private double[] hsvColor;

        public HSV_Color()
        {
            HsvColor = new double[3];
        }

        public double[] HsvColor { get => hsvColor; set => hsvColor = value; }
    }
}