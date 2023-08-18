using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace SpinMuse
{
    public class AnimatedGifCreator
    {
        public void SaveGifWithMagickNET(List<Bitmap> bitmaps, string filePath, int delay)
        {
            using (MagickImageCollection collection = new MagickImageCollection())
            {
                for (int i = 0; i < bitmaps.Count; i++)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bitmaps[i].Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        var magickImage = new MagickImage(memory.ToArray());
                        magickImage.AnimationDelay = delay; // delay is in 1/100ths of a second
                        collection.Add(magickImage);
                    }
                }

                // Quantize settings defines quality of the gif
                var settings = new QuantizeSettings
                {
                    Colors = 256
                };

                collection.Quantize(settings);

                collection.Optimize(); // Optimize reduces the file size

                collection.Write(filePath);
            }
        }
    }
}
