﻿using Emgu.CV;
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
        public Form1()
        {
            InitializeComponent();
        }
        Image<Bgr, byte> imgImage;
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

            int w_l = 60; // на какое кол-во кусков будет делиться картинка
            int h_l = 50; 

            int W = pictureBox1.Width / w_l; // ширина куска
            int H = pictureBox1.Height / h_l; // длина куска

            List<Point> countours = new List<Point>();
           
            List<Image> imageParts = new List<Image>();
            for(int i = 0; i < h_l; i++)
            {
                for(int j = 0; j < w_l; j++)
                {
                    Rectangle rectangle = new Rectangle(W * j, H * i, W, H);
                    var pic = (Bitmap)pictureBox1.Image;
                    imageParts.Add(pic.Clone(rectangle, PixelFormat.Format16bppRgb555));
                }
            }
            pictureBox2.Image = imageParts[0];
        }

        public Arr ConverToArray(Image img, Point p, int W, int H)
        {
            Bitmap picture = new Bitmap(img);
            Arr a = new Arr(W, H, p.X, p.Y);
            Color clr = new Color();
            for(int i = p.Y; i < H; i++)
            {
                for(int j = p.X; j < W; j++ )
                {
                    clr = picture.GetPixel(i, j);
                    if(clr.Name == "ffffffff")
                    {
                        a.array[i, j] = false;            
                    }
                    else
                    {
                        a.array[i, j] = true;
                    }
                }
            }
            
            return a;
        }
    }
}

public struct Arr
{
    public Arr(int w, int h, int x, int y)
    {
        array = new bool[w, h];
        this.x = x;
        this.y = y;
    }
    public bool[,] array;
    public int x, y;
}
