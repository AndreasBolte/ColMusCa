using System;
using System.Drawing;

// using System.Windows.Media;

namespace ColMusCa
{
    public class ImitationColor
    {
        public Color Pix;

        private int paletteIndex;
        private int originalIndex;
        private Color paletteNewColor;

        private int targetIndex;
        private int paletteListIndex;
        private int originalListIndex;

        public ImitationColor()
        {
        }

        public ImitationColor(int calcMode, double[] correction, Color pix, Bitmap[] palBitmap)
        {
            //default settings
            PaletteNewColor = Color.FromArgb(0, 222, 222, 222);
            TargetIndex = -1;
            PaletteListIndex = -1;
            OriginalIndex = -1;
            OriginalListIndex = -1;

            int jMin = -1;
            Pix = pix;

            // distance to palette 0..4
            double distance = Double.MaxValue;

            // Minium distance to palette 0..4
            double minDistance = Double.MaxValue;

            //Lab-color from item
            double[] lab0 = new double[3];
            lab0 = ColorSpace.RGB2Lab(pix);

            //Lab-color from palette
            double[] lab1 = new double[3];

            //HSV-color from item
            double[] hsv0 = new double[3];
            hsv0 = ColorSpace.RGB2HSV(pix);

            //Lab-color from palette
            double[] hsv1 = new double[3];

            // j = Palette Bitmap 0 ..4
            for (int j = 0; j < palBitmap.Length; j++)
            {
                //Calculate the min distance to palette 0..4
                if (palBitmap[j] != null)
                {
                    for (int x = 0; x < palBitmap[j].Width; x++)
                    {
                        for (int y = 0; y < palBitmap[j].Height; y++)
                        {
                            lab1 = ColorSpace.RGB2Lab(palBitmap[j].GetPixel(x, y));
                            hsv1 = ColorSpace.RGB2HSV(palBitmap[j].GetPixel(x, y));
                            if (calcMode == 0) // Color-distance
                            {
                                distance = ColorSpace.ColorDistance2(lab0, lab1) - correction[j];
                            }
                            if (calcMode == 1) // HSV-Color H (Farbwert)
                            {
                                distance = Math.Abs(hsv0[0] - hsv1[0]) - correction[j];
                            }
                            if (calcMode == 2) // HSV-Color S (Sätigung)
                            {
                                distance = Math.Abs(hsv0[1] - hsv1[1]) - correction[j];
                            }
                            if (calcMode == 3) // HSV-Color V (Hellwert)
                            {
                                distance = Math.Abs(hsv0[2] - hsv1[2]) - correction[j];
                            }
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                jMin = j;
                            }
                        }
                    }
                }
            }
            this.PaletteIndex = jMin;
        }

        //19.11.2017 TODO set rausnehmen
        public int PaletteIndex { get => paletteIndex; set => paletteIndex = value; }

        public Color PaletteNewColor { get => paletteNewColor; set => paletteNewColor = value; }
        public int TargetIndex { get => targetIndex; set => targetIndex = value; }
        public int PaletteListIndex { get => paletteListIndex; set => paletteListIndex = value; }
        public int OriginalIndex { get => originalIndex; set => originalIndex = value; }
        public int OriginalListIndex { get => originalListIndex; set => originalListIndex = value; }
    }
}