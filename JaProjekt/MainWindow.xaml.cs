using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JaProjekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport(@"..\..\..\..\..\x64\Debug\JaCpp.dll")]
        static extern void VignetteCpp(byte[] pixelArray, double[] pixelArrayMask, int width, int height, double force, double vignetteRadius, int numberThread, int maxThread);

        [DllImport(@"..\..\..\..\..\x64\Debug\JaAsm.dll")]
        static extern void VignetteAsm(byte[] pixelArray, double[] pixelArrayMask, int width, int height, double force, double vignetteRadius, int numberThread, int maxThread);

        public Bitmap bitmapInput;
        public Bitmap bitmapOutput;
        public double force = 15;
        public double radius = 0.25;
        public int numberThread = 1;
        Stopwatch stopwatch;

        private void ForceControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            force = forceControl.Value;
        }

        private void RadiusControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            radius = radiusControl.Value;
        }

        private void ThreadControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            numberThread = (int)threadControl.Value;
        }

        private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (bitmapInput != null && (CheckBoxASM.IsChecked == true || CheckBoxCPP.IsChecked == true))
            {
                byte[] pixelArray = ConvertBitmapToRGBArray(bitmapInput);
                double[] pixelArrayMask = new double[bitmapInput.Height * bitmapInput.Width];
                if (CheckBoxASM.IsChecked == true)
                {
                    stopwatch = Stopwatch.StartNew();
                    Parallel.For(0, numberThread, i =>
                    {
                        VignetteAsm(pixelArray, pixelArrayMask, bitmapInput.Width, bitmapInput.Height, force, radius, i, numberThread);
                    });
                    stopwatch.Stop();
                }
                else
                {
                    stopwatch = Stopwatch.StartNew();
                    Parallel.For(0, numberThread, i =>
                    {
                        VignetteCpp(pixelArray, pixelArrayMask, bitmapInput.Width, bitmapInput.Height, force, radius, i, numberThread);
                    });
                    stopwatch.Stop();
                }
                ExecutionTimeTextBlock.Text = $"{stopwatch.ElapsedMilliseconds} ms";

                bitmapOutput = ConvertRGBArrayToBitmap(pixelArray, bitmapInput.Width, bitmapInput.Height);
                bitmapOutput.Save("output.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                ImageViewer.Source = ConvertBitmapToImageSource(bitmapOutput);
            }
        }

        private void PickFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki BMP (*.bmp)|*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    ImageViewer.Source = new BitmapImage(new Uri(filePath));

                    bitmapInput = new Bitmap(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas wczytywania obrazu: {ex.Message}",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private byte[] ConvertBitmapToRGBArray(Bitmap bitmapInput)
        {
            int width = bitmapInput.Width;
            int height = bitmapInput.Height;
            byte[] pixelArray = new byte[height * width * 3];

            System.Drawing.Color pixelColor;
            for (int i = 0; i < (width * height * 3); i += 3)
            {
                pixelColor = bitmapInput.GetPixel((i / 3) % width, (i / 3) / width);
                pixelArray[i] = pixelColor.R;
                pixelArray[i + 1] = pixelColor.G;
                pixelArray[i + 2] = pixelColor.B;
            }

            return pixelArray;
        }

        private Bitmap ConvertRGBArrayToBitmap(byte[] pixelArray, int width, int height)
        {
            Bitmap bitmapInput = new Bitmap(width, height);
            byte red;
            byte green;
            byte blue;

            System.Drawing.Color color;
            for (int i = 0; i < (width * height * 3); i += 3)
            {
                red = pixelArray[i];
                green = pixelArray[i + 1];
                blue = pixelArray[i + 2];
                color = System.Drawing.Color.FromArgb(red, green, blue);
                bitmapInput.SetPixel((i / 3) % width, (i / 3) / width, color);
            }
            return bitmapInput;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == CheckBoxASM && CheckBoxASM.IsChecked == true)
            {
                CheckBoxCPP.IsChecked = false;
            }
            else if (sender == CheckBoxCPP && CheckBoxCPP.IsChecked == true)
            {
                CheckBoxASM.IsChecked = false;
            }
        }

        private void AutoTest_Click(object sender, RoutedEventArgs e)
        {
            double testForce = 10;
            double testRadius = 0.25;
            Bitmap bitmapTest = null;
            Stopwatch testStopwatchAsm;
            Stopwatch testStopwatchCpp;
            try
            {
                bitmapTest = new Bitmap(Environment.CurrentDirectory + "\\test.bmp");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania obrazu testowego",
                                "Błąd", MessageBoxButton.OK);
            }
            if (bitmapTest != null)
            {
                byte[] pixelArray = ConvertBitmapToRGBArray(bitmapTest);
                double[] pixelArrayMask = new double[bitmapTest.Height * bitmapTest.Width];

                using (StreamWriter writer = new StreamWriter("TestFile.txt"))
                {
                    writer.WriteLine("testNumberThread StopwatchAsm(ms) StopwatchCpp(ms)");
                    for (int testNumberThread = 1; testNumberThread <= 64; testNumberThread++)
                    {
                        testStopwatchAsm = Stopwatch.StartNew();
                        Parallel.For(0, numberThread, i =>
                        {
                            VignetteAsm(pixelArray, pixelArrayMask, bitmapTest.Width, bitmapTest.Height, testForce, testRadius, i, testNumberThread);
                        });
                        testStopwatchAsm.Stop();

                        testStopwatchCpp = Stopwatch.StartNew();
                        Parallel.For(0, numberThread, i =>
                        {
                            VignetteCpp(pixelArray, pixelArrayMask, bitmapTest.Width, bitmapTest.Height, testForce, testRadius, i, testNumberThread);
                        });
                        testStopwatchCpp.Stop();

                        writer.WriteLine($"{testNumberThread} {testStopwatchAsm.ElapsedMilliseconds} {testStopwatchCpp.ElapsedMilliseconds}");
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            forceControl.ValueChanged += ForceControl_ValueChanged;
            radiusControl.ValueChanged += RadiusControl_ValueChanged;
            threadControl.ValueChanged += ThreadControl_ValueChanged;
        }
    }
}