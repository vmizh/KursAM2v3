using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Helper
{
    public static class ImageManager
    {
        public static Image ByteToImage(byte[] bytes)
        {
            if (bytes == null) return null;
            var image = new Image();
            var strm = new MemoryStream(bytes);
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = strm;
            img.EndInit();
            strm.Flush();
            image.Source = img;
            return image;
        }
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }
        public static ImageSource ByteToImageSource(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg;
            return imgSrc;
        }
        public static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }

        public static System.Drawing.Image Crop(this System.Drawing.Image image, Rectangle selection)
        {
            Bitmap bmp = image as Bitmap;
            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("No valid bitmap");
            // Crop the image:
            Bitmap cropBmp = bmp.Clone(selection, bmp.PixelFormat);
            // Release the resources:
            image.Dispose();
            return cropBmp;
        }
    }

}