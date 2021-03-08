using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SergeImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxSide = 1024;
            bool isOverrideOldSmallImages = false;

            Console.WriteLine("example of command line parameters:");
            Console.WriteLine();
            Console.WriteLine("C:\\SergeImageResizer.exe 800");
            Console.WriteLine("resize with max side 800px, new images will not be overriden: 800");
            Console.WriteLine();
            Console.WriteLine("C:\\SergeImageResizer.exe 800 override");
            Console.WriteLine("resize with max side 800px, new images will not overriden, not reccomended!");

            if (args.Length > 0)
            {
                maxSide = Convert.ToInt16(args[0]);
            }

            if (args.Length >=2)
            {
                isOverrideOldSmallImages = true;
            }

            string path = Directory.GetCurrentDirectory();
            string smallPath;

            string[] files = Directory.GetFiles(path, "*.jpg");

            if (files != null && files.Length > 0)
                smallPath = GetSmallDir(path);
            else
            {
                Console.WriteLine("*.jpg files not found");
                Console.ReadLine();
                return;
            }

            Console.WriteLine(files.Length + " image files found.");

            foreach (string fileName in files)
            {
                ProcessFile(fileName, smallPath, maxSide, isOverrideOldSmallImages);
            }

            Console.WriteLine("All files are done, please press Enter.");
            Console.ReadLine();
        }

        static string GetSmallDir(string path)
        {
            path += @"\small";
            Directory.CreateDirectory(path); // If the directory already exists, this method does nothing.
            
            return path;
        }

        static void ProcessFile(string fileName, string smallPath, int maxSide, bool isOverride)
        {
            Console.WriteLine("Processing file:" + fileName);

            using (var image = Image.FromFile(fileName))
            {
                using (var newImage = ScaleImage(image, maxSide))
                {
                    string fileNameAndExtension = Path.GetFileName(fileName);
                    string newImagePath = smallPath + @"\" + fileNameAndExtension;

                    if (File.Exists(newImagePath) && !isOverride)
                        Console.WriteLine("File {0} exists.", newImagePath);
                    else
                    {
                        newImage.Save(newImagePath, ImageFormat.Jpeg);
                        Console.WriteLine("Small image {0} saved.", newImagePath);
                    }
                }
            }
            
            Console.WriteLine();
        }

        // my original design
        // unfortunately it didn't rotate some pictures.. like in case when I got LG cell phone (how it's related??)
        // need to add System.Drawing.dll
        public static Image ScaleImage(Image image, int maxSide)
        {
            double shrinkRatio;
            int newHeight;
            int newWidth;

            if (image.Height > image.Width)
            {
                shrinkRatio = (double) image.Height / maxSide;
                newHeight = maxSide;
                newWidth = (int) (image.Width / shrinkRatio);
            }
            else
            {
                shrinkRatio = (double)image.Width / maxSide;
                newWidth = maxSide;
                newHeight = (int)(image.Height / shrinkRatio);
            }

            var newImage = new Bitmap(newWidth, newHeight);  // old code 

            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        private const int OrientationKey = 0x0112;
        private const int NotSpecified = 0;
        private const int NormalOrientation = 1;
        private const int MirrorHorizontal = 2;
        private const int UpsideDown = 3;
        private const int MirrorVertical = 4;
        private const int MirrorHorizontalAndRotateRight = 5;
        private const int RotateLeft = 6;
        private const int MirorHorizontalAndRotateLeft = 7;
        private const int RotateRight = 8;

        /// <summary>
        /// Some images are being rotated when resized
        /// based on stackoverflow 
        /// https://stackoverflow.com/questions/33310562/some-images-are-being-rotated-when-resized
        /// 
        /// TODO: It's not ready yet!!!
        /// </summary>
        public static Image ScaleImageWithOrientation(Image image, int maxSide)
        {
            double shrinkRatio;
            int newHeight;
            int newWidth;

            if (image.Height > image.Width)
            {
                shrinkRatio = (double)image.Height / maxSide;
                newHeight = maxSide;
                newWidth = (int)(image.Width / shrinkRatio);
            }
            else
            {
                shrinkRatio = (double)image.Width / maxSide;
                newWidth = maxSide;
                newHeight = (int)(image.Height / shrinkRatio);
            }

            using (var newBitmap = new Bitmap(newWidth, newHeight))
            {
                using (var imageScaler = Graphics.FromImage(newBitmap))
                {
                    imageScaler.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    imageScaler.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    imageScaler.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                    imageScaler.DrawImage(image, imageRectangle);

                    // Fix orientation if needed.
                    if (image.PropertyIdList.Contains(OrientationKey))
                    {
                        var orientation = (int)image.GetPropertyItem(OrientationKey).Value[0];
                        switch (orientation)
                        {
                            case NotSpecified: // Assume it is good.
                            case NormalOrientation:
                                // No rotation required.
                                break;
                            case MirrorHorizontal:
                                newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case UpsideDown:
                                newBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            case MirrorVertical:
                                newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                                break;
                            case MirrorHorizontalAndRotateRight:
                                newBitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                                break;
                            case RotateLeft:
                                newBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case MirorHorizontalAndRotateLeft:
                                newBitmap.RotateFlip(RotateFlipType.Rotate270FlipX);
                                break;
                            case RotateRight:
                                newBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            default:
                                throw new NotImplementedException("An orientation of " + orientation + " isn't implemented.");
                        }
                    }

                    //newBitmap.Save(output, image.RawFormat);
                    Graphics.FromImage(newBitmap).DrawImage(image, 0, 0, newWidth, newHeight);
                    return newBitmap;

                }
            }

        }

    }
}
