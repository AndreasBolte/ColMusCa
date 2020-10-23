using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColMusCa
{
    public class PaletteColors
    {
        private string colorName;
        private int colorCount;
        private double distanceCorrector;

        private List<Color> Col;

        /// <summary>
        /// Full path name of the color palette
        /// </summary>
        public string ColorName { get => colorName; set => colorName = value; }

        /// <summary>
        /// replaced countend colors from the palette to the imitation picture
        /// </summary>
        public int ColorCount { get => colorCount; set => colorCount = value; }

        /// <summary>
        /// Distance sub to increase the colors of used color in the imitation picture
        /// </summary>
        public double DistanceCorrector { get => distanceCorrector; set => distanceCorrector = value; }
    }
}