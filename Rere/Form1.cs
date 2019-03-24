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
using System.Threading;


namespace Rere
{
    public partial class Form1 : Form
    {
        const int k = 30;
        bool[,] r = new bool[k, k];
        bool[,] l = new bool[k, k];
        

        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
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
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if( j < 6)
                    {
                        bool temp = false;
                        temp = l[i, j];
                        l[i, j] = l[i, k - 1 - j];
                        l[i, k - 1 - j] = temp;
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

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    clr = picture.GetPixel(i, j);
                    if (clr.Name == "ffffffff")
                    {
                        temp[i, j] = true;
                    }
                    else temp[i, j] = false;
                }
            }

            int cnt = 0;

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if (temp[i, j] == r[i, j])
                        cnt++;
                }
            }

            if (cnt >= k)
            {
                y += h;
                for(int k = 0; k < pictureBox1.Height - h; k++)
                {
                    cnt = 0;
                    for (int i = 0; i <  h; i++)
                    {
                        for (int j = 0; j < w; j++)
                        {
                            clr = picture.GetPixel(i, j);
                            if (clr.Name == "ffffffff")
                            {
                                temp[i, j] = true;
                            }
                            else temp[i, j] = false;
                        }
                    }
                    y += k;

                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < k; j++)
                        {
                            if (temp[i, j] == r[i, j])
                                cnt++;
                        }
                    }
                    if (cnt >= 2 * k) return true;     
                }
            }

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

            int w_l = k; // на какое кол-во кусков будет делиться картинка
            int h_l = k;

            int W = pictureBox1.Width / w_l; // ширина куска
            int H = pictureBox1.Height / h_l; // длина куска
            
            List<Image> imageParts = new List<Image>();
            
            List<Image> ans = new List<Image>();
            for (int i = 0; i < pictureBox1.Height - h_l; i += 20)
            {
                for (int j = 0; j < pictureBox1.Width - w_l; j += 20)
                {
                    Rectangle rectangle = new Rectangle(j, i, w_l, h_l);
                    var pic = (Bitmap)pictureBox1.Image;
                    Image img = pic.Clone(rectangle, pic.PixelFormat);
                    if (CheckMatr(new Bitmap(img), i, j))
                    {
                        pictureBox2.Image = img;
                        Refresh();
                        Graphics g = this.CreateGraphics();

                        g.FillEllipse(new SolidBrush(Color.Red), i, j, 10, 10);
                        ans.Add(img);
                        richTextBox1.Text += "x: " + i + " y: " + j;
                        richTextBox1.Text += Environment.NewLine;
                    }
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