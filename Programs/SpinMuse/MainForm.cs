﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpinMuse
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            //ドラッグドロップの処理
            DragDropMethod(e);
        }
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
        }

        private void DragDropMethod(DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames == null && filenames.Length == 0)
            {
                return;
            }

            if (!File.Exists(filenames[0])) { return; }

            // backgroundWorker1.RunWorkerAsync(filenames[0]);

            string filename = filenames[0];
            Bitmap bmp = ReadImageFile(filename);
            byte[][,] pixelData = ConvertBitmapToByteArray(bmp);

            //pixelData[][0,0]の色を白、それ以外を黒にします。
            int width = pixelData[0].GetLength(1);
            int height = pixelData[0].GetLength(0);

            byte r = pixelData[0][0, 0];
            byte g = pixelData[1][0, 0];
            byte b = pixelData[2][0, 0];

            //一番下にある黒いピクセルの座標を検索します。
            int x0 = 0;
            int y0 = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if(pixelData[0][y,x]!=r || pixelData[1][y, x] != g || pixelData[2][y, x] != b)
                    {
                        pixelData[0][y, x] = 0;
                        pixelData[1][y, x] = 0;
                        pixelData[2][y, x] = 0;

                        if (y > y0)
                        {
                            x0 = x;
                            y0 = y;
                        }
                    }
                    else
                    {
                        pixelData[0][y, x] = 255;
                        pixelData[1][y, x] = 255;
                        pixelData[2][y, x] = 255;
                    }
                }
            }

            //n分割します
            int n = 10;
            //10で真横向き,10で反転拡大、10で反転縮小して真横、10で正方向の拡大
            Bitmap[] animeKoma = new Bitmap[(n - 1) * 4 + 1];
            animeKoma[0] = new Bitmap(ConvertByteArrayToBitmap(pixelData));



            if (bmp != null)
            {
                bmp.Dispose();
            }

            bmp = ConvertByteArrayToBitmap(pixelData);

            bmp.Save(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_a.png", ImageFormat.Png);

            Image oldImage = this.pictureBox1.Image;

            this.pictureBox1.Image = bmp;

            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        public Bitmap ReadImageFile(string filename)
        {
            Bitmap bmp = null;
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // メモリストリームにデータをコピー
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);

                        // メモリストリームから画像をロード
                        using (Image image = Image.FromStream(ms))
                        {
                            // 新しいBitmapオブジェクトを作成
                            bmp = new Bitmap(image);
                            // bmp = image.Clone();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            return bmp;
        }

        public byte[][,] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            // Bitmapをロックし、BitmapDataオブジェクトを取得
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite,bitmap.PixelFormat);

            // BitmapDataからのピクセルデータのサイズを計算
            int byteCount = bmpData.Stride * bitmap.Height;
            byte[] byteArray = new byte[byteCount];

            // BitmapDataからbyte配列にピクセルデータをコピー
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, byteArray, 0, byteCount);

            // Bitmapのロックを解除
            bitmap.UnlockBits(bmpData);

            byte[][,] byteRGBArray = new byte[3][,] {
                new byte[bitmap.Height, bitmap.Width],
                new byte[bitmap.Height, bitmap.Width],
                new byte[bitmap.Height, bitmap.Width] };

            int onePixByte = byteCount / (bitmap.Height * bitmap.Width);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    byteRGBArray[0][y, x] = byteArray[onePixByte * (y * bitmap.Width + x) + 2];
                    byteRGBArray[1][y, x] = byteArray[onePixByte * (y * bitmap.Width + x) + 1];
                    byteRGBArray[2][y, x] = byteArray[onePixByte * (y * bitmap.Width + x)];
                }
            }
            return byteRGBArray;
        }

        public Bitmap ConvertByteArrayToBitmap(byte[][,] byteArray)
        {
            int width = byteArray[0].GetLength(1);
            int height = byteArray[0].GetLength(0);
            Byte[] bytes = new Byte[4 * width * height];

            for (Int32 y = 0; y < height; y++)
            {
                for (Int32 x = 0; x < width; x++)
                {
                    bytes[4 * (y * width + x)] = byteArray[2][y, x];
                    bytes[4 * (y * width + x) + 1] = byteArray[1][y, x];
                    bytes[4 * (y * width + x) + 2] = byteArray[0][y, x];
                    bytes[4 * (y * width + x) + 3] = 255;
                }
            }

            // ビットマップを定義
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // ビットマップデータを操作するために、マップされているアンマネージドメモリ領域をロックします。
            BitmapData bitmapData = bitmap.LockBits(
                                 new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                 ImageLockMode.WriteOnly, bitmap.PixelFormat);

            try
            {
                // 画像メモリ領域にデータをコピーします。
                Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
            }
            catch
            {

            }
            finally
            {
                // メモリ領域のロックを解放します。
                bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string filename = e.Argument as string;
            Bitmap bmp = ReadImageFile(filename);
            byte[][,] pixelData = ConvertBitmapToByteArray(bmp);

            bmp.Save(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_a.png", ImageFormat.Png);




            e.Result = pixelData;
            bmp.Dispose();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}