using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FamilySuper.Host.WPF.Services
{
    public static class IconGenerator
    {
        public static void GenerateFamilySuperIcon(string outputPath)
        {
            var iconSizes = new[] { 16, 32, 48, 64, 128, 256 };
            var bitmaps = new List<Bitmap>();

            foreach (var size in iconSizes)
            {
                var bitmap = CreateLogoBitmap(size);
                bitmaps.Add(bitmap);
            }

            using (var fs = new FileStream(outputPath, FileMode.Create))
            {
                WriteIcoFile(fs, bitmaps);
            }

            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }
        }

        private static Bitmap CreateLogoBitmap(int size)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);

                var centerX = size / 2f;
                var centerY = size / 2f;
                var radius = size * 0.35f;

                var gradient = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(size, size),
                    Color.FromArgb(66, 133, 244),
                    Color.FromArgb(234, 67, 53));

                var circlePath = new GraphicsPath();
                circlePath.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
                graphics.FillPath(gradient, circlePath);

                float fontSize = size * 0.45f;
                if (size <= 16) fontSize = size * 0.5f;
                if (size >= 256) fontSize = size * 0.4f;

                var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString("家", font, Brushes.White, centerX, centerY, stringFormat);

                var glowBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        graphics.DrawString("家", font, glowBrush, centerX + i, centerY + j, stringFormat);
                    }
                }

                font.Dispose();
                gradient.Dispose();
                circlePath.Dispose();
                glowBrush.Dispose();
            }

            return bitmap;
        }

        private static void WriteIcoFile(Stream stream, List<Bitmap> bitmaps)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)bitmaps.Count);

                long offset = 6 + (16 * bitmaps.Count);

                var imageDataList = new List<byte[]>();

                foreach (var bitmap in bitmaps)
                {
                    var pngStream = new MemoryStream();
                    bitmap.Save(pngStream, ImageFormat.Png);
                    var pngData = pngStream.ToArray();
                    imageDataList.Add(pngData);

                    writer.Write((byte)(bitmap.Width >= 256 ? 0 : bitmap.Width));
                    writer.Write((byte)(bitmap.Height >= 256 ? 0 : bitmap.Height));
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((ushort)0);
                    writer.Write((ushort)32);
                    writer.Write((uint)pngData.Length);
                    writer.Write((uint)offset);

                    offset += pngData.Length;
                }

                foreach (var imageData in imageDataList)
                {
                    writer.Write(imageData);
                }
            }
        }

        public static void GenerateLogoPng(string outputPath, int size = 256)
        {
            using (var bitmap = CreateLogoBitmap(size))
            {
                bitmap.Save(outputPath, ImageFormat.Png);
            }
        }
    }
}