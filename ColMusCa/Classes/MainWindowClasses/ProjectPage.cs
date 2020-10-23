namespace ColMusCa
{
    public class ProjectPage
    {
        private bool? checkboxPin0;
        private bool? checkboxPin1;
        private bool? checkboxPin2;
        private bool? checkboxPin3;
        private bool? checkboxPin4;
        private int colorReduce;
        private int originalResize;
        private bool? checkboxNoSinus;
        private bool? checkboxPageCalculate;
        private bool? checkboxPageInfoData;
        private bool? checkboxSinus0;
        private bool? checkboxSinus1;
        private bool? checkboxSinus2;
        private bool? checkboxSinus3;
        private bool? checkboxSinus4;
        private bool? checkboxSinus5;
        private bool? checkboxX0;
        private bool? checkboxX1;
        private bool? checkboxX2;
        private bool? checkboxX3;
        private bool? checkboxX4;
        private bool? checkboxX5;
        private bool? checkboxY0;
        private bool? checkboxY1;
        private bool? checkboxY2;
        private bool? checkboxY3;
        private bool? checkboxY4;
        private bool? checkboxY5;
        private int colorCount0;
        private int colorCount1;
        private int colorCount2;
        private int colorCount3;
        private int colorCount4;
        private double correction0;
        private double correction1;
        private double correction2;
        private double correction3;
        private double correction4;
        private double farbanteil0;
        private double farbanteil1;
        private double farbanteil2;
        private double farbanteil3;
        private double farbanteil4;
        private double farbanteil5;
        private int frequenz0;
        private int frequenz1;
        private int frequenz2;
        private int frequenz3;
        private int frequenz4;
        private int frequenz5;
        private string imitationColors;
        private string imitationNameBMP;
        private string imitationNameJpg;
        private string imitationSize;
        private string originalColors;
        private string originalNameBMP;
        private string originalNameBMP_Resize;
        private string originalNameJpg;
        private string originalSize;
        private string paletteNameBmp0;
        private string paletteNameBmp1;
        private string paletteNameBmp2;
        private string paletteNameBmp3;
        private string paletteNameBmp4;
        private string paletteNameJpg0;
        private string paletteNameJpg1;
        private string paletteNameJpg2;
        private string paletteNameJpg3;
        private string paletteNameJpg4;
        private double perCent0;
        private double perCent1;
        private double perCent2;
        private double perCent3;
        private double perCent4;
        private int phi0;
        private int phi1;
        private int phi2;
        private int phi3;
        private int phi4;
        private int phi5;
        private int proportion0;
        private int proportion1;
        private int proportion2;
        private int proportion3;
        private int proportion4;
        private int proportion5;
        private bool? radioBtnFarbabstand;
        private bool? radioBtnFarbwert;
        private bool? radioBtnHellwert;
        private bool? radioBtnSaetigung;
        private int targetIndex0;
        private int targetIndex1;
        private int targetIndex2;
        private int targetIndex3;
        private int targetIndex4;
        private int textBoxX0;
        private int textBoxX1;
        private int textBoxX2;
        private int textBoxX3;
        private int textBoxX4;
        private int textBoxX5;
        private int textBoxY0;
        private int textBoxY1;
        private int textBoxY2;
        private int textBoxY3;
        private int textBoxY4;
        private int textBoxY5;

        //Constructors
        public ProjectPage()
        {
            OriginalColors = "OriginalName";
            OriginalNameBMP = "OriginalNameBMP";
            OriginalNameJpg = "OriginalNameJpg";
            OriginalSize = "OriginalSize";
            ImitationColors = "ImitationColors";
            ImitationNameBMP = "ImitationNameBMP";
            ImitationSize = "ImitationSize";
        }

        // Copy constructor.
        public ProjectPage(ProjectPage previousPage)
        {
            OriginalColors = previousPage.OriginalColors;
            OriginalNameBMP = previousPage.OriginalNameBMP;
            OriginalNameJpg = previousPage.OriginalNameJpg;
            OriginalSize = previousPage.OriginalSize;
            ImitationColors = previousPage.ImitationColors;
            ImitationNameBMP = previousPage.ImitationNameBMP;
            ImitationSize = previousPage.ImitationSize;
        }

        public bool? CheckboxNoSinus { get => checkboxNoSinus; set => checkboxNoSinus = value; }
        public bool? CheckboxPageCalculate { get => checkboxPageCalculate; set => checkboxPageCalculate = value; }
        public bool? CheckboxPageInfoData { get => checkboxPageInfoData; set => checkboxPageInfoData = value; }
        public bool? CheckboxSinus0 { get => checkboxSinus0; set => checkboxSinus0 = value; }
        public bool? CheckboxSinus1 { get => checkboxSinus1; set => checkboxSinus1 = value; }
        public bool? CheckboxSinus2 { get => checkboxSinus2; set => checkboxSinus2 = value; }
        public bool? CheckboxSinus3 { get => checkboxSinus3; set => checkboxSinus3 = value; }
        public bool? CheckboxSinus4 { get => checkboxSinus4; set => checkboxSinus4 = value; }
        public bool? CheckboxSinus5 { get => checkboxSinus5; set => checkboxSinus5 = value; }
        public bool? CheckboxX0 { get => checkboxX0; set => checkboxX0 = value; }
        public bool? CheckboxX1 { get => checkboxX1; set => checkboxX1 = value; }
        public bool? CheckboxX2 { get => checkboxX2; set => checkboxX2 = value; }
        public bool? CheckboxX3 { get => checkboxX3; set => checkboxX3 = value; }
        public bool? CheckboxX4 { get => checkboxX4; set => checkboxX4 = value; }
        public bool? CheckboxX5 { get => checkboxX5; set => checkboxX5 = value; }
        public bool? CheckboxY0 { get => checkboxY0; set => checkboxY0 = value; }
        public bool? CheckboxY1 { get => checkboxY1; set => checkboxY1 = value; }
        public bool? CheckboxY2 { get => checkboxY2; set => checkboxY2 = value; }
        public bool? CheckboxY3 { get => checkboxY3; set => checkboxY3 = value; }
        public bool? CheckboxY4 { get => checkboxY4; set => checkboxY4 = value; }
        public bool? CheckboxY5 { get => checkboxY5; set => checkboxY5 = value; }
        public int ColorCount0 { get => colorCount0; set => colorCount0 = value; }
        public int ColorCount1 { get => colorCount1; set => colorCount1 = value; }
        public int ColorCount2 { get => colorCount2; set => colorCount2 = value; }
        public int ColorCount3 { get => colorCount3; set => colorCount3 = value; }
        public int ColorCount4 { get => colorCount4; set => colorCount4 = value; }
        public double Correction0 { get => correction0; set => correction0 = value; }
        public double Correction1 { get => correction1; set => correction1 = value; }
        public double Correction2 { get => correction2; set => correction2 = value; }
        public double Correction3 { get => correction3; set => correction3 = value; }
        public double Correction4 { get => correction4; set => correction4 = value; }
        public double Farbanteil0 { get => farbanteil0; set => farbanteil0 = value; }
        public double Farbanteil1 { get => farbanteil1; set => farbanteil1 = value; }
        public double Farbanteil2 { get => farbanteil2; set => farbanteil2 = value; }
        public double Farbanteil3 { get => farbanteil3; set => farbanteil3 = value; }
        public double Farbanteil4 { get => farbanteil4; set => farbanteil4 = value; }
        public double Farbanteil5 { get => farbanteil5; set => farbanteil5 = value; }
        public int Frequenz0 { get => frequenz0; set => frequenz0 = value; }
        public int Frequenz1 { get => frequenz1; set => frequenz1 = value; }
        public int Frequenz2 { get => frequenz2; set => frequenz2 = value; }
        public int Frequenz3 { get => frequenz3; set => frequenz3 = value; }
        public int Frequenz4 { get => frequenz4; set => frequenz4 = value; }
        public int Frequenz5 { get => frequenz5; set => frequenz5 = value; }
        public string ImitationColors { get => imitationColors; set => imitationColors = value; }
        public string ImitationNameBMP { get => imitationNameBMP; set => imitationNameBMP = value; }
        public string ImitationNameJpg { get => imitationNameJpg; set => imitationNameJpg = value; }
        public string ImitationSize { get => imitationSize; set => imitationSize = value; }
        public string OriginalColors { get => originalColors; set => originalColors = value; }
        public string OriginalNameBMP { get => originalNameBMP; set => originalNameBMP = value; }
        public string OriginalNameBMP_Resize { get => originalNameBMP_Resize; set => originalNameBMP_Resize = value; }
        public string OriginalNameJpg { get => originalNameJpg; set => originalNameJpg = value; }
        public string OriginalSize { get => originalSize; set => originalSize = value; }
        public string PaletteNameBmp0 { get => paletteNameBmp0; set => paletteNameBmp0 = value; }
        public string PaletteNameBmp1 { get => paletteNameBmp1; set => paletteNameBmp1 = value; }
        public string PaletteNameBmp2 { get => paletteNameBmp2; set => paletteNameBmp2 = value; }
        public string PaletteNameBmp3 { get => paletteNameBmp3; set => paletteNameBmp3 = value; }
        public string PaletteNameBmp4 { get => paletteNameBmp4; set => paletteNameBmp4 = value; }
        public string PaletteNameJpg0 { get => paletteNameJpg0; set => paletteNameJpg0 = value; }
        public string PaletteNameJpg1 { get => paletteNameJpg1; set => paletteNameJpg1 = value; }
        public string PaletteNameJpg2 { get => paletteNameJpg2; set => paletteNameJpg2 = value; }
        public string PaletteNameJpg3 { get => paletteNameJpg3; set => paletteNameJpg3 = value; }
        public string PaletteNameJpg4 { get => paletteNameJpg4; set => paletteNameJpg4 = value; }
        public double PerCent0 { get => perCent0; set => perCent0 = value; }
        public double PerCent1 { get => perCent1; set => perCent1 = value; }
        public double PerCent2 { get => perCent2; set => perCent2 = value; }
        public double PerCent3 { get => perCent3; set => perCent3 = value; }
        public double PerCent4 { get => perCent4; set => perCent4 = value; }
        public int Phi0 { get => phi0; set => phi0 = value; }
        public int Phi1 { get => phi1; set => phi1 = value; }
        public int Phi2 { get => phi2; set => phi2 = value; }
        public int Phi3 { get => phi3; set => phi3 = value; }
        public int Phi4 { get => phi4; set => phi4 = value; }
        public int Phi5 { get => phi5; set => phi5 = value; }
        public int Proportion0 { get => proportion0; set => proportion0 = value; }
        public int Proportion1 { get => proportion1; set => proportion1 = value; }
        public int Proportion2 { get => proportion2; set => proportion2 = value; }
        public int Proportion3 { get => proportion3; set => proportion3 = value; }
        public int Proportion4 { get => proportion4; set => proportion4 = value; }
        public int Proportion5 { get => proportion5; set => proportion5 = value; }
        public bool? RadioBtnFarbabstand { get => radioBtnFarbabstand; set => radioBtnFarbabstand = value; }
        public bool? RadioBtnFarbwert { get => radioBtnFarbwert; set => radioBtnFarbwert = value; }
        public bool? RadioBtnHellwert { get => radioBtnHellwert; set => radioBtnHellwert = value; }
        public bool? RadioBtnSaetigung { get => radioBtnSaetigung; set => radioBtnSaetigung = value; }
        public int TargetIndex0 { get => targetIndex0; set => targetIndex0 = value; }
        public int TargetIndex1 { get => targetIndex1; set => targetIndex1 = value; }
        public int TargetIndex2 { get => targetIndex2; set => targetIndex2 = value; }
        public int TargetIndex3 { get => targetIndex3; set => targetIndex3 = value; }
        public int TargetIndex4 { get => targetIndex4; set => targetIndex4 = value; }
        public int TextBoxX0 { get => textBoxX0; set => textBoxX0 = value; }
        public int TextBoxX1 { get => textBoxX1; set => textBoxX1 = value; }
        public int TextBoxX2 { get => textBoxX2; set => textBoxX2 = value; }
        public int TextBoxX3 { get => textBoxX3; set => textBoxX3 = value; }
        public int TextBoxX4 { get => textBoxX4; set => textBoxX4 = value; }
        public int TextBoxX5 { get => textBoxX5; set => textBoxX5 = value; }
        public int TextBoxY0 { get => textBoxY0; set => textBoxY0 = value; }
        public int TextBoxY1 { get => textBoxY1; set => textBoxY1 = value; }
        public int TextBoxY2 { get => textBoxY2; set => textBoxY2 = value; }
        public int TextBoxY3 { get => textBoxY3; set => textBoxY3 = value; }
        public int TextBoxY4 { get => textBoxY4; set => textBoxY4 = value; }
        public int TextBoxY5 { get => textBoxY5; set => textBoxY5 = value; }
        public int ColorReduce { get => colorReduce; set => colorReduce = value; }
        public int OriginalResize { get => originalResize; set => originalResize = value; }
        public bool? CheckboxPin0 { get => checkboxPin0; set => checkboxPin0 = value; }
        public bool? CheckboxPin1 { get => checkboxPin1; set => checkboxPin1 = value; }
        public bool? CheckboxPin2 { get => checkboxPin2; set => checkboxPin2 = value; }
        public bool? CheckboxPin3 { get => checkboxPin3; set => checkboxPin3 = value; }
        public bool? CheckboxPin4 { get => checkboxPin4; set => checkboxPin4 = value; }
    }
}