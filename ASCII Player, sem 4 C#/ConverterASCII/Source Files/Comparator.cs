

using Accord.Imaging;
using Accord.Statistics.Distributions.Univariate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConverterASCII
{

    //----------------------------------------------------------------------------------------------------------------------------
    //      Comparator Base
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A base class used for comparing ImageParts with CharTemplates
    /// </summary>
    public abstract class ComparatorBase
    {
        /// <summary>
        /// Lets dervied classes prepare charcaters list for easier comparasion
        /// </summary>
        /// <param name="characters">Character list that will be used for comparing the image</param>
        public virtual void Prepare(List<CharTemplate> characters) { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from its internal set
        /// call Prepare method to initialize its structure and then evertime a CharList is being changed
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <returns></returns>
        public abstract CharTemplate Compare(ImagePart image);
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Comparator Grayscale
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Compares Image with character by mean gray value
    /// </summary>
    public class ComparatorGrayscale : ComparatorBase
    {
        /// <summary>
        /// list of CharTemplates used for comparison
        /// </summary>
        private Dictionary<CharTemplate, double> CharList;

        /// <summary>
        /// constant values used for calculating grayscale value
        /// </summary>
        protected const double RedCoefficient = 0.2125,
                                GreenCoefficient = 0.7154,
                                BlueCoefficient = 0.0721;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calculates average Grayscale for given bitmap
        /// </summary>
        /// <param name="bmp">bitmap that Grayscale value is calculated for</param>
        /// <returns></returns>
        protected unsafe double GetGrayscale(Bitmap bmp)
        {
            BitmapData bData = bmp.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);

            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;

            //get proper offest to compensate for Stride
            int srcOffset = sourceData.Stride - SourceWidth;
            byte* src = (byte*)sourceData.ImageData.ToPointer();

            //calculate total sum for each segment
            double Result = 0;
            for (int y = 0; y < SourceHeight; y++)
            {
                for (int x = 0; x < SourceWidth; x++)
                {
                    Result += src[0];
                    src++;
                }
                src += srcOffset;
            }

            //free resources
            bmp.UnlockBits(bData);
            return Result / (SourceWidth * SourceHeight);
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets characters for later comparisions and calculates average grayscale for each character
        /// </summary>
        /// <param name="characters"></param>
        public override void Prepare(List<CharTemplate> characters)
        {
            CharList = new Dictionary<CharTemplate, double>();
            foreach(var ch in characters)
            {
                CharList.Add(ch, GetGrayscale(ch.CharImage));
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from characters list
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <param name="characters">list of available characters</param>
        /// <returns></returns>
        public override CharTemplate Compare(ImagePart image)
        {
            //We could get grayscale from Average Color in image but that only is a good idea 
            //if the gray from source is the same as gray from average color which in case of Edge plitting and perhaps others will not
            double ImageGrayscale = GetGrayscale(image.SubImage);
            CharTemplate closest = null;
            double SmallestDifference = double.MaxValue;

            foreach (var ch in CharList)
            {
                double Difference = Math.Abs(ch.Value - ImageGrayscale);
                if (Difference < SmallestDifference)
                {
                    closest = ch.Key;
                    SmallestDifference = Difference;
                }
            }
            return closest;
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Comparator Exact
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// /compares characters based on total difference between each segment and character
    /// gives best resulat but is extremly inefficient
    /// </summary>
    public class ComparatorExact : ComparatorBase
    {
        /// <summary>
        /// list of CharTemplates used for comparison
        /// </summary>
        private List<CharTemplate> CharList;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets characters for later comparisions
        /// </summary>
        /// <param name="characters"></param>
        public override void Prepare(List<CharTemplate> characters)
        {
            CharList = characters;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from its internal set
        /// call Prepare method to initialize its structure and then evertime a CharList is being changed
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <returns></returns>
        public unsafe override CharTemplate Compare(ImagePart image)
        {
            CharTemplate closest = null;
            double SmallestDifference = double.MaxValue;


            //lock bitmap and create unmanaged image
            BitmapData bData = image.SubImage.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);

            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;
            int srcOffset = sourceData.Stride - SourceWidth;

            foreach(var ch in CharList)
            {
                //get unmanaged image from char image 
                BitmapData CharData = ch.CharImage.LockBits(ImageLockMode.ReadOnly);
                UnmanagedImage CharImage = new UnmanagedImage(CharData);
                int CharOffset = CharImage.Stride - CharImage.Width;

                byte* CharPtr = (byte*)CharImage.ImageData.ToPointer();
                byte* SourcePtr = (byte*)sourceData.ImageData.ToPointer();

                //calculate total sum for each segment
                double Difference = 0;            
                for (int y = 0; y < SourceHeight; y++)
                {
                    for (int x = 0; x < SourceWidth; x++)
                    {
                        Difference += Math.Abs(CharPtr[0] - SourcePtr[0]);

                        SourcePtr++;
                        CharPtr++;
                    }
                    SourcePtr += srcOffset;
                    CharPtr += CharOffset;
                }
                //free Char Image
                ch.CharImage.UnlockBits(CharData);

                if(Difference < SmallestDifference)
                {
                    SmallestDifference = Difference;
                    closest = ch;
                }
            }

            image.SubImage.UnlockBits(bData);
            return closest;
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Comparator Centralized
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Comparasion is weighted sum of different areas, center, top, botom, left, right
    /// All areas are avarege grayscale of that region, central area has the greatest weight
    /// Gives simmilar result to ComparatorExact with complexity of ComparatorGrayscale
    /// Requires prepearing data for characters
    /// </summary>
    public class ComparatorCentralized : ComparatorBase
    {
        /// <summary>
        /// stores grayscale values of the Center, Up, Down, Left, Right areas for the proper CharTemplate 
        /// </summary>
        private Dictionary<CharTemplate,Tuple<double, double, double, double, double>> CharValues;

        /// <summary>
        /// Set of default values, for when the current ones have been changed and we want to restore default value
        /// </summary>
        public static readonly double DefaultLeftRightMargin = 0.25;
        public static readonly double DefaultUpDownMargin = 0.25;
        public static readonly int DefaultCenterWeight = 4;
        public static readonly int DefaultLeftRightWeight = 2;
        public static readonly int DefaultUpDownWeight = 1;

        /// <summary>
        /// Values useed for caculating margins and their weights
        /// </summary>
        public double LeftRightMargin = DefaultLeftRightMargin;
        public double UpDownMargin = DefaultUpDownMargin;
        public int CenterWeight = DefaultCenterWeight;
        public int LeftRightWeight = DefaultLeftRightWeight;
        public int UpDownWeight = DefaultUpDownWeight;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initialzies margins and weights with defualt values
        /// </summary>
        public ComparatorCentralized() { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes Weights of cells with custom values
        /// </summary>
        /// <param name="Cweight"></param>
        /// <param name="LRweight"></param>
        /// <param name="UDweight"></param>
        public ComparatorCentralized(int Cweight, int LRweight, int UDweight)
        {
            CenterWeight = Cweight;
            LeftRightWeight = LRweight;
            UpDownWeight = UDweight;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// initializes Margins and weights of cells with custom values
        /// </summary>
        /// <param name="LRmargin">percent of image to use for Left and Right Margins</param>
        /// <param name="UDmargin">percent of image to use for Up and Down Margins</param>
        /// <param name="Cweight">weighted value of central part of the image</param>
        /// <param name="LRweight">weighted value for Left and Right part of the image</param>
        /// <param name="UDweight">weighted value for Up and Down part of the image</param>
        public ComparatorCentralized(int Cweight, int LRweight, int UDweight, double LRmargin, double UDmargin)
        {
            LeftRightMargin = LRmargin;
            UpDownMargin = UDmargin;

            CenterWeight = Cweight;
            LeftRightWeight = LRweight;
            UpDownWeight = UDweight;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns a Tuple of avarage grayscale for Center, Up, Down, Left, Right
        /// </summary>
        /// <param name="bmp">grayscale bitmap for witch to generate the Tuple</param>
        /// <returns></returns>
        unsafe private Tuple<double, double, double, double, double> Centralize(Bitmap bmp)
        {
            //lock bitmap and create unmanaged image
            BitmapData bData = bmp.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);

            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;

            //get proper pixel size and offest to compensate for Stride
            int srcOffset = sourceData.Stride - SourceWidth;

            double Center = 0, Up = 0, Down = 0, Left = 0, Right = 0;
            int CenterCount = 0, UpCount = 0, DownCount = 0, LeftCount = 0, RightCount = 0;
            byte* src = (byte*)sourceData.ImageData.ToPointer();

            //for each section of subimage
            for (int y = 0; y < SourceHeight; y++)
            {
                for (int x = 0; x < SourceWidth; x++)
                {
                    if (x <= Math.Ceiling(SourceWidth * LeftRightMargin))
                    {
                        Left += (int)(src[0]);
                        LeftCount++;
                    }
                    else if (x >= Math.Floor(SourceWidth * (1-LeftRightMargin)))
                    {
                        Right += (int)(src[0]);
                        RightCount++;
                    }

                    if (y <= Math.Ceiling(SourceHeight * UpDownMargin))
                    {
                        Up += (int)(src[0]);
                        UpCount++;
                    }
                    else if (y >= Math.Floor(SourceHeight * (1-UpDownMargin)))
                    {
                        Down += (int)(src[0]);
                        DownCount++;
                    }

                    if (x >= Math.Floor(SourceWidth * LeftRightMargin) && x <= Math.Ceiling(SourceWidth * (1-LeftRightMargin))
                        && y >= Math.Floor(SourceHeight * UpDownMargin) && y <= Math.Ceiling(SourceHeight * (1-UpDownMargin)))
                    {
                        Center += (int)(src[0]);
                        CenterCount++;
                    }
                    src++;
                }
                src += srcOffset;
            }

            bmp.UnlockBits(bData);
            return Tuple.Create(Center/CenterCount, Up/UpCount, Down/DownCount, Left/LeftCount, Right/RightCount);
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates touple for every character in chatracters list and stores them in dictionary
        /// </summary>
        /// <param name="characters">Character list that will be used for comparing the image</param>
        public override void Prepare(List<CharTemplate> characters)
        {
            CharValues = new Dictionary<CharTemplate, Tuple<double, double, double, double, double>>();
            foreach(var ch in characters)
            {
                CharValues.Add(ch, Centralize(ch.CharImage));
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from its internal set
        /// call Prepare method to initialize its structure and then evertime a CharList is being changed
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <returns></returns>
        public override CharTemplate Compare(ImagePart image)
        {
            var ImageRegions = Centralize(image.SubImage);
            CharTemplate Closest = null;
            double SmallestDifference = double.MaxValue;

            foreach(var ch in CharValues)
            {
                double Difference = Math.Abs(ch.Value.Item1 - ImageRegions.Item1) * CenterWeight
                              + (Math.Abs(ch.Value.Item2 - ImageRegions.Item2) + Math.Abs(ch.Value.Item3 - ImageRegions.Item3)) * UpDownWeight
                              + (Math.Abs(ch.Value.Item4 - ImageRegions.Item4) + Math.Abs(ch.Value.Item5 - ImageRegions.Item5)) * LeftRightWeight;

                if(Difference < SmallestDifference)
                {
                    SmallestDifference = Difference;
                    Closest = ch.Key;
                }
            }

            return Closest;
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Comparator Cellular
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Compares by splitting image into grid and using average grayscale of each cell
    /// To make sure it gives 'resonable' results make sure that fontsize is not smaller than gridsize
    /// </summary>
    public class ComparatorCellular : ComparatorBase
    {
        /// <summary>
        /// stores grayscale value for each of 9 cells in each ChrTemplate
        /// </summary>
        protected Dictionary<CharTemplate, double[,]> CharValues;

        /// <summary>
        /// Set of default values, for when the current ones have been changed and we want to restore default value
        /// </summary>
        public static readonly int DefaultGridX = 3;
        public static readonly int DefaultGridY = 3;

        /// <summary>
        /// number of Horizontal and Vertical cells
        /// </summary>
        public int GridX = 3;
        public int GridY = 3;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes number of X and Y cells with defualt values
        /// </summary>
        public ComparatorCellular() { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Assigns custom numbers of X and Y cells used for somparing
        /// </summary>
        /// <param name="X">number of cells in X dimension</param>
        /// <param name="Y">number of cells in Y dimension</param>
        public ComparatorCellular(int X, int Y)
        {
            GridX = X;
            GridY = Y;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates 2D Array of Average Grayscale Values
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        unsafe protected double[,] GetValues(Bitmap bmp)
        {
            double[,] Result = new double[GridY, GridX];
            int[,] Count = new int[GridY, GridX];

            //lock bitmap and create unmanaged image
            BitmapData bData = bmp.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);

            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;

            //get proper offest to compensate for Stride
            int srcOffset = sourceData.Stride - SourceWidth;
            byte* src = (byte*)sourceData.ImageData.ToPointer();

            //calculate total sum for each segment
            for (int y = 0; y < SourceHeight; y++)
            {
                for (int x = 0; x < SourceWidth; x++)
                {
                    Result[(y * GridY / bmp.Height), (x * GridX / bmp.Width)] += src[0];
                    Count[(y * GridY / bmp.Height), (x * GridX / bmp.Width)]++;

                    src++;
                }
                src += srcOffset;
            }

            //avareage the sum by the number of pixels
            for (int y = 0; y < GridY; y++)
            {
                for (int x = 0; x < GridX; x++)
                {
                    //if number of cells is higher than the font size some parts may never be countes
                    if (Count[y, x] != 0)
                        Result[y, x] /= Count[y, x];
                }
            }

            bmp.UnlockBits(bData);
            return Result;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// creates 2D array for each bitmap of character and stores them in dictionary
        /// </summary>
        /// <param name="characters">List of CharTemplates for which to generate arrays</param>
        public override void Prepare(List<CharTemplate> characters)
        {
            CharValues = new Dictionary<CharTemplate, double[,]>();

            foreach(var ch in characters)
            {
                CharValues.Add(ch, GetValues(ch.CharImage));
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from its internal set
        /// call Prepare method to initialize its structure and then evertime a CharList is being changed
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <returns></returns>
        public override CharTemplate Compare(ImagePart image)
        {
            double[,] imageValues = GetValues(image.SubImage);
            CharTemplate Closest = null;
            double SmallestDifference = double.MaxValue;

            //calculate difference between arrays for each character
            foreach (var ch in CharValues)
            {
                double Difference = 0;
                for (int y = 0; y < GridY; y++)
                {
                    for (int x = 0; x < GridX; x++)
                    {
                        Difference += Math.Abs(imageValues[y, x] - ch.Value[y, x]);
                    }
                }

                if (Difference < SmallestDifference)
                {
                    SmallestDifference = Difference;
                    Closest = ch.Key;
                }
            }

            return Closest;

        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    // Comparator Gaussian
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Uses the same way for generating internal structare as ComparatorCellular
    /// When comparing applies Gaussian Distribution to the grid so the values in the middle are the most relevant
    /// </summary>
    public class ComparatorGaussian : ComparatorCellular
    {
        /// <summary>
        /// Normal Distribution containing all GridCells within 1 Normal Distribution from the middle
        /// </summary>
        private NormalDistribution NormalX, NormalY;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes number of X and Y cells with defualt values
        /// </summary>
        public ComparatorGaussian() : base()
        {
            NormalX = new NormalDistribution(((double)GridX) / 2, ((double)GridX) / 2);
            NormalY = new NormalDistribution(((double)GridY) / 2, ((double)GridY) / 2);
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Assigns custom numbers of X and Y cells used for somparing
        /// </summary>
        /// <param name="X">number of cells in X dimension</param>
        /// <param name="Y">number of cells in Y dimension</param>
        public ComparatorGaussian(int X, int Y): base(X,Y)
        {
            NormalX = new NormalDistribution(((double)GridX) / 2, ((double)GridX) / 2);
            NormalY = new NormalDistribution(((double)GridY) / 2, ((double)GridY) / 2);
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns the most similar CharTemplate to ImagePart from its internal set
        /// call Prepare method to initialize its structure and then evertime a CharList is being changed
        /// </summary>
        /// <param name="image">subimage that is being compared to</param>
        /// <returns></returns>
        public override CharTemplate Compare(ImagePart image)
        {
            double[,] imageValues = GetValues(image.SubImage);
            CharTemplate Closest = null;
            double SmallestDifference = double.MaxValue;

            //calculate difference between arrays for each character
            foreach (var ch in CharValues)
            {
                double Difference = 0;
                for (int y = 0; y < GridY; y++)
                {
                    for (int x = 0; x < GridX; x++)
                    {
                        Difference += (Math.Abs(imageValues[y, x] - ch.Value[y, x])) * NormalX.ProbabilityDensityFunction(x) * NormalY.ProbabilityDensityFunction(y);
                    }
                }

                if (Difference < SmallestDifference)
                {
                    SmallestDifference = Difference;
                    Closest = ch.Key;
                }
            }

            return Closest;
        }
    }
}