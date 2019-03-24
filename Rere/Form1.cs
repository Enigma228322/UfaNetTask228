using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Rere
{
    public partial class Form1 : Form
    {
        bool[,] r = new bool[10, 10];
        bool[,] l = new bool[10, 10];
        

        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (i == j || i == j + 1 || j == i + 1
                        || i == j + 2 || j == i + 2
                        || i == j - 1 || j == i - 1
                        || i == j - 2 || j == i - 2)
                    {
                        r[i, j] = true;
                    }
                }
            }
            l = r;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if( j < 6)
                    {
                        bool temp = false;
                        temp = l[i, j];
                        l[i, j] = l[i, 9 - j];
                        l[i, 9 - j] = temp;
                    }
                }
            }
        }
        Image<Bgr, byte> imgImage;
        
        bool CheckMatr(Bitmap picture, int x, int y)
        {
            int w = picture.Width;
            int h = picture.Height;

            bool[,] temp = new bool[h, w];
            Color clr = new Color();

            for(int i = y; i < x + h; i++)
            {
                for(int j = x; j < x + w; j++)
                {
                    clr = picture.GetPixel(i, j);
                    if (clr.Name == "ffffffff") temp[i, j] = true;
                    else temp[i, j] = false;
                }
            }

            int cnt = 0;

            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 10; j++)
                {
                    if (temp[i, j] == r[i, j])
                        cnt++;
                }
            }

            if (cnt >= 10) return true;

            return false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imgImage = new Image<Bgr, byte>(ofd.FileName);
                pictureBox1.Image = imgImage.Bitmap;
            }

            Image<Gray, byte> imgOutput = imgImage.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hier = new Mat();
            Image<Gray, byte> imgout = new Image<Gray, byte>(imgImage.Width, imgImage.Height, new Gray(0));
            CvInvoke.FindContours(imgOutput, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(imgout, contours, -1, new MCvScalar(255, 0, 0));
            pictureBox1.Width = imgout.Width;
            pictureBox1.Height = imgout.Height;
            pictureBox1.Image = imgout.Bitmap;

            int w_l = 10; // на какое кол-во кусков будет делиться картинка
            int h_l = 10;

            int W = pictureBox1.Width / w_l; // ширина куска
            int H = pictureBox1.Height / h_l; // длина куска
            
            List<Image> imageParts = new List<Image>();
            
            int n = 2300;
            for (int i = 0; i < h_l; i++)
            {
                for (int j = 0; j < w_l; j++)
                {
                    Rectangle rectangle = new Rectangle(W * j, H * i, w_l, h_l);
                    var pic = (Bitmap)pictureBox1.Image;
                    imageParts.Add(pic.Clone(rectangle, PixelFormat.Format16bppRgb555));
                }
            }

            bool[,] ar = new bool[10, 10];

            int cnt = 0;

            List<Image> ans = new List<Image>();

            for(int i = 0; i < pictureBox1.Height - 10; i++)
            {
                for(int j = 0; j < pictureBox1.Width - 10; j++)
                {
                    if (CheckMatr(new Bitmap(imageParts[cnt]), i, j))
                    {
                        ans.Add(imageParts[cnt]);
                    }
                    cnt++;
                }
            }         
        }
    }
}



public struct Arr
{
    public Arr(int w, int h, int x, int y)
    {
        array = new bool[h, w];
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
    public bool[,] array;
    public int x, y;
    public int w, h;
}

class LagrangeInterpolator
{
    private readonly double[] xValues;
    private readonly double[] coefficients;

    public LagrangeInterpolator(double[] xValues, double[] yValues)
    {
        if (xValues.Length != yValues.Length) throw new ArgumentException();
        this.xValues = (double[])xValues.Clone();
        coefficients = new double[xValues.Length];
        for (int i = 0; i < xValues.Length; i++)
        {
            coefficients[i] = yValues[i];
            for (int j = 0; j < xValues.Length; j++)
            {
                if (i != j) coefficients[i] /= (xValues[i] - xValues[j]);
            }
        }
    }

    public double f(double x)
    {
        double result = 0.0;
        for (int i = 0; i < xValues.Length; i++)
        {
            double s = coefficients[i];
            for (int j = 0; j < xValues.Length; j++)
            {
                if (i != j) s *= (x - xValues[j]);
            }
            result += s;
        }
        return result;
    }

}