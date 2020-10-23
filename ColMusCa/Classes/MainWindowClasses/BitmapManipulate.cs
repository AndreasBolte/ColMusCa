using System.Drawing;
using System.Drawing.Drawing2D;

namespace ColMusCa
{
    public static class BitmapManipulate
    {
        /// <summary>
        /// Resizes the pic by width.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="newWidth">The new width.</param>
        /// <returns></returns>
        public static Bitmap ResizePicByWidth(Image sourceImage, double newWidth)
        {
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
    }
}