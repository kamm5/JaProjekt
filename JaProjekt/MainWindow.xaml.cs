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

        public MainWindow()
        {
            InitializeComponent();
            int x = 5, y = 3;
            int retVal = MyProc1(x, y);
            MessageBox.Show(retVal.ToString());
        }
    }
}