using Accord.Imaging;
using Accord.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConverterASCII
{
    //----------------------------------------------------------------------------------------------------------------------------
    //      Image Splitter Base
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Base class for Image splitting
    /// </summary>
    public abstract class ImageSplitterBase
    {
        /// <summary>
        /// amount of characters in Horizontal and Vertical direction
        /// </summary>
        public int HorizontalCount = 0;
        public int VerticalCount = 0;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns 2d array of ImageParts from Image based on font
        /// </summary>
        /// <param name="RawImage">Image that needs to be split</param>
        /// <param name="Width">Width of hte single ImagePart after splitting, for comparison use the same as font Width</param>
        /// <param name="Height">Height of hte single ImagePart after splitting, for comparison use the same as font Height</param>
        public abstract ImagePart[,] Split(Bitmap RawImage, int Width, int Height);
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Image Splitter
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Base class for splitting image int ImageParts
    /// </summary>
    public class ImageSplitter : ImageSplitterBase
    {
        /// <summary>
        /// constant values used for calculating grayscale value
        /// </summary>
        protected const double  RedCoefficient = 0.2125,
                                GreenCoefficient = 0.7154,
                                BlueCoefficient = 0.0721;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split depending on font size
        /// </summary>
        public ImageSplitter() { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be plist into W Images horizontally and resized verically to keep aspect ratio
        /// </summary>
        /// <param name="Width">Number of Horizontal characters</param>
        public ImageSplitter(int Width)
        {
            HorizontalCount = Width;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split into exactly W by H characters
        /// </summary>
        /// <param name="Width">number of Horizontal characters</param>
        /// <param name="Height">number of Vertical characters</param>
        public ImageSplitter(int Width, int Height)
        {
            HorizontalCount = Width;
            VerticalCount = Height;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns 2d array of ImageParts from Image based on font
        /// </summary>
        /// <param name="RawImage">Image that needs to be split</param>
        /// <param name="Width">Width of hte single ImagePart after splitting, for comparison use the same as font Width</param>
        /// <param name="Height">Height of hte single ImagePart after splitting, for comparison use the same as font Height</param>
        public override unsafe ImagePart[,] Split(Bitmap RawImage, int Width, int Height)
        {
            int HCount, VCount;

            //round HCount and VCoount to the nearest whole multiple of Width and Height
            if (HorizontalCount == 0 && VerticalCount == 0)
            {
                HCount = RawImage.Width % Width != 0 ? (RawImage.Width / Width) + 1 : RawImage.Width / Width;
                VCount = RawImage.Height % Height != 0 ? (RawImage.Height / Height) + 1 : RawImage.Height / Height;
            }
            //if HorizontalCount is set in constructor, set VCount to keep aspect ratio
            else if (VerticalCount == 0)
            {
                HCount = HorizontalCount;

                int DesiredHeight = (HCount * Width * RawImage.Height / RawImage.Width );
                VCount = DesiredHeight % Height != 0 ? (DesiredHeight / Height) + 1 : DesiredHeight / Height;
            }
            //if both HorizontalCount and VerticalCount have been set just use those
            else
            {
                HCount = HorizontalCount;
                VCount = VerticalCount;
            }

            //Create Subimages
            ImagePart[,] Result = new ImagePart[VCount, HCount];

            //resize original image to proper size and lock it
            ResizeBilinear resizer = new ResizeBilinear(HCount * Width, VCount * Height);
            Bitmap Resized = resizer.Apply(RawImage);

            BitmapData bData = Resized.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);

            //create help image
            UnmanagedImage destinationData = UnmanagedImage.Create(Width, Height, PixelFormat.Format8bppIndexed);


            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;

            
            //Special Case for when Image is already in grayscale
            if (srcPixelFormat == PixelFormat.Format8bppIndexed)
            {
                int srcOffset = sourceData.Stride - SourceWidth;
                int dstOffset = destinationData.Stride - Width;

                long TotalGray;
                byte* src, dst;

                for (int yOffset = 0; yOffset < VCount; yOffset++)
                {
                    for (int xOffset = 0; xOffset < HCount; xOffset++)
                    {
                        TotalGray = 0;

                        src = (byte*)sourceData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * Height) * Width) + (yOffset * Height * srcOffset);
                        dst = (byte*)destinationData.ImageData.ToPointer();

                        //copy the image part to destination in grayscle and count values
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                *dst = src[0];
                                TotalGray += src[0];

                                src++;
                                dst++;
                            }
                            src += srcOffset;
                            src += (SourceWidth - Width);

                            dst += dstOffset;
                        }

                        Result[yOffset, xOffset] = new ImagePart(   destinationData.ToManagedImage(true),
                                                                    (int)TotalGray / (Width * Height),
                                                                    (int)TotalGray / (Width * Height),
                                                                    (int)TotalGray / (Width * Height));
                    }
                }

                Resized.UnlockBits(bData);
                Resized.Dispose();
                destinationData.Dispose();

                return Result;
            }
           
            //defualt behaviour
            else if ((srcPixelFormat == PixelFormat.Format24bppRgb) || (srcPixelFormat == PixelFormat.Format32bppRgb) || (srcPixelFormat == PixelFormat.Format32bppArgb))
            {
                int pixelSize = (srcPixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                int srcOffset = sourceData.Stride - SourceWidth * pixelSize;
                int dstOffset = destinationData.Stride - Width;//- SourceWidth;

                int rc = (int)(0x10000 * RedCoefficient);
                int gc = (int)(0x10000 * GreenCoefficient);
                int bc = (int)(0x10000 * BlueCoefficient);

                // make sure sum of coefficients equals to 0x10000
                while (rc + gc + bc < 0x10000)
                {
                    bc++;
                }

                long TotalRed, TotalGreen, TotalBlue;
                byte* src, dst;

                //for each section of subimage
                for (int yOffset = 0; yOffset < VCount; yOffset++)
                {
                    for (int xOffset = 0; xOffset < HCount; xOffset++)
                    {
                        TotalRed = TotalGreen = TotalBlue = 0;

                        src = (byte*)sourceData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * Height) * Width * pixelSize) + (yOffset * Height * srcOffset);
                        dst = (byte*)destinationData.ImageData.ToPointer();

                        //copy the image part to destination in grayscle and count values
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                *dst = (byte)((rc * src[2] + gc * src[1] + bc * src[0]) >> 16);

                                TotalBlue += src[0];    //src[RGB.B];
                                TotalGreen += src[1];   //src[RGB.G];
                                TotalRed += src[2];     //src[RGB.R];

                                src += pixelSize;
                                dst++;
                            }
                            src += srcOffset;
                            src += (SourceWidth - Width) * pixelSize;

                            dst += dstOffset;
                        }

                        Result[yOffset, xOffset] = new ImagePart(   destinationData.ToManagedImage(true),
                                                                    (int)TotalRed / (Width * Height),
                                                                    (int)TotalGreen / (Width * Height),
                                                                    (int)TotalBlue / (Width * Height));
                    }
                }

                Resized.UnlockBits(bData);
                Resized.Dispose();
                destinationData.Dispose();

                return Result;
            }
            //throw exception if unsuported format
            else
            {
                throw new System.Exception("Unsupported image format");
            }
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //          Image Splitter Filter
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A base class for splittigng image with applied filters
    /// It first resize the image to the right size, performs filters on a copy of the bitmap and splits the filtered one but assigns colors of the original one
    /// </summary>
    public abstract class ImageSplitterFilter : ImageSplitterBase
    {
        protected int HCount, VCount;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split depending on font size
        /// </summary>
        public ImageSplitterFilter() { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be plist into W Images horizontally and resized verically to keep aspect ratio
        /// </summary>
        /// <param name="Width">Number of Horizontal characters</param>
        public ImageSplitterFilter(int Width)
        {
            HorizontalCount = Width;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split into exactly W by H characters
        /// </summary>
        /// <param name="Width">number of Horizontal characters</param>
        /// <param name="Height">number of Vertical characters</param>
        public ImageSplitterFilter(int Width, int Height)
        {
            HorizontalCount = Width;
            VerticalCount = Height;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Finds the proper size to fit all characters in Bitmaps and resizes them
        /// </summary>
        /// <param name="RawImage"></param>
        /// <param name="Resized"></param>
        /// <param name="FontWidth"></param>
        /// <param name="FontHeight"></param>
        protected Bitmap Resize(Bitmap RawImage, int FontWidth, int FontHeight)
        {
            //round HCount and VCoount to the nearest whole multiple of Width and Height
            if (HorizontalCount == 0 && VerticalCount == 0)
            {
                HCount = RawImage.Width % FontWidth != 0 ? (RawImage.Width / FontWidth) + 1 : RawImage.Width / FontWidth;
                VCount = RawImage.Height % FontHeight != 0 ? (RawImage.Height / FontHeight) + 1 : RawImage.Height / FontHeight;
            }
            //if HorizontalCount is set in constructor, set VCount to keep aspect ratio
            else if (VerticalCount == 0)
            {
                HCount = HorizontalCount;

                int DesiredHeight = (HCount * FontWidth * RawImage.Height / RawImage.Width);
                VCount = DesiredHeight % FontHeight != 0 ? (DesiredHeight / FontHeight) + 1 : DesiredHeight / FontHeight;
            }
            //if both HorizontalCount and VerticalCount have been set just use those
            else
            {
                HCount = HorizontalCount;
                VCount = VerticalCount;
            }

            //resize original image to proper size and lock it
            ResizeBilinear resizer = new ResizeBilinear(HCount * FontWidth, VCount * FontHeight);
            Bitmap Resized = resizer.Apply(RawImage);

            return Resized;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Applies specified filters to the Bitmap
        /// </summary>
        /// <param name="Resized"></param>
        protected virtual Bitmap Filter(Bitmap Resized)
        {
            return Resized;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns 2d array of ImageParts from Image based on font
        /// </summary>
        /// <param name="RawImage">Image that needs to be split</param>
        /// <param name="Width">Width of hte single ImagePart after splitting, for comparison use the same as font Width</param>
        /// <param name="Height">Height of hte single ImagePart after splitting, for comparison use the same as font Height</param>
        public override unsafe ImagePart[,] Split(Bitmap RawImage, int FontWidth, int FontHeight)
        {
            Bitmap Resized = Resize(RawImage, FontWidth, FontHeight);
            Bitmap Filtered = Filter(Resized);

            //Create Subimages
            ImagePart[,] Result = new ImagePart[VCount, HCount];

            //use sourceData for copying colors but filteredData for splitting
            BitmapData bData = Resized.LockBits(ImageLockMode.ReadOnly);
            BitmapData fData = Filtered.LockBits(ImageLockMode.ReadOnly);
            UnmanagedImage sourceData = new UnmanagedImage(bData);
            UnmanagedImage filteredData = new UnmanagedImage(fData);

            //create help image
            UnmanagedImage destinationData = UnmanagedImage.Create(FontWidth, FontHeight, PixelFormat.Format8bppIndexed);


            // get width and height
            int SourceWidth = sourceData.Width;
            int SourceHeight = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;

            //special case for when source image was grayscale
            if (srcPixelFormat == PixelFormat.Format8bppIndexed)
            {
                int sourceOffset = sourceData.Stride - SourceWidth;
                int filterOffset = filteredData.Stride - filteredData.Width;
                int dstOffset = destinationData.Stride - FontWidth;

                long TotalGray;
                byte* SourcePtr, DstPtr, FilterPtr;

                //for each section of subimage
                for (int yOffset = 0; yOffset < VCount; yOffset++)
                {
                    for (int xOffset = 0; xOffset < HCount; xOffset++)
                    {
                        TotalGray = 0;

                        SourcePtr = (byte*)sourceData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * FontHeight) * FontWidth) + (yOffset * FontHeight * sourceOffset);
                        FilterPtr = (byte*)filteredData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * FontHeight) * FontWidth) + (yOffset * FontHeight * filterOffset);
                        DstPtr = (byte*)destinationData.ImageData.ToPointer();

                        //copy the image part to destination in grayscle and count values
                        for (int y = 0; y < FontHeight; y++)
                        {
                            for (int x = 0; x < FontWidth; x++)
                            {
                                *DstPtr = (byte)FilterPtr[0];

                                TotalGray += SourcePtr[0];

                                SourcePtr++;
                                FilterPtr++;
                                DstPtr++;
                            }
                            SourcePtr += sourceOffset;
                            SourcePtr += SourceWidth - FontWidth;

                            FilterPtr += filterOffset;
                            FilterPtr += SourceWidth - FontWidth;

                            DstPtr += dstOffset;
                        }

                        Result[yOffset, xOffset] = new ImagePart(destinationData.ToManagedImage(true),
                                                                    (int)TotalGray / (FontWidth * FontHeight),
                                                                    (int)TotalGray / (FontWidth * FontHeight),
                                                                    (int)TotalGray / (FontWidth * FontHeight));
                    }
                }

                Resized.UnlockBits(bData);
                Resized.Dispose();

                Filtered.UnlockBits(fData);
                Filtered.Dispose();

                destinationData.Dispose();

                return Result;

            }
            //default case for RGB images
            else if ((srcPixelFormat == PixelFormat.Format24bppRgb) || (srcPixelFormat == PixelFormat.Format32bppRgb) || (srcPixelFormat == PixelFormat.Format32bppArgb))
            {
                int pixelSize = (srcPixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                int sourceOffset = sourceData.Stride - SourceWidth * pixelSize;
                int filterOffset = filteredData.Stride - filteredData.Width;
                int dstOffset = destinationData.Stride - FontWidth;


                long TotalRed, TotalGreen, TotalBlue;
                byte* SourcePtr, DstPtr, FilterPtr;

                //for each section of subimage
                for (int yOffset = 0; yOffset < VCount; yOffset++)
                {
                    for (int xOffset = 0; xOffset < HCount; xOffset++)
                    {
                        TotalRed = TotalGreen = TotalBlue = 0;

                        SourcePtr = (byte*)sourceData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * FontHeight) * FontWidth * pixelSize) + (yOffset * FontHeight * sourceOffset);
                        FilterPtr = (byte*)filteredData.ImageData.ToPointer() + ((xOffset + yOffset * HCount * FontHeight) * FontWidth) + (yOffset * FontHeight * filterOffset);
                        DstPtr = (byte*)destinationData.ImageData.ToPointer();

                        //copy the image part to destination in grayscle and count values
                        for (int y = 0; y < FontHeight; y++)
                        {
                            for (int x = 0; x < FontWidth; x++)
                            {
                                *DstPtr = (byte)FilterPtr[0];

                                TotalBlue += SourcePtr[0];
                                TotalGreen += SourcePtr[1];
                                TotalRed += SourcePtr[2];

                                SourcePtr += pixelSize;
                                FilterPtr++;
                                DstPtr++;
                            }
                            SourcePtr += sourceOffset;
                            SourcePtr += (SourceWidth - FontWidth) * pixelSize;

                            FilterPtr += filterOffset;
                            FilterPtr += SourceWidth - FontWidth;

                            DstPtr += dstOffset;
                        }

                        Result[yOffset, xOffset] = new ImagePart(destinationData.ToManagedImage(true),
                                                                    (int)TotalRed / (FontWidth * FontHeight),
                                                                    (int)TotalGreen / (FontWidth * FontHeight),
                                                                    (int)TotalBlue / (FontWidth * FontHeight));
                    }
                }

                Resized.UnlockBits(bData);
                Resized.Dispose();

                Filtered.UnlockBits(fData);
                Filtered.Dispose();

                destinationData.Dispose();

                return Result;
            }
            else
            {
                throw new Exception("Unsuported Image format");
            }
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    // Image Splitter Edge
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Before splitting image applies Canny Edge detection to the oroginal image
    /// </summary>
    public class ImageSplitterEdge : ImageSplitterFilter
    {
        /// <summary>
        /// Low and High Threshold values for the Canny Edge
        /// </summary>
        public byte LowByte = 20;
        public byte HighByte = 100;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split depending on font size
        /// </summary>
        public ImageSplitterEdge() { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be plist into W Images horizontally and resized verically to keep aspect ratio
        /// </summary>
        /// <param name="Width">Number of Horizontal characters</param>
        public ImageSplitterEdge(int Width) : base(Width) { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Image will be split into exactly W by H characters
        /// </summary>
        /// <param name="Width">number of Horizontal characters</param>
        /// <param name="Height">number of Vertical characters</param>
        public ImageSplitterEdge(int Width, int Height) : base(Width, Height) { }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Applies specified filters to the Bitmap
        /// </summary>
        /// <param name="Resized"></param>
        protected override Bitmap Filter(Bitmap Resized)
        {
            //Apply Canny Edge to the image
            Bitmap Filtered = new Bitmap(Resized);

            if (Filtered.PixelFormat != PixelFormat.Format8bppIndexed)
                Filtered = Grayscale.CommonAlgorithms.BT709.Apply(Filtered);

            RobinsonEdgeDetector robinson = new RobinsonEdgeDetector();
            Filtered = robinson.Apply(Filtered);
            CannyEdgeDetector canny = new CannyEdgeDetector(LowByte, HighByte);
            canny.ApplyInPlace(Filtered);

            return Filtered;
        }

    }
}
