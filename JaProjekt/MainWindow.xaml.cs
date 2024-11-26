using Microsoft.Win32;
using System;
using System.Drawing;
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
        static extern void Vignette(byte[] pixelArray, double[] pixelArrayMask, int width, int height, double force);

        public byte[] pixelArray;
        public double[] pixelArrayMask;

        private void PickFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Tworzymy dialog do wyboru pliku
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki BMP (*.bmp)|*.bmp"
            };

            // Pokazujemy okno dialogowe
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Wczytanie obrazu BMP do ImageViewer
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(filePath);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    ImageViewer.Source = bitmapImage;

                    // Wczytanie obrazu BMP do tablicy pikseli
                    Bitmap bitmap = new Bitmap(filePath);
                    pixelArray = ConvertBitmapToRGBArray(bitmap);
                    pixelArrayMask = new double[bitmap.Height * bitmap.Width];
                    Vignette(pixelArray, pixelArrayMask, bitmap.Width, bitmap.Height, 1);

                    Bitmap bitmap1 = ConvertRGBArrayToBitmap(pixelArray, bitmap.Width, bitmap.Height);
                    bitmap1.Save("output.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas wczytywania obrazu: {ex.Message}",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                /*MessageBox.Show(pixelArrayMask[0].ToString() + "\n" 
                    + pixelArrayMask[100000].ToString() + "\n"
                    + pixelArrayMask[1151039].ToString() + "\n"
                    + pixelArrayMask[1151040].ToString() + "\n"
                    + pixelArrayMask[1151041].ToString());*/
                MessageBox.Show(pixelArrayMask[0].ToString() + "\n"
                    + pixelArray[0].ToString() + "\n"
                    + pixelArray[1].ToString() + "\n"
                    + pixelArray[2].ToString() + "\n"
                    + pixelArray[3].ToString());
            }
        }

        private byte[] ConvertBitmapToRGBArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Tablica jednowymiarowa, która pomieści wszystkie piksele (3 wartości RGB na piksel)
            byte[] pixelArray = new byte[height * width * 3];  // 3 elementy na piksel (RGB)

            //int index = 0;
            /*for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Pobieramy kolor piksela
                    System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);

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
                pixelColor = bitmap.GetPixel((i / 3) % width, (i / 3) / width);
                pixelArray[i] = pixelColor.R;  // Red
                pixelArray[i + 1] = pixelColor.G;  // Green
                pixelArray[i + 2] = pixelColor.B;  // Blue
            }

            return pixelArray;
        }

        private Bitmap ConvertRGBArrayToBitmap(byte[] pixelArray, int width, int height)
        {
            // Tworzymy obiekt Bitmap o podanych wymiarach
            Bitmap bitmap = new Bitmap(width, height);
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
                    bitmap.SetPixel(x, y, color);
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
                bitmap.SetPixel((i / 3) % width, (i / 3) / width, color);
            }
            return bitmap;
        }

        public MainWindow()
        {
            InitializeComponent();
            //int x = 5, y = 3;
            //int retVal = MyProc1(x, y);
            //MessageBox.Show(retVal.ToString());
        }
    }
}