using System.Collections.Generic;
using System.Drawing;

namespace ColMusCa
{
    //create comparer Distance
    internal class ComparerDistanceToWhite : IComparer<ColorDistanceToWhite>
    {
        public int Compare(ColorDistanceToWhite x, ColorDistanceToWhite y)
        {
            //first by DistanceMin
            int result = x.DistanceToWhite.CompareTo(y.DistanceToWhite);

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

    public class ColorDistanceToWhite
    {
        public Color Pix;
        private double distanceToWhite;

        public double DistanceToWhite { get => distanceToWhite; set => distanceToWhite = value; }
    }
}