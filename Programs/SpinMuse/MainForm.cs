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
            Bitmap koma;

            BitmapAxisCompression bmpAxComp = new BitmapAxisCompression(bmp);
            Point baseXY = bmpAxComp.GetLowestBlackPixelCoordinates();

            List<double> komasu = new List<double>();
            List<Bitmap> komas = new List<Bitmap>();

            for (int ang = 0; ang < 90; ang += 15)
            {
                komasu.Add(Math.Cos(ang / 180.0 * Math.PI));
            }

            //0～90度
            for (int i = 0; i < komasu.Count; i++)
            {
                koma = bmpAxComp.CompressAroundAxis(baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //90度
            koma = bmpAxComp.CompressAroundAxis(baseXY.X, 0);
            komas.Add(koma);

            //90～180度
            for (int i = komasu.Count - 1; i >= 0; i--)
            {
                koma = bmpAxComp.CompressAfterFlipAroundAxis(baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //180～270度
            for (int i = 0; i < komasu.Count; i++)
            {
                koma = bmpAxComp.CompressAfterFlipAroundAxis(baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            //270度
            koma = bmpAxComp.CompressAroundAxis(baseXY.X, 0);
            komas.Add(koma);

            //270～360度
            for (int i = komasu.Count - 1; i >= 0; i--)
            {
                koma = bmpAxComp.CompressAroundAxis(baseXY.X, komasu[i]);
                komas.Add(koma);
            }

            bmpAxComp?.Dispose();
            bmp?.Dispose();

            string gifFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);

            AnimatedGifCreator agc = new AnimatedGifCreator();
            agc.SaveGifWithMagickNET(komas, gifFilename + "_ani.gif", 5);

            for (int i = 1; i < komas.Count; i++)
            {
                komas[i]?.Dispose();
            }

            Image oldImage = this.pictureBox1.Image;

            this.pictureBox1.Image = komas[0];

            oldImage?.Dispose();
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
    }
}