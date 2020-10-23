using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ColMusCa
{
    public static class OriginalManipulate
    {
        /// <summary>
        /// Resizes the pic by width.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="newWidth">The new width.</param>
        /// <returns></returns>
        public static Bitmap ResizePicByWidth(Image sourceImage, double newWidth)
        {
            if (newWidth == 0) return new Bitmap(sourceImage);
            // New Width = -1,0,1  means no zoom factor
            if (newWidth <= 1) newWidth = sourceImage.Width;

            double sizeFactor = newWidth / sourceImage.Width;
            double newHeigth = sizeFactor * sourceImage.Height;
            Bitmap newImage = new Bitmap((int)newWidth, (int)newHeigth);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceImage, new Rectangle(0, 0, (int)newWidth, (int)newHeigth));
            }
            return newImage;
        }

        //Das hier alles testen und weitermachen 26.10.2017
        /// <summary>
        /// Calculate the distance to palette 0..4
        /// </summary>
        /// <param name="item">color of the original palette element of sortedList</param>
        /// <param name="palBitmap">Bitmap from Palette 0..4</param>
        public static void CalcDistances(int calcMode, ref OriginalColor item, params Bitmap[] palBitmap)
        {
            // distance to palette 0..4
            double[] distance = new double[palBitmap.Length];

            // Minium distance to palette 0..4
            double[] minDistance = new double[palBitmap.Length];
            // Initialize Minimum with max. from Double
            for (int i = 0; i < minDistance.Length; i++)
            {
                minDistance[i] = Double.MaxValue;
            }

            //Lab-color from item
            double[] lab0 = new double[3];
            lab0 = ColorSpace.RGB2Lab(item.Pix);

            //Lab-color from palette
            double[] lab1 = new double[3];

            //HSV-color from item
            double[] hsv0 = new double[3];
            hsv0 = ColorSpace.RGB2HSV(item.Pix);

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
                            }
                        }
                    }

                    switch (j)
                    {
                        case 0:
                            {
                                item.DistanceTo0 = minDistance[j]; // Min Distance to palette 0
                                break;
                            }
                        case 1:
                            {
                                item.DistanceTo1 = minDistance[j]; // Min Distance to palette 1
                                break;
                            }
                        case 2:
                            {
                                item.DistanceTo2 = minDistance[j]; // Min Distance to palette 2
                                break;
                            }
                        case 3:
                            {
                                item.DistanceTo3 = minDistance[j]; // Min Distance to palette 3
                                break;
                            }
                        case 4:
                            {
                                item.DistanceTo4 = minDistance[j]; // Min Distance to palette 4
                                break;
                            }
                        default:
                            // Error no more palettes
                            break;
                    }
                }
            }
        }
    }
}