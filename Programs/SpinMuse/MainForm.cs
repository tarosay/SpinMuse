using System;
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
        private Point? _lastClickedPoint;
        private string _imageFilemane = "";

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

            this._imageFilemane = filenames[0];
            Bitmap bmp = ReadImageFile(this._imageFilemane);

            BitmapAxisCompression bmpAxComp = new BitmapAxisCompression(bmp);
            Point baseXY = bmpAxComp.GetLowestBlackPixelCoordinates();

            bmpAxComp.getMonochromeImage(out Bitmap silhouette);

            bmpAxComp?.Dispose();
            bmp?.Dispose();

            this.pbxEdit.Image?.Dispose();
            this.pbxEdit.Image = silhouette;

            this._lastClickedPoint = GetImagePointOnZoom(baseXY);
            pbxEdit.Invalidate(); // PictureBoxを再描画する

            this.btnCreateGifWithRedLineAxisColor.Enabled = true;
            this.btnCreateGifWithRedLineAxisMono.Enabled = true;
        }

        private string CreateGifWithRedLineAxi(string filename, bool IsMonochrome = true)
        {
            string gifanimeFilename = "";
            if (!File.Exists(filename)) { return gifanimeFilename; }

            if (!this._lastClickedPoint.HasValue) { return gifanimeFilename; }

            Point point = new Point(this._lastClickedPoint.Value.X, this._lastClickedPoint.Value.Y);

            Bitmap bmp = ReadImageFile(filename);
            Bitmap koma;

            Point baseXY = GetPixcelOnPicture(point);

            BitmapAxisCompression bmpAxComp = new BitmapAxisCompression(bmp);

            Bitmap sample;
            if (IsMonochrome)
            {
                bmpAxComp.getMonochromeImage(out sample);
            }
            else
            {
                bmpAxComp.getImage(out sample);
            }

            List<double> komasu = new List<double>();
            List<Bitmap> komas = new List<Bitmap>();

            for (int ang = 0; ang < 90; ang += 15)
            {
                komasu.Add(Math.Cos(ang / 180.0 * Math.PI));
            }

            //0～90度
            for (int i = 0; i < komasu.Count; i++)
            {
                koma = bmpAxComp.CompressAroundAxis(sample, baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //90度
            koma = bmpAxComp.CompressAroundAxis(sample, baseXY.X, 0);
            komas.Add(koma);

            //90～180度
            for (int i = komasu.Count - 1; i >= 0; i--)
            {
                koma = bmpAxComp.CompressAfterFlipAroundAxis(sample, baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //180～270度
            for (int i = 0; i < komasu.Count; i++)
            {
                koma = bmpAxComp.CompressAfterFlipAroundAxis(sample, baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //270度
            koma = bmpAxComp.CompressAroundAxis(sample, baseXY.X, 0);
            komas.Add(koma);

            //270～360度
            for (int i = komasu.Count - 1; i >= 0; i--)
            {
                koma = bmpAxComp.CompressAroundAxis(sample, baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            bmpAxComp?.Dispose();
            bmp?.Dispose();
            sample?.Dispose();

            gifanimeFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_ani.gif";

            AnimatedGifCreator agc = new AnimatedGifCreator();
            agc.SaveGifWithMagickNET(komas, gifanimeFilename, 5);

            for (int i = 1; i < komas.Count; i++)
            {
                komas[i]?.Dispose();
            }

            return gifanimeFilename;
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
        private Point GetImagePointOnZoom(Point point)
        {
            float imageAspect = (float)this.pbxEdit.Image.Width / this.pbxEdit.Image.Height;
            float controlAspect = (float)this.pbxEdit.Width / this.pbxEdit.Height;
            float xOffset = 0;
            float yOffset = 0;
            float adjustedWidth = this.pbxEdit.Width;
            float adjustedHeight = this.pbxEdit.Height;

            if (imageAspect > controlAspect)
            {
                // If image is wider, adjust height and calculate y offset
                adjustedHeight = this.pbxEdit.Width / imageAspect;
                yOffset = (this.pbxEdit.Height - adjustedHeight) / 2;
            }
            else
            {
                // If image is taller, adjust width and calculate x offset
                adjustedWidth = this.pbxEdit.Height * imageAspect;
                xOffset = (this.pbxEdit.Width - adjustedWidth) / 2;
            }

            int x = (int)((point.X * adjustedWidth / this.pbxEdit.Image.Width) + xOffset);
            int y = (int)((point.Y * adjustedHeight / this.pbxEdit.Image.Height) + yOffset);

            return new Point(x, y);
        }
        private Point GetPixcelOnPicture(Point location)
        {
            float imageAspect = (float)this.pbxEdit.Image.Width / this.pbxEdit.Image.Height;
            float controlAspect = (float)this.pbxEdit.Width / this.pbxEdit.Height;
            float xOffset = 0;
            float yOffset = 0;
            float adjustedWidth = this.pbxEdit.Width;
            float adjustedHeight = this.pbxEdit.Height;

            if (imageAspect > controlAspect)
            {
                // If image is wider, adjust height and calculate y offset
                adjustedHeight = this.pbxEdit.Width / imageAspect;
                yOffset = (this.pbxEdit.Height - adjustedHeight) / 2;
            }
            else
            {
                // If image is taller, adjust width and calculate x offset
                adjustedWidth = this.pbxEdit.Height * imageAspect;
                xOffset = (this.pbxEdit.Width - adjustedWidth) / 2;
            }

            int originalX = (int)((location.X - xOffset) * (this.pbxEdit.Image.Width / adjustedWidth));
            int originalY = (int)((location.Y - yOffset) * (this.pbxEdit.Image.Height / adjustedHeight));

            return new Point(originalX, originalY);
        }


        private void pbxEdit_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.pbxEdit.Image == null)
            {
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            Rectangle pictureBoxBounds = new Rectangle(0, 0, pbxEdit.Width, pbxEdit.Height);

            if (pictureBoxBounds.Contains(e.Location))
            {
                this._lastClickedPoint = e.Location;
                pbxEdit.Invalidate(); // PictureBoxを再描画する
            }
            else
            {
                this._lastClickedPoint = null;
            }
        }

        private void pbxEdit_Paint(object sender, PaintEventArgs e)
        {
            if (this.pbxEdit.Image == null)
            {
                return;
            }

            if (this._lastClickedPoint.HasValue)
            {
                using (Pen pen = new Pen(Color.Red, 2)) // 赤い色と太さ2のペンを使用します。
                {
                    int x = this._lastClickedPoint.Value.X;
                    e.Graphics.DrawLine(pen, x, 0, x, pbxEdit.Height);
                }
            }
        }

        private void btnCreateGifWithRedLineAxisColor_Click(object sender, EventArgs e)
        {
            this.pbxAnimation.Image?.Dispose();

            string gifAnimeFilename = CreateGifWithRedLineAxi(this._imageFilemane, false);

            if (gifAnimeFilename == "")
            {
                return;
            }

            this.pbxAnimation.Image?.Dispose();
            this.pbxAnimation.Image = Image.FromFile(gifAnimeFilename);
        }

        private void btnCreateGifWithRedLineAxisMono_Click(object sender, EventArgs e)
        {
            this.pbxAnimation.Image?.Dispose();

            string gifAnimeFilename = CreateGifWithRedLineAxi(this._imageFilemane);

            if (gifAnimeFilename == "")
            {
                return;
            }

            this.pbxAnimation.Image?.Dispose();
            this.pbxAnimation.Image = Image.FromFile(gifAnimeFilename);
        }
    }
}