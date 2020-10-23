using System;

namespace ColMusCa
{
    internal class DataGridSource
    {
        private int numberLine;
        private double perCent;

        // Color.Pix
        private Byte a;

        private Byte r;
        private Byte g;
        private Byte b;
        private string colorName;

        private long count;

        private double distanceMin;

        private int distanceMinIndex;

        private double distanceTo0;

        private double distanceTo1;

        private double distanceTo2;

        private double distanceTo3;

        private double distanceTo4;

        //Constructor
        public DataGridSource()
        {
        }

        public int NumberLine { get => numberLine; set => numberLine = value; }

        /// <summary>
        /// Counted pixel from Original Picture with this color
        /// </summary>
        public long Count { get => count; set => count = value; }

        public double PerCent { get => perCent; set => perCent = value; }
        public byte A { get => a; set => a = value; }
        public byte R { get => r; set => r = value; }
        public byte G { get => g; set => g = value; }
        public byte B { get => b; set => b = value; }
        public string ColorName { get => colorName; set => colorName = value; }

        public double DistanceMin { get => distanceMin; set => distanceMin = value; }
        public int DistanceMinIndex { get => distanceMinIndex; set => distanceMinIndex = value; }

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
    }
}