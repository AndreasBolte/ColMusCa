using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ColMusCa
{
    /// <summary>
    /// Interaktionslogik für PaletteDataGrid0.xaml
    /// </summary>
    public partial class PaletteDataGrid0 : Window
    {
        private DataGridSource listElement;
        public SortedSet<OriginalColor> OriginalPaletteChart;
        private List<DataGridSource> DaGriSource;
        private double previousCount;
        private double pixelCount;

        public PaletteDataGrid0()

        {
            InitializeComponent();
        }

        public PaletteDataGrid0(SortedSet<OriginalColor> originalPaletteChart)
        {
        }

        public PaletteDataGrid0(SortedSet<OriginalColor> originalPaletteChart, Double pixelCount) : this(originalPaletteChart)
        {
            InitializeComponent();

            this.pixelCount = pixelCount;
            OriginalPaletteChart = originalPaletteChart;
            previousCount = 0;

            DaGriSource = new List<DataGridSource>(OriginalPaletteChart.Count);

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                listElement = new DataGridSource
                {
                    NumberLine = DaGriSource.Count + 1,
                    Count = item.Count,
                    PerCent = Math.Round(((Convert.ToDouble(item.Count) + previousCount) * 100.0 / pixelCount), 2),
                    A = item.Pix.A,
                    R = item.Pix.R,
                    G = item.Pix.G,
                    B = item.Pix.B,
                    ColorName = item.Pix.Name,
                    DistanceMin = Math.Round(item.DistanceMin, 2),
                    DistanceMinIndex = item.DistanceMinIndex,
                    DistanceTo0 = Math.Round(item.DistanceTo0, 2),
                    DistanceTo1 = Math.Round(item.DistanceTo1, 2),
                    DistanceTo2 = Math.Round(item.DistanceTo2, 2),
                    DistanceTo3 = Math.Round(item.DistanceTo3, 2),
                    DistanceTo4 = Math.Round(item.DistanceTo4, 2),
                };
                previousCount += listElement.Count;
                DaGriSource.Add(listElement);
            }

            DataGridPalette.ItemsSource = DaGriSource;
        }

        private void PalDaGri0Closed(object sender, EventArgs e)
        {
        }
    }
}