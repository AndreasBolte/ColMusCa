using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace ColMusCa
{
    /// <summary>
    /// Interaktionslogik für BmpPictureInfo.xaml
    /// </summary>
    public partial class BmpPictureInfo : Window
    {
        public BmpPictureInfo()
        {
            InitializeComponent();
        }

        private void MenuOpenClick(object sender, RoutedEventArgs e)
        {
            string bmpPictureFullPath;

            System.Uri pathUri;

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Bmp-Bild Infos erstellen", // Default file name
                                                       //dlg.DefaultExt = ".bmp"; // Default file extension
                                                       //dlg.Filter = "bmp-Bild (.bmp)|*.bmp"; // Filter files by extension
                Filter = "*.Bmp files (*.bmp) | *.bmp"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                bmpPictureFullPath = dlg.FileName;
            }

            if (File.Exists(dlg.FileName))
            {
                // Load the image.
                System.Drawing.Image image1 = System.Drawing.Image.FromFile(dlg.FileName);
                System.Drawing.Image image1Resize;
            }
        }


        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
        }

        private void BtnStartColorClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
