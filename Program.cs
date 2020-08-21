using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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
            path = path + @"\small";
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

        //need to add System.Drawing.dll
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

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
    }
}
