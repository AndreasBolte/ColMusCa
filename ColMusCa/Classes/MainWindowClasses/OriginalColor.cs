using System;
using System.Collections.Generic;
using System.Drawing;

// using System.Windows.Media;

namespace ColMusCa
{
    public class OriginalColor
    {
        public Color Pix;

        private long count;

        private double distanceMin;

        private int distanceMinIndex;

        private double distanceTo0;

        private double distanceTo1;

        private double distanceTo2;

        private double distanceTo3;

        private double distanceTo4;
        private int calcMode;
        private Bitmap[] bitmapPalette;

        //Constructor
        public OriginalColor()
        {
            Count = 0;
            distanceTo0 = double.MaxValue;
            distanceTo1 = double.MaxValue;
            distanceTo2 = double.MaxValue;
            distanceTo3 = double.MaxValue;
            distanceTo4 = double.MaxValue;
        }

        public OriginalColor(int calcMode, Color pix, Bitmap[] bitmapPalette)
        {
            this.calcMode = calcMode;
            Pix = pix;
            this.bitmapPalette = bitmapPalette;

            int jMin = -1;

            // distance to palette 0..4
            double[] distance = new double[4];

            // Minium distance to palette 0..4
            double[] minDistance = new double[4];

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
            for (int j = 0; j < bitmapPalette.Length; j++)
            {
                //Calculate the min distance to palette 0..4
                this.distanceMinIndex = -1;
                this.DistanceMin = Double.MaxValue;
                this.DistanceTo0 = Double.MaxValue;
                this.DistanceTo1 = Double.MaxValue;
                this.DistanceTo2 = Double.MaxValue;
                this.DistanceTo3 = Double.MaxValue;
                this.DistanceTo4 = Double.MaxValue;

                if (bitmapPalette[j] != null)
                {
                    minDistance[j] = Double.MaxValue;

                    for (int x = 0; x < bitmapPalette[j].Width; x++)
                    {
                        for (int y = 0; y < bitmapPalette[j].Height; y++)
                        {
                            lab1 = ColorSpace.RGB2Lab(bitmapPalette[j].GetPixel(x, y));
                            hsv1 = ColorSpace.RGB2HSV(bitmapPalette[j].GetPixel(x, y));
                            if (calcMode == 0) // Color-distance
                            {
                                distance[j] = ColorSpace.ColorDistance2(lab0, lab1);
                            }
                            if (calcMode == 1) // HSV-Color H (Farbwert)
                            {
                                distance[j] = Math.Abs(hsv0[0] - hsv1[0]);
                            }
                            if (calcMode == 2) // HSV-Color S (Sätigung)
                            {
                                distance[j] = Math.Abs(hsv0[1] - hsv1[1]);
                            }
                            if (calcMode == 3) // HSV-Color V (Hellwert)
                            {
                                distance[j] = Math.Abs(hsv0[2] - hsv1[2]);
                            }
                            if (distance[j] < minDistance[j])
                            {
                                minDistance[j] = distance[j];
                                jMin = j;
                                if (j == 0) this.DistanceTo0 = minDistance[j];
                                if (j == 1) this.DistanceTo1 = minDistance[j];
                                if (j == 2) this.DistanceTo2 = minDistance[j];
                                if (j == 3) this.DistanceTo3 = minDistance[j];
                                if (j == 4) this.DistanceTo4 = minDistance[j];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Counted pixel from Original Picture with this color
        /// </summary>
        public long Count { get => count; set => count = value; }

        /// <summary>
        /// Calculate minium distance from Original color to Palette 0 color
        /// </summary>
        public double DistanceTo0 { get => distanceTo0; set => distanceTo0 = value; }

        /// <summary>
        /// Calculate minium distance from Original color to Palette 1 color
        /// </summary>
        public double DistanceTo1 { get => distanceTo1; set => distanceTo1 = value; }

        /// <summary>
        /// Calculate minium distance from Original color to Palette 2 color
        /// </summary>
        public double DistanceTo2 { get => distanceTo2; set => distanceTo2 = value; }

        /// <summary>
        /// Calculate minium distance from Original color to Palette 3 color
        /// </summary>
        public double DistanceTo3 { get => distanceTo3; set => distanceTo3 = value; }

        /// <summary>
        /// Calculate minium distance from Original color to Palette 0 color
        /// </summary>
        public double DistanceTo4 { get => distanceTo4; set => distanceTo4 = value; }

        public double DistanceMin { get => distanceMin; set => distanceMin = value; }
        public int DistanceMinIndex { get => distanceMinIndex; set => distanceMinIndex = value; }
    }

    //create comparer RGBA
    internal class PixelComparerRGBA : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by R
            int result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance
    internal class PixelComparerDistanceMin : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceMin
            int result = x.DistanceMin.CompareTo(y.DistanceMin);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance0
    internal class PixelComparerDistance0 : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceTo0
            int result = x.DistanceTo0.CompareTo(y.DistanceTo0);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance1
    internal class PixelComparerDistance1 : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceTo1
            int result = x.DistanceTo1.CompareTo(y.DistanceTo1);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance2
    internal class PixelComparerDistance2 : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceTo2
            int result = x.DistanceTo2.CompareTo(y.DistanceTo2);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance3
    internal class PixelComparerDistance3 : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceTo3
            int result = x.DistanceTo3.CompareTo(y.DistanceTo3);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }

    //create comparer Distance4
    internal class PixelComparerDistance4 : IComparer<OriginalColor>
    {
        public int Compare(OriginalColor x, OriginalColor y)
        {
            //first by DistanceTo4
            int result = x.DistanceTo4.CompareTo(y.DistanceTo4);

            //then R
            if (result == 0)
                result = x.Pix.R.CompareTo(y.Pix.R);

            //then G
            if (result == 0)
                result = x.Pix.G.CompareTo(y.Pix.G);

            // third sort by B
            if (result == 0)
                result = x.Pix.B.CompareTo(y.Pix.B);

            // 4 sort by Alpha
            if (result == 0)
                result = x.Pix.A.CompareTo(y.Pix.A);

            return result;
        }
    }
}