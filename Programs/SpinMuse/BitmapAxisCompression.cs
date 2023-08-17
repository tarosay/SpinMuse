using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpinMuse
{
    public class BitmapAxisCompression
    {
        private readonly Bitmap _bitmap;
        private Bitmap _monochromeImage = null;
        private byte[][,] _pixelData = null;

        public BitmapAxisCompression(Bitmap inputBitmap)
        {
            this._bitmap = inputBitmap;
            ConvertToMonochromeAndSet();
            this._pixelData = ConvertBitmapToByteArray(this._monochromeImage);
        }

        public byte[][,] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            // Bitmapをロックし、BitmapDataオブジェクトを取得
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

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

        public void ConvertToMonochromeAndSet()
        {
            this._monochromeImage = ConvertToMonochromeBasedOnTopLeftPixel(this._bitmap);
        }

        public Bitmap ConvertToMonochromeBasedOnTopLeftPixel(Bitmap original)
        {
            if (original == null)
            {
                if (this._monochromeImage != null)
                {
                    this._monochromeImage.Dispose();
                }
                this._monochromeImage = null;
                this._pixelData = null;
                return null;
            }

            byte[][,] pixelData = ConvertBitmapToByteArray(original);

            //pixelData[][0,0]の色を白、それ以外を黒にします。
            int width = pixelData[0].GetLength(1);
            int height = pixelData[0].GetLength(0);

            byte r = pixelData[0][0, 0];
            byte g = pixelData[1][0, 0];
            byte b = pixelData[2][0, 0];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixelData[0][y, x] != r || pixelData[1][y, x] != g || pixelData[2][y, x] != b)
                    {
                        pixelData[0][y, x] = 0;
                        pixelData[1][y, x] = 0;
                        pixelData[2][y, x] = 0;
                    }
                    else
                    {
                        pixelData[0][y, x] = 255;
                        pixelData[1][y, x] = 255;
                        pixelData[2][y, x] = 255;
                    }
                }
            }
            return new Bitmap(ConvertByteArrayToBitmap(pixelData));
        }

        public Point GetLowestBlackPixelCoordinates()
        {
            int width = this._pixelData[0].GetLength(1);
            int height = this._pixelData[0].GetLength(0);

            int y0 = 0;
            int x0 = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (this._pixelData[0][y, x] == 0 && this._pixelData[1][y, x] == 0 && this._pixelData[2][y, x] == 0)
                    {
                        if (y0 < y)
                        {
                            x0 = x;
                            y0 = y;
                        }
                    }
                }
            }
            return new Point(x0, y0);
        }


        public Bitmap BalanceBitmapByXCoordinate(Bitmap original, int xCoordinate)
        {
            int leftWidth = xCoordinate;
            int rightWidth = original.Width - xCoordinate;

            // 左右のどちらが短いかを判断
            int diff = Math.Abs(leftWidth - rightWidth);
            Bitmap result = new Bitmap(original.Width + diff, original.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                // 元のBitmapを描画
                g.DrawImage(original, 0, 0);

                // 左側が短い場合、左側に白色の縁を追加
                if (leftWidth < rightWidth)
                {
                    g.FillRectangle(Brushes.White, 0, 0, diff, original.Height);
                    g.DrawImage(original, diff, 0);
                }
                // 右側が短い場合、右側に白色の縁を追加
                else if (rightWidth < leftWidth)
                {
                    g.FillRectangle(Brushes.White, original.Width, 0, diff, original.Height);
                }
            }
            return result;
        }

        // 指定したX座標を通るY軸を基軸として、外側の画素を指定比率で縮める
        public Bitmap CompressAroundAxis(int xAxis, double compressionRatio)
        {
            Bitmap bbbmp = BalanceBitmapByXCoordinate(this._monochromeImage, xAxis);

            if (compressionRatio >= 1.0)
            {
                return bbbmp;
            }

            Bitmap result = ResizeAndCombine(bbbmp, compressionRatio);
            bbbmp.Dispose();

            return result;



            //Bitmap withBorder;
            ////先ず、Xの値が画像の真ん中に来るように画像の幅を調整する
            //int width = this._monochromeImage.Width;
            //int height = this._monochromeImage.Height;
            //int left_width = xAxis;
            //int right_width = width - xAxis;

            //if (left_width > right_width)
            //{
            //    //左の方が大きかったら、右側に足りない部分を足す
            //    int borderWidth = left_width - right_width;

            //    // 新しいBitmapのサイズを、オリジナルの幅 + 境界の幅、オリジナルの高さとして設定
            //    withBorder = new Bitmap(width + borderWidth, height);

            //    using (Graphics g = Graphics.FromImage(withBorder))
            //    {
            //        // 元の画像を描画
            //        g.DrawImage(this._monochromeImage, 0, 0);

            //        // 白色のブラシを使って、右側に縁を追加
            //        g.FillRectangle(Brushes.White, width, 0, borderWidth, height);
            //    }
            //}
            //else
            //{
            //    int borderWidth = right_width - left_width;

            //    // 新しいBitmapのサイズを、オリジナルの幅 + 境界の幅、オリジナルの高さとして設定
            //    withBorder = new Bitmap(width + borderWidth, height);

            //    using (Graphics g = Graphics.FromImage(withBorder))
            //    {
            //        // 元の画像を縁の分だけ右に移動して描画
            //        g.DrawImage(this._monochromeImage, borderWidth, 0);

            //        // 白色のブラシを使って、左側に縁を追加
            //        g.FillRectangle(Brushes.White, 0, 0, borderWidth, height);
            //    }
            //}

            //if (compressionRatio >= 1.0)
            //{
            //    return withBorder;
            //}

            //Bitmap result = ResizeAndCombine(withBorder, compressionRatio);
            //withBorder.Dispose();
            //return result;
        }

        public Bitmap ResizeAndCombine(Bitmap original, double resizeRatio)
        {
            int halfWidth = original.Width / 2;

            // 元のBitmapを2つに等分する
            using (Bitmap leftPart = original.Clone(new Rectangle(0, 0, halfWidth, original.Height), original.PixelFormat))
            using (Bitmap rightPart = original.Clone(new Rectangle(halfWidth, 0, halfWidth, original.Height), original.PixelFormat))
            {
                // それぞれのBitmapを指定の割合でX方向に縮める
                using (Bitmap resizedLeft = new Bitmap((int)(halfWidth * resizeRatio), original.Height))
                using (Bitmap resizedRight = new Bitmap((int)(halfWidth * resizeRatio), original.Height))
                {
                    using (Graphics g = Graphics.FromImage(resizedLeft))
                    {
                        g.DrawImage(leftPart, 0, 0, resizedLeft.Width, original.Height);
                    }

                    using (Graphics g = Graphics.FromImage(resizedRight))
                    {
                        g.DrawImage(rightPart, 0, 0, resizedRight.Width, original.Height);
                    }

                    // 縮めたBitmapのサイズが小さくなった分、白色の幅を追加する
                    using (Bitmap finalLeft = new Bitmap(resizedLeft.Width + (halfWidth - resizedLeft.Width), original.Height))
                    using (Bitmap finalRight = new Bitmap(resizedRight.Width + (halfWidth - resizedRight.Width), original.Height))
                    {
                        using (Graphics g = Graphics.FromImage(finalLeft))
                        {
                            g.FillRectangle(Brushes.White, 0, 0, finalLeft.Width, original.Height);
                            g.DrawImage(resizedLeft, finalLeft.Width - resizedLeft.Width, 0);
                        }

                        using (Graphics g = Graphics.FromImage(finalRight))
                        {
                            g.FillRectangle(Brushes.White, 0, 0, finalRight.Width, original.Height);
                            g.DrawImage(resizedRight, 0, 0);
                        }

                        // 2つのBitmapを再度結合して、1つのBitmapにする
                        Bitmap result = new Bitmap(original.Width, original.Height);
                        using (Graphics g = Graphics.FromImage(result))
                        {
                            g.DrawImage(finalLeft, 0, 0);
                            g.DrawImage(finalRight, finalLeft.Width, 0);
                        }

                        return result;
                    }
                }
            }
        }
        public Bitmap FlipHorizontally(Bitmap original)
        {
            Bitmap flipped = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(flipped))
            {
                g.Clear(Color.White);  // Optional: Clear with white if desired

                // Set up the transformation matrix for horizontal flip
                Matrix matrix = new Matrix(-1, 0, 0, 1, original.Width, 0);
                g.Transform = matrix;

                // Draw the original bitmap onto the graphics of the new bitmap
                g.DrawImage(original, 0, 0);
            }

            return flipped;
        }

        // ビットマップを左右反転させてから、指定したX座標を通るY軸を基軸として、外側の画素を指定比率で縮める
        public Bitmap CompressAfterFlipAroundAxis(int xAxis, double compressionRatio)
        {
            Bitmap bbbmp = BalanceBitmapByXCoordinate(this._monochromeImage, xAxis);
            Bitmap flipbmp = FlipHorizontally(bbbmp);
            bbbmp.Dispose();

            if (compressionRatio >= 1.0)
            {
                return flipbmp;
            }

            Bitmap result = ResizeAndCombine(flipbmp, compressionRatio);
            flipbmp.Dispose();

            return result;
        }
    }
}
