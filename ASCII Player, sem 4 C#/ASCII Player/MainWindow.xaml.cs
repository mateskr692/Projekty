using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Accord.Video.FFMPEG;

namespace ASCIIPlayer
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ColorDialog colorDialog = new ColorDialog();
        private FontDialog fontDialog = new FontDialog();
        private OpenFileDialog openDialog = new OpenFileDialog();
        private SaveFileDialog saveDialog = new SaveFileDialog();

        MainLogic logic = new MainLogic();

        public MainWindow()
        {

            InitializeComponent();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            //initiazie dialogs
            colorDialog.SolidColorOnly = true;
            colorDialog.AllowFullOpen = true;
            colorDialog.AnyColor = true;

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;

            openDialog.Multiselect = false;
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;

            //initiaze some default values 
            //not doing it in xamal so that converter and dialogs are ready first

            ComboBox_Splitter.SelectedIndex = 0;
            ComboBox_Comparator.SelectedIndex = 0;

            //change data context to bind to mainlogic properties
            DataContext = logic;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      General Events
        //----------------------------------------------------------------------------------------------------------------------------


        private void Button_openFile_Click(object sender, RoutedEventArgs e)
        {
            var result = openDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                logic.Input = openDialog.FileName;
            }
        }

        private void Button_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var result = saveDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                logic.Output = saveDialog.FileName;
            }
        }


        private void Button_Convert_Click(object sender, RoutedEventArgs e)
        {
             logic.RenderVideo();
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Font Events
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates Foreground Color and Preview Image
        /// </summary>
        private void Button_ColorForeground_Click(object sender, RoutedEventArgs e)
        {
            var result = colorDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                logic.ForegroundColor = colorDialog.Color;
        }

        /// <summary>
        /// Updates Background Color and Preview Image
        /// </summary>
        private void Button_ColorBackground_Click(object sender, RoutedEventArgs e)
        {
            var result = colorDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                logic.BackgroundColor = colorDialog.Color;

        }

        /// <summary>
        /// Updates Font and Preview Image
        /// </summary>
        private void Button_ChooseFont_Click(object sender, RoutedEventArgs e)
        {
            var result = fontDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                logic.Font = fontDialog.Font;
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Splitter Events
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Switch to a different Splitter
        /// </summary>
        private void ComboBox_Splitter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ComboBox_Splitter.SelectedIndex;
            logic.SetSplitter(index);
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Comparators Events
        //----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Switch to a different Comparator
        /// </summary>
        private void ComboBox_Comparator_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = ComboBox_Comparator.SelectedIndex;
            Border_ComparatorCellular.Visibility = Visibility.Collapsed;
            Border_ComparatorCentralized.Visibility = Visibility.Collapsed;

            if (index == 0)
                Border_ComparatorCentralized.Visibility = Visibility.Visible;

            else if (index == 1 || index == 2)
                Border_ComparatorCellular.Visibility = Visibility.Visible;


            logic.SetComparator(index);
        }

        //----------------------------------------------------------------------------------------------------------------------------
        //      Enabes or Disables Save and color options in General
        //----------------------------------------------------------------------------------------------------------------------------

        //Toogle Color options
        private void CheckBox_Color_Checked(object sender, RoutedEventArgs e) => Border_UseColor.IsEnabled = true;
        private void CheckBox_Color_Unchecked(object sender, RoutedEventArgs e) => Border_UseColor.IsEnabled = false;

        //Toogle Save Options
        private void SaveOutput_Checked(object sender, RoutedEventArgs e) => Border_Save.IsEnabled = true;
        private void SaveOutput_Unchecked(object sender, RoutedEventArgs e) => Border_Save.IsEnabled = false;

        private void Button_SaveImage_Click(object sender, RoutedEventArgs e) => logic.SaveImage();
        private void Button_CopyText_Click(object sender, RoutedEventArgs e) => System.Windows.Clipboard.SetText(logic.ASCII_Image_Text);

        private void Button_Pause_Click(object sender, RoutedEventArgs e)
        {
            logic.Thread_Abort();
        }
    }
}
