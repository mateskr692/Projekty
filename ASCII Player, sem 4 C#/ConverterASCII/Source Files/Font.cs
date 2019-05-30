using System.Drawing;
using System.Windows.Forms;
using Accord.Imaging.Filters;
using System.Collections.Generic;

namespace ConverterASCII
{
    //----------------------------------------------------------------------------------------------------------------------------
    //      Char Template
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores Bitmap and avarage grayscale value representation of a character in specified font
    /// </summary>
    public class CharTemplate
    {
        /// <summary>
        /// ASCII character that is being represented
        /// </summary>
        public readonly char Character;
        /// <summary>
        /// The Grayscale Bitmap representation of character
        /// </summary>
        public readonly Bitmap CharImage;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes CharBitmap object with given character and font
        /// </summary>
        /// <param name="ch">The ASCII character</param>
        /// <param name="font">font used to draw character</param>
        public CharTemplate(char ch, Font font, Color Foreground, Color Background)
        {
            Character = ch;
            //create temporary graphic and use it to figure out the true size
            CharImage = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(CharImage);
            Size s = TextRenderer.MeasureText(g, Character.ToString(), font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            //create bitmap and render text onto it
            CharImage = new Bitmap(s.Width, s.Height);
            g = Graphics.FromImage(CharImage);
            TextRenderer.DrawText(g, Character.ToString(), font, new Point(0, 0), Foreground, Background, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            g.DrawImage(this.CharImage, s.Width, s.Height);

            //turn into grayscale to compensate for antialiasing and interpolation
            CharImage = Grayscale.CommonAlgorithms.BT709.Apply(CharImage);
        }
    }




    //----------------------------------------------------------------------------------------------------------------------------
    //      Font Template
    //----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores Font information and a list of character bitmaps
    /// </summary>
    public class FontTemplate
    {
        /// <summary>
        /// Indexes of ASCII Characters range
        /// </summary>
        const int FirstChar = 32, LastChar = 126;
        /// <summary>
        /// List of Templates of all available characters
        /// </summary>
        public readonly List<CharTemplate> CharList;
        /// <summary>
        /// Font used to generate Character Bitmaps
        /// </summary>
        public readonly Font Font;
        /// <summary>
        /// Foreground Color (font Color) of the characters
        /// </summary>
        public readonly Color Foreground;
        /// <summary>
        /// Background Color of bitmap that characters are drawn onto
        /// </summary>
        public readonly Color Background;
        /// <summary>
        /// Dimension of the Bitmaps of characters
        /// </summary>
        public readonly int Width, Height;

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes object with proper font and creates list of CharTemplates
        /// </summary>
        /// <param name="font">Font objcet to use for drawing</param>
        /// <param name="Foreground">Color of the letter, ideally black or white</param>
        /// <param name="Background">Color of the background, best if opposite of Foreground</param>
        /// <param name="Characters">If specified uses those characters instead of regular ones</param>
        public FontTemplate(Font font, Color Foreground, Color Background, string Characters = null)
        {
            //initalize fields
            Font = font;
            this.Foreground = Foreground;
            this.Background = Background;

            //create list of characters
            //ASCII characters start at 32 (space) up to 126 (`)
            CharList = new List<CharTemplate>();
            if (Characters == null || Characters == "")
            {
                for (int i = FirstChar; i <= LastChar; i++)
                {
                    CharList.Add(new CharTemplate((char)i, Font, Foreground, Background));
                }
            }
            else
            {
                foreach (var ch in Characters)
                {
                    CharList.Add(new CharTemplate(ch, Font, Foreground, Background));
                }
            }

            //All characters should have the same size so store the simensions of the first one
            Width = CharList[0].CharImage.Width;
            Height = CharList[0].CharImage.Height;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Saves all generated Character templates to specified Directory
        /// </summary>
        /// <param name="Directory">The Directory in which to save</param>
        public void Save(string Directory)
        {
            foreach (var Ch in CharList)
                Ch.CharImage.Save(Directory + @"\char" + (int)Ch.Character + @".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}