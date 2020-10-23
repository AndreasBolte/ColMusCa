using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ColMusCa
{
    /// <summary>
    /// Interaktionslogik für Palette.xaml
    /// </summary>
    public partial class Palette : Window, IDisposable
    {
        private Dictionary<string, HSV_Color> hsvColors;

        // Picture Palette
        public System.Windows.Media.Imaging.BitmapImage BitmapImagePalette;  // For the Image in MainWindow

        public System.Drawing.Bitmap BitmapPalette;                              // For pixel acces
        public System.Drawing.Bitmap BitmapPaletteCopy;                          // For copy functions
        public Uri pathUri;

        public PaletteGradient PalGra;

        public Palette()
        {
            InitializeComponent();
            PalGra = new PaletteGradient();
        }

        private void MenuNewClick(object sender, RoutedEventArgs e)
        {
        }

        private void MenuOpenClick(object sender, RoutedEventArgs e)
        {
            string name_bmp = "";
            string name_jpg = "";

            // Label text start and End Color
            string start;
            string end;

            string[] arrayPath;

            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Palette öffnen",

                DefaultExt = ".bmp", // Default file extension
                Filter = "Palette Bitmap (.bmp)|*.bmp" // Filter files by extension
            };

            // Show open file dialog box
            Nullable<bool> res = dlg.ShowDialog();

            // Process open file dialog box results
            if (res == true)
            {
                name_bmp = dlg.FileName;
            }

            if (File.Exists(name_bmp))
            {
                PalGra.PathBitmap = name_bmp;   //open bmp
                BitmapPalette = new Bitmap(PalGra.PathBitmap);
                // read first pixel
                PalGra.RGB_StartColor = BitmapPalette.GetPixel(0, 0);
                PalGra.StartColor = System.Windows.Media.Color.FromArgb(
                PalGra.StartColor.A,
                PalGra.StartColor.R,
                PalGra.StartColor.G,
                PalGra.StartColor.B);

                // read last pixel
                PalGra.RGB_EndColor = BitmapPalette.GetPixel(BitmapPalette.Width - 1, 0);
                PalGra.EndColor = System.Windows.Media.Color.FromArgb(
                PalGra.EndColor.A,
                PalGra.EndColor.R,
                PalGra.EndColor.G,
                PalGra.EndColor.B);

                // Label text start and End Color
                start = "Startfarbe: " + PalGra.RGB_StartColor.Name;
                end = "Endfarbe: " + PalGra.RGB_EndColor.Name;
                LblStartColor.Content = start;
                LblEndColor.Content = end;
                // Text box Pixel Counter
                TxtBoxPixelCounter.Text = BitmapPalette.Width.ToString();

                //open jpg
                // Bitmap name as basic
                arrayPath = name_bmp.Split('.');
                for (int i = 0; i < arrayPath.Length - 1; ++i)
                {
                    name_jpg = name_jpg + arrayPath[i] + ".jpg";
                }
                PalGra.PathBitmapImage = name_jpg;

                // Show Bitmap
                if (File.Exists(name_jpg))
                {
                    ImageColor.BeginInit();
                    ImageColor.Source = new BitmapImage(new Uri(PalGra.PathBitmapImage));
                    ImageColor.EndInit();

                    this.Title = "Palette: " + PalGra.PathBitmapImage;
                }
            }
        }

        private void MenuSaveClick(object sender, RoutedEventArgs e)
        {
        }

        private void MenuSaveAsClick(object sender, RoutedEventArgs e)
        {
            string name_bmp = "";
            string name_jpg = "";

            string[] arrayPath;

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Palette speichern unter",

                DefaultExt = ".bmp", // Default file extension
                Filter = "Palette Bitmap (.bmp)|*.bmp" // Filter files by extension
            };

            // Show open file dialog box
            Nullable<bool> res = dlg.ShowDialog();

            // Process open file dialog box results
            if (res == true)
            {
                name_bmp = dlg.FileName;

                //copy bmp
                if (File.Exists(PalGra.PathTempBitmap))
                {
                    try
                    {
                        System.IO.File.Copy(PalGra.PathTempBitmap, name_bmp, true);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("  Copy PathTempBitmap " + ex.Message);
                    }

                    //copy jpg
                    // Bitmap name as basic
                    arrayPath = name_bmp.Split('.');
                    for (int i = 0; i < arrayPath.Length - 1; ++i)
                    {
                        name_jpg = name_jpg + arrayPath[i] + ".jpg";
                    }

                    try
                    {
                        System.IO.File.Copy(PalGra.PathTempBitmapImage, name_jpg, true);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("  Copy PathTempBitmapBitmap " + ex.Message);
                    }
                }
            }
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnStartColorClick(object sender, RoutedEventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog
            {
                // Keeps the user from selecting a custom color.
                AllowFullOpen = true,
                // Allows the user to get help. (The default is false.)
                ShowHelp = false
            };
            // Sets the initial color select to the current text color.
            //MyDialog.Color = Color.Red;
            try
            {
                MyDialog.Color = BitmapPalette.GetPixel(0, 0);
            }
            catch (Exception)
            {
                MyDialog.Color = Color.Red;
            }
            // Update the text box color if the user clicks OK

            if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PalGra.StartColor = System.Windows.Media.Color.FromArgb(
                MyDialog.Color.A, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);
            }
            // bool ok = ColorPickerWindow.ShowDialog(out PalGra.StartColor);
        }

        private void BtnEndColorClick(object sender, RoutedEventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog
            {
                // Keeps the user from selecting a custom color.
                AllowFullOpen = true,
                // Allows the user to get help. (The default is false.)
                ShowHelp = false
            };
            // Sets the initial color select to the current text color.
            try
            {
                MyDialog.Color = BitmapPalette.GetPixel(BitmapPalette.Width - 1, BitmapPalette.Height - 1);
            }
            catch (Exception)
            {
                MyDialog.Color = Color.OrangeRed;
            }

            // Update the text box color if the user clicks OK

            if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PalGra.EndColor = System.Windows.Media.Color.FromArgb(
                MyDialog.Color.A, MyDialog.Color.R, MyDialog.Color.G, MyDialog.Color.B);
            }
            // bool ok = ColorPickerWindow.ShowDialog(out PalGra.EndColor);
        }

        private void BtnPreviewClick(object sender, RoutedEventArgs e)
        {
            if (RadioBtnLinear.IsChecked == true)
            {
                PreviewLinear();
            }
            if (RadioBtn3D.IsChecked == true)
            {
                Preview3D();
            }
        }

        private void Preview3D()
        {
            bool saveTemporaryBmp;
            bool saveTemporaryJpeg;

            // corditates from bitmap for setpixel
            int x = 0;
            int y = 0;

            // Color Palette sorted by distance to 0..4
            this.hsvColors = new Dictionary<string, HSV_Color>();

            //the new calculate hsv-Color
            HSV_Color newHsvColor = new HSV_Color();
            //the gradients (3d) hsv-Color
            double[] partHsvColor = new double[3];
            // step hsv

            bool error = PaletteWindowToPaletteGradient();
            ;
            if (!error)
            {
                //string for temporary file
                string result = "";                  // temporary path
                string result_bmp = "";              // full name temporary bmp
                string result_jpeg = "";             // full name temporary jpeg

                // Label text start and End Color
                string start;
                string end;
                start = "Startfarbe: " + PalGra.StartColor.ToString();
                end = "Endfarbe: " + PalGra.EndColor.ToString();
                LblStartColor.Content = start;
                LblEndColor.Content = end;

                // Calculate the HSV start color
                Color drawingcolorStart = System.Drawing.Color.FromArgb(
                                          PalGra.StartColor.A,
                                          PalGra.StartColor.R,
                                          PalGra.StartColor.G,
                                          PalGra.StartColor.B);
                PalGra.HsvStartColor = ColorSpace.RGB2HSV(drawingcolorStart);

                // Calculate the HSV end color
                Color drawingcolorEnd = System.Drawing.Color.FromArgb(
                                          PalGra.EndColor.A,
                                          PalGra.EndColor.R,
                                          PalGra.EndColor.G,
                                          PalGra.EndColor.B);
                PalGra.HsvEndColor = ColorSpace.RGB2HSV(drawingcolorEnd);

                // Label HSV Start colors
                LblStartFarbwert.Content = "Farbwert HSV " + PalGra.HsvStartColor[0].ToString();
                LblStartHellwert.Content = "Hellwert HSV " + PalGra.HsvStartColor[1].ToString();
                LblStartSaetigung.Content = "Saetigung HSV " + PalGra.HsvStartColor[2].ToString();

                // Label HSV End colors
                LblEndFarbwert.Content = "Farbwert HSV " + PalGra.HsvEndColor[0].ToString();
                LblEndHellwert.Content = "Hellwert HSV " + PalGra.HsvEndColor[1].ToString();
                LblEndSaetigung.Content = "Saetigung HSV " + PalGra.HsvEndColor[2].ToString();

                // Define the width and height from the bitmap
                if (PalGra.HsvCounter1 >= PalGra.HsvCounter0 * PalGra.HsvCounter2)
                {
                    BitmapPalette = new System.Drawing.Bitmap(PalGra.HsvCounter0 * PalGra.HsvCounter2,
                        PalGra.HsvCounter1);
                }
                else if (PalGra.HsvCounter2 >= PalGra.HsvCounter0 * PalGra.HsvCounter1)
                {
                    BitmapPalette = new System.Drawing.Bitmap(PalGra.HsvCounter0 * PalGra.HsvCounter1,
                        PalGra.HsvCounter2);
                }
                else
                {
                    BitmapPalette = new System.Drawing.Bitmap(PalGra.HsvCounter1 * PalGra.HsvCounter2,
                        PalGra.HsvCounter0);
                }
                PalGra.RGB_StartColor = new Color();
                PalGra.RGB_StartColor = System.Drawing.Color.FromArgb(
                PalGra.StartColor.A,
                PalGra.StartColor.R,
                PalGra.StartColor.G,
                PalGra.StartColor.B);
                BitmapPalette.SetPixel(0, 0, PalGra.RGB_StartColor);

                // last pixel
                PalGra.RGB_EndColor = new System.Drawing.Color();
                PalGra.RGB_EndColor = System.Drawing.Color.FromArgb(
                PalGra.EndColor.A,
                PalGra.EndColor.R,
                PalGra.EndColor.G,
                PalGra.EndColor.B);
                BitmapPalette.SetPixel(BitmapPalette.Width - 1, BitmapPalette.Height - 1, PalGra.RGB_EndColor);

                partHsvColor[0] = (PalGra.HsvEndColor[0] - PalGra.HsvStartColor[0]) / Convert.ToDouble(PalGra.HsvCounter0);
                partHsvColor[1] = (PalGra.HsvEndColor[1] - PalGra.HsvStartColor[1]) / Convert.ToDouble(PalGra.HsvCounter1);
                partHsvColor[2] = (PalGra.HsvEndColor[2] - PalGra.HsvStartColor[2]) / Convert.ToDouble(PalGra.HsvCounter2);
                ;
                //gradient pixels
                newHsvColor.HsvColor[0] = PalGra.HsvStartColor[0];
                ;
                do
                {
                    newHsvColor.HsvColor[1] = PalGra.HsvStartColor[1];
                    ;
                    do

                    {
                        newHsvColor.HsvColor[2] = PalGra.HsvStartColor[2];
                        ;
                        do
                        {
                            // the new color from hsv
                            newHsvColor.Pix = ColorSpace.HSV2RGB(newHsvColor.HsvColor);
                            // setpixel in the bitmap
                            BitmapPalette.SetPixel(x, y, newHsvColor.Pix);
                            ;
                            // define x and y for the next pixel
                            if (y < BitmapPalette.Height - 1)
                            {
                                y = Math.Min(++y, BitmapPalette.Height - 1);
                            }
                            else if (x < BitmapPalette.Width - 1)
                            {
                                y = 0;
                                x = Math.Min(++x, BitmapPalette.Width - 1);
                            }

                            newHsvColor.HsvColor[2] += partHsvColor[2];
                        }
                        while ((Math.Abs(newHsvColor.HsvColor[2] - PalGra.HsvEndColor[2]) >= 0.0001 &&
                                (PalGra.HsvEndColor[2] != PalGra.HsvStartColor[2])));

                        newHsvColor.HsvColor[1] += partHsvColor[1];
                    }
                    while ((Math.Abs(newHsvColor.HsvColor[1] - PalGra.HsvEndColor[1]) >= 0.0001 &&
                                (PalGra.HsvEndColor[1] != PalGra.HsvStartColor[1])));

                    newHsvColor.HsvColor[0] += partHsvColor[0];
                }
                while ((Math.Abs(newHsvColor.HsvColor[0] - PalGra.HsvEndColor[0]) >= 0.0001 &&
                                (PalGra.HsvEndColor[0] != PalGra.HsvStartColor[0])));

                BitmapPaletteCopy = new Bitmap(BitmapPalette);
                //Save Bitmap in TempPath
                result = System.IO.Path.GetTempPath();
                saveTemporaryBmp = false;
                for (int i = 1; !saveTemporaryBmp && i < 10; i++)
                {
                    result_bmp = result + i.ToString() + "Palette.bmp";
                    try
                    {
                        BitmapPalette.Save(result_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                        saveTemporaryBmp = true;
                    }
                    catch (Exception ex)
                    {
                        if (i > 8)
                        {
                            System.Windows.MessageBox.Show(i.ToString() + "  Save *.Bmp in temporary path: " + ex.Message);
                        }
                    }
                }
                PalGra.PathTempBitmap = result_bmp;

                //Save Bitmap in TempPath as jpeg
                saveTemporaryJpeg = false;
                for (int i = 1; !saveTemporaryJpeg && i < 10; i++)
                {
                    result_jpeg = result + i.ToString() + "Palette.jpg";
                    try
                    {
                        BitmapPaletteCopy.Save(result_jpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                        saveTemporaryJpeg = true;
                    }
                    catch (Exception ex)
                    {
                        if (i > 8)
                        {
                            System.Windows.MessageBox.Show(i.ToString() + "  Save *.jpg in temporary path: " + ex.Message);
                        }
                    }
                }
                PalGra.PathTempBitmapImage = result_jpeg;

                // Show Bitmap
                if (File.Exists(PalGra.PathTempBitmapImage))
                {
                    ImageColor.BeginInit();
                    ImageColor.Source = new BitmapImage(new Uri(PalGra.PathTempBitmapImage));
                    ImageColor.EndInit();
                    this.Title = "Palette: " + PalGra.PathTempBitmapImage;
                }
            }
        }

        private void PreviewLinear()
        {
            bool saveTemporaryBmp;
            bool saveTemporaryJpeg;

            bool error = PaletteWindowToPaletteGradient();
            if (!error)
            {
                //string for temporary file
                string result = "";                  // temporary path
                string result_bmp = "";              // full name temporary bmp
                string result_jpeg = "";             // full name temporary jpeg

                // Label text start and End Color
                string start;
                string end;
                start = "Startfarbe: " + PalGra.StartColor.ToString();
                end = "Endfarbe: " + PalGra.EndColor.ToString();
                LblStartColor.Content = start;
                LblEndColor.Content = end;

                // Write the pixels in the temp Bitmap
                BitmapPalette = new System.Drawing.Bitmap(PalGra.PixelCounter, 1);

                // first pixel
                PalGra.RGB_StartColor = new System.Drawing.Color();
                PalGra.RGB_StartColor = System.Drawing.Color.FromArgb(
                PalGra.StartColor.A,
                PalGra.StartColor.R,
                PalGra.StartColor.G,
                PalGra.StartColor.B);
                BitmapPalette.SetPixel(0, 0, PalGra.RGB_StartColor);

                // last pixel
                PalGra.RGB_EndColor = new System.Drawing.Color();
                PalGra.RGB_EndColor = System.Drawing.Color.FromArgb(
                PalGra.EndColor.A,
                PalGra.EndColor.R,
                PalGra.EndColor.G,
                PalGra.EndColor.B);
                BitmapPalette.SetPixel(PalGra.PixelCounter - 1, 0, PalGra.RGB_EndColor);

                //gradient pixels
                if (PalGra.PixelCounter > 2)
                {
                    PalGra.LabStartColor = ColorSpace.RGB2Lab(PalGra.RGB_StartColor);
                    PalGra.LabEndColor = ColorSpace.RGB2Lab(PalGra.RGB_EndColor);
                    for (int i = 0; i < 3; i++)
                    {
                        PalGra.LabStepColor[i] = (PalGra.LabEndColor[i] - PalGra.LabStartColor[i]) /
                                                 (Convert.ToDouble(PalGra.PixelCounter) - 1.0);
                        PalGra.LabNewColor[i] = PalGra.LabStartColor[i] + PalGra.LabStepColor[i];
                    }

                    for (int i = 1; i < PalGra.PixelCounter - 1; i++)
                    {
                        PalGra.RGB_NewColor = ColorSpace.Lab2RGB(PalGra.LabNewColor);
                        BitmapPalette.SetPixel(i, 0, PalGra.RGB_NewColor);
                        for (int j = 0; j < 3; j++)
                        {
                            PalGra.LabNewColor[j] = PalGra.LabNewColor[j] + PalGra.LabStepColor[j];
                        }
                    }
                }

                BitmapPaletteCopy = new Bitmap(BitmapPalette);
                //Save Bitmap in TempPath
                result = System.IO.Path.GetTempPath();
                saveTemporaryBmp = false;
                for (int i = 1; !saveTemporaryBmp && i < 10; i++)
                {
                    result_bmp = result + i.ToString() + "Palette.bmp";
                    try
                    {
                        BitmapPalette.Save(result_bmp, System.Drawing.Imaging.ImageFormat.Bmp);
                        saveTemporaryBmp = true;
                    }
                    catch (Exception ex)
                    {
                        if (i > 8)
                        {
                            System.Windows.MessageBox.Show(i.ToString() + "  Save *.Bmp in temporary path: " + ex.Message);
                        }
                    }
                }
                PalGra.PathTempBitmap = result_bmp;

                //Save Bitmap in TempPath as jpeg
                saveTemporaryJpeg = false;
                for (int i = 1; !saveTemporaryJpeg && i < 10; i++)
                {
                    result_jpeg = result + i.ToString() + "Palette.jpg";
                    try
                    {
                        BitmapPaletteCopy.Save(result_jpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                        saveTemporaryJpeg = true;
                    }
                    catch (Exception ex)
                    {
                        if (i > 8)
                        {
                            System.Windows.MessageBox.Show(i.ToString() + "  Save *.jpg in temporary path: " + ex.Message);
                        }
                    }
                }
                PalGra.PathTempBitmapImage = result_jpeg;

                // Show Bitmap
                if (File.Exists(PalGra.PathTempBitmapImage))
                {
                    ImageColor.BeginInit();
                    ImageColor.Source = new BitmapImage(new Uri(PalGra.PathTempBitmapImage));
                    ImageColor.EndInit();
                    this.Title = "Palette: " + PalGra.PathTempBitmapImage;
                }
            }
        }

        /// <summary>
        /// copy the MainWindoes Controls contend to the ProjPage
        /// return error = 0 -> no mistakes in textboxes
        /// </summary>
        private bool PaletteWindowToPaletteGradient()
        {
            bool error = false;

            try
            {
                PalGra.PixelCounter = Convert.ToInt32(TxtBoxPixelCounter.Text);
            }
            catch (Exception ex)

            {
                System.Windows.MessageBox.Show("Convert.ToInt32(TxtBoxPixelCounter.Text) " + ex.Message);
                error = true;
            }

            try
            {
                PalGra.HsvCounter0 = Convert.ToInt32(TxtBoxAnzahlFarbwertHSV.Text);
            }
            catch (Exception ex)

            {
                System.Windows.MessageBox.Show("Convert.ToInt32(TxtBoxAnzahlFarbwertHSV) " + ex.Message);
                error = true;
            }

            try
            {
                PalGra.HsvCounter1 = Convert.ToInt32(TxtBoxAnzahlHellwertHSV.Text);
            }
            catch (Exception ex)

            {
                System.Windows.MessageBox.Show("Convert.ToInt32(TxtBoxAnzahlHellwertHSV) " + ex.Message);
                error = true;
            }

            try
            {
                PalGra.HsvCounter2 = Convert.ToInt32(TxtBoxAnzahSaetigung.Text);
            }
            catch (Exception ex)

            {
                System.Windows.MessageBox.Show("Convert.ToInt32(TxtBoxAnzahSaetigung) " + ex.Message);
                error = true;
            }

            if (RadioBtn3D.IsChecked == true)
            {
                int value = PalGra.HsvCounter0 * PalGra.HsvCounter1 * PalGra.HsvCounter2;
                TxtBoxPixelCounter.Text = value.ToString();
            }

            return error;
        }

        #region IDisposable Support

        // Flag for already disposed
        private bool alreadyDisposed = false;

        // Implementation of IDisposable.
        // Call the virtual Dispose method.
        // Suppress Finalization.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Virtual Dispose method
        protected virtual void Dispose(bool isDisposing)
        {
            // Don't dispose more than once.
            if (alreadyDisposed)
                return;
            if (isDisposing)
            {
                // elided: free managed resources here.
            }

            // elided: free unmanaged resources here.
            // Set disposed flag:
            alreadyDisposed = true;
        }

        #endregion IDisposable Support

        private void PalWinClosed(object sender, EventArgs e)
        {
            BitmapImagePalette = null;  // For the Image in MainWindow
            BitmapPalette = null;                               // For pixel acces
            BitmapPaletteCopy = null;                       // For copy functions
            pathUri = null;

            if (BitmapPalette != null)
            {
                BitmapPalette.Dispose();
            }

            if (BitmapPaletteCopy != null)
            {
                BitmapPaletteCopy.Dispose();
            }

            PalWin.Close();
            PalWin = null;
            this.Dispose();
        }

        private void RadioBtn3DChecked(object sender, RoutedEventArgs e)
        {
            StackPanelFarbwert.Visibility = Visibility.Visible;
            StackPanelHellwert.Visibility = Visibility.Visible;
            StackPanelSaetigung.Visibility = Visibility.Visible;
        }

        private void RadioBtnLinearChecked(object sender, RoutedEventArgs e)
        {
            StackPanelFarbwert.Visibility = Visibility.Hidden;
            StackPanelHellwert.Visibility = Visibility.Hidden;
            StackPanelSaetigung.Visibility = Visibility.Hidden;
        }
    }
}