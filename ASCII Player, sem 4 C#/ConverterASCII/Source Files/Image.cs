using System.Drawing;


namespace ConverterASCII
{
    //----------------------------------------------------------------------------------------------------------------------------
    //      ImagePart
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores a part of an Image of the same size as CharTemplate
    /// Image is stored in grayscale along avarage color and avarage gray
    /// </summary>
    public class ImagePart
    {
        /// <summary>
        /// Grayscale fraction of original Image that can be easily compared to CharTemplate Bitmap
        /// </summary>
        public readonly Bitmap SubImage;
        /// <summary>
        /// The Average RGB color of the original SubImage
        /// the smaller the size the more accurate it is
        /// </summary>
        public readonly Color AvgColor;
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Create ImagePart object from bmp
        /// </summary>
        /// <param name="bmp">Bitmap from which the Image will be created</param>
        public ImagePart(Bitmap bmp, int avgRed, int avgGreen, int avgBlue)
        {
            SubImage = bmp;
            AvgColor = Color.FromArgb(avgRed, avgGreen, avgBlue);
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Image
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores 2D array of ImageParts and 2D array of best matches for each Imageparts 
    /// If the best representant for ImagePart hasnt been yet found it's stored as null
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Dimension of the ImageParts array
        /// </summary>
        public int HorizontalCount { get; private set; }
        public int VerticalCount { get; private set; }
        /// <summary>
        /// 2D array of ImageParts
        /// Used for choosing best CharTemplate for proper Image part
        /// </summary>
        public ImagePart[,] ImageParts { get; private set; }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Splits RawImage into bitmaps of Width and Height dimension and stores them as ImageParts
        /// </summary>
        /// <param name="Width">The width of the ImagePart object</param>
        /// <param name="Height">The height of the ImagePart object</param>
        /// <param name="RawImage">Bitmap that is split into multiple ImageParts</param>
        public Image(ImagePart[,] parts)
        {
            ImageParts = parts;

            HorizontalCount = parts.GetLength(1);
            VerticalCount = parts.GetLength(0);
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Saves all ImageParts into specified Directory
        /// </summary>
        /// <param name="Directory">The Directory in which to save</param>
        public void Save(string Directory)
        {
            for (int y = 0; y < VerticalCount; y++)
            {
                for (int x = 0; x < HorizontalCount; x++)
                {
                    ImageParts[y, x].SubImage.Save(Directory + @"\Img " + y + " " + x + @".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }
    }
}