using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ColMusCa
{
    /// <summary>
    /// Interaktionslogik für PaletteDataGrid.xaml
    /// </summary>
    public partial class PaletteDataGrid : Window
    {
        private DataGridSource listElement;
        public SortedSet<OriginalColor> OriginalPaletteChart;
        private List<DataGridSource> DaGriSource;
        private double previousCount;
        private double pixelCount;

        public PaletteDataGrid()

        {
            InitializeComponent();
        }

        public PaletteDataGrid(SortedSet<OriginalColor> originalPaletteChart, Double pixelCount)
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

        private void PalDaGriClosed(object sender, EventArgs e)
        {
        }

        private void BtnShowColorClick(object sender, EventArgs e)
        {
            string color = textBoxShowColor.Text;
            color = "#" + color.Substring(2);
            Color col = (Color)ColorConverter.ConvertFromString(color);
            SolidColorBrush ColorFromString = new SolidColorBrush(col);
            this.PalDaGriGridBackground.Background = ColorFromString;
            ;
        }
    }
}