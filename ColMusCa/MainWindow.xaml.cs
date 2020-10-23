using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Deployment;

namespace ColMusCa
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window

    {
        public Dictionary<string, ImitationColor> DictionaryImitationColor;

        public Dictionary<string, PicturePixelInfos> DictColInfo;

        //Add this value for color count palette
        public const double SAFETY_ADD = 0.02;

        // Picture Original
        public System.Windows.Media.Imaging.BitmapImage BitmapImageOriginal;

        public System.Drawing.Bitmap BitmapOriginal;

        // Picture Original
        public System.Windows.Media.Imaging.BitmapImage BitmapImageImitation;

        public System.Drawing.Bitmap BitmapImitation;

        public System.Drawing.Bitmap BitmapOriginalResize;

        public System.Drawing.Bitmap[] BitmapPalette;

        public System.Windows.Media.SolidColorBrush BrushError = System.Windows.Media.Brushes.Red;

        public System.Windows.Media.SolidColorBrush BrushNoError = System.Windows.Media.Brushes.White;

        public CalculationMode CalcMode;

        // Zuweisung einer anonymen Methode ohne ausführbaren Code
        public System.Action EmptyDelegate = delegate () { };

        // Color Palette from Original picture with distance to palette 0..4 and color counter reduced
        public SortedSet<OriginalColor> OriginalPalette;

        // Color Palette sorted by distance to 0..4
        private SortedSet<OriginalColor> OriginalPaletteChart;

        //Sorted set palettes for calculating and sinus
        public SortedSet<ColorDistanceToWhite>[] palettes;

        //List palettes for calculating and sinus
        public List<ColorDistanceToWhite>[] listPalettes;

        // Palette Window
        public Palette PalWin;

        // BmpPictureInfo Window
        public BmpPictureInfo BmpPictureInfoWin;

        //Palette Grid View 0
        public PaletteDataGrid PalDaGri;

        public Project Proj;

        public ProjectPage ProjPage;

        public WindowBindings WindowRefresh = new WindowBindings
        {
            LabelProgress = "100 %",
            ProgressBarThink = 100.0,
        };

        private const string PATH_IMITATION = "ImitationPictures\\";

        private const string PATH_ORIGINAL = "OrginalPictures\\";

        private const string PATH_PALETTES = "Palettes\\";

        private const string PATH_PROJ_PAGES = "ProjPages\\";

        private const string PATH_DATA_GRID = "DataGrid\\";

        private const string PROJ_NAME = "ColMusCa";

        private double[] correctionOld;

        // For pixel accces
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = WindowRefresh;
            Proj = new Project();

            BitmapPalette = new System.Drawing.Bitmap[5];

            // Create the ProjectPage
            ProjPage = new ProjectPage();

            //Events
            Proj.NameChangedEvent += Proj_NameChangedEvent;
            Proj.PageIndexChangedEvent += Proj_PageIndexChangedEvent;

            correctionOld = new double[5];

            // Palette Window
            PalWin = new Palette();

            MainWindowToProjectPage();
            ProjectCommandsChangeEnabled(false);
        }

        // Calculation mode for color min. distance between original picture and palette 0..4
        public enum CalculationMode
        {
            ColorDistance,
            HSV_H_Distance,
            HSV_S_Distance,
            HSV_V_Distance,
        }

        // For the Image in MainWindow
        private void BtnCalculateClick(object sender, RoutedEventArgs e)
        {
            this.DictionaryImitationColor = new Dictionary<string, ImitationColor>();
            ImitationColor imitationColor = null;
            this.BitmapImitation = new Bitmap(this.BitmapOriginal.Width, this.BitmapOriginal.Height);
            double[] correction = new double[5]
            {
                this.ProjPage.Correction0,
                this.ProjPage.Correction1,
                this.ProjPage.Correction2,
                this.ProjPage.Correction3,
                this.ProjPage.Correction4
            };
            // lost the file acess to the jpg
            this.ImageImitationPicture.BeginInit();
            this.ImageImitationPicture.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/ImageImitationCalcBild.JPG"));
            this.ImageImitationPicture.EndInit();
            System.Drawing.Color pixel;

            // colors in BitmapOriginal
            for (int x = 0; x < this.BitmapOriginal.Width; x++)
            {
                for (int y = 0; y < this.BitmapOriginal.Height; y++)
                {
                    pixel = this.BitmapOriginal.GetPixel(x, y);
                    if (!this.DictionaryImitationColor.ContainsKey(pixel.Name))
                    {
                        // first easy way to get the color
                        // better way is to use the targetIndex
                        // and the relative distance from white for the orginal picture and the palette 0..4
                        // in constructor is now color Pix and PaletteIndex calculate
                        imitationColor = new ImitationColor((int)this.CalcMode, correction, pixel, this.BitmapPalette);
                        imitationColor.Pix = pixel;
                        // This only use for the PaletteIndex for of the target index
                        // TODO TESTEN 28.01.2018
                        this.DictionaryImitationColor.Add(pixel.Name, imitationColor);
                    }
                    // Aktualisierung der ProgressBar ("refresh")
                    this.BitmapImitation.SetPixel(x, y, this.DictionaryImitationColor[pixel.Name].PaletteNewColor);
                    if ((x + 1) * 100 % this.BitmapOriginal.Width <= 100)
                    {
                        this.WindowRefresh.ProgressBarThink = Convert.ToDouble(x * 100 / this.BitmapOriginal.Width);
                        base.Dispatcher.Invoke(this.EmptyDelegate, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
            //Lab-color from item
            double[] lab = new double[3];
            //Lab-color from White Color
            double[] whiteLab = new double[3];
            whiteLab = ColorSpace.RGB2Lab(System.Drawing.Color.White);
            //HSV-color from item
            double[] hsv = new double[3];
            //HSV-color from White Color
            double[] whiteHsv = new double[3];
            whiteHsv = ColorSpace.RGB2HSV(System.Drawing.Color.White);

            double distance = 0.0;

            // sorted set and list for palettes 0..4
            this.palettes = new SortedSet<ColorDistanceToWhite>[5];
            this.listPalettes = new List<ColorDistanceToWhite>[5];
            for (int i = 0; i < 5; i++)
            {
                this.palettes[i] = new SortedSet<ColorDistanceToWhite>(new ComparerDistanceToWhite());
            }

            // sorted set and list for original picture 0..4
            SortedSet<ColorDistanceToWhite>[] original = new SortedSet<ColorDistanceToWhite>[5];
            List<ColorDistanceToWhite>[] listoriginal = new List<ColorDistanceToWhite>[5];

            // j = Palette Bitmap 0 ..4
            for (int i = 0; i < 5; i++)
            {
                original[i] = new SortedSet<ColorDistanceToWhite>(new ComparerDistanceToWhite());
            }
            distance = 0.0;

            for (int j = 0; j < this.BitmapPalette.Length; j++)
            {
                //Calculate the min distance to palette 0..4
                if (this.BitmapPalette[j] != null)
                {
                    for (int x = 0; x < this.BitmapPalette[j].Width; x++)
                    {
                        for (int y = 0; y < this.BitmapPalette[j].Height; y++)
                        {
                            System.Drawing.Color pixel2 = this.BitmapPalette[j].GetPixel(x, y);
                            lab = ColorSpace.RGB2Lab(pixel2);
                            hsv = ColorSpace.RGB2HSV(pixel2);
                            if (this.CalcMode == CalculationMode.ColorDistance)
                            {
                                distance = ColorSpace.ColorDistance2(lab, whiteLab);
                            }
                            if (this.CalcMode == CalculationMode.HSV_H_Distance)
                            {
                                distance = Math.Abs(hsv[0] - whiteHsv[0]);
                            }
                            if (this.CalcMode == CalculationMode.HSV_S_Distance)
                            {
                                distance = Math.Abs(hsv[1] - whiteHsv[1]);
                            }
                            if (this.CalcMode == CalculationMode.HSV_V_Distance)
                            {
                                distance = Math.Abs(hsv[2] - whiteHsv[2]);
                            }
                            ColorDistanceToWhite item = new ColorDistanceToWhite
                            {
                                DistanceToWhite = distance,
                                Pix = pixel2
                            };
                            this.palettes[j].Add(item);
                        }
                    }
                }
            }
            distance = 0.0;

            foreach (KeyValuePair<string, ImitationColor> element in this.DictionaryImitationColor)
            {
                ColorDistanceToWhite item = new ColorDistanceToWhite
                {
                    Pix = element.Value.Pix
                };
                lab = ColorSpace.RGB2Lab(element.Value.Pix);
                hsv = ColorSpace.RGB2HSV(element.Value.Pix);
                if (this.CalcMode == CalculationMode.ColorDistance) // Color-distance
                {
                    distance = ColorSpace.ColorDistance2(lab, whiteLab);
                }
                if (this.CalcMode == CalculationMode.HSV_H_Distance) // Color-distance
                {
                    distance = Math.Abs(hsv[0] - whiteHsv[0]);
                }
                if (this.CalcMode == CalculationMode.HSV_S_Distance) // HSV-Color S (Sätigung)
                {
                    distance = Math.Abs(hsv[1] - whiteHsv[1]);
                }
                if (this.CalcMode == CalculationMode.HSV_V_Distance) // HSV-Color V (Hellwert)
                {
                    distance = Math.Abs(hsv[2] - whiteHsv[2]);
                }
                item.DistanceToWhite = distance;
                original[element.Value.PaletteIndex].Add(item);
            }

            // initialize the  list for palettes and original 0..4
            for (int i = 0; i < 5; i++)
            {
                this.listPalettes[i] = new List<ColorDistanceToWhite>(this.palettes[i].Count);
                listoriginal[i] = new List<ColorDistanceToWhite>(original[i].Count);
                foreach (ColorDistanceToWhite item3 in original[i])
                {
                    listoriginal[i].Add(item3);
                }
                foreach (ColorDistanceToWhite item4 in this.palettes[i])
                {
                    this.listPalettes[i].Add(item4);
                }
            }

            // second exact way to get the color
            // use the targetIndex
            // and the relative distance from white for the orginal picture and the palette 0..4
            this.DictionaryImitationColor = null;
            this.DictionaryImitationColor = new Dictionary<string, ImitationColor>();
            for (int i1 = 0; i1 < 5; i1++)
            {
                for (int i2 = 0; i2 < listoriginal[i1].Count; i2++)
                {
                    imitationColor = new ImitationColor();
                    if (i1 == 0)
                    {
                        imitationColor.PaletteIndex = this.ProjPage.TargetIndex0;
                    }
                    if (i1 == 1)
                    {
                        imitationColor.PaletteIndex = this.ProjPage.TargetIndex1;
                    }
                    if (i1 == 2)
                    {
                        imitationColor.PaletteIndex = this.ProjPage.TargetIndex2;
                    }
                    if (i1 == 3)
                    {
                        imitationColor.PaletteIndex = this.ProjPage.TargetIndex3;
                    }
                    if (i1 == 4)
                    {
                        imitationColor.PaletteIndex = this.ProjPage.TargetIndex4;
                    }
                    int j1 = imitationColor.PaletteIndex;
                    if (this.listPalettes[imitationColor.PaletteIndex].Count != 0)
                    {
                        int best_j2 = 0;
                        best_j2 = i2 * this.listPalettes[j1].Count / listoriginal[i1].Count;
                        imitationColor.OriginalIndex = i1;
                        imitationColor.OriginalListIndex = i2;
                        imitationColor.Pix = listoriginal[i1][i2].Pix;
                        imitationColor.PaletteListIndex = best_j2;
                        imitationColor.PaletteNewColor = this.listPalettes[imitationColor.PaletteIndex][best_j2].Pix;
                        this.DictionaryImitationColor.Add(imitationColor.Pix.Name, imitationColor);
                    }
                }
            }
            for (int x = 0; x < this.BitmapOriginal.Width; x++)
            {
                for (int y = 0; y < this.BitmapOriginal.Height; y++)
                {
                    pixel = this.BitmapOriginal.GetPixel(x, y);
                    if (this.DictionaryImitationColor.ContainsKey(pixel.Name))
                    {
                        System.Drawing.Color paletteNewColor = this.DictionaryImitationColor[pixel.Name].PaletteNewColor;
                        System.Drawing.Color color = this.ImitationPixelWithSinus(x, y, this.DictionaryImitationColor[pixel.Name]);
                        this.BitmapImitation.SetPixel(x, y, color);
                    }
                    else
                    {
                        this.BitmapImitation.SetPixel(x, y, pixel);
                    }
                    if ((x + 1) * 100 % this.BitmapOriginal.Width <= 100)
                    {
                        this.WindowRefresh.ProgressBarThink = Convert.ToDouble(x * 100 / this.BitmapOriginal.Width);
                        base.Dispatcher.Invoke(this.EmptyDelegate, DispatcherPriority.Background);
                    }
                }
            }
            string[] array7 = this.ProjPage.OriginalNameBMP.Split('\\', '.');
            string[] obj = new string[6]
            {
                this.Proj.PathName,
                "ImitationPictures\\",
                null,
                null,
                null,
                null
            };
            int num9 = this.Proj.PageIndex;
            obj[2] = num9.ToString("D5");
            obj[3] = "_";
            obj[4] = array7[array7.Length - 2];
            obj[5] = ".Bmp";
            string text = string.Concat(obj);
            this.ProjPage.ImitationNameBMP = text;
            string[] array8 = this.ProjPage.OriginalNameJpg.Split('\\', '.');
            string[] obj2 = new string[6]
            {
                this.Proj.PathName,
                "ImitationPictures\\",
                null,
                null,
                null,
                null
            };
            num9 = this.Proj.PageIndex;
            obj2[2] = num9.ToString("D5");
            obj2[3] = "_";
            obj2[4] = array8[array8.Length - 2];
            obj2[5] = ".Jpg";
            string text2 = string.Concat(obj2);
            this.ProjPage.ImitationNameJpg = text2;
            this.BitmapImitation.Save(text, ImageFormat.Bmp);
            for (int num10 = 1; num10 <= 3; num10++)
            {
                try
                {
                    this.BitmapImitation.Save(text2, ImageFormat.Jpeg);
                }
                catch (Exception)
                {
                    string[] obj3 = new string[7]
                    {
                        this.Proj.PathName,
                        "ImitationPictures\\",
                        null,
                        null,
                        null,
                        null,
                        null
                    };
                    num9 = this.Proj.PageIndex;
                    obj3[2] = num9.ToString("D5");
                    obj3[3] = "_";
                    obj3[4] = num10.ToString();
                    obj3[5] = array8[array8.Length - 2];
                    obj3[6] = ".Jpg";
                    text2 = string.Concat(obj3);
                }
            }
            this.ProjPage.ImitationSize = this.BitmapImitation.Width + " * " + this.BitmapImitation.Height;
            ProjectPage projPage = this.ProjPage;
            num9 = this.DictionaryImitationColor.Count;
            projPage.ImitationColors = num9.ToString();
            this.ProgressBarThink.Value = 0.0;
            base.Dispatcher.Invoke(this.EmptyDelegate, DispatcherPriority.Background);
            this.ProjectPageSave();
            this.ProjectSave();
            this.ProjectPageToMainWindow();
            if (this.ProjPage.CheckboxPageInfoData == true)
            {
                this.CreateInfoImitation(null, null);
            }
        }

        /// <summary>
        /// Calculate the sinus for the picture
        /// </summary>
        /// <param name="x">x Position from the Pixel</param>
        /// <param name="y">x Position from the Pixel</param>
        /// <param name="paletteNewColor">Color before sinus Calculation</param>
        /// <returns></returns>
        private Color ImitationPixelWithSinus(int x, int y, ImitationColor imitationColor)
        {
            //Todo Hier weitermachen
            int palIndex = imitationColor.PaletteIndex;
            int x_int = imitationColor.PaletteIndex;
            Color col = imitationColor.Pix;
            col = listPalettes[imitationColor.PaletteIndex][imitationColor.PaletteListIndex].Pix;
            //this.listPalettes[imitationColor.PaletteIndex].
            //listPalettes[0].Count

            // IMPORT From FARBFILTER START
            int pos_x = x;
            int pos_y = y;

            bool?[] checkBoxSinus =
            {
                ProjPage.CheckboxSinus0, ProjPage.CheckboxSinus1,
                ProjPage.CheckboxSinus2,ProjPage.CheckboxSinus3,
                ProjPage.CheckboxSinus4, ProjPage.CheckboxSinus5
            };
            bool?[] checkBoxX =
            {
                ProjPage.CheckboxX0, ProjPage.CheckboxX1, ProjPage.CheckboxX2,
                ProjPage.CheckboxX3, ProjPage.CheckboxX4, ProjPage.CheckboxX5
            };
            bool?[] checkBoxY =
            {
                ProjPage.CheckboxY0, ProjPage.CheckboxY1, ProjPage.CheckboxY2,
                ProjPage.CheckboxY3, ProjPage.CheckboxY4, ProjPage.CheckboxY5
            };
            //, bool?[] checkBoxY, ref Picture pic, ref List<Picture> palets)

            Color trCo = Color.White;         //changed color
            int index = -1;
            int pointer = -1;

            bool sinusUsed = false;
            double value = 0;
            double count = 0;
            double maxDistance = Math.Sqrt(Convert.ToDouble(BitmapOriginal.Height) *
                                           Convert.ToDouble(BitmapOriginal.Height)
                                         + Convert.ToDouble(BitmapOriginal.Width) *
                                           Convert.ToDouble(BitmapOriginal.Width));

            double[] X =
            {
                Convert.ToDouble(ProjPage.TextBoxX0), Convert.ToDouble(ProjPage.TextBoxX1),
                Convert.ToDouble(ProjPage.TextBoxX2), Convert.ToDouble(ProjPage.TextBoxX3),
                Convert.ToDouble(ProjPage.TextBoxX4), Convert.ToDouble(ProjPage.TextBoxX5)
            };

            double[] Y =
            {
                Convert.ToDouble(ProjPage.TextBoxY0), Convert.ToDouble(ProjPage.TextBoxY1),
                Convert.ToDouble(ProjPage.TextBoxY2), Convert.ToDouble(ProjPage.TextBoxY3),
                Convert.ToDouble(ProjPage.TextBoxY4), Convert.ToDouble(ProjPage.TextBoxY5)
            };

            double[] A = new double[5];
            double[] B = new double[5];
            double[] C = new double[5];

            double[] frequenz =
            {
                Convert.ToDouble(ProjPage.Frequenz0), Convert.ToDouble(ProjPage.Frequenz1),
                Convert.ToDouble(ProjPage.Frequenz2), Convert.ToDouble(ProjPage.Frequenz3),
                Convert.ToDouble(ProjPage.Frequenz4), Convert.ToDouble(ProjPage.Frequenz5),
            };

            double[] phi =
            {
                Convert.ToDouble(ProjPage.Phi0), Convert.ToDouble(ProjPage.Phi1),
                Convert.ToDouble(ProjPage.Phi2), Convert.ToDouble(ProjPage.Phi3),
                Convert.ToDouble(ProjPage.Phi4), Convert.ToDouble(ProjPage.Phi5),
            };

            double[] Proportion =
            {
                Convert.ToDouble(ProjPage.Proportion0), Convert.ToDouble(ProjPage.Proportion1),
                Convert.ToDouble(ProjPage.Proportion2), Convert.ToDouble(ProjPage.Proportion3),
                Convert.ToDouble(ProjPage.Proportion4), Convert.ToDouble(ProjPage.Proportion5),
            };

            double[] distance_x;
            distance_x = new double[6];
            double[] distance_y;
            distance_y = new double[6];
            double[] sinus_x;
            sinus_x = new double[6];
            double[] sinus_y;
            sinus_y = new double[6];

            index = imitationColor.PaletteIndex;
            pointer = imitationColor.PaletteListIndex;

            //insert the sinus function using paletteArray, pointer and x,y frequenz, Phi
            // y = a * sin (b*x + c)
            // p = 2 * Pi /b => b = 2*Pi /p
            // x = distance
            sinus_y[0] = Convert.ToDouble(pointer);
            for (int i = 0; i < 5; i++)
            {
                //sinus_y[i] = sinus_y[i - 1];
                sinus_y[i] = Convert.ToDouble(pointer);
                if ((bool)checkBoxSinus[i])
                {
                    // calculate the distance
                    if ((bool)checkBoxX[i])
                    {
                        distance_x[i] += X[i] - Convert.ToDouble(pos_x);
                        distance_x[i] *= distance_x[i];
                    }
                    if ((bool)checkBoxY[i])
                    {
                        distance_y[i] += Y[i] - Convert.ToDouble(pos_y);
                        distance_y[i] *= distance_y[i];
                    }
                    sinus_x[i] = Math.Abs(distance_x[i] + distance_y[i]);
                    sinus_x[i] = Math.Sqrt(sinus_x[i]) + phi[i];

                    // calculate the sinus
                    if (i != 0)
                    {
                        A[i] = Convert.ToDouble(listPalettes[imitationColor.PaletteIndex].Count) - 1.0;
                        A[i] = A[i] / 2.0;
                        B[i] = 2.0 * Math.PI / Convert.ToDouble(frequenz[i]);
                        C[i] = 0.0; // Math.Asin(sinus_y[i] - (A[i] / 2.0)) / (A[i] / 2.0) - B[i] * sinus_x[i];
                        sinus_y[i] = A[i] + A[i] * Math.Sin(B[i] * sinus_x[i] + C[i]);
                    }

                    // calculate the Proportion and Attenuation (Dämpfung nicht implementiert)
                    //if (attenuation[i] == 0)
                    //{
                    value += sinus_y[i] * Proportion[i];
                    count += Proportion[i];
                    //}
                    //if (attenuation[i] != 0)
                    //{
                    //    double part1, part2;
                    //    // part1 = color with no sinus,
                    //    // part2 = color with sinus,
                    //    // part1 + part2 = 1;
                    //    // Attenuation in per Cent Distance x
                    //    part1 = 100.0 - Convert.ToDouble(1 / Attenuation[i]) * sinus_x[i] / maxDistance / 100.0;
                    //    part2 = Convert.ToDouble(1 / Attenuation[i]) * sinus_x[i] / maxDistance / 100.0;

                    //    value += (part1 * sinus_y[i] + part2 * sinus_y[0]) * Proportion[i];
                    //    count += Proportion[i];               //* Convert.ToDouble(int.MaxValue - attenuation[i]) / sinus_x[i];
                    //}

                    // one check box is sinus true
                    sinusUsed = true;
                }
            }
            if (sinusUsed)
            {
                pointer = Math.Min(Convert.ToInt32(value / count), (listPalettes[imitationColor.PaletteIndex].Count - 1));
            }

            // End sinus function

            trCo = listPalettes[imitationColor.PaletteIndex][pointer].Pix;

            return trCo;

            // END IMPORT FROM FARBFILTER

            ;
        }

        private void BtnCheckInputsClick(object sender, RoutedEventArgs e)
        {
            ;
            bool error;
            error = MainWindowToProjectPage();
            if (!error)
            {
                ProjectPageSave();
            }
        }

        private void BtnClosePalettes0Click(object sender, RoutedEventArgs e)
        {
            ImagePalettePicture0.BeginInit();
            ImagePalettePicture0.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette0.JPG"));
            ImagePalettePicture0.EndInit();

            BitmapPalette[0] = null;

            ProjPage.PaletteNameBmp0 = "Dateiname 0";
            ProjPage.PaletteNameJpg0 = ImagePalettePicture0.Source.ToString();

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnClosePalettes1Click(object sender, RoutedEventArgs e)
        {
            ImagePalettePicture1.BeginInit();
            ImagePalettePicture1.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette1.JPG"));
            ImagePalettePicture1.EndInit();

            BitmapPalette[1] = null;

            ProjPage.PaletteNameBmp1 = "Dateiname 1";
            ProjPage.PaletteNameJpg1 = ImagePalettePicture1.Source.ToString();

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnClosePalettes2Click(object sender, RoutedEventArgs e)
        {
            ImagePalettePicture2.BeginInit();
            ImagePalettePicture2.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette2.JPG"));
            ImagePalettePicture2.EndInit();

            BitmapPalette[2] = null;

            ProjPage.PaletteNameBmp2 = "Dateiname 2";
            ProjPage.PaletteNameJpg2 = ImagePalettePicture2.Source.ToString();

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnClosePalettes3Click(object sender, RoutedEventArgs e)
        {
            ImagePalettePicture3.BeginInit();
            ImagePalettePicture3.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette3.JPG"));
            ImagePalettePicture3.EndInit();

            BitmapPalette[3] = null;

            ProjPage.PaletteNameBmp3 = "Dateiname 3";
            ProjPage.PaletteNameJpg3 = ImagePalettePicture3.Source.ToString();

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnClosePalettes4Click(object sender, RoutedEventArgs e)
        {
            ImagePalettePicture4.BeginInit();
            ImagePalettePicture4.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette4.JPG"));
            ImagePalettePicture4.EndInit();

            BitmapPalette[4] = null;

            ProjPage.PaletteNameBmp4 = "Dateiname 4";
            ProjPage.PaletteNameJpg4 = ImagePalettePicture4.Source.ToString();

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnColors0Click(object sender, RoutedEventArgs e)
        {
            double itemCorrection = 0.0;
            double itemCorrectionMax = Double.MinValue;
            bool startCalculating = false;

            int ColorCount0Memory = 0;

            //Rescue the old correction for later use if Pin == true
            correctionOld[0] = ProjPage.Correction0;
            correctionOld[1] = ProjPage.Correction1;
            correctionOld[2] = ProjPage.Correction2;
            correctionOld[3] = ProjPage.Correction3;
            correctionOld[4] = ProjPage.Correction4;

            // check the contend of the textbox colorcount0
            try
            {
                ProjPage.ColorCount0 = Convert.ToInt32(textBoxColorCount0.Text);
                ColorCount0Memory = ProjPage.ColorCount0;
                textBoxColorCount0.Background = BrushNoError;
                startCalculating = true;
            }
            catch (Exception ex)
            {
                textBoxColorCount0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxColorCount0.Text) " + ex.Message);
            }

            double[] perCent = new double[5];
            double pixelCount = 0;

            if (startCalculating)
            {
                //Create Palette
                CreatePaletteOriginal();
                CalculatePaletteColorCounters();

                Correction();

                // Create a sorted set by min distance to any palette.
                OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance0());
                foreach (OriginalColor item in OriginalPalette)
                {
                    item.DistanceTo0 += ProjPage.Correction0;

                    // Find minima distance and minima index
                    item.DistanceMin = Double.MaxValue;
                    if (item.DistanceTo0 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo0;
                        item.DistanceMinIndex = 0;
                    }
                    if (item.DistanceTo1 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo1;
                        item.DistanceMinIndex = 1;
                    }
                    if (item.DistanceTo2 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo2;
                        item.DistanceMinIndex = 2;
                    }
                    if (item.DistanceTo3 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo3;
                        item.DistanceMinIndex = 3;
                    }
                    if (item.DistanceTo4 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo4;
                        item.DistanceMinIndex = 4;
                    }

                    OriginalPaletteChart.Add(item);
                }

                ProjPage.ColorCount0 = 0;
                ProjPage.ColorCount1 = 0;
                ProjPage.ColorCount2 = 0;
                ProjPage.ColorCount3 = 0;
                ProjPage.ColorCount4 = 0;

                foreach (OriginalColor item in OriginalPaletteChart)
                {
                    switch (item.DistanceMinIndex)
                    {
                        case 0:
                            ProjPage.ColorCount0++;
                            perCent[0] += item.Count;
                            break;

                        case 1:
                            ProjPage.ColorCount1++;
                            perCent[1] += item.Count;
                            break;

                        case 2:
                            ProjPage.ColorCount2++;
                            perCent[2] += item.Count;
                            break;

                        case 3:
                            ProjPage.ColorCount3++;
                            perCent[3] += item.Count;
                            break;

                        case 4:
                            ProjPage.ColorCount4++;
                            perCent[4] += item.Count;
                            break;

                        default:
                            break;
                    }
                }

                pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
                Convert.ToDouble(BitmapOriginalResize.Height);

                ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
                ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
                ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
                ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
                ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

                // work with the changed color count
                int colCounter;

                colCounter = ColorCount0Memory - 1;
                double Min;
                foreach (OriginalColor item in OriginalPaletteChart)
                {
                    if (colCounter < 0)
                    {
                        break;
                    }

                    //calculate item corretion
                    Min = Math.Min(item.DistanceTo1, item.DistanceTo2);
                    Min = Math.Min(Min, item.DistanceTo3);
                    Min = Math.Min(Min, item.DistanceTo4);
                    itemCorrection = item.DistanceTo0 - Min;

                    //calculate the necessary correction to get the counted color in the text box
                    itemCorrectionMax = Math.Max(itemCorrectionMax, itemCorrection);

                    colCounter--;
                }

                // calculate the new correction0
                itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD;
                ProjPage.Correction0 = itemCorrectionMax;

                //if Pin == true the changing of the color count is not allowed
                if (CheckboxPin1.IsChecked == true)
                {
                    ProjPage.Correction1 = ProjPage.Correction0 - correctionOld[0] + correctionOld[1];
                }
                if (CheckboxPin2.IsChecked == true)
                {
                    ProjPage.Correction2 = ProjPage.Correction0 - correctionOld[0] + correctionOld[2];
                }
                if (CheckboxPin3.IsChecked == true)
                {
                    ProjPage.Correction3 = ProjPage.Correction0 - correctionOld[0] + correctionOld[3];
                }
                if (CheckboxPin4.IsChecked == true)
                {
                    ProjPage.Correction4 = ProjPage.Correction0 - correctionOld[0] + correctionOld[4];
                }

                ProjectPageSave();
                ProjectSave();
                ProjectPageToMainWindow();

                // calculate it new with correction 0
                BtnCorrection0Click(null, null);
                // open the DataGrid
                BtnMinPerCent0Click(null, null);
            }
        }

        private void BtnColors1Click(object sender, RoutedEventArgs e)
        {
            double itemCorrection = 0.0;
            double itemCorrectionMax = Double.MinValue;
            bool startCalculating = false;

            int ColorCount1Memory = 0;

            //Rescue the old correction for later use if Pin == true
            correctionOld[0] = ProjPage.Correction0;
            correctionOld[1] = ProjPage.Correction1;
            correctionOld[2] = ProjPage.Correction2;
            correctionOld[3] = ProjPage.Correction3;
            correctionOld[4] = ProjPage.Correction4;

            // check the contend of the textbox colorcount1
            try
            {
                ProjPage.ColorCount1 = Convert.ToInt32(textBoxColorCount1.Text);
                ColorCount1Memory = ProjPage.ColorCount1;
                textBoxColorCount1.Background = BrushNoError;
                startCalculating = true;
            }
            catch (Exception ex)
            {
                textBoxColorCount1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxColorCount1.Text) " + ex.Message);
            }

            double[] perCent = new double[5];
            double pixelCount = 0;

            if (startCalculating)
            {
                //Create Palette
                CreatePaletteOriginal();
                CalculatePaletteColorCounters();

                Correction();
            }

            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance1());
            foreach (OriginalColor item in OriginalPalette)
            {
                item.DistanceTo1 += ProjPage.Correction1;

                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            ProjPage.ColorCount0 = 0;
            ProjPage.ColorCount1 = 0;
            ProjPage.ColorCount2 = 0;
            ProjPage.ColorCount3 = 0;
            ProjPage.ColorCount4 = 0;

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                switch (item.DistanceMinIndex)
                {
                    case 0:
                        ProjPage.ColorCount0++;
                        perCent[0] += item.Count;
                        break;

                    case 1:
                        ProjPage.ColorCount1++;
                        perCent[1] += item.Count;
                        break;

                    case 2:
                        ProjPage.ColorCount2++;
                        perCent[2] += item.Count;
                        break;

                    case 3:
                        ProjPage.ColorCount3++;
                        perCent[3] += item.Count;
                        break;

                    case 4:
                        ProjPage.ColorCount4++;
                        perCent[4] += item.Count;
                        break;

                    default:
                        break;
                }
            }

            pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
            ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
            ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
            ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
            ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

            // work with the changed color count
            int colCounter;

            colCounter = ColorCount1Memory - 1;
            double Min;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                if (colCounter < 0)
                {
                    break;
                }

                //calculate item correction
                // Suite this in every BtnColor
                Min = Math.Min(item.DistanceTo0, item.DistanceTo2);
                Min = Math.Min(Min, item.DistanceTo3);
                Min = Math.Min(Min, item.DistanceTo4);
                itemCorrection = item.DistanceTo1 - Min;

                //calculate the necessary correction to get the counted color in the text box
                itemCorrectionMax = Math.Max(itemCorrectionMax, itemCorrection);

                colCounter--;
            }

            // calculate the new correction1
            itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD;
            ProjPage.Correction1 = itemCorrectionMax;

            //if Pin == true the changing of the color count is not allowed
            if (CheckboxPin0.IsChecked == true)
            {
                ProjPage.Correction0 = ProjPage.Correction1 - correctionOld[1] + correctionOld[0];
            }
            if (CheckboxPin2.IsChecked == true)
            {
                ProjPage.Correction2 = ProjPage.Correction1 - correctionOld[1] + correctionOld[2];
            }
            if (CheckboxPin3.IsChecked == true)
            {
                ProjPage.Correction3 = ProjPage.Correction1 - correctionOld[1] + correctionOld[3];
            }
            if (CheckboxPin4.IsChecked == true)
            {
                ProjPage.Correction4 = ProjPage.Correction1 - correctionOld[1] + correctionOld[4];
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();

            // calculate it new with correction 1
            BtnCorrection1Click(null, null);
            // open the DataGrid
            BtnMinPerCent1Click(null, null);
        }

        private void BtnColors2Click(object sender, RoutedEventArgs e)
        {
            double itemCorrection = 0.0;
            double itemCorrectionMax = Double.MinValue;
            bool startCalculating = false;

            int ColorCount2Memory = 0;

            //Rescue the old correction for later use if Pin == true
            correctionOld[0] = ProjPage.Correction0;
            correctionOld[1] = ProjPage.Correction1;
            correctionOld[2] = ProjPage.Correction2;
            correctionOld[3] = ProjPage.Correction3;
            correctionOld[4] = ProjPage.Correction4;

            // check the contend of the textbox colorcount2
            try
            {
                ProjPage.ColorCount2 = Convert.ToInt32(textBoxColorCount2.Text);
                ColorCount2Memory = ProjPage.ColorCount2;
                textBoxColorCount2.Background = BrushNoError;
                startCalculating = true;
            }
            catch (Exception ex)
            {
                textBoxColorCount2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxColorCount2.Text) " + ex.Message);
            }

            double[] perCent = new double[5];
            double pixelCount = 0;

            if (startCalculating)
            {
                //Create Palette
                CreatePaletteOriginal();
                CalculatePaletteColorCounters();

                Correction();
            }

            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance2());
            foreach (OriginalColor item in OriginalPalette)
            {
                item.DistanceTo2 += ProjPage.Correction2;

                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            ProjPage.ColorCount0 = 0;
            ProjPage.ColorCount1 = 0;
            ProjPage.ColorCount2 = 0;
            ProjPage.ColorCount3 = 0;
            ProjPage.ColorCount4 = 0;

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                switch (item.DistanceMinIndex)
                {
                    case 0:
                        ProjPage.ColorCount0++;
                        perCent[0] += item.Count;
                        break;

                    case 1:
                        ProjPage.ColorCount1++;
                        perCent[1] += item.Count;
                        break;

                    case 2:
                        ProjPage.ColorCount2++;
                        perCent[2] += item.Count;
                        break;

                    case 3:
                        ProjPage.ColorCount3++;
                        perCent[3] += item.Count;
                        break;

                    case 4:
                        ProjPage.ColorCount4++;
                        perCent[4] += item.Count;
                        break;

                    default:
                        break;
                }
            }

            pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
            ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
            ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
            ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
            ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

            // work with the changed color count
            int colCounter;

            colCounter = ColorCount2Memory - 1;
            double Min;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                if (colCounter < 0)
                {
                    break;
                }

                //calculate item correction
                // Suite this in every BtnColor
                Min = Math.Min(item.DistanceTo0, item.DistanceTo1);
                Min = Math.Min(Min, item.DistanceTo3);
                Min = Math.Min(Min, item.DistanceTo4);
                itemCorrection = item.DistanceTo2 - Min;

                //calculate the necessary correction to get the counted color in the text box
                itemCorrectionMax = Math.Max(itemCorrectionMax, itemCorrection);

                colCounter--;
            }

            // calculate the new correction2
            itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD;
            ProjPage.Correction2 = itemCorrectionMax;

            //if Pin == true the changing of the color count is not allowed
            if (CheckboxPin0.IsChecked == true)
            {
                ProjPage.Correction0 = ProjPage.Correction2 - correctionOld[2] + correctionOld[0];
            }
            if (CheckboxPin1.IsChecked == true)
            {
                ProjPage.Correction1 = ProjPage.Correction2 - correctionOld[2] + correctionOld[1];
            }
            if (CheckboxPin3.IsChecked == true)
            {
                ProjPage.Correction3 = ProjPage.Correction2 - correctionOld[2] + correctionOld[3];
            }
            if (CheckboxPin4.IsChecked == true)
            {
                ProjPage.Correction4 = ProjPage.Correction2 - correctionOld[2] + correctionOld[4];
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();

            // calculate it new with correction 2
            BtnCorrection2Click(null, null);
            // open the DataGrid
            BtnMinPerCent2Click(null, null);
        }

        private void BtnColors3Click(object sender, RoutedEventArgs e)
        {
            double itemCorrection = 0.0;
            double itemCorrectionMax = Double.MinValue;
            bool startCalculating = false;

            int ColorCount3Memory = 0;

            //Rescue the old correction for later use if Pin == true
            correctionOld[0] = ProjPage.Correction0;
            correctionOld[1] = ProjPage.Correction1;
            correctionOld[2] = ProjPage.Correction2;
            correctionOld[3] = ProjPage.Correction3;
            correctionOld[4] = ProjPage.Correction4;

            // check the contend of the textbox colorcount3
            try
            {
                ProjPage.ColorCount3 = Convert.ToInt32(textBoxColorCount3.Text);
                ColorCount3Memory = ProjPage.ColorCount3;
                textBoxColorCount3.Background = BrushNoError;
                startCalculating = true;
            }
            catch (Exception ex)
            {
                textBoxColorCount3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxColorCount3.Text) " + ex.Message);
            }

            double[] perCent = new double[5];
            double pixelCount = 0;

            if (startCalculating)
            {
                //Create Palette
                CreatePaletteOriginal();
                CalculatePaletteColorCounters();

                Correction();
            }

            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance3());
            foreach (OriginalColor item in OriginalPalette)
            {
                item.DistanceTo3 += ProjPage.Correction3;

                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            ProjPage.ColorCount0 = 0;
            ProjPage.ColorCount1 = 0;
            ProjPage.ColorCount2 = 0;
            ProjPage.ColorCount3 = 0;
            ProjPage.ColorCount4 = 0;

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                switch (item.DistanceMinIndex)
                {
                    case 0:
                        ProjPage.ColorCount0++;
                        perCent[0] += item.Count;
                        break;

                    case 1:
                        ProjPage.ColorCount1++;
                        perCent[1] += item.Count;
                        break;

                    case 2:
                        ProjPage.ColorCount2++;
                        perCent[2] += item.Count;
                        break;

                    case 3:
                        ProjPage.ColorCount3++;
                        perCent[3] += item.Count;
                        break;

                    case 4:
                        ProjPage.ColorCount4++;
                        perCent[4] += item.Count;
                        break;

                    default:
                        break;
                }
            }

            pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
            ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
            ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
            ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
            ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

            // work with the changed color count
            int colCounter;

            colCounter = ColorCount3Memory - 1;
            double Min;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                if (colCounter < 0)
                {
                    break;
                }

                //calculate item correction
                // Suite this in every BtnColor
                Min = Math.Min(item.DistanceTo0, item.DistanceTo1);
                Min = Math.Min(Min, item.DistanceTo2);
                Min = Math.Min(Min, item.DistanceTo4);
                itemCorrection = item.DistanceTo3 - Min;

                //calculate the necessary correction to get the counted color in the text box
                itemCorrectionMax = Math.Max(itemCorrectionMax, itemCorrection);

                colCounter--;
            }

            // calculate the new correction3
            itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD;
            ProjPage.Correction3 = itemCorrectionMax;

            //if Pin == true the changing of the color count is not allowed
            if (CheckboxPin0.IsChecked == true)
            {
                ProjPage.Correction0 = ProjPage.Correction3 - correctionOld[3] + correctionOld[0];
            }
            if (CheckboxPin1.IsChecked == true)
            {
                ProjPage.Correction1 = ProjPage.Correction3 - correctionOld[3] + correctionOld[1];
            }
            if (CheckboxPin2.IsChecked == true)
            {
                ProjPage.Correction2 = ProjPage.Correction3 - correctionOld[3] + correctionOld[2];
            }
            if (CheckboxPin4.IsChecked == true)
            {
                ProjPage.Correction4 = ProjPage.Correction3 - correctionOld[3] + correctionOld[4];
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();

            // calculate it new with correction 3
            BtnCorrection3Click(null, null);
            // open the DataGrid
            BtnMinPerCent3Click(null, null);
        }

        private void BtnColors4Click(object sender, RoutedEventArgs e)
        {
            double itemCorrection = 0.0;
            double itemCorrectionMax = Double.MinValue;
            bool startCalculating = false;

            int ColorCount4Memory = 0;

            //Rescue the old correction for later use if Pin == true
            correctionOld[0] = ProjPage.Correction0;
            correctionOld[1] = ProjPage.Correction1;
            correctionOld[2] = ProjPage.Correction2;
            correctionOld[3] = ProjPage.Correction3;
            correctionOld[4] = ProjPage.Correction4;

            // check the contend of the textbox colorcount4
            try
            {
                ProjPage.ColorCount4 = Convert.ToInt32(textBoxColorCount4.Text);
                ColorCount4Memory = ProjPage.ColorCount4;
                textBoxColorCount4.Background = BrushNoError;
                startCalculating = true;
            }
            catch (Exception ex)
            {
                textBoxColorCount4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxColorCount4.Text) " + ex.Message);
            }

            double[] perCent = new double[5];
            double pixelCount = 0;

            if (startCalculating)
            {
                //Create Palette
                CreatePaletteOriginal();
                CalculatePaletteColorCounters();

                Correction();
            }

            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance4());
            foreach (OriginalColor item in OriginalPalette)
            {
                item.DistanceTo4 += ProjPage.Correction4;

                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            ProjPage.ColorCount0 = 0;
            ProjPage.ColorCount1 = 0;
            ProjPage.ColorCount2 = 0;
            ProjPage.ColorCount3 = 0;
            ProjPage.ColorCount4 = 0;

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                switch (item.DistanceMinIndex)
                {
                    case 0:
                        ProjPage.ColorCount0++;
                        perCent[0] += item.Count;
                        break;

                    case 1:
                        ProjPage.ColorCount1++;
                        perCent[1] += item.Count;
                        break;

                    case 2:
                        ProjPage.ColorCount2++;
                        perCent[2] += item.Count;
                        break;

                    case 3:
                        ProjPage.ColorCount3++;
                        perCent[3] += item.Count;
                        break;

                    case 4:
                        ProjPage.ColorCount4++;
                        perCent[4] += item.Count;
                        break;

                    default:
                        break;
                }
            }

            pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
            ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
            ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
            ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
            ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

            // work with the changed color count
            int colCounter;

            colCounter = ColorCount4Memory - 1;
            double Min;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                if (colCounter < 0)
                {
                    break;
                }

                //calculate item correction
                // Suite this in every BtnColor
                Min = Math.Min(item.DistanceTo0, item.DistanceTo1);
                Min = Math.Min(Min, item.DistanceTo2);
                Min = Math.Min(Min, item.DistanceTo3);
                itemCorrection = item.DistanceTo4 - Min;

                //calculate the necessary correction to get the counted color in the text box
                itemCorrectionMax = Math.Max(itemCorrectionMax, itemCorrection);

                colCounter--;
            }

            // calculate the new correction4
            // itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD
            itemCorrectionMax = Math.Round(itemCorrectionMax, 2) + SAFETY_ADD;
            ProjPage.Correction4 = itemCorrectionMax;

            //if Pin == true the changing of the color count is not allowed
            if (CheckboxPin0.IsChecked == true)
            {
                ProjPage.Correction0 = ProjPage.Correction4 - correctionOld[4] + correctionOld[0];
            }
            if (CheckboxPin1.IsChecked == true)
            {
                ProjPage.Correction1 = ProjPage.Correction4 - correctionOld[4] + correctionOld[1];
            }
            if (CheckboxPin2.IsChecked == true)
            {
                ProjPage.Correction2 = ProjPage.Correction4 - correctionOld[4] + correctionOld[2];
            }
            if (CheckboxPin3.IsChecked == true)
            {
                ProjPage.Correction3 = ProjPage.Correction4 - correctionOld[4] + correctionOld[3];
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();

            // calculate it new with correction 4
            BtnCorrection4Click(null, null);
            // open the DataGrid
            BtnMinPerCent4Click(null, null);
        }

        private void BtnCorrection0Click(object sender, RoutedEventArgs e)
        {
            bool startCalculating = true;

            try
            {
                ProjPage.Correction0 = Convert.ToDouble(textBoxCorrection0.Text);
                textBoxCorrection0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection0.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxCorrection0.Text) " + ex.Message);
                startCalculating = false;
            }

            try
            {
                ProjPage.Correction1 = Convert.ToDouble(textBoxCorrection1.Text);
                textBoxCorrection1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection1.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxCorrection1.Text) " + ex.Message);
                startCalculating = false;
            }

            try
            {
                ProjPage.Correction2 = Convert.ToDouble(textBoxCorrection2.Text);
                textBoxCorrection2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection2.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxCorrection2.Text) " + ex.Message);
                startCalculating = false;
            }

            try
            {
                ProjPage.Correction3 = Convert.ToDouble(textBoxCorrection3.Text);
                textBoxCorrection3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection3.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxCorrection3.Text) " + ex.Message);
                startCalculating = false;
            }

            try
            {
                ProjPage.Correction4 = Convert.ToDouble(textBoxCorrection4.Text);
                textBoxCorrection4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection4.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxCorrection4.Text) " + ex.Message);
                startCalculating = false;
            }

            if (startCalculating)
            {
                Correction();
            }
        }

        private void BtnCorrection1Click(object sender, RoutedEventArgs e)
        {
            BtnCorrection0Click(null, null);
        }

        private void BtnCorrection2Click(object sender, RoutedEventArgs e)
        {
            BtnCorrection0Click(null, null);
        }

        private void BtnCorrection3Click(object sender, RoutedEventArgs e)
        {
            BtnCorrection0Click(null, null);
        }

        private void BtnCorrection4Click(object sender, RoutedEventArgs e)
        {
            BtnCorrection0Click(null, null);
        }

        /// <summary>
        /// Original Picture Colors with distance to palette 0..4 without Distance correction and color reducing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateInfoOriginalClick(object sender, RoutedEventArgs e)
        {
            CreateInfoOriginal(null, null);
            CreateInfoImitation(null, null);
        }

        private void BtnCreatePaletteOriginalClick(object sender, RoutedEventArgs e)
        {
            CreatePaletteOriginal();
            CalculatePaletteColorCounters();
            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnDeletePageClick(object sender, RoutedEventArgs e)
        {
            string path = Proj.PathName + PATH_PROJ_PAGES + Proj.PageIndex.ToString() + "ProjPage.xls";
            string oldPath;
            string newPath;
            String folder = Proj.PathName + PATH_PROJ_PAGES;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
                                Convert.ToDouble(BitmapOriginalResize.Height);

            if (Proj.PageCounter > 1)
            {
                File.Delete(path);

                // Rename the other files
                int j;
                for (int i = Proj.PageIndex + 1; i < Proj.PageCounter; i++)
                {
                    j = i - 1;
                    oldPath = Proj.PathName + PATH_PROJ_PAGES + i.ToString() + "ProjPage.xls";
                    newPath = Proj.PathName + PATH_PROJ_PAGES + j.ToString() + "ProjPage.xls";
                    File.Move(oldPath, newPath);
                }

                DirectoryInfo di = new DirectoryInfo(folder);
                Proj.PageCounter = di.GetFiles().Length;

                if (Proj.PageIndex > 0)
                {
                    Proj.PageIndex--;
                }
            }
            ProjectPageToMainWindow();
            ProjectChanged();
        }

        private void BtnInputsResetClick(object sender, RoutedEventArgs e)
        {
            ProjectPageOpen();
            ProjectPageToMainWindow();
            ProjectChanged();
        }

        private void BtnMinPerCent0Click(object sender, RoutedEventArgs e)
        {
            const string FILE_NAME_TXT = "DataGrid0.txt";
            const string DATA_GRID_HEAD = ("Number Color Pix.A Pix.R Pix.G Pix.B Count DistanceMin DistanceMinIndex DistanceTo0 DistanceTo1 DistanceTo2  DistanceTo3 DistanceTo4");
            string pathDataGrid = Proj.PathName + PATH_DATA_GRID + FILE_NAME_TXT;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            try
            {
                ProjPage.PerCent0 = Convert.ToDouble(textBoxPerCent0.Text);
                textBoxPerCent0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxPerCent0.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxPerCent0) " + ex.Message);
            }

            if (OriginalPaletteChart == null)
            {
                BtnColors0Click(null, null);
            }

            //Calculating with new correction the Original Palette Chart
            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = null;
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance0());
            foreach (OriginalColor item in OriginalPalette)
            {
                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            StreamWriter sw = new StreamWriter(pathDataGrid);
            sw.WriteLine(DATA_GRID_HEAD);

            int number = 1;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                sw.WriteLine(number + " " + item.Pix.Name + " " + item.Pix.A + " " + item.Pix.R + " " + item.Pix.G + " " + item.Pix.B + " " +
                             item.Count + " " + item.DistanceMin + " " + item.DistanceMinIndex + " " +
                             item.DistanceTo0 + " " + item.DistanceTo1 + " " + item.DistanceTo2 + " " +
                             item.DistanceTo3 + " " + item.DistanceTo4);
                number++;
            }
            sw.Close();

            PalDaGri = new PaletteDataGrid(OriginalPaletteChart, pixelCount);
            PalDaGri.Show();
        }

        private void BtnMinPerCent1Click(object sender, RoutedEventArgs e)
        {
            const string FILE_NAME_TXT = "DataGrid1.txt";
            const string DATA_GRID_HEAD = ("Number Color Pix.A Pix.R Pix.G Pix.B Count DistanceMin DistanceMinIndex DistanceTo0 DistanceTo1 DistanceTo2  DistanceTo3 DistanceTo4");
            string pathDataGrid = Proj.PathName + PATH_DATA_GRID + FILE_NAME_TXT;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            try
            {
                ProjPage.PerCent1 = Convert.ToDouble(textBoxPerCent1.Text);
                textBoxPerCent1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxPerCent1.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxPerCent1) " + ex.Message);
            }

            if (OriginalPaletteChart == null)
            {
                BtnColors1Click(null, null);
            }

            //Calculating with new correction the Original Palette Chart
            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = null;
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance1());
            foreach (OriginalColor item in OriginalPalette)
            {
                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            StreamWriter sw = new StreamWriter(pathDataGrid);
            sw.WriteLine(DATA_GRID_HEAD);

            int number = 1;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                sw.WriteLine(number + " " + item.Pix.Name + " " + item.Pix.A + " " + item.Pix.R + " " + item.Pix.G + " " + item.Pix.B + " " +
                             item.Count + " " + item.DistanceMin + " " + item.DistanceMinIndex + " " +
                             item.DistanceTo0 + " " + item.DistanceTo1 + " " + item.DistanceTo2 + " " +
                             item.DistanceTo3 + " " + item.DistanceTo4);
                number++;
            }
            sw.Close();

            PalDaGri = new PaletteDataGrid(OriginalPaletteChart, pixelCount);
            PalDaGri.Show();
        }

        private void BtnMinPerCent2Click(object sender, RoutedEventArgs e)
        {
            const string FILE_NAME_TXT = "DataGrid2.txt";
            const string DATA_GRID_HEAD = ("Number Color Pix.A Pix.R Pix.G Pix.B Count DistanceMin DistanceMinIndex DistanceTo0 DistanceTo1 DistanceTo2  DistanceTo3 DistanceTo4");
            string pathDataGrid = Proj.PathName + PATH_DATA_GRID + FILE_NAME_TXT;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            try
            {
                ProjPage.PerCent2 = Convert.ToDouble(textBoxPerCent2.Text);
                textBoxPerCent2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxPerCent2.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxPerCent2) " + ex.Message);
            }

            if (OriginalPaletteChart == null)
            {
                BtnColors2Click(null, null);
            }

            //Calculating with new correction the Original Palette Chart
            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = null;
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance2());
            foreach (OriginalColor item in OriginalPalette)
            {
                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            StreamWriter sw = new StreamWriter(pathDataGrid);
            sw.WriteLine(DATA_GRID_HEAD);

            int number = 1;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                sw.WriteLine(number + " " + item.Pix.Name + " " + item.Pix.A + " " + item.Pix.R + " " + item.Pix.G + " " + item.Pix.B + " " +
                             item.Count + " " + item.DistanceMin + " " + item.DistanceMinIndex + " " +
                             item.DistanceTo0 + " " + item.DistanceTo1 + " " + item.DistanceTo2 + " " +
                             item.DistanceTo3 + " " + item.DistanceTo4);
                number++;
            }
            sw.Close();

            PalDaGri = new PaletteDataGrid(OriginalPaletteChart, pixelCount);
            PalDaGri.Show();
        }

        private void BtnMinPerCent3Click(object sender, RoutedEventArgs e)
        {
            const string FILE_NAME_TXT = "DataGrid3.txt";
            const string DATA_GRID_HEAD = ("Number Color Pix.A Pix.R Pix.G Pix.B Count DistanceMin DistanceMinIndex DistanceTo0 DistanceTo1 DistanceTo2  DistanceTo3 DistanceTo4");
            string pathDataGrid = Proj.PathName + PATH_DATA_GRID + FILE_NAME_TXT;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            try
            {
                ProjPage.PerCent3 = Convert.ToDouble(textBoxPerCent3.Text);
                textBoxPerCent3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxPerCent3.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxPerCent3) " + ex.Message);
            }

            if (OriginalPaletteChart == null)
            {
                BtnColors3Click(null, null);
            }

            //Calculating with new correction the Original Palette Chart
            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = null;
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance3());
            foreach (OriginalColor item in OriginalPalette)
            {
                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            StreamWriter sw = new StreamWriter(pathDataGrid);
            sw.WriteLine(DATA_GRID_HEAD);

            int number = 1;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                sw.WriteLine(number + " " + item.Pix.Name + " " + item.Pix.A + " " + item.Pix.R + " " + item.Pix.G + " " + item.Pix.B + " " +
                             item.Count + " " + item.DistanceMin + " " + item.DistanceMinIndex + " " +
                             item.DistanceTo0 + " " + item.DistanceTo1 + " " + item.DistanceTo2 + " " +
                             item.DistanceTo3 + " " + item.DistanceTo4);
                number++;
            }
            sw.Close();

            PalDaGri = new PaletteDataGrid(OriginalPaletteChart, pixelCount);
            PalDaGri.Show();
        }

        private void BtnMinPerCent4Click(object sender, RoutedEventArgs e)
        {
            const string FILE_NAME_TXT = "DataGrid4.txt";
            const string DATA_GRID_HEAD = ("Number Color Pix.A Pix.R Pix.G Pix.B Count DistanceMin DistanceMinIndex DistanceTo0 DistanceTo1 DistanceTo2  DistanceTo3 DistanceTo4");
            string pathDataGrid = Proj.PathName + PATH_DATA_GRID + FILE_NAME_TXT;

            double pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            try
            {
                ProjPage.PerCent4 = Convert.ToDouble(textBoxPerCent4.Text);
                textBoxPerCent4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxPerCent4.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxPerCent4) " + ex.Message);
            }

            if (OriginalPaletteChart == null)
            {
                BtnColors4Click(null, null);
            }

            //Calculating with new correction the Original Palette Chart
            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = null;
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistance4());
            foreach (OriginalColor item in OriginalPalette)
            {
                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            StreamWriter sw = new StreamWriter(pathDataGrid);
            sw.WriteLine(DATA_GRID_HEAD);

            int number = 1;
            foreach (OriginalColor item in OriginalPaletteChart)
            {
                sw.WriteLine(number + " " + item.Pix.Name + " " + item.Pix.A + " " + item.Pix.R + " " + item.Pix.G + " " + item.Pix.B + " " +
                             item.Count + " " + item.DistanceMin + " " + item.DistanceMinIndex + " " +
                             item.DistanceTo0 + " " + item.DistanceTo1 + " " + item.DistanceTo2 + " " +
                             item.DistanceTo3 + " " + item.DistanceTo4);
                number++;
            }
            sw.Close();

            PalDaGri = new PaletteDataGrid(OriginalPaletteChart, pixelCount);
            PalDaGri.Show();
        }

        private void BtnNewPageClick(object sender, RoutedEventArgs e)
        {
            ProjectPageSave();
            ProjectPageCopy();
            Proj.PageCounter++;
            Proj.PageIndex = Proj.PageCounter - 1;
            ProjectPageToMainWindow();
            ProjectChanged();
        }

        private void BtnNextPageClick(object sender, RoutedEventArgs e)
        {
            bool error;
            error = MainWindowToProjectPage();
            if (!error)
            {
                ProjectPageSave();

                if (Proj.PageIndex < Proj.PageCounter - 1)
                {
                    Proj.PageIndex++;
                }
                else
                {
                    Proj.PageIndex = 0;
                }
                ProjectPageOpen();
                ProjectPageToMainWindow();
                ProjectChanged();
            }
        }

        private void BtnOpenPalettes0Click(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string fileJpg = "";

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Palette 0", // Default file name
                                        //dlg.DefaultExt = ".bmp"; // Default file extension
                                        //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.BMP) | *.BMP"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.PaletteNameBmp0 = dlg.FileName;
                BitmapPalette[0] = new Bitmap(ProjPage.PaletteNameBmp0);
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('.');
                fileJpg = arrayPath[0] + ".Jpg";
                ProjPage.PaletteNameBmp0 = dlg.FileName;

                if (File.Exists(fileJpg))
                {
                    ProjPage.PaletteNameJpg0 = fileJpg;
                }
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnOpenPalettes1Click(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string fileJpg = "";

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Palette 1", // Default file name
                                        //dlg.DefaultExt = ".bmp"; // Default file extension
                                        //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.BMP) | *.BMP"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.PaletteNameBmp1 = dlg.FileName;
                BitmapPalette[1] = new Bitmap(ProjPage.PaletteNameBmp1);
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('.');
                fileJpg = arrayPath[0] + ".Jpg";
                ProjPage.PaletteNameBmp1 = dlg.FileName;

                if (File.Exists(fileJpg))
                {
                    ProjPage.PaletteNameJpg1 = fileJpg;
                }
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnOpenPalettes2Click(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string fileJpg = "";

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Palette 2", // Default file name
                                        //dlg.DefaultExt = ".bmp"; // Default file extension
                                        //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.BMP) | *.BMP"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.PaletteNameBmp2 = dlg.FileName;
                BitmapPalette[2] = new Bitmap(ProjPage.PaletteNameBmp2);
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('.');
                fileJpg = arrayPath[0] + ".Jpg";
                ProjPage.PaletteNameBmp2 = dlg.FileName;

                if (File.Exists(fileJpg))
                {
                    ProjPage.PaletteNameJpg2 = fileJpg;
                }
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnOpenPalettes3Click(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string fileJpg = "";

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Palette 3", // Default file name
                                        //dlg.DefaultExt = ".bmp"; // Default file extension
                                        //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.BMP) | *.BMP"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.PaletteNameBmp3 = dlg.FileName;
                BitmapPalette[3] = new Bitmap(ProjPage.PaletteNameBmp3);
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('.');
                fileJpg = arrayPath[0] + ".Jpg";
                ProjPage.PaletteNameBmp3 = dlg.FileName;

                if (File.Exists(fileJpg))
                {
                    ProjPage.PaletteNameJpg3 = fileJpg;
                }
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnOpenPalettes4Click(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string fileJpg = "";

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Palette 4", // Default file name
                                        //dlg.DefaultExt = ".bmp"; // Default file extension
                                        //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.BMP) | *.BMP"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.PaletteNameBmp4 = dlg.FileName;
                BitmapPalette[4] = new Bitmap(ProjPage.PaletteNameBmp4);
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('.');
                fileJpg = arrayPath[0] + ".Jpg";
                ProjPage.PaletteNameBmp4 = dlg.FileName;

                if (File.Exists(fileJpg))
                {
                    ProjPage.PaletteNameJpg4 = fileJpg;
                }
            }
            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void BtnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            bool error;
            error = MainWindowToProjectPage();
            if (!error)
            {
                ProjectPageSave();
                if (Proj.PageIndex > 0)
                {
                    Proj.PageIndex--;
                }
                else
                {
                    Proj.PageIndex = Proj.PageCounter - 1;
                }
                ProjectPageOpen();
                ProjectPageToMainWindow();
                ProjectChanged();
            }
        }

        /// this method change only the  counter only in original palette
        /// do not calculate the correction
        /// the correrection must be calculate in original palette chard!!!!
        private void CalculatePaletteColorCounters()
        {
            Dictionary<string, OriginalColor> dictionary =
            new Dictionary<string, OriginalColor>();

            int a, r, g, b;
            Color item1;

            foreach (OriginalColor element in OriginalPalette)
            {
                element.Count = 0;
                dictionary.Add(element.Pix.Name, element);
            }

            //Pixel Counter
            //outside pixel from x,y
            for (int x1 = 0; x1 < BitmapOriginalResize.Width; x1++)
            {
                for (int y1 = 0; y1 < BitmapOriginalResize.Height; y1++)
                {
                    item1 = BitmapOriginalResize.GetPixel(x1, y1);
                    //Resize color count
                    a = item1.A;
                    r = item1.R / ProjPage.ColorReduce;
                    r = r * ProjPage.ColorReduce;
                    g = item1.G / ProjPage.ColorReduce;
                    g = g * ProjPage.ColorReduce;
                    b = item1.B / ProjPage.ColorReduce;
                    b = b * ProjPage.ColorReduce;
                    item1 = new Color();
                    item1 = Color.FromArgb(a, r, g, b);

                    // See whether Dictionary contains this string.
                    if (dictionary.ContainsKey(item1.Name))
                    {
                        OriginalColor value = dictionary[item1.Name];
                        value.Count++;
                        dictionary.Remove(item1.Name);
                        dictionary.Add(item1.Name, value);
                    }
                }
            }
            // Refresh  OriginalPalette with changed counters
            OriginalPalette = null;
            OriginalPalette = new SortedSet<OriginalColor>(new PixelComparerRGBA());

            foreach (KeyValuePair<string, OriginalColor> pair in dictionary)
            {
                OriginalPalette.Add(pair.Value);
            }
        }

        private void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            String command, targetobj;
            command = ((RoutedCommand)e.Command).Name;
            targetobj = ((FrameworkElement)target).Name;
            //MessageBox.Show("The " + command + " command has been invoked on target object " + targetobj);
        }

        private void CloseFileOriginal(object sender, RoutedEventArgs e)
        {
        }

        private void CloseFilePalette(object sender, RoutedEventArgs e)
        {
        }

        /// calculate correction 0.4
        private void Correction()
        {
            double[] perCent = new double[5];
            double pixelCount = 0;

            SortedSet<OriginalColor> OriginalPaletteChart;

            //Create Palette
            CreatePaletteOriginal();
            CalculatePaletteColorCounters();

            // Create a sorted set by min distance to any palette.
            OriginalPaletteChart = new SortedSet<OriginalColor>(new PixelComparerDistanceMin());
            foreach (OriginalColor item in OriginalPalette)
            {
                item.DistanceTo0 -= ProjPage.Correction0;
                item.DistanceTo1 -= ProjPage.Correction1;
                item.DistanceTo2 -= ProjPage.Correction2;
                item.DistanceTo3 -= ProjPage.Correction3;
                item.DistanceTo4 -= ProjPage.Correction4;

                // Find minima distance and minima index
                item.DistanceMin = Double.MaxValue;
                if (item.DistanceTo0 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo0;
                    item.DistanceMinIndex = 0;
                }
                if (item.DistanceTo1 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo1;
                    item.DistanceMinIndex = 1;
                }
                if (item.DistanceTo2 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo2;
                    item.DistanceMinIndex = 2;
                }
                if (item.DistanceTo3 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo3;
                    item.DistanceMinIndex = 3;
                }
                if (item.DistanceTo4 < item.DistanceMin)
                {
                    item.DistanceMin = item.DistanceTo4;
                    item.DistanceMinIndex = 4;
                }

                OriginalPaletteChart.Add(item);
            }

            ProjPage.ColorCount0 = 0;
            ProjPage.ColorCount1 = 0;
            ProjPage.ColorCount2 = 0;
            ProjPage.ColorCount3 = 0;
            ProjPage.ColorCount4 = 0;

            foreach (OriginalColor item in OriginalPaletteChart)
            {
                switch (item.DistanceMinIndex)
                {
                    case 0:
                        ProjPage.ColorCount0++;
                        perCent[0] += item.Count;
                        break;

                    case 1:
                        ProjPage.ColorCount1++;
                        perCent[1] += item.Count;
                        break;

                    case 2:
                        ProjPage.ColorCount2++;
                        perCent[2] += item.Count;
                        break;

                    case 3:
                        ProjPage.ColorCount3++;
                        perCent[3] += item.Count;
                        break;

                    case 4:
                        ProjPage.ColorCount4++;
                        perCent[4] += item.Count;
                        break;

                    default:
                        break;
                }
            }

            pixelCount = Convert.ToDouble(BitmapOriginalResize.Width) *
            Convert.ToDouble(BitmapOriginalResize.Height);

            ProjPage.PerCent0 = perCent[0] * 100 / pixelCount;
            ProjPage.PerCent1 = perCent[1] * 100 / pixelCount;
            ProjPage.PerCent2 = perCent[2] * 100 / pixelCount;
            ProjPage.PerCent3 = perCent[3] * 100 / pixelCount;
            ProjPage.PerCent4 = perCent[4] * 100 / pixelCount;

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void CreateInfoImitation(object sender, RoutedEventArgs e)
        {
            System.Drawing.Color copy_color;

            // value for counting pixels
            int pixel_count = 0;

            Double[] lab = new Double[3];
            Double[] hsv = new Double[3];

            Bitmap Bm_copy;
            Bitmap Bm_copy1;
            Bitmap Bm_copy2;

            // Create a subdirectory
            string text_target;
            string text_target1;
            string text_target2;
            string[] path = ProjPage.ImitationNameBMP.Split('.');
            string[] parts = ProjPage.ImitationNameBMP.Split('.', '\\');
            string[] path1 = ProjPage.ImitationNameBMP.Split('.');
            string[] parts1 = ProjPage.ImitationNameBMP.Split('.', '\\');
            string[] path2 = ProjPage.ImitationNameBMP.Split('.');
            string[] parts2 = ProjPage.ImitationNameBMP.Split('.', '\\');

            Directory.CreateDirectory(path[0]);

            // file name from *.txt
            text_target = path[0] + "\\" + parts[parts.Length - 2] + ".txt";
            text_target1 = path1[0] + "\\" + parts1[parts1.Length - 2] + ".txt";
            text_target2 = path2[0] + "\\" + parts2[parts2.Length - 2] + ".txt";

            FileStream fs;
            StreamWriter sw;

            try
            {
                fs = new FileStream(text_target, FileMode.Create);
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                sw = new StreamWriter(fs);
            }
            catch (Exception ex)
            {
                throw;
            }

            // first Line
            // FARBE;ROT;GELB;BLAU;BLAU;HSV-FARBWERT;HSV-SÄTIGUNG;HSV-HELLWERT;LAB L;LAB A;LAB B;Distance fom White;PIXEL"
            sw.WriteLine("FARBE;ROT;GELB;BLAU;HSV-FARBWERT;HSV-SÄTIGUNG;HSV-HELLWERT;LAB L;LAB A;LAB B;Distance from White;PIXEL");

            double count = 0;

            //PictureColors[] picArray = new PictureColors[Pic.Palette.Count];
            //Pic.Palette.CopyTo(picArray);

            // Todo PaletteNewColor verwenden!
            foreach (KeyValuePair<string, ImitationColor> s in this.DictionaryImitationColor)

            {
                // Distance from white
                double distance = 0;

                // value for counting pixels
                pixel_count = 0;

                string target = path[0] + "\\" + s.Value.PaletteNewColor.Name + "_Color" + parts[parts.Length - 2] + "." + "JPG";
                string target1 = path1[0] + "\\" + s.Value.Pix.Name + "_Black" + parts1[parts1.Length - 2] + "." + "JPG";
                string target2 = path1[0] + "\\" + s.Value.Pix.Name + "_White" + parts1[parts1.Length - 2] + "." + "JPG";
                // File.Copy(Pic.Path, target);

                Bm_copy = new Bitmap(BitmapImitation.Width, BitmapImitation.Height);
                Bm_copy1 = new Bitmap(BitmapImitation.Width, BitmapImitation.Height);
                Bm_copy2 = new Bitmap(BitmapImitation.Width, BitmapImitation.Height);

                // Get the color of a pixel
                // Double Max_Pr = Convert.ToDouble(Bm_copy.Width) * Convert.ToDouble(Bm_copy.Height) * Pic.Palette.Count;

                // drawing in the copy Bitmap
                double i = 1.0;
                for (int x = 0; x < Bm_copy.Width; x++)
                {
                    for (int y = 0; y < Bm_copy.Height; y++)
                    {
                        // Code for drawing
                        System.Drawing.Color org_color = BitmapImitation.GetPixel(x, y);
                        if (string.Equals(s.Value.Pix.Name, BitmapImitation.GetPixel(x, y).Name))
                        {
                            copy_color = System.Drawing.Color.FromArgb(org_color.A, org_color.R, org_color.G, org_color.B);
                            Bm_copy.SetPixel(x, y, copy_color);
                            Bm_copy1.SetPixel(x, y, System.Drawing.Color.Black);
                            Bm_copy2.SetPixel(x, y, System.Drawing.Color.White);
                            // count the pixels
                            ++pixel_count;
                        }
                        else
                        {
                            Bm_copy.SetPixel(x, y, System.Drawing.Color.White);
                            Bm_copy1.SetPixel(x, y, System.Drawing.Color.White);
                            Bm_copy2.SetPixel(x, y, System.Drawing.Color.Black);
                        }

                        // ProgressBarThink.Value = Math.Round(100.0 * (++count) / Max_Pr, 0);
                        ProgressBarThink.Value = 100;

                        if (ProgressBarThink.Value >= i)
                        {
                            // Aktualisierung der ProgressBar ("refresh")
                            ProgressBarThink.Dispatcher.Invoke(
                            EmptyDelegate,
                            System.Windows.Threading.DispatcherPriority.Background
                            );
                            i += 1;
                        }
                       ;
                    }
                }
                Bm_copy.Save(target, ImageFormat.Jpeg);
                Bm_copy1.Save(target1, ImageFormat.Jpeg);
                Bm_copy2.Save(target2, ImageFormat.Jpeg);

                lab = ColorSpace.RGB2Lab(s.Value.Pix);
                hsv = ColorSpace.RGB2HSV(s.Value.Pix);

                distance = ColorSpace.ColorDistance2(lab, ColorSpace.RGB2Lab(System.Drawing.Color.White));

                // writing the color in the info-file
                // "FARBE;ROT;GELB;BLAU;BLAU;HSV-FARBWERT;HSV-SÄTIGUNG;HSV-HELLWERT;LAB L;LAB A;LAB B;Distance fom White;PIXEL"
                sw.WriteLine(s.Value.Pix.Name + ";" + s.Value.Pix.R + ";" + s.Value.Pix.G + ";" + s.Value.Pix.B + ";" + hsv[0] + ";" + hsv[1] + ";" + hsv[2] + ";" + lab[0] + ";" + lab[1] + ";" + lab[2] + ";" + distance + ";" + pixel_count);
            }
            ProgressBarThink.Value = 0;
            // Close the text file
            sw.Close();

            // CreateFarbbilder
            count = 0;
            foreach (KeyValuePair<string, ImitationColor> s in this.DictionaryImitationColor)
            {
                string target = path[0] + "\\" + s.Value.Pix.Name + "." + path[1];
                // File.Copy(Pic.Path, target);

                Bm_copy = new Bitmap(600, 400);

                // Get the color of a pixel
                Double Max_Pr = Convert.ToDouble(Bm_copy.Width) * Convert.ToDouble(Bm_copy.Height);// * Pic.Palette.Count;

                // drawing in the copy Bitmap
                double i = 1.0;
                for (int x = 0; x < Bm_copy.Width; x++)
                {
                    for (int y = 0; y < Bm_copy.Height; y++)
                    {
                        // Code for drawing

                        copy_color = System.Drawing.Color.FromArgb(s.Value.Pix.A, s.Value.Pix.R, s.Value.Pix.G, s.Value.Pix.B);
                        Bm_copy.SetPixel(x, y, copy_color);

                        //ProgressBarThink.Value = Math.Round(100.0 * (++count) / Max_Pr, 0);
                        //if (ProgressBarThink.Value >= i)
                        //{
                        //    // Aktualisierung der ProgressBar ("refresh")
                        //    ProgressBarThink.Dispatcher.Invoke(
                        //        EmptyDelegate,
                        //        System.Windows.Threading.DispatcherPriority.Background
                        //        );
                        //    i += 1;
                        //}
                        // Aktualisierung der ProgressBar ("refresh")
                        this.BitmapImitation.SetPixel(x, y, this.DictionaryImitationColor[s.Value.Pix.Name].PaletteNewColor);

                        if ((x + 1) * 100 % this.BitmapOriginal.Width <= 100)
                        {
                            this.WindowRefresh.ProgressBarThink = Convert.ToDouble(x * 100 / this.BitmapOriginal.Width);
                            base.Dispatcher.Invoke(this.EmptyDelegate, System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                }
                Bm_copy.Save(target);
            }
            ProgressBarThink.Value = 0;

            sw.Close();
            fs.Close();
        }

        private void CreateInfoOriginal(object sender, RoutedEventArgs e)
        {
        }

        private void CreatePaletteOriginal()
        {
            bool newItem;

            OriginalColor item = new OriginalColor();

            int a, r, g, b;

            string[] fileNames ={ProjPage.PaletteNameBmp0,
                                    ProjPage.PaletteNameBmp1,
                                    ProjPage.PaletteNameBmp2,
                                    ProjPage.PaletteNameBmp3,
                                    ProjPage.PaletteNameBmp4};

            // Create a sorted set using the PixelComparer.
            OriginalPalette = new SortedSet<OriginalColor>(new PixelComparerRGBA());

            // Reduce colors in BitmapOriginalResize
            for (int x = 0; x < BitmapOriginalResize.Width; x++)
            {
                for (int y = 0; y < BitmapOriginalResize.Height; y++)
                {
                    item = new OriginalColor
                    {
                        Pix = BitmapOriginalResize.GetPixel(x, y),
                        Count = 0
                    };

                    //Resize color count
                    a = item.Pix.A;
                    r = item.Pix.R / ProjPage.ColorReduce;
                    r = r * ProjPage.ColorReduce;
                    g = item.Pix.G / ProjPage.ColorReduce;
                    g = g * ProjPage.ColorReduce;
                    b = item.Pix.B / ProjPage.ColorReduce;
                    b = b * ProjPage.ColorReduce;
                    item.Pix = Color.FromArgb(a, r, g, b);

                    // ref item is the return value
                    OriginalManipulate.CalcDistances((int)CalcMode, ref item, BitmapPalette);

                    // Find minima distance and minima index
                    item.DistanceMin = Double.MaxValue;
                    if (item.DistanceTo0 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo0;
                        item.DistanceMinIndex = 0;
                    }
                    if (item.DistanceTo1 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo1;
                        item.DistanceMinIndex = 1;
                    }
                    if (item.DistanceTo2 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo2;
                        item.DistanceMinIndex = 2;
                    }
                    if (item.DistanceTo3 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo3;
                        item.DistanceMinIndex = 3;
                    }
                    if (item.DistanceTo4 < item.DistanceMin)
                    {
                        item.DistanceMin = item.DistanceTo4;
                        item.DistanceMinIndex = 4;
                    }

                    newItem = OriginalPalette.Add(item);

                    // Aktualisierung der ProgressBar ("refresh")
                    if ((((x + 1) * 100) % BitmapOriginalResize.Width) <= 100)
                    {
                        WindowRefresh.ProgressBarThink = Convert.ToDouble(x * 100 / (BitmapOriginalResize.Width)); //step 1 from 3 TODO wird 3* hintereinander aufgerufen!!!!!
                        this.Dispatcher.Invoke(EmptyDelegate, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
            ProgressBarThink.Value = 0; //step 1 from 3
            this.Dispatcher.Invoke(EmptyDelegate, System.Windows.Threading.DispatcherPriority.Background);

            ProjPage.OriginalColors = OriginalPalette.Count.ToString();
        }

        /// <summary>
        /// copy the MainWindoes Controls contend to the ProjPage
        /// return error = 0 -> no mistakes in textboxes
        /// </summary>
        private bool MainWindowToProjectPage()
        {
            bool error = false;

            ProjPage.CheckboxPin0 = CheckboxPin0.IsChecked;
            ProjPage.CheckboxPin1 = CheckboxPin1.IsChecked;
            ProjPage.CheckboxPin2 = CheckboxPin2.IsChecked;
            ProjPage.CheckboxPin3 = CheckboxPin3.IsChecked;
            ProjPage.CheckboxPin4 = CheckboxPin4.IsChecked;
            ProjPage.CheckboxNoSinus = CheckboxNoSinus.IsChecked;
            ProjPage.CheckboxPageCalculate = CheckboxPageCalculate.IsChecked;
            ProjPage.CheckboxPageInfoData = CheckboxPageInfoData.IsChecked;
            ProjPage.CheckboxSinus0 = CheckboxNoSinus.IsChecked;
            ProjPage.CheckboxSinus1 = CheckboxSinus1.IsChecked;
            ProjPage.CheckboxSinus2 = CheckboxSinus2.IsChecked;
            ProjPage.CheckboxSinus3 = CheckboxSinus3.IsChecked;
            ProjPage.CheckboxSinus4 = CheckboxSinus4.IsChecked;
            ProjPage.CheckboxSinus5 = CheckboxSinus5.IsChecked;
            ProjPage.CheckboxX0 = CheckboxX0.IsChecked;
            ProjPage.CheckboxX1 = CheckboxX1.IsChecked;
            ProjPage.CheckboxX2 = CheckboxX2.IsChecked;
            ProjPage.CheckboxX3 = CheckboxX3.IsChecked;
            ProjPage.CheckboxX4 = CheckboxX4.IsChecked;
            ProjPage.CheckboxX5 = CheckboxX5.IsChecked;
            ProjPage.CheckboxY0 = CheckboxY0.IsChecked;
            ProjPage.CheckboxY1 = CheckboxY1.IsChecked;
            ProjPage.CheckboxY2 = CheckboxY2.IsChecked;
            ProjPage.CheckboxY3 = CheckboxY3.IsChecked;
            ProjPage.CheckboxY4 = CheckboxY4.IsChecked;
            ProjPage.CheckboxY5 = CheckboxY5.IsChecked;

            try
            {
                ProjPage.Farbanteil0 = Convert.ToDouble(textBoxCorrection0.Text);
                textBoxCorrection0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                textBoxCorrection0.Background = BrushError;
                MessageBox.Show("Convert.ToDouble(textBoxFarbanteil0.Text) " + ex.Message);
                error = true;
            }

            try
            {
                ProjPage.Frequenz0 = Convert.ToInt32(TextBoxFrequenz0.Text);
                TextBoxFrequenz0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz0.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Frequenz1 = Convert.ToInt32(TextBoxFrequenz1.Text);
                TextBoxFrequenz1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz1.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Frequenz2 = Convert.ToInt32(TextBoxFrequenz2.Text);
                TextBoxFrequenz2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz2.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Frequenz3 = Convert.ToInt32(TextBoxFrequenz3.Text);
                TextBoxFrequenz3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz3.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Frequenz4 = Convert.ToInt32(TextBoxFrequenz4.Text);
                TextBoxFrequenz4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz4.Text) " + ex.Message);
                error = true;
            }

            try
            {
                ProjPage.Frequenz5 = Convert.ToInt32(TextBoxFrequenz5.Text);
                TextBoxFrequenz5.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxFrequenz5.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxFrequenz5.Text) " + ex.Message);
                error = true;
            }

            try
            {
                ProjPage.Phi0 = Convert.ToInt32(TextBoxPhi0.Text);
                TextBoxPhi0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi0.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Phi1 = Convert.ToInt32(TextBoxPhi1.Text);
                TextBoxPhi1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi1.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Phi2 = Convert.ToInt32(TextBoxPhi2.Text);
                TextBoxPhi2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi2.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Phi3 = Convert.ToInt32(TextBoxPhi3.Text);
                TextBoxPhi3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi3.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Phi4 = Convert.ToInt32(TextBoxPhi4.Text);
                TextBoxPhi4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi4.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Phi5 = Convert.ToInt32(TextBoxPhi5.Text);
                TextBoxPhi5.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxPhi5.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(textBoxPhi5.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion0 = Convert.ToInt32(TextBoxSinusProportion0.Text);
                TextBoxSinusProportion0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion0.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion1 = Convert.ToInt32(TextBoxSinusProportion1.Text);
                TextBoxSinusProportion1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion1.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion2 = Convert.ToInt32(TextBoxSinusProportion2.Text);
                TextBoxSinusProportion2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion2.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion3 = Convert.ToInt32(TextBoxSinusProportion3.Text);
                TextBoxSinusProportion3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion3.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion4 = Convert.ToInt32(TextBoxSinusProportion4.Text);
                TextBoxSinusProportion4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion4.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.Proportion5 = Convert.ToInt32(TextBoxSinusProportion5.Text);
                TextBoxSinusProportion5.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxSinusProportion5.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxSinusProportion4.Text) " + ex.Message);
                error = true;
            }
            ProjPage.RadioBtnFarbwert = false;
            ProjPage.RadioBtnHellwert = false;
            ProjPage.RadioBtnSaetigung = false;
            ProjPage.RadioBtnFarbabstand = false;
            if (RadioButtonFarbwert.IsChecked == true) ProjPage.RadioBtnFarbwert = true;
            else if (RadioButtonHellwert.IsChecked == true) ProjPage.RadioBtnHellwert = true;
            else if (RadioButtonSaetigung.IsChecked == true) ProjPage.RadioBtnSaetigung = true;
            else ProjPage.RadioBtnFarbabstand = true;

            try
            {
                int index = Convert.ToInt32(TextBoxTargetIndex0.Text);
                if (index >= 0 && index <= 4)
                {
                    ProjPage.TargetIndex0 = index;
                    TextBoxTargetIndex0.Background = BrushNoError;
                }
                else
                {
                    TextBoxTargetIndex0.Background = BrushError;
                    MessageBox.Show("TextBoxTargetIndex0: Nur Wert 0,1,2,3,4 erlaubt");
                    error = true;
                }
            }
            catch (Exception ex)
            {
                TextBoxTargetIndex0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxTargetIndex0.Text) " + ex.Message);
                error = true;
            }

            try
            {
                int index = Convert.ToInt32(TextBoxTargetIndex1.Text);
                if (index >= 0 && index <= 4)
                {
                    ProjPage.TargetIndex1 = index;
                    TextBoxTargetIndex1.Background = BrushNoError;
                }
                else
                {
                    TextBoxTargetIndex1.Background = BrushError;
                    MessageBox.Show("TextBoxTargetIndex1: Nur Wert 0,1,2,3,4 erlaubt");
                    error = true;
                }
            }
            catch (Exception ex)
            {
                TextBoxTargetIndex1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxTargetIndex1.Text) " + ex.Message);
                error = true;
            }

            try
            {
                int index = Convert.ToInt32(TextBoxTargetIndex2.Text);
                if (index >= 0 && index <= 4)
                {
                    ProjPage.TargetIndex2 = index;
                    TextBoxTargetIndex2.Background = BrushNoError;
                }
                else
                {
                    TextBoxTargetIndex2.Background = BrushError;
                    MessageBox.Show("TextBoxTargetIndex2: Nur Wert 0,1,2,3,4 erlaubt");
                    error = true;
                }
            }
            catch (Exception ex)
            {
                TextBoxTargetIndex2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxTargetIndex2.Text) " + ex.Message);
                error = true;
            }

            try
            {
                int index = Convert.ToInt32(TextBoxTargetIndex3.Text);
                if (index >= 0 && index <= 4)
                {
                    ProjPage.TargetIndex3 = index;
                    TextBoxTargetIndex3.Background = BrushNoError;
                }
                else
                {
                    TextBoxTargetIndex3.Background = BrushError;
                    MessageBox.Show("TextBoxTargetIndex3: Nur Wert 0,1,2,3,4 erlaubt");
                    error = true;
                }
            }
            catch (Exception ex)
            {
                TextBoxTargetIndex3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxTargetIndex3.Text) " + ex.Message);
                error = true;
            }

            try
            {
                int index = Convert.ToInt32(TextBoxTargetIndex4.Text);
                if (index >= 0 && index <= 4)
                {
                    ProjPage.TargetIndex4 = index;
                    TextBoxTargetIndex4.Background = BrushNoError;
                }
                else
                {
                    TextBoxTargetIndex4.Background = BrushError;
                    MessageBox.Show("TextBoxTargetIndex4: Nur Wert 0,1,2,3,4 erlaubt");
                    error = true;
                }
            }
            catch (Exception ex)
            {
                TextBoxTargetIndex4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxTargetIndex4.Text) " + ex.Message);
                error = true;
            }

            try
            {
                ProjPage.TextBoxX0 = Convert.ToInt32(TextBoxX0.Text);
                TextBoxX0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX0.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxX1 = Convert.ToInt32(TextBoxX1.Text);
                TextBoxX1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX1.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxX2 = Convert.ToInt32(TextBoxX2.Text);
                TextBoxX2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX2.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxX3 = Convert.ToInt32(TextBoxX3.Text);
                TextBoxX3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX3.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxX4 = Convert.ToInt32(TextBoxX4.Text);
                TextBoxX4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX4.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxX5 = Convert.ToInt32(TextBoxX5.Text);
                TextBoxX5.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxX5.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxX5.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY0 = Convert.ToInt32(TextBoxY0.Text);
                TextBoxY0.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY0.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY0.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY1 = Convert.ToInt32(TextBoxY1.Text);
                TextBoxY1.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY1.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY1.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY2 = Convert.ToInt32(TextBoxY2.Text);
                TextBoxY2.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY2.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY2.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY3 = Convert.ToInt32(TextBoxY3.Text);
                TextBoxY3.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY3.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY3.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY4 = Convert.ToInt32(TextBoxY4.Text);
                TextBoxY4.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY4.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY4.Text) " + ex.Message);
                error = true;
            }
            try
            {
                ProjPage.TextBoxY5 = Convert.ToInt32(TextBoxY5.Text);
                TextBoxY5.Background = BrushNoError;
            }
            catch (Exception ex)
            {
                TextBoxY5.Background = BrushError;
                MessageBox.Show("Convert.ToInt32(TextBoxY5.Text) " + ex.Message);
                error = true;
            }

            if (RadioButtonResize1920.IsChecked == true) ProjPage.OriginalResize = 1920;
            else if (RadioButtonResize960.IsChecked == true) ProjPage.OriginalResize = 960;
            else if (RadioButtonResize480.IsChecked == true) ProjPage.OriginalResize = 480;
            else if (RadioButtonResize240.IsChecked == true) ProjPage.OriginalResize = 240;
            else ProjPage.OriginalResize = 1;

            if (File.Exists(ProjPage.OriginalNameBMP) && File.Exists(ProjPage.OriginalNameBMP_Resize))

            {
                string[] arrayPath = ProjPage.OriginalNameJpg.Split('.');
                string pathBmp = arrayPath[0] + ".BMP";
                string pathBmpResize = arrayPath[0] + ProjPage.OriginalResize.ToString() + "Resize.BMP";

                // Load the image.
                System.Drawing.Image image1 = System.Drawing.Image.FromFile(ProjPage.OriginalNameJpg);
                System.Drawing.Image image1Resize;

                // Save the resize image in Bmp format for creating original palette
                image1Resize = OriginalManipulate.ResizePicByWidth(image1, ProjPage.OriginalResize);

                if (!File.Exists(pathBmpResize))
                {
                    try
                    {
                        image1Resize.Save(pathBmpResize, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("image1Resize.Save(pathBmpResize, System.Drawing.Imaging.ImageFormat.Bmp) " + ex.Message);
                    }
                }

                //ProjPage.OriginalNameBMP = pathBmp;
                ProjPage.OriginalNameBMP_Resize = pathBmpResize;
                //ProjectPageSave();
            }
            if (File.Exists(ProjPage.OriginalNameBMP) && File.Exists(ProjPage.OriginalNameBMP_Resize))
            {
                //Uri pathUri = new Uri(ProjPage.OriginalNameJpg);
                //BitmapImageOriginal = new BitmapImage(pathUri);
                BitmapOriginal = new Bitmap(ProjPage.OriginalNameBMP);
                BitmapOriginalResize = new Bitmap(ProjPage.OriginalNameBMP_Resize);
                //ImageOriginalPicture.Source = BitmapImageOriginal;

                string[] arrayString = ProjPage.OriginalNameBMP.Split('\\');
                LabelOriginalName.Content = "Original Name: " + arrayString[arrayString.Length - 1];
                LabelOriginalName.ToolTip = "Original: " + ProjPage.OriginalNameBMP +
                                             " Original verkleinert für Palette: " + ProjPage.OriginalNameBMP_Resize;
                LabelOriginalColors.Content = "Original ähnliche Farben: " + ProjPage.OriginalColors.ToString();
                LabelOriginalColors.ToolTip = "Farben vom verkleinerten Bild. ARGB-Farben mit einer maximalen Abweichung von " + ProjPage.ColorReduce.ToString() +
                                              " bei A,R,G oder B gelten als ähnliche Farben";

                LabelOriginalSize.Content = "Original Größe: " + BitmapOriginal.Width + "*" + BitmapOriginal.Height;
                LabelOriginalSize.ToolTip = "Original Größe: " + BitmapOriginal.Width + "*" + BitmapOriginal.Height +
                    " Original Größe verkleinert für Palette: " + BitmapOriginalResize.Width + "*" + BitmapOriginalResize.Height;
            }

            if (RadioButtonColorReduce64.IsChecked == true) ProjPage.ColorReduce = 64;
            else if (RadioButtonColorReduce32.IsChecked == true) ProjPage.ColorReduce = 32;
            else if (RadioButtonColorReduce16.IsChecked == true) ProjPage.ColorReduce = 16;
            else if (RadioButtonColorReduce8.IsChecked == true) ProjPage.ColorReduce = 8;
            else if (RadioButtonColorReduce4.IsChecked == true) ProjPage.ColorReduce = 4;
            else if (RadioButtonColorReduce2.IsChecked == true) ProjPage.ColorReduce = 2;
            else ProjPage.ColorReduce = 1;

            return error;
        }

        private void NewFilePalette(object sender, RoutedEventArgs e)
        {
        }

        private void OpenFileOriginal(object sender, RoutedEventArgs e)
        {
            string[] arrayPath;
            string pathBmp = "";
            string pathBmpResize = "";
            string pathJpg = "";

            System.Uri pathUri;

            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "Original Bild", // Default file name
                                            //dlg.DefaultExt = ".bmp"; // Default file extension
                                            //dlg.Filter = "JPEG-Bild (.jpg)|*.jpg"; // Filter files by extension
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp"
            };

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProjPage.OriginalNameBMP = dlg.FileName;
            }

            // Bitmap
            // BitmapImageOriginal = new Bitmap(dlg.FileName);

            // If the file name is not an empty string open it for saving.
            if (File.Exists(dlg.FileName))
            {
                arrayPath = dlg.FileName.Split('\\', '.');
                pathBmp = Proj.PathName + PATH_ORIGINAL + arrayPath[arrayPath.Length - 2] + ".BMP";
                pathBmpResize = Proj.PathName + PATH_ORIGINAL + arrayPath[arrayPath.Length - 2] + "Resize.BMP";
                pathJpg = Proj.PathName + PATH_ORIGINAL + arrayPath[arrayPath.Length - 2] + ".Jpg";

                if (!pathBmp.Equals(dlg.FileName))
                {
                    // Load the image.
                    System.Drawing.Image image1 = System.Drawing.Image.FromFile(dlg.FileName);
                    System.Drawing.Image image1Resize;

                    // Save the image in Bmp format.
                    try
                    {
                        image1.Save(pathBmp, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("image1.Save(pathBmp, System.Drawing.Imaging.ImageFormat.Bmp) " + ex.Message +
                        " *.Bmp File befindet sich im Original-Verzechnis und die alte Datei wird verwendet!");
                    }

                    // Todo Resize window berechnen wenn Check box sich änder!
                    // Save the resize image in Bmp format for creating original palette
                    image1Resize = OriginalManipulate.ResizePicByWidth(image1, ProjPage.OriginalResize);
                    image1Resize.Save(pathBmpResize, System.Drawing.Imaging.ImageFormat.Bmp);

                    // Save the image in Jpg format.
                    try
                    {
                        image1.Save(pathJpg, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("image1.Save(pathJpg, System.Drawing.Imaging.ImageFormat.Jpeg) " + ex.Message +
                        " *.Jpg File befindet sich im Original-Verzechnis und die alte Datei wird verwendet!");
                    }
                }

                ProjPage.OriginalNameBMP = pathBmp;
                ProjPage.OriginalNameBMP_Resize = pathBmpResize;
                ProjPage.OriginalNameJpg = pathJpg;
            }

            if (File.Exists(ProjPage.OriginalNameBMP))
            {
                pathUri = new Uri(ProjPage.OriginalNameJpg);
                BitmapImageOriginal = new BitmapImage(pathUri);
                BitmapOriginal = new Bitmap(ProjPage.OriginalNameBMP);
                BitmapOriginalResize = new Bitmap(ProjPage.OriginalNameBMP_Resize);
                ImageOriginalPicture.Source = BitmapImageOriginal;
            }

            ProjectPageSave();
            ProjectSave();
            ProjectPageToMainWindow();
        }

        private void OpenFilePalette(object sender, RoutedEventArgs e)
        {
            
            // Palette Window
            PalWin = new Palette();
            PalWin.Show();
        }

        private void ProgrammExit(object sender, RoutedEventArgs e)
        {
            WindowMain.Close();
        }

        private void Proj_NameChangedEvent(object sender, CustomEventArgs e)
        {
            ProjectChanged();
        }

        private void Proj_PageIndexChangedEvent(object sender, CustomEventArgs e)
        {
            ProjectChanged();
        }

        private void ProjectChanged()
        {
            ProjectSave();

            WindowMain.Title = PROJ_NAME + " " + Proj.Name;
            GroupBoxPage.Header = "Seite " + Proj.PageIndex + " von " + (Proj.PageCounter - 1);
            ProjectPageOpen();
            ProjectPageSave();
        }

        private void ProjectNew(object sender, RoutedEventArgs e)
        {
            ProjectNew();
        }

        private void ProjectNew()
        {
            string[] arrayPath;

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Neues Projekt erstellen",

                DefaultExt = ".prj", // Default file extension
                Filter = "Project files (.prj)|*.prj" // Filter files by extension
            };

            // Show open file dialog box
            Nullable<bool> res = dlg.ShowDialog();

            // Process save file dialog box results
            if (res == true)
            {
                Proj.Name = dlg.FileName;
                Proj.PathName = "";

                arrayPath = Proj.Name.Split('\\');
                for (int i = 0; i < arrayPath.Length - 1; ++i)
                {
                    Proj.PathName = Proj.PathName + arrayPath[i] + "\\";
                }
                Proj.Name = dlg.FileName;

                //creath the projects-path
                try
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(Proj.PathName + PATH_ORIGINAL);
                    Directory.CreateDirectory(Proj.PathName + PATH_IMITATION);
                    Directory.CreateDirectory(Proj.PathName + PATH_PALETTES);
                    Directory.CreateDirectory(Proj.PathName + PATH_PROJ_PAGES);
                    Directory.CreateDirectory(Proj.PathName + PATH_DATA_GRID);
                }
                catch (Exception)
                {
                    MessageBox.Show("Direktory create failed");
                }
                finally { }

                // Enable the object in the UI
                ProjectCommandsChangeEnabled(true);
            }
            ProjectChanged();
            MainWindowToProjectPage();
        }

        private void ProjectOpen(object sender, RoutedEventArgs e)
        {
            String test = Proj.Name;
            string name = "";

            //Proj = new Project();

            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Projekt öffnen",

                DefaultExt = ".prj", // Default file extension
                Filter = "Project files (.prj)|*.prj" // Filter files by extension
            };

            // Show open file dialog box
            Nullable<bool> res = dlg.ShowDialog();

            // Process open file dialog box results
            if (res == true)
            {
                name = dlg.FileName;

                // read the serialized project ...
                System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(Project));
                System.IO.StreamReader file = new System.IO.StreamReader(name);
                Proj = (Project)reader.Deserialize(file);
                file.Close();

                // Enable the object in the UI
                ProjectCommandsChangeEnabled(true);

                ProjectChanged();
                ProjectPageToMainWindow();
            }
        }

        private void ProjectPageCopy()
        {
            //serialize projekt
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(ProjectPage));

            string path = Proj.PathName + PATH_PROJ_PAGES + Proj.PageCounter.ToString() + "ProjPage.xls";    //SerializationOverview.xml";
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, ProjPage);
            file.Close();
        }

        private void ProjectPageOpen()
        {
            // read the serialized project ...
            System.Xml.Serialization.XmlSerializer reader =
            new System.Xml.Serialization.XmlSerializer(typeof(ProjectPage));
            string path = Proj.PathName + PATH_PROJ_PAGES + Proj.PageIndex.ToString() + "ProjPage.xls";    //SerializationOverview.xml";
            if (File.Exists(path))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                ProjPage = (ProjectPage)reader.Deserialize(file);
                file.Close();
            }

            //creath the projects-path
            try
            {
                // Try to create the directory.
                Directory.CreateDirectory(Proj.PathName + PATH_ORIGINAL);
                Directory.CreateDirectory(Proj.PathName + PATH_IMITATION);
                Directory.CreateDirectory(Proj.PathName + PATH_PALETTES);
                Directory.CreateDirectory(Proj.PathName + PATH_PROJ_PAGES);
                Directory.CreateDirectory(Proj.PathName + PATH_DATA_GRID);
            }
            catch (Exception)
            {
                MessageBox.Show("Direktory create failed");
            }
            finally { }
        }

        private void ProjectPageSave()
        {
            //serialize projekt
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(ProjectPage));

            string path = Proj.PathName + PATH_PROJ_PAGES + Proj.PageIndex.ToString() + "ProjPage.xls";    //SerializationOverview.xml";
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, ProjPage);
            file.Close();
        }

        /// <summary>
        /// copy the MainWindoes Controls contend to the ProjPage
        /// </summary>
        private void ProjectPageToMainWindow()
        {
            string[] arrayString;

            if (ProjPage.CheckboxPin0 == null)
            {
                ProjPage.CheckboxPin0 = false;
            }
            CheckboxPin0.IsChecked = ProjPage.CheckboxPin0;

            if (ProjPage.CheckboxPin1 == null)
            {
                ProjPage.CheckboxPin1 = false;
            }
            CheckboxPin1.IsChecked = ProjPage.CheckboxPin1;

            if (ProjPage.CheckboxPin2 == null)
            {
                ProjPage.CheckboxPin2 = false;
            }
            CheckboxPin2.IsChecked = ProjPage.CheckboxPin2;

            if (ProjPage.CheckboxPin3 == null)
            {
                ProjPage.CheckboxPin3 = false;
            }
            CheckboxPin3.IsChecked = ProjPage.CheckboxPin3;

            if (ProjPage.CheckboxPin4 == null)
            {
                ProjPage.CheckboxPin4 = false;
            }
            CheckboxPin4.IsChecked = ProjPage.CheckboxPin4;

            if (ProjPage.CheckboxPageCalculate == null)
            {
                ProjPage.CheckboxPageCalculate = false;
            }
            CheckboxPageCalculate.IsChecked = ProjPage.CheckboxPageCalculate;
            if (ProjPage.CheckboxPageInfoData == null)
            {
                ProjPage.CheckboxPageInfoData = false;
            }
            CheckboxPageInfoData.IsChecked = ProjPage.CheckboxPageInfoData;
            if (ProjPage.CheckboxSinus0 == null)
            {
                ProjPage.CheckboxSinus0 = false;
            }
            CheckboxNoSinus.IsChecked = ProjPage.CheckboxSinus0;
            if (ProjPage.CheckboxSinus1 == null)
            {
                ProjPage.CheckboxSinus1 = false;
            }
            CheckboxSinus1.IsChecked = ProjPage.CheckboxSinus1;
            if (ProjPage.CheckboxSinus2 == null)
            {
                ProjPage.CheckboxSinus2 = false;
            }
            CheckboxSinus2.IsChecked = ProjPage.CheckboxSinus2;
            if (ProjPage.CheckboxSinus3 == null)
            {
                ProjPage.CheckboxSinus3 = false;
            }
            CheckboxSinus3.IsChecked = ProjPage.CheckboxSinus3;
            if (ProjPage.CheckboxSinus4 == null)
            {
                ProjPage.CheckboxSinus4 = false;
            }
            CheckboxSinus4.IsChecked = ProjPage.CheckboxSinus4;
            if (ProjPage.CheckboxSinus5 == null)
            {
                ProjPage.CheckboxSinus5 = false;
            }
            CheckboxSinus5.IsChecked = ProjPage.CheckboxSinus5;
            if (ProjPage.CheckboxX0 == null)
            {
                ProjPage.CheckboxX0 = false;
            }
            CheckboxX0.IsChecked = ProjPage.CheckboxX0;
            if (ProjPage.CheckboxX1 == null)
            {
                ProjPage.CheckboxX1 = false;
            }
            CheckboxX1.IsChecked = ProjPage.CheckboxX1;
            if (ProjPage.CheckboxX2 == null)
            {
                ProjPage.CheckboxX2 = false;
            }
            CheckboxX2.IsChecked = ProjPage.CheckboxX2;
            if (ProjPage.CheckboxX3 == null)
            {
                ProjPage.CheckboxX3 = false;
            }
            CheckboxX3.IsChecked = ProjPage.CheckboxX3;
            if (ProjPage.CheckboxX4 == null)
            {
                ProjPage.CheckboxX4 = false;
            }
            CheckboxX4.IsChecked = ProjPage.CheckboxX4;
            if (ProjPage.CheckboxX5 == null)
            {
                ProjPage.CheckboxX5 = false;
            }
            CheckboxX5.IsChecked = ProjPage.CheckboxX5;
            if (ProjPage.CheckboxY0 == null)
            {
                ProjPage.CheckboxY0 = false;
            }
            CheckboxY0.IsChecked = ProjPage.CheckboxY0;
            if (ProjPage.CheckboxY1 == null)
            {
                ProjPage.CheckboxY1 = false;
            }
            CheckboxY1.IsChecked = ProjPage.CheckboxY1;
            if (ProjPage.CheckboxY2 == null)
            {
                ProjPage.CheckboxY2 = false;
            }
            CheckboxY2.IsChecked = ProjPage.CheckboxY2;
            if (ProjPage.CheckboxY3 == null)
            {
                ProjPage.CheckboxY3 = false;
            }
            CheckboxY3.IsChecked = ProjPage.CheckboxY3;
            if (ProjPage.CheckboxY4 == null)
            {
                ProjPage.CheckboxY4 = false;
            }
            CheckboxY4.IsChecked = ProjPage.CheckboxY4;
            if (ProjPage.CheckboxY5 == null)
            {
                ProjPage.CheckboxY5 = false;
            }
            CheckboxY5.IsChecked = ProjPage.CheckboxY5;
            TextBoxFrequenz0.Text = ProjPage.Frequenz0.ToString();
            TextBoxFrequenz1.Text = ProjPage.Frequenz1.ToString();
            TextBoxFrequenz2.Text = ProjPage.Frequenz2.ToString();
            TextBoxFrequenz3.Text = ProjPage.Frequenz3.ToString();
            TextBoxFrequenz4.Text = ProjPage.Frequenz4.ToString();
            TextBoxFrequenz5.Text = ProjPage.Frequenz5.ToString();

            if (File.Exists(ProjPage.OriginalNameBMP))
            {
                arrayString = ProjPage.OriginalNameBMP.Split('\\');
                LabelOriginalName.Content = arrayString[arrayString.Length - 1];
                LabelOriginalName.ToolTip = (ProjPage.OriginalNameBMP);
            }

            LabelOriginalSize.Content = ProjPage.OriginalSize;

            arrayString = ProjPage.ImitationColors.Split('\\');
            LabelImitationColors.Content = arrayString[arrayString.Length - 1];

            if (File.Exists(ProjPage.ImitationNameBMP))
            {
                arrayString = ProjPage.ImitationNameBMP.Split('\\');
                LabelImitationName.Content = arrayString[arrayString.Length - 1];
                LabelImitationName.ToolTip = (ProjPage.OriginalNameBMP);
            }

            LabelImitationSize.Content = ProjPage.ImitationSize;

            TextBoxPhi0.Text = ProjPage.Phi0.ToString();
            TextBoxPhi1.Text = ProjPage.Phi1.ToString();
            TextBoxPhi2.Text = ProjPage.Phi2.ToString();
            TextBoxPhi3.Text = ProjPage.Phi3.ToString();
            TextBoxPhi4.Text = ProjPage.Phi4.ToString();
            TextBoxPhi5.Text = ProjPage.Phi5.ToString();
            TextBoxSinusProportion0.Text = ProjPage.Proportion0.ToString();
            TextBoxSinusProportion1.Text = ProjPage.Proportion1.ToString();
            TextBoxSinusProportion2.Text = ProjPage.Proportion2.ToString();
            TextBoxSinusProportion3.Text = ProjPage.Proportion3.ToString();
            TextBoxSinusProportion4.Text = ProjPage.Proportion4.ToString();
            TextBoxSinusProportion5.Text = ProjPage.Proportion5.ToString();

            RadioButtonFarbwert.IsChecked = false;
            RadioButtonHellwert.IsChecked = false;
            RadioButtonSaetigung.IsChecked = false;
            RadioButtonFarbabstand.IsChecked = false;
            if (ProjPage.RadioBtnFarbwert == true) RadioButtonFarbwert.IsChecked = true;
            else if (ProjPage.RadioBtnHellwert == true) RadioButtonHellwert.IsChecked = true;
            else if (ProjPage.RadioBtnSaetigung == true) RadioButtonSaetigung.IsChecked = true;
            else RadioButtonFarbabstand.IsChecked = true;

            // Calculation mode for color min. distance between original picture and palette 0..4
            if (ProjPage.RadioBtnFarbabstand == true)
            {
                CalcMode = CalculationMode.ColorDistance;
            }
            if (ProjPage.RadioBtnFarbwert == true)
            {
                CalcMode = CalculationMode.HSV_H_Distance;
            }
            if (ProjPage.RadioBtnSaetigung == true)
            {
                CalcMode = CalculationMode.HSV_S_Distance;
            }
            if (ProjPage.RadioBtnHellwert == true)
            {
                CalcMode = CalculationMode.HSV_V_Distance;
            }

            TextBoxTargetIndex0.Text = ProjPage.TargetIndex0.ToString();
            TextBoxTargetIndex1.Text = ProjPage.TargetIndex1.ToString();
            TextBoxTargetIndex2.Text = ProjPage.TargetIndex2.ToString();
            TextBoxTargetIndex3.Text = ProjPage.TargetIndex3.ToString();
            TextBoxTargetIndex4.Text = ProjPage.TargetIndex4.ToString();

            TextBoxX0.Text = ProjPage.TextBoxX0.ToString();
            TextBoxX1.Text = ProjPage.TextBoxX1.ToString();
            TextBoxX2.Text = ProjPage.TextBoxX2.ToString();
            TextBoxX3.Text = ProjPage.TextBoxX3.ToString();
            TextBoxX4.Text = ProjPage.TextBoxX4.ToString();
            TextBoxX5.Text = ProjPage.TextBoxX5.ToString();
            TextBoxY0.Text = ProjPage.TextBoxY0.ToString();
            TextBoxY1.Text = ProjPage.TextBoxY1.ToString();
            TextBoxY2.Text = ProjPage.TextBoxY2.ToString();
            TextBoxY3.Text = ProjPage.TextBoxY3.ToString();
            TextBoxY4.Text = ProjPage.TextBoxY4.ToString();
            TextBoxY5.Text = ProjPage.TextBoxY5.ToString();

            // text boxes from palettes
            textBoxCorrection0.Text = ProjPage.Correction0.ToString();
            textBoxCorrection1.Text = ProjPage.Correction1.ToString();
            textBoxCorrection2.Text = ProjPage.Correction2.ToString();
            textBoxCorrection3.Text = ProjPage.Correction3.ToString();
            textBoxCorrection4.Text = ProjPage.Correction4.ToString();
            textBoxColorCount0.Text = ProjPage.ColorCount0.ToString();
            textBoxColorCount1.Text = ProjPage.ColorCount1.ToString();
            textBoxColorCount2.Text = ProjPage.ColorCount2.ToString();
            textBoxColorCount3.Text = ProjPage.ColorCount3.ToString();
            textBoxColorCount4.Text = ProjPage.ColorCount4.ToString();
            textBoxPerCent0.Text = ProjPage.PerCent0.ToString();
            textBoxPerCent1.Text = ProjPage.PerCent1.ToString();
            textBoxPerCent2.Text = ProjPage.PerCent2.ToString();
            textBoxPerCent3.Text = ProjPage.PerCent3.ToString();
            textBoxPerCent4.Text = ProjPage.PerCent4.ToString();

            if (!File.Exists(ProjPage.OriginalNameBMP) || !File.Exists(ProjPage.OriginalNameBMP_Resize))
            {
                string[] arrayPath = ProjPage.OriginalNameJpg.Split('.');
                string pathBmp = arrayPath[0] + ".BMP";
                string pathBmpResize = arrayPath[0] + "Resize.BMP";

                // Load the image.
                // Don't use try because image1 must be sure declared
                if (!File.Exists(ProjPage.OriginalNameJpg))
                {
                    ProjPage.OriginalNameJpg = GetStandardOriginalName();
                }

                System.Drawing.Image image1 = System.Drawing.Image.FromFile(ProjPage.OriginalNameJpg);

                System.Drawing.Image image1Resize;

                // Save the image in Bmp format.
                try
                {
                    image1.Save(pathBmp, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("image1.Save(pathBmp, System.Drawing.Imaging.ImageFormat.Bmp) " + ex.Message);
                }

                // Save the resize image in Bmp format for creating original palette
                image1Resize = OriginalManipulate.ResizePicByWidth(image1, ProjPage.OriginalResize);
                try
                {
                    image1Resize.Save(pathBmpResize, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("image1Resize.Save(pathBmpResize, System.Drawing.Imaging.ImageFormat.Bmp) " + ex.Message);
                }

                ProjPage.OriginalNameBMP = pathBmp;
                ProjPage.OriginalNameBMP_Resize = pathBmpResize;
                ProjectPageSave();
            }
            if (File.Exists(ProjPage.OriginalNameBMP) && File.Exists(ProjPage.OriginalNameBMP_Resize))
            {
                //Uri pathUri = new Uri(ProjPage.OriginalNameJpg);
                //BitmapImageOriginal = new BitmapImage(pathUri);
                BitmapOriginal = new Bitmap(ProjPage.OriginalNameBMP);
                BitmapOriginalResize = new Bitmap(ProjPage.OriginalNameBMP_Resize);
                //ImageOriginalPicture.Source = BitmapImageOriginal;

                arrayString = ProjPage.OriginalNameBMP.Split('\\');
                LabelOriginalName.Content = "Original Name: " + arrayString[arrayString.Length - 1];
                LabelOriginalName.ToolTip = "Original: " + ProjPage.OriginalNameBMP +
                                             " Original verkleinert für Palette: " + ProjPage.OriginalNameBMP_Resize;
                LabelOriginalColors.Content = "Original ähnliche Farben: " + ProjPage.OriginalColors.ToString();
                LabelOriginalColors.ToolTip = "Farben vom verkleinerten Bild. ARGB-Farben mit einer maximalen Abweichung von " + ProjPage.ColorReduce.ToString() +
                                              " bei A,R,G oder B gelten als ähnliche Farben";

                LabelOriginalSize.Content = "Original Größe: " + BitmapOriginal.Width + "*" + BitmapOriginal.Height;
                LabelOriginalSize.ToolTip = "Original Größe: " + BitmapOriginal.Width + "*" + BitmapOriginal.Height +
                    " Original Größe verkleinert für Palette: " + BitmapOriginalResize.Width + "*" + BitmapOriginalResize.Height;
            }
            // Show Bitmap
            // MessageBox.Show("ImageOriginalPicture.Source: " + ProjPage.OriginalNameJpg);
            if (File.Exists(ProjPage.OriginalNameJpg))
            {
                ImageOriginalPicture.BeginInit();
                ImageOriginalPicture.Source = new BitmapImage(new Uri(ProjPage.OriginalNameJpg));
                ImageOriginalPicture.EndInit();
            }
            else
            {
                ImageOriginalPicture.BeginInit();
                ImageOriginalPicture.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/ImageOriginalBild.JPG"));
                ImageOriginalPicture.EndInit();
            }
            ImageOriginalPicture.ToolTip = ImageOriginalPicture.Source.ToString();

            // ImageImitationPicture
            if (File.Exists(ProjPage.ImitationNameJpg))
            {
                ImageImitationPicture.BeginInit();
                ImageImitationPicture.Source = new BitmapImage(new Uri(ProjPage.ImitationNameJpg));
                ImageImitationPicture.EndInit();
            }
            else
            {
                ImageImitationPicture.BeginInit();
                ImageImitationPicture.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/ImageImitationBild.JPG"));
                ImageImitationPicture.EndInit();
            }
            ImageImitationPicture.ToolTip = ImageImitationPicture.Source.ToString();

            if (ProjPage.PaletteNameBmp0 != null)
            {
                arrayString = ProjPage.PaletteNameBmp0.Split('\\');
                LabelPath0.Content = arrayString[arrayString.Length - 1];
                LabelPath0.ToolTip = ProjPage.PaletteNameBmp0;
                if (File.Exists(ProjPage.PaletteNameBmp0))
                {
                    BitmapPalette[0] = new Bitmap(ProjPage.PaletteNameBmp0);
                }
            }

            if (File.Exists(ProjPage.PaletteNameJpg0))
            {
                ImagePalettePicture0.BeginInit();
                ImagePalettePicture0.Source = new BitmapImage(new Uri(ProjPage.PaletteNameJpg0));
                ImagePalettePicture0.EndInit();
            }
            else
            {
                ImagePalettePicture0.BeginInit();
                ImagePalettePicture0.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette0.JPG"));
                ImagePalettePicture0.EndInit();
            }
            ImagePalettePicture0.ToolTip = ImagePalettePicture0.Source.ToString();

            if (ProjPage.PaletteNameBmp1 != null)
            {
                arrayString = ProjPage.PaletteNameBmp1.Split('\\');
                LabelPath1.Content = arrayString[arrayString.Length - 1];
                LabelPath1.ToolTip = ProjPage.PaletteNameBmp1;
                if (File.Exists(ProjPage.PaletteNameBmp1))
                {
                    BitmapPalette[1] = new Bitmap(ProjPage.PaletteNameBmp1);
                }
            }
            if (File.Exists(ProjPage.PaletteNameJpg1))
            {
                ImagePalettePicture1.BeginInit();
                ImagePalettePicture1.Source = new BitmapImage(new Uri(ProjPage.PaletteNameJpg1));
                ImagePalettePicture1.EndInit();
            }
            else
            {
                ImagePalettePicture1.BeginInit();
                ImagePalettePicture1.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette1.JPG"));
                ImagePalettePicture1.EndInit();
            }
            ImagePalettePicture1.ToolTip = ImagePalettePicture1.Source.ToString();

            if (ProjPage.PaletteNameBmp2 != null)
            {
                arrayString = ProjPage.PaletteNameBmp2.Split('\\');
                LabelPath2.Content = arrayString[arrayString.Length - 1];
                LabelPath2.ToolTip = ProjPage.PaletteNameBmp2;
                if (File.Exists(ProjPage.PaletteNameBmp2))
                {
                    BitmapPalette[2] = new Bitmap(ProjPage.PaletteNameBmp2);
                }
            }

            if (File.Exists(ProjPage.PaletteNameJpg2))
            {
                ImagePalettePicture2.BeginInit();
                ImagePalettePicture2.Source = new BitmapImage(new Uri(ProjPage.PaletteNameJpg2));
                ImagePalettePicture2.EndInit();
            }
            else
            {
                ImagePalettePicture2.BeginInit();
                ImagePalettePicture2.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette2.JPG"));
                ImagePalettePicture2.EndInit();
            }
            ImagePalettePicture2.ToolTip = ImagePalettePicture2.Source.ToString();

            if (ProjPage.PaletteNameBmp3 != null)
            {
                arrayString = ProjPage.PaletteNameBmp3.Split('\\');
                LabelPath3.Content = arrayString[arrayString.Length - 1];
                LabelPath3.ToolTip = ProjPage.PaletteNameBmp3;
                if (File.Exists(ProjPage.PaletteNameBmp3))
                {
                    BitmapPalette[3] = new Bitmap(ProjPage.PaletteNameBmp3);
                }
            }

            if (File.Exists(ProjPage.PaletteNameJpg3))
            {
                ImagePalettePicture3.BeginInit();
                ImagePalettePicture3.Source = new BitmapImage(new Uri(ProjPage.PaletteNameJpg3));
                ImagePalettePicture3.EndInit();
            }
            else
            {
                ImagePalettePicture3.BeginInit();
                ImagePalettePicture3.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette3.JPG"));
                ImagePalettePicture3.EndInit();
            }
            ImagePalettePicture3.ToolTip = ImagePalettePicture3.Source.ToString();

            if (ProjPage.PaletteNameBmp4 != null)
            {
                arrayString = ProjPage.PaletteNameBmp4.Split('\\');
                LabelPath4.Content = arrayString[arrayString.Length - 1];
                LabelPath4.ToolTip = ProjPage.PaletteNameBmp4;
                if (File.Exists(ProjPage.PaletteNameBmp4))
                {
                    BitmapPalette[4] = new Bitmap(ProjPage.PaletteNameBmp4);
                }
            }

            if (File.Exists(ProjPage.PaletteNameJpg4))
            {
                ImagePalettePicture4.BeginInit();
                ImagePalettePicture4.Source = new BitmapImage(new Uri(ProjPage.PaletteNameJpg4));
                ImagePalettePicture4.EndInit();
            }
            else
            {
                ImagePalettePicture4.BeginInit();
                ImagePalettePicture4.Source = new BitmapImage(new Uri("Pack://application:,,,/Pictures/NoPalette4.JPG"));
                ImagePalettePicture4.EndInit();
            }
            ImagePalettePicture4.ToolTip = ImagePalettePicture4.Source.ToString();

            RadioButtonResizeNone.IsChecked = false;
            RadioButtonResize1920.IsChecked = false;
            RadioButtonResize960.IsChecked = false;
            RadioButtonResize480.IsChecked = false;
            RadioButtonResize240.IsChecked = false;
            if (ProjPage.OriginalResize == 1920) RadioButtonResize1920.IsChecked = true;
            else if (ProjPage.OriginalResize == 960) RadioButtonResize960.IsChecked = true;
            else if (ProjPage.OriginalResize == 480) RadioButtonResize480.IsChecked = true;
            else if (ProjPage.OriginalResize == 240) RadioButtonResize240.IsChecked = true;
            else RadioButtonResizeNone.IsChecked = true;

            RadioButtonColorReduce64.IsChecked = false;
            RadioButtonColorReduce32.IsChecked = false;
            RadioButtonColorReduce16.IsChecked = false;
            RadioButtonColorReduce8.IsChecked = false;
            RadioButtonColorReduce4.IsChecked = false;
            RadioButtonColorReduce2.IsChecked = false;
            RadioButtonColorReduceNone.IsChecked = false;
            if (ProjPage.ColorReduce == 64) RadioButtonColorReduce64.IsChecked = true;
            else if (ProjPage.ColorReduce == 32) RadioButtonColorReduce32.IsChecked = true;
            else if (ProjPage.ColorReduce == 16) RadioButtonColorReduce16.IsChecked = true;
            else if (ProjPage.ColorReduce == 8) RadioButtonColorReduce8.IsChecked = true;
            else if (ProjPage.ColorReduce == 4) RadioButtonColorReduce4.IsChecked = true;
            else if (ProjPage.ColorReduce == 2) RadioButtonColorReduce2.IsChecked = true;
            else RadioButtonColorReduceNone.IsChecked = true;
        }

        /// <summary>
        /// Get the standard Original name if file proj.OriginalName not Exist
        /// </summary>
        /// <returns>Standard Original Name with path</returns>
        private string GetStandardOriginalName()
        {
            // Ergebnis: Debug - oder Release - Ordner im Projektordner.
            string projectPath = Environment.CurrentDirectory;
            // Mit jedem Durchlauf geht es im Verzeichnisbaum eine Stufe höher.
            for (int i = 0; i < 2; i++)
            {
                projectPath = System.IO.Path.GetDirectoryName(projectPath);
            }
            return projectPath + @"\Pictures\ImageOriginalBild.JPG";
        }

        private void ProjectSave(object sender, RoutedEventArgs e)
        {
            ProjectSave();
        }

        private void ProjectSave()
        {
            //serialize projekt
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(Project));

            string path = Proj.Name;    //SerializationOverview.xml";
            try
            {
                System.IO.FileStream file = System.IO.File.Create(path);

                writer.Serialize(file, Proj);
                file.Close();
            }
            catch (Exception)
            {
                ;
            }
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

            //BitmapOriginal.Dispose();
            //BitmapOriginalResize.Dispose();
            //BitmapPalette[0].Dispose();
            //BitmapPalette[1].Dispose();
            //BitmapPalette[2].Dispose();
            //BitmapPalette[3].Dispose();
            //BitmapPalette[4].Dispose();

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

        //public void ExampleMethod()
        //{
        //    if (alreadyDisposed)
        //        throw new
        //            ObjectDisposedException("MyResourceHog",
        //            "Called Example Method on Disposed object");
        //     remainder elided.
        //    }

        #endregion IDisposable Support

        private void WindowMainClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Picture Original
            BitmapImageOriginal = null;
            BitmapOriginal = null;
            BitmapOriginalResize = null;
            BitmapPalette = null;
            BrushError = null;
            BrushNoError = null;

            // Zuweisung einer anonymen Methode ohne ausführbaren Code
            EmptyDelegate = null;

            // Color Palette from Original picture with distance to palette 0..4 and color counter
            OriginalPalette = null;

            // Palette Window
            PalWin = null;
            Proj = null;
            ProjPage = null;
            WindowRefresh = null;

            if (BitmapOriginal != null)
            {
                BitmapOriginal.Dispose();
            }
            if (BitmapOriginalResize != null)
            {
                BitmapOriginalResize.Dispose();
            }
            if (BitmapPalette != null)
            {
                if (BitmapPalette[0] != null)
                {
                    BitmapPalette[0].Dispose();
                }
                if (BitmapPalette[1] != null)
                {
                    BitmapPalette[1].Dispose();
                }
                if (BitmapPalette[2] != null)
                {
                    BitmapPalette[2].Dispose();
                }
                if (BitmapPalette[3] != null)
                {
                    BitmapPalette[3].Dispose();
                }
                if (BitmapPalette[4] != null)
                {
                    BitmapPalette[4].Dispose();
                }
            }

            this.Dispose();
        }

        /// <summary>
        /// Hide the Grids and the Menue if no project loaded
        /// </summary>
        /// <param name="enable">isEnabled parameter from object</param>
        private void ProjectCommandsChangeEnabled(bool enable)
        {
            MenuItemDatei.IsEnabled = enable;
            MenuItemOpenFileOriginal.IsEnabled = enable;
            // MenuItemDateiFarbpaletten.IsEnabled = enable;
            // MenueOpenFilePalette.IsEnabled = enable;
            GridUp.IsEnabled = enable;
            GridPage.IsEnabled = enable;
            GridCalc.IsEnabled = enable;
            StackPanelPalette.IsEnabled = enable;
            GridSinus.IsEnabled = enable;
            GridColorReducing.IsEnabled = enable;
            GridlResizeWidth.IsEnabled = enable;
        }

        private void MenueCreateInfoFilesClick(object sender, RoutedEventArgs e)
        {
            // Palette Window
            BmpPictureInfoWin = new BmpPictureInfo();
            BmpPictureInfoWin.Show();
        }

        private void MenueVersionClick(object sender, RoutedEventArgs e)
        {
            string version = AssemblyVersion;
            MessageBox.Show(version, "Version");
        }
        public string AssemblyVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                string appVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                return appVersion;
            }
        }
    }
}