using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace JaProjekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport(@"..\..\..\..\..\x64\Debug\JaAsm.dll")]
        static extern int MyProc1(int a, int b);

        [DllImport(@"..\..\..\..\..\x64\Debug\JaCpp.dll")]
        static extern int Myproc(int a, int b);

        [DllImport(@"..\..\..\..\..\x64\Debug\JaCpp.dll")]
        static extern void Vignette(byte[] pixelArray, double[] pixelArrayMask, int width, int height, double force, double vignetteRadius, int numberThread, int maxThread);

        //public byte[] pixelArray;
        //public double[] pixelArrayMask;
        public Bitmap bitmapInput;
        public Bitmap bitmapOutput;
        public double force = 10;
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
                // Zapisz bitmapę do strumienia
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                // Wczytaj strumień do BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Zamrożenie obrazu dla wątku UI

                return bitmapImage;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (bitmapInput != null)
            {
                byte[] pixelArray = ConvertBitmapToRGBArray(bitmapInput);
                double[] pixelArrayMask = new double[bitmapInput.Height * bitmapInput.Width];
                stopwatch = Stopwatch.StartNew();
                Parallel.For(0, numberThread, i =>
                {
                    Vignette(pixelArray, pixelArrayMask, bitmapInput.Width, bitmapInput.Height, force, radius, i, numberThread);
                });
                //Vignette(pixelArray, pixelArrayMask, bitmapInput.Width, bitmapInput.Height, force, radius, 3, 3);
                stopwatch.Stop();
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

            // Tablica jednowymiarowa, która pomieści wszystkie piksele (3 wartości RGB na piksel)
            byte[] pixelArray = new byte[height * width * 3];  // 3 elementy na piksel (RGB)

            //int index = 0;
            /*for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Pobieramy kolor piksela
                    System.Drawing.Color pixelColor = bitmapInput.GetPixel(x, y);

                    // Zapisujemy RGB w tablicy (po jednym elemencie na kanał RGB)
                    pixelArray[index] = pixelColor.R;  // Red
                    pixelArray[index + 1] = pixelColor.G;  // Green
                    pixelArray[index + 2] = pixelColor.B;  // Blue

                    // Przechodzimy do następnego zestawu RGB
                    index += 3;
                }
            }*/
            System.Drawing.Color pixelColor;
            for (int i = 0; i < (width * height * 3); i += 3)
            {
                pixelColor = bitmapInput.GetPixel((i / 3) % width, (i / 3) / width);
                pixelArray[i] = pixelColor.R;  // Red
                pixelArray[i + 1] = pixelColor.G;  // Green
                pixelArray[i + 2] = pixelColor.B;  // Blue
            }

            return pixelArray;
        }

        private Bitmap ConvertRGBArrayToBitmap(byte[] pixelArray, int width, int height)
        {
            // Tworzymy obiekt Bitmap o podanych wymiarach
            Bitmap bitmapInput = new Bitmap(width, height);
            // Iterujemy po każdym pikselu
            /*for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Obliczamy indeks piksela w tablicy
                    int pixelIndex = (y * width + x) * 3;

                    // Pobieramy wartości RGB
                    byte red = pixelArray[pixelIndex];
                    byte green = pixelArray[pixelIndex + 1];
                    byte blue = pixelArray[pixelIndex + 2];

                    // Tworzymy kolor z wartości RGB
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(red, green, blue);

                    // Ustawiamy piksel w bitmapie
                    bitmapInput.SetPixel(x, y, color);
                }
            }*/
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

        public MainWindow()
        {
            InitializeComponent();
            forceControl.ValueChanged += ForceControl_ValueChanged;
            radiusControl.ValueChanged += RadiusControl_ValueChanged;
            threadControl.ValueChanged += ThreadControl_ValueChanged;
            //int x = 5, y = 3;
            //int retVal = MyProc1(x, y);
            //MessageBox.Show(retVal.ToString());
        }
    }
}