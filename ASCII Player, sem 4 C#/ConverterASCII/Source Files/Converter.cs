using Accord.Imaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace ConverterASCII
{
    //----------------------------------------------------------------------------------------------------------------------------
    //      Converter
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// A class that wraps around the image, characters and a comparator
    /// </summary>
    public class Converter
    {
        /// <summary>
        /// Comparator used for comparing SubImages with font template
        /// </summary>
        //changing comparator doesn't do anything as ImageParts and FontTemplate are not modified
        public ComparatorBase Comparator
        {
            get => _Comparator;
            set
            {
                if (value == null)
                    return;

                _ComparatorReady = false;

                _Comparator = value;
            }
        }
        private ComparatorBase _Comparator;

        /// <summary>
        /// internal variable that signalizes wether we need to use Comparator.Prepare before converting
        /// gets changed to true after prepearing and false when changing comparator and Font fields
        /// </summary>
        private bool _ComparatorReady = false;

        /// <summary>
        /// splitter used for generating ImageParts
        /// </summary>
        //Changing splitter means new ImagePArts have to be generated from bitmap
        public ImageSplitterBase Splitter
        {
            get => _Splitter;
            set
            {
                if (value == null)
                    return;

                Image = null;

                _Splitter = value;
            }
        }
        private ImageSplitterBase _Splitter;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// font in which the ASCII result will be generated
        /// </summary>
        //changing anything font realated means new FontTemplate has to be generated
        //if size of the font have changes also needs to set image to null so ImageSplitter splits it into proper size
        public Font Font
        {
            get => _Font;
            set
            {
                if (value == null)
                    return;

                FontTemplate = null;
                _ComparatorReady = false;
                if (_Font == null || value.Size != _Font.Size || value.Unit != _Font.Unit)
                    Image = null;

                _Font = value;
            }
        }
        private Font _Font;

        /// <summary>
        /// The color of the actual text that is being drawn
        /// </summary>
        public Color Foreground
        {
            get => _Foreground;
            set
            {
                if (value == null)
                    return;

                FontTemplate = null;
                _ComparatorReady = false;

                _Foreground = value;
            }
        }
        private Color _Foreground = Color.White;

        /// <summary>
        /// The color of the Background that characters are being drawn on
        /// </summary>
        public Color Background
        {
            get => _Background;
            set
            {
                if (value == null)
                    return;

                FontTemplate = null;
                _ComparatorReady = false;

                _Background = value;
            }
        }
        private Color _Background = Color.Black;

        /// <summary>
        /// characters that can be used for generating font
        /// if null, uses defualt value
        /// </summary>
        //Characters can be null, delete FontTemplate on change
        public string Characters
        {
            get => _Characters;
            set
            {
                FontTemplate = null;
                _ComparatorReady = false;

                _Characters = value;
            }
        }
        private string _Characters = null;

        /// <summary>
        /// Bitmap that is being converted to the ASCII
        /// </summary>
        //
        public Bitmap Bitmap
        {
            get => _Bitmap;
            set
            {
                if (value == null)
                    return;

                Image = null;

                _Bitmap = value;
            }
        }
        private Bitmap _Bitmap;

       

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Containter for Subimages
        /// </summary>
        private Image Image;

        /// <summary>
        /// container for bitmaps of Characters
        /// </summary>
        private FontTemplate FontTemplate;

        /// <summary>
        /// 2D array of the same size as ImageParts
        /// Holds the best CharTemplate found for the ImagePart at the same index
        /// Holds null if CharTemplate hasnt been found 
        /// </summary>
        private CharTemplate[,] ImageChars;


        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Converter() { }

        /// <summary>
        /// Generates ImageParts and CharTemplates based on font and bmp
        /// Uses default white and black colors for drawing characters
        /// </summary>
        /// <param name="bmp">Bitmap that will be turned into ASCII</param>
        /// <param name="font">font used for generating ASCII art. Needs to be monospace to work properly</param>
        /// <param name="comparator">Comparator used for finding best match between ImageParts and CharTemplates</param>
        /// <param name="splitter">Splitter used for creating ImageParts from Bitmap</param>
        public Converter(Bitmap bmp, Font font, ComparatorBase comparator, ImageSplitterBase splitter)
        {
            Comparator = comparator;
            Splitter = splitter;
            Font = font;
            Bitmap = bmp;  
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates ImageParts and CharTemplates based on font and bmp in Foregrounf and Background colors
        /// </summary>
        /// <param name="bmp">Bitmap that will be turned into ASCII</param>
        /// <param name="font">font used for generating ASCII art. Needs to be monospace to work properly</param>
        /// <param name="comparator">Comparator used for finding best match between ImageParts and CharTemplates</param>
        /// <param name="splitter">Splitter used for creating ImageParts from Bitmap</param>
        /// <param name="Foreground">The color of actual character</param>
        /// <param name="Background">the color of background on which characters will be rendered</param>
        /// <param name="characters">List of characters used in font</param>
        public Converter(Bitmap bmp, Font font, ComparatorBase comparator, ImageSplitterBase splitter, Color Foreground, Color Background)
        {
            Comparator = comparator;
            Splitter = splitter;
            this.Foreground = Foreground;
            this.Background = Background;
            Font = font;
            Bitmap = bmp;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates ImageParts and CharTemplates based on font and bmp in Foregrounf and Background colors
        /// </summary>
        /// <param name="bmp">Bitmap that will be turned into ASCII</param>
        /// <param name="font">font used for generating ASCII art. Needs to be monospace to work properly</param>
        /// <param name="comparator">Comparator used for finding best match between ImageParts and CharTemplates</param>
        /// <param name="splitter">Splitter used for creating ImageParts from Bitmap</param>
        /// <param name="Foreground">The color of actual character</param>
        /// <param name="Background">the color of background on which characters will be rendered</param>
        /// <param name="characters">List of characters used in font</param>
        public Converter(Bitmap bmp, Font font, ComparatorBase comparator, ImageSplitterBase splitter,  Color Foreground, Color Background, string characters)
        {
            Comparator = comparator;
            Splitter = splitter;
            this.Foreground = Foreground;
            this.Background = Background;
            Font = font;
            Characters = characters;
            Bitmap = bmp;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// finds best match for each ImagePart
        /// </summary>
        public void Apply()
        {
            //instead of generating all the FontTemplate and Image only do for those that are null
            //changing Bitmap, Font, Colors etc will set the proper fields to null automaticlly

            if(FontTemplate == null)
                FontTemplate = new FontTemplate(Font, Foreground, Background, Characters);

            if(Image == null)
                   Image = new Image(Splitter.Split(Bitmap, FontTemplate.Width, FontTemplate.Height));

            if (!_ComparatorReady)
            {
                Comparator.Prepare(FontTemplate.CharList);
                _ComparatorReady = true;
            }

            ImageChars = new CharTemplate[Image.VerticalCount, Image.HorizontalCount];
            for (int y = 0; y < Image.VerticalCount; y++)
            {
                for (int x = 0; x < Image.HorizontalCount; x++)
                {
                    ImageChars[y, x] = Comparator.Compare(Image.ImageParts[y, x]);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// creates a string representation
        /// </summary>
        public string ConvertToString()
        {
            if (Image == null)
                return "";

            StringBuilder output = new StringBuilder(Image.VerticalCount*(Image.HorizontalCount+1));

            for (int y = 0; y < Image.VerticalCount; y++)
            {
                for (int x = 0; x < Image.HorizontalCount; x++)
                {
                    output.Append(ImageChars[y, x].Character);
                }
                output.Append('\n');
            }
            output.Remove(output.Length - 1, 1);
            return output.ToString();
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates a html source code in color for the string representation
        /// </summary>
        /// <returns></returns>
        public string ConvertToStringColored()
        {
            int Size = "<font color=\"#585046\">)</font>".Length;
            int Extra = "<body style=\"background - color:#000000;\"><pre>".Length * 2;

            StringBuilder output = new StringBuilder(Image.VerticalCount * (Image.HorizontalCount * Size + 4) + Extra);
            output.Append("<body style=\"background-color:#"
                + Background.R.ToString("X2")
                + Background.G.ToString("X2")
                + Background.B.ToString("X2")
                + ";\"><pre>");

            for (int y = 0; y < Image.VerticalCount; y++)
            {
                for (int x = 0; x < Image.HorizontalCount; x++)
                {
                    output.Append("<font color=\"#"
                        + Image.ImageParts[y, x].AvgColor.R.ToString("X2")
                        + Image.ImageParts[y, x].AvgColor.G.ToString("X2")
                        + Image.ImageParts[y, x].AvgColor.B.ToString("X2")
                        + "\">"
                        + ImageChars[y, x].Character
                        + "</font>");
                }
                output.Append("<br>");
            }

            output.Append("</pre></body>");
            return output.ToString();
        }

        //----------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Creates a Bitmap of the internal image in proper font
        /// </summary>
        public Bitmap ConvertToBitmap()
        {
            string txt = ConvertToString();

            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            Size s = TextRenderer.MeasureText(g, txt, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            //create bitmap and render text onto it
            bmp = new Bitmap(s.Width, s.Height);
            g = Graphics.FromImage(bmp);
            TextRenderer.DrawText(g, txt, Font, new Point(0, 0), Foreground, Background, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            g.DrawImage(bmp, s.Width, s.Height);
            g.Dispose();

            return bmp;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a Bitmap of the internal image in proper font
        /// Each Character in the image is of the avarego color of the ImagePart (therefore Foreground color doesnt matter)
        /// <param name="Fast">Draws image with lower quality but is magnitude times faster. Enabled by Default</param>
        /// </summary>
        unsafe public Bitmap ConvertToBitmapColored(bool Fast = true)
        {
            //Create Image of the proper size
            Bitmap Result = new Bitmap(FontTemplate.Width * Image.HorizontalCount, FontTemplate.Height * Image.VerticalCount);

            Graphics g = Graphics.FromImage(Result);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            if (Fast == true)
            {
                Bitmap tmp = new Bitmap(Result.Width, Result.Height);
                Graphics TextG = Graphics.FromImage(tmp);

                //draw Backgrounds separatly
                for (int y = 0; y < Image.VerticalCount; y++)
                {
                    for (int x = 0; x < Image.HorizontalCount; x++)
                    {
                        //draws rectangle of specified color that acts as a background
                        SolidBrush brush = new SolidBrush(Image.ImageParts[y, x].AvgColor);
                        g.FillRectangle(brush, x * FontTemplate.Width, y * FontTemplate.Height, FontTemplate.Width, FontTemplate.Height);
                    }
                }

                //draws opaqe text over the graphics
                string txt = ConvertToString();
                TextRenderer.DrawText(TextG, txt, Font, new Point(0, 0), Color.FromArgb(255,255,255,255), Color.Transparent, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextG.DrawImage(tmp, 0,0);


                //inverse transparency of the text
                BitmapData bdata = tmp.LockBits(ImageLockMode.ReadWrite);
                UnmanagedImage unmanaged = new UnmanagedImage(bdata);

                int Offset = unmanaged.Stride - unmanaged.Width * 4;
                byte* Ptr = (byte*)unmanaged.ImageData.ToPointer();

                for (int y = 0; y < unmanaged.Height; y++)
                {
                    for (int x = 0; x < unmanaged.Width; x++)
                    {
                        if (Ptr[3] == 0)
                        {
                            Ptr[3] = 255;

                            Ptr[2] = Background.R;
                            Ptr[1] = Background.G;
                            Ptr[0] = Background.B;
                        }
                        else
                        {
                            Ptr[3] = (byte)(Math.Min(Ptr[0],Math.Min(Ptr[1],Ptr[2]))/2);
                        }
                        Ptr += 4;
                    }
                    Ptr += Offset;
                }
                tmp.UnlockBits(bdata);

                //Draw first background colors and then inverted text 
                g.DrawImage(Result, Result.Width, Result.Height);
                g.DrawImage(tmp, 0,0);

                g.Dispose();
                TextG.Dispose();
                tmp.Dispose();
                return Result;
            }
            else
            {
                for (int y = 0; y < Image.VerticalCount; y++)
                {
                    for (int x = 0; x < Image.HorizontalCount; x++)
                    {
                        //draws character in specified font and color
                        TextRenderer.DrawText(g, ImageChars[y, x].Character.ToString(),
                                                Font, new Point(x * FontTemplate.Width, y * FontTemplate.Height), Image.ImageParts[y, x].AvgColor, Background,
                                                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                }

                g.DrawImage(Result, Result.Width, Result.Height);
                g.Dispose();

                return Result;
            }
        }
    }
}