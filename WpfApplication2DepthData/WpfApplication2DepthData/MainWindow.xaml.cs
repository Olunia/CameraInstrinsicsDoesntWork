using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;

namespace WpfApplication2DepthData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        int width;
        int height;       
        ushort minDepth;
        ushort maxDepth;
        ushort[] pixelData = new ushort[217088];
        byte[] pixels = new byte[1058304]; //pixels.Length=1058304;
        string[] pixelDataString = new string[217088]; //pixelData.Length= 217088;
        byte[] intensity2 = new byte[217088];
        string[] intensity3 = new string[217088];
        ushort depth;
        ushort[,] pixelMatrixData = new ushort[424, 512]; //tablica 2 wymiarowa zapisująca mapę głębokości w ushort - konwersja w trakcie zapisu do pliku
        uint bytesperpixel;
        float diagonalfieldofview;
        float horizontalfieldofview;
        uint lengthinpixels;
        float verticalfieldofview;
        public static DepthSpacePoint[] depthSpacePoints;
        float FPX; //CHECK 1___________________________________________________________________________________________________________
        CameraIntrinsics camera_intrinsics; //CHECK 2___________________________________________________________________________________________________________

        
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            
        }
        KinectSensor sensor;
        DepthFrameReader depthReader;
        ColorFrameReader colorReader;
        

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();
            this.sensor.Open();

            depthReader = sensor.DepthFrameSource.OpenReader();
            colorReader = sensor.ColorFrameSource.OpenReader();

            this.depthReader.FrameArrived += OnFrameArrived;
        }

        void OnFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = ProcessFrame(frame);
                }
            }
        }
        //public struct DepthSpacePoint { };

        //public ImageSource aa(DepthSpacePoint dd) { }

        

        public ImageSource ProcessFrame(DepthFrame frame)//, CameraIntrinsics camera_intrinsics)
        {
            width = frame.FrameDescription.Width;
            height = frame.FrameDescription.Height;
            
            bytesperpixel = frame.FrameDescription.BytesPerPixel;
            diagonalfieldofview = frame.FrameDescription.DiagonalFieldOfView;
            horizontalfieldofview = frame.FrameDescription.HorizontalFieldOfView;
            lengthinpixels = frame.FrameDescription.LengthInPixels;
            verticalfieldofview = frame.FrameDescription.VerticalFieldOfView;

            FPX = camera_intrinsics.FocalLengthX; //CHECK 3___________________________________________________________________________________________________________

            PixelFormat format = PixelFormats.Bgr32;
            minDepth = frame.DepthMinReliableDistance;
            maxDepth = frame.DepthMaxReliableDistance;
            pixelData = new ushort[width * height];
            pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];//zeby zaokraglic do bajtow
            frame.CopyFrameDataToArray(pixelData); //zapis mapy głębokości do tablicy o wymiarach 217088x1
            int colorIndex = 0;
            
            //---------zapis mapy głębokości do tablicy o wymiarach 424x512
            for (int row = 0; row < 424; row++)
                for (int col = 0; col < 512; col++)
                    pixelMatrixData[row, col] = pixelData[row * 512 + col];
            //---------

            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                depth = pixelData[depthIndex];
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red
                //Console.WriteLine(depthSpacePoints);
                ++colorIndex;
                intensity2[depthIndex] = intensity;
            }
                                   
            int stride = width * format.BitsPerPixel / 8;
            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
                        
        }
             

       public void SaveDepth_Click(object sender, RoutedEventArgs e) //event handler do buttona
        {
            //------------------------------------------------------Ola-----------------------------------------------------------------------
            pixelDataString = new string[pixelData.Length];

             for (int iconvert = 0; iconvert < pixelData.Length; ++iconvert) // convert to srting
             {

                 pixelDataString[iconvert] = Convert.ToString(pixelData[iconvert]);
                intensity3[iconvert] = Convert.ToString(intensity2[iconvert]);                
            }
                               

            //------------------------------------------------------Ola-----------------------------------------------------------------------
            //zapis mapy głębokości do tablicy o wymiarach 217088x1 do pliku tekstowego
            System.IO.File.WriteAllLines(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\pixelDataString.txt", pixelDataString);

            // zapis mapy głębokości do tablicy o wymiarach 217088x1 do pliku tekstowego do folderu moje obrazy z inną nazwą
            string time = System.DateTime.UtcNow.ToString("M'-'d'-'yyyy' ('hh'-'mm'-'ss')'");//, CultureInfo.CurrentUICulture.DateTimeFormat);


            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string myloc = @"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\";
            //string path = System.IO.Path.Combine(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\pixelDataString" + time + ".txt");
            string path = System.IO.Path.Combine(myloc, "KinectScreenshot-Depth-" + time + ".txt");
            System.IO.File.WriteAllLines(path, pixelDataString);


            //zapis mapy głębokości do tablicy o wymiarach 217088x1 do pliku csv
            System.IO.File.WriteAllLines(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\DepthArray.csv", pixelDataString);

            //zapis mapy głębokości do tablicy o wymiarach 424x512 do pliku 
            using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\DepthMatrix.csv")) 
            {
                for (int x = 0; x < 424; x++)
                {
                    string content = "";
                    for (int y = 0; y < 512; y++)
                    {
                        content += pixelMatrixData[x, y].ToString("0.00") + ";";
                    }
                    outfile.WriteLine(content);
                }
            }
            
            string width2 = Convert.ToString(width);
            string height2 = Convert.ToString(height);
            string bytesperpixel2 = Convert.ToString(bytesperpixel);            
            string diagonalfieldofview2 = Convert.ToString(diagonalfieldofview);
            string horizontalfieldofview2 = Convert.ToString(horizontalfieldofview);
            string lengthinpixels2 = Convert.ToString(lengthinpixels);
            string verticalfieldofview2 = Convert.ToString(verticalfieldofview);

            string[] framedecription2 = { "width", width2, "height", height2, "bytesperpixel", bytesperpixel2, "diagonalfieldofview", diagonalfieldofview2, "horizontalfieldofview", horizontalfieldofview2, "lengthinpixels", lengthinpixels2, "verticalfieldofview", verticalfieldofview2 };
            System.IO.File.WriteAllLines(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\ALL.txt", framedecription2);
                    
            System.IO.File.WriteAllLines(@"D:\Studia IIst EL\MAGISTERKA\Projekty\ZAPISANE PLIKI\Intensity.txt", intensity3);
        }

        
        private void SaveWidth_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveHeight_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}   