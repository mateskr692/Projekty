using ConverterASCII;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Accord.Video.FFMPEG;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using Accord.Video.VFW;

namespace ASCIIPlayer
{
    public enum ComparatorIndex
    {
        Centralized = 0,
        Cellular = 1,
        Gaussian = 2,
        Grayscale = 3,
        Exact = 4
    }
    public enum SplitterIndex
    {
        Regular = 0,
        Edge = 1
    }

    /// <summary>
    /// Provides accesors and databinding to the Frontent and events
    /// </summary>
    public class MainLogic : INotifyPropertyChanged
    {
        /// <summary>
        /// Converter used for generating ASCII images
        /// </summary>
        private Converter Converter = new Converter();

        /// <summary>
        /// list of available comparators
        /// creatign it now means we dont have to worry aout one not existing when changing values
        /// </summary>
        private List<ComparatorBase> Comparators = new List<ComparatorBase>
        {
            new ComparatorCentralized(),
            new ComparatorCellular(),
            new ComparatorGaussian(),
            new ComparatorGrayscale(),
            new ComparatorExact()
        };

        /// <summary>
        /// list of available Image Splitters
        /// creating it now means we dont have to worry about one not existing when changing values
        /// </summary>
        private List<ImageSplitterBase> Splitters = new List<ImageSplitterBase>
        {
            new ImageSplitter(),
            new ImageSplitterEdge()
        };


        private VideoFileReader VideoReader = new VideoFileReader();
        private AVIWriter VideoWriter = new AVIWriter();

        //thread for rendering and mutex so it can be safely halt
        private Thread RenderingThread;
        private bool _ExitFlag = false;
        private Mutex ExitFlag = new Mutex();

        //----------------------------------------------------------------------------------------------------------------------------
        //      General Properties
        //----------------------------------------------------------------------------------------------------------------------------

        private string _Input;
        public string Input
        {
            get => _Input;
            set
            {
                Thread_Abort();

                _Input = value;
                try
                {
                    VideoReader.Open(value);
                    RawImage = VideoReader.ReadVideoFrame(0);

                    Preview_Update();
                }
                catch { }
                OnPropertyChanged("Input");
            }
        }

        private string _Output;
        public string Output
        {
            get => _Output;
            set
            {
                Thread_Abort();

                _Output = value;
                OnPropertyChanged("Output");
            }
        }

        public Bitmap RawImage
        {
            get => Converter.Bitmap;
            set => Converter.Bitmap = value;
        }
        private Bitmap ASCII_Image_Bitmp;

        private BitmapImage _ASCII_Image;
        public BitmapImage ASCII_Image
        {
            set => _ASCII_Image = value;

            get
            {
                if(_ASCII_Image == null)
                {
                    Bitmap defaultbmp = new Bitmap(@"..\..\Resources\Preview.png");
                    return ToBitmapImage(defaultbmp);
                }
                return _ASCII_Image;
            }
        }
        public string ASCII_Image_Text
        {
            get
            {
                return Converter.ConvertToString();
            }
        }

        private bool _UseColor = false;
        public bool UseColor
        {
            get { return _UseColor; }
            set
            {
                Thread_Abort();

                _UseColor = value;
                Preview_Update();
            }
        }

        private bool _FastColor = true;
        public bool FastColor
        {
            get { return _FastColor; }
            set
            {
                Thread_Abort();

                _FastColor = value;
                Preview_Update();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Font Properties
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Accesors for binding to Font properties
        /// </summary>
        public string FontName => Converter.Font.Name;
        public string FontSize => Converter.Font.Size.ToString();
        public string FontStyle => Converter.Font.Style.ToString();

        /// <summary>
        /// setter for converter font
        /// </summary>
        public Font Font
        {
            set
            {
                Thread_Abort();

                Converter.Font = value;
                FontPreview_Update();

                //notifies Xamal to update stuff bound to those properties
                OnPropertyChanged("FontName");
                OnPropertyChanged("FontSize");
                OnPropertyChanged("FontStyle");
            }
        }

        public string FontCharacters
        {
            get { return Converter.Characters; }
            set
            {
                Thread_Abort();

                Converter.Characters = value;
                Preview_Update();
            }
        }

        /// <summary>
        /// Setters for Converter Font colors
        /// </summary>
        public Color ForegroundColor
        {
            set
            {
                Thread_Abort();

                Converter.Foreground = value;
                FontPreview_Update();

                //notify to get new brush color
                OnPropertyChanged("ForegroundBrush");
            }
        }
        public Color BackgroundColor
        {
            set
            {
                Thread_Abort();

                Converter.Background = value;
                FontPreview_Update();

                //notify to get new brush color
                OnPropertyChanged("BackgroundBrush");
            }
        }

        /// <summary>
        /// Accesors that turns Converter Colors to Brushes for filling rectangles
        /// </summary>
        public System.Windows.Media.SolidColorBrush ForegroundBrush
        {
            get
            {
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush();
                brush.Color = System.Windows.Media.Color.FromRgb(Converter.Foreground.R, Converter.Foreground.G, Converter.Foreground.B);
                return brush;
            }
        }
        public System.Windows.Media.SolidColorBrush BackgroundBrush
        {
            get
            {
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush();
                brush.Color = System.Windows.Media.Color.FromRgb(Converter.Background.R, Converter.Background.G, Converter.Background.B);
                return brush;
            }
        }

        /// <summary>
        /// A preview that is displayed in font options
        /// </summary>
        private BitmapImage _FontPreview;
        public BitmapImage FontPreview
        {
            get { return _FontPreview; }
            set
            {
                _FontPreview = value;
                OnPropertyChanged("FontPreview");

                //update Main Image
                Preview_Update();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Splitter Properties
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// sets proper splitter to be used in converter
        /// </summary>
        public void SetSplitter(int value)
        {
            Thread_Abort();

            Converter.Splitter = Splitters[value];

            //update Main Image
            Preview_Update();
        }

        /// <summary>
        /// Accesors for changing width of the splitter
        /// sets to all splitters but only reads from one
        /// </summary>
        public uint SplitterWidth
        {
            set
            {
                Thread_Abort();

                foreach (var elem in Splitters)
                    elem.HorizontalCount = (int)value;

                //altough we updated all splitters we need to modify one via comparator so its setter gets called and next generation works properly
                Converter.Splitter = Converter.Splitter;

                //update Main Image
                Preview_Update();
            }

            get => (uint)Converter.Splitter.HorizontalCount;
        }
        public uint SplitterHeight
        {
            set
            {
                Thread_Abort();

                foreach (var elem in Splitters)
                    elem.VerticalCount = (int)value;

                //altough we updated all splitters we need to modify one via comparator so its setter gets called and next generation works properly
                Converter.Splitter = Converter.Splitter;

                //update Main Image
                Preview_Update();
            }

            get => (uint)Converter.Splitter.VerticalCount;
        }


        //----------------------------------------------------------------------------------------------------------------------------
        //      Comparator Properties
        //----------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// setts proper comparator to be used in converter
        /// </summary>
        public void SetComparator(int value)
        {
            Thread_Abort();

            Converter.Comparator = Comparators[value];

            //update Main Image
            Preview_Update();
        }

        /// <summary>
        /// Accesors for Centralized Comparator
        /// </summary>
        public double Centralized_LRMargin
        {
            get => (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).LeftRightMargin;
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).LeftRightMargin = value;

                //update Main Image
                Preview_Update();
            }
        }
        public double Centralized_UDMargin
        {
            get => (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).UpDownMargin;
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).UpDownMargin = value;

                //update Main Image
                Preview_Update();
            }
        }
        public uint Centralized_CenterWeight
        {
            get => (uint)(Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).CenterWeight;
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).CenterWeight = (int)value;

                //update Main Image
                Preview_Update();
            }
        }
        public uint Centralized_LRWeight
        {
            get => (uint)(Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).LeftRightWeight;
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).LeftRightWeight = (int)value;

                //update Main Image
                Preview_Update();
            }
        }
        public uint Centralized_UDWeight
        {
            get => (uint)(Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).UpDownWeight;
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Centralized] as ComparatorCentralized).UpDownWeight = (int)value;

                //update Main Image
                Preview_Update();
            }
        }

        /// <summary>
        /// Accesors for Cellular and Gaussian Comparators
        /// </summary>
        public uint Cellular_X
        {
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Cellular] as ComparatorCellular).GridX = (int)value;
                (Comparators[(int)ComparatorIndex.Gaussian] as ComparatorGaussian).GridX = (int)value;

                //modify comparator via converters' accesor so the flags are set properly
                Converter.Comparator = Converter.Comparator;

                //update Main Image
                Preview_Update();
            }
            get => (uint)(Comparators[(int)ComparatorIndex.Cellular] as ComparatorCellular).GridX;
        }
        public uint Cellular_Y
        {
            set
            {
                Thread_Abort();

                (Comparators[(int)ComparatorIndex.Cellular] as ComparatorCellular).GridY = (int)value;
                (Comparators[(int)ComparatorIndex.Gaussian] as ComparatorGaussian).GridY = (int)value;

                //modify comparator via converters' accesor so the flags are set properly
                Converter.Comparator = Converter.Comparator;

                //update Main Image
                Preview_Update();
            }
            get => (uint)(Comparators[(int)ComparatorIndex.Cellular] as ComparatorCellular).GridY;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Methods
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes Converter with proper values
        /// </summary>
        public MainLogic()
        {
            //initiaze Converter
            Converter.Font = new Font("Courier New", 8, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            Converter.Foreground = Color.White;
            Converter.Background = Color.Black;
            FontPreview_Update();


            Converter.Splitter = Splitters[0];
            Converter.Comparator = Comparators[0];
        }

        /// <summary>
        /// Used in rendering thread to check if user speciified to abort rendering
        /// </summary>

        private void Thread_SetExitFlg()
        {
            ExitFlag.WaitOne();
            _ExitFlag = true;
            ExitFlag.ReleaseMutex();
        }

        private bool Thread_GetExitFlag()
        {
            ExitFlag.WaitOne();
            bool b = _ExitFlag;
            _ExitFlag = false;
            ExitFlag.ReleaseMutex();

            return b;
        }

        public void Thread_Abort()
        {
            if (RenderingThread == null || !RenderingThread.IsAlive)
                return;

            RenderingThread.Abort();
            RenderingThread.Join();
            //Thread_SetExitFlg();
            //RenderingThread.Join();
        }

        /// <summary>
        /// Generates BitmapImage based on converter font and colors and sets it for FontPreview
        /// </summary>
        private void FontPreview_Update()
        {
            string PreviewText = "Preview";

            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            Font Fixed = new Font(Converter.Font.FontFamily, 20, Converter.Font.Style, Converter.Font.Unit);
            Size s = TextRenderer.MeasureText(g, PreviewText, Fixed, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            //create bitmap and render text onto it
            bmp = new Bitmap(s.Width, s.Height);
            g = Graphics.FromImage(bmp);
            TextRenderer.DrawText(g, PreviewText, Fixed, new Point(0, 0), Converter.Foreground, Converter.Background, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            g.DrawImage(bmp, 0, 0);

            //convert Bitmap to ImageSource
            FontPreview = ToBitmapImage(bmp);
            bmp.Dispose();
        }



        /// <summary>
        /// Updates Preview based on conveter settings
        /// needs to be done here because OnPPropertyChanged runs on main thread
        /// </summary>
        /// 
        private void Preview_Update()
        {
            if (RawImage == null)
                return;

            RenderingThread = new Thread(() => 
            {
                try
                {
                    Converter.Apply();

                    //Try to quit if new reqest is available
                    //if (Thread_GetExitFlag() == true)
                    //    return;

                    Bitmap bmp = UseColor ? Converter.ConvertToBitmapColored(FastColor) : Converter.ConvertToBitmap();
                    BitmapImage bmpImage = ToBitmapImage(bmp);

                    bmpImage.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        ASCII_Image_Bitmp = bmp;
                        ASCII_Image = bmpImage;
                    });


                    OnPropertyChanged("ASCII_Image");
                }
                catch { }
            });

            RenderingThread.Start();
        }

        /// <summary>
        /// Starts a new thread that converts all frame of input
        /// </summary>
        public void RenderVideo()
        {
            if (!VideoReader.IsOpen)
                return;

            //Start thread depending on if rendering supposed to proceed frame by frame or all at once

            RenderingThread = new Thread(() =>
            {
                try
                {
                    //setup stopwatch to display the frames with proper timing
                    Stopwatch stopwatch = new Stopwatch();
                    int SleepDuration = (VideoReader.FrameRate.Denominator * 1000) / VideoReader.FrameRate.Numerator;

                    if (Output != null)
                    {
                        VideoWriter.Open(Output, ((ASCII_Image_Bitmp.Width + 1) / 2) * 2, ((ASCII_Image_Bitmp.Height + 1) / 2) * 2);
                        VideoWriter.FrameRate = VideoReader.FrameRate.Numerator;
                    }

                    Bitmap frame = VideoReader.ReadVideoFrame(0);
                    Bitmap bmp;
                    BitmapImage bmpImage;

                    while (frame != null)
                    {
                        //measeure time of the start of the generation
                        stopwatch.Restart();
                        RawImage = frame;

                        Converter.Apply();
                        
                        //Try to quit if new reqest is available
                        //if (Thread_GetExitFlag() == true)
                        //    return;

                        bmp = UseColor ? Converter.ConvertToBitmapColored(FastColor) : Converter.ConvertToBitmap();
                        bmpImage = ToBitmapImage(bmp);

                        bmpImage.Freeze();
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            ASCII_Image_Bitmp = bmp;
                            ASCII_Image = bmpImage;
                        });

                        if (Output != null)
                        {
                            //resize Bitmap since avi can only accept multiples of 2
                            bmp = new Bitmap(bmp, (bmp.Width + 1) / 2 * 2, (bmp.Height + 1) / 2 * 2);
                            VideoWriter.AddFrame(bmp);
                        }

                        OnPropertyChanged("ASCII_Image");

                        //Try to quit if new reqest is available
                        //if (Thread_GetExitFlag() == true)
                        //    return;

                        //If we generated preview faster than the frame interval (unlikely) wait for the remaining time
                        frame = VideoReader.ReadVideoFrame();
                        stopwatch.Stop();
                        if (stopwatch.Elapsed.Milliseconds < SleepDuration)
                            Thread.Sleep(SleepDuration - stopwatch.Elapsed.Milliseconds);

                        //Try to quit if new reqest is available
                        //if (Thread_GetExitFlag() == true)
                        //    return;

                    }

                    if (Output != null)
                        VideoWriter.Close();

                }
                catch
                {
                    if (Output != null)
                        VideoWriter.Close();
                }
                });

            //Start the thread
            RenderingThread.Start();
        }

        public void SaveImage()
        {
            if (Output == null || Output == "")
                return;

            ASCII_Image_Bitmp.Save(Output, System.Drawing.Imaging.ImageFormat.Bmp);
        }


        /// <summary>
        /// converts bitmap to bitmapImage that can be displayed in main windows
        /// </summary>
        /// <param name="bmp">bitmap that is being converted</param>
        /// <returns></returns>
        private BitmapImage ToBitmapImage(Bitmap bmp)
        {
            //convert Bitmap to ImageSource
            BitmapImage bmpImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                bmpImage.BeginInit();
                bmpImage.StreamSource = stream;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();
            }
            return bmpImage;
        }

        //Used to notify Windows about changed values
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
