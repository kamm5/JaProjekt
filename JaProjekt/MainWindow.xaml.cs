using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public byte[,,] pixelArray;

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
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas wczytywania obrazu: {ex.Message}",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                MessageBox.Show(pixelArray[1, 0, 0].ToString() + "," + pixelArray[1, 0, 1].ToString() + "," + pixelArray[1, 0, 2].ToString());
            }
        }

        private byte[,,] ConvertBitmapToRGBArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Tablica trójwymiarowa do przechowywania wartości RGB
            byte[,,] pixelArray = new byte[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Pobieramy kolor piksela
                    System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);

                    // Wypełniamy wartości RGB
                    pixelArray[y, x, 0] = pixelColor.R; // Red
                    pixelArray[y, x, 1] = pixelColor.G; // Green
                    pixelArray[y, x, 2] = pixelColor.B; // Blue
                }
            }

            return pixelArray;
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