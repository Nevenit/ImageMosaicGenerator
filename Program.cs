﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using NDesk.Options;

namespace ImageMosaicGenerator
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Define variables for arguments
            var showHelp = false;
            var imagePath = "";
            var tilesPath = "";
            var tileSize = 0;
            var imageSize = 0;
            var threadCount = 0;
            
            // Define arguments
            var p = new OptionSet () {
                { 
                    "i|image=", "Path to the image",
                    v => imagePath = v 
                },
                {
                    "t|tiles=", "Path to the folder containing tile images",
                    v => tilesPath = v 
                },
                {
                    "s|tileSize=", "The size of individual tiles",
                    (int v) => tileSize = v
                },
                {
                    "w|imageSize=", "The size of the image in tiles",
                    (int v) => imageSize = v
                },
                {
                    "c|threads=", "The number of threads to use",
                    (int v) => threadCount = v
                },
                { 
                    "h|help",  "show this message and exit", 
                    v => showHelp = v != null 
                },
            };

            // Parse arguments
            List<string> extra;
            try 
            {
                extra = p.Parse(args);
            }
            catch (OptionException e) 
            {
                Console.Write("Invalid Arguments: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Use `--help' for more information.");
                return;
            }
            
            // Display help
            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            // Find all image files 
            var ext = new List<string> { ".jpg", ".png" };
            var tileImages = Directory.GetFiles(tilesPath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s))).ToArray();

            // Define arrays objects to be shared between classes
            var storage = new ThreadedStorage(tileImages, imagePath, tileSize);
            var threadList = new Task[threadCount];

            Console.WriteLine("Pricess Tiles");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Start the threads to process images
            // This calculates the average color foe each pixel
            for (var i = 0; i < threadCount; i++)
            {
                threadList[i] = Task.Factory.StartNew(() => ProcessTileImages(storage));
            }

            // Wait for the threads to finish
            Task.WaitAll(threadList);

            stopwatch.Stop();
            Console.WriteLine("Elapsed time: " + stopwatch.ElapsedMilliseconds / 1000);

            stopwatch.Restart();
            Console.WriteLine("Pricess Image");

            // Define the color queue
            Queue<PixelColorAndPosition> ImageColorQueue;
            int[] tiledImageSize;

            // Calculate image size after scaling it down to tiles
            using (Bitmap bm = new Bitmap(imagePath))
            {
                int smallestSide = Math.Min(bm.Width, bm.Height);

                tiledImageSize = new int[2] { (int)Math.Round((float)bm.Width / smallestSide * imageSize), (int)Math.Round((float)bm.Height / smallestSide * imageSize) };

                // Create the color queue to be used by the threads
                // This stores the Image as a color array in a queue used for multi threading
                using (Bitmap sBm = ImageProcessing.ResizeImage(bm, tiledImageSize[0], tiledImageSize[1]))
                {
                    ImageColorQueue = new Queue<PixelColorAndPosition>(Misc.BitmapToColorList(sBm));
                }
            }

            Bitmap finalImage = new Bitmap(tiledImageSize[0] * tileSize, tiledImageSize[1] * tileSize);

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.None;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                // override the old thread array with an empty one
                threadList = new Task[threadCount];

                // Start the threads to process the image
                // This picks the image for each pixel of the image
                for (var i = 0; i < threadCount; i++)
                {
                    threadList[i] = Task.Factory.StartNew(() => ProcessImage(storage, ref ImageColorQueue, g));
                }

                // Wait for the threads to finish
                Task.WaitAll(threadList);
            }
            stopwatch.Stop();
            Console.WriteLine("Elapsed time: " + stopwatch.ElapsedMilliseconds / 1000);
            finalImage.Save("yeet.png");
        }

        /*
         * This function shows the user how to use the program
         */
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine ("Usage: dotnet ImageMosaicGenerator.dll [OPTIONS]");
            Console.WriteLine ("Convert the 'image' to a mosaic using 'tiles'");
            Console.WriteLine ();
            Console.WriteLine ("Options:");
            p.WriteOptionDescriptions (Console.Out);
        }

        /*
         * This function goes through all of the images and calculates their average color
         */
        private static void ProcessTileImages(ThreadedStorage storage)
        {
            while (storage.ImagePathsQueue.Count > 0)
            {
                // Get next image to process
                string pathToImage;
                lock (storage.ImagePathsQueue)
                {
                    pathToImage = storage.ImagePathsQueue.Dequeue();
                }

                // Verify an image got returned
                if (pathToImage == null)
                    break;

                // Run the code in a try loop in case something goes wrong
                try
                {
                    // Load the image
                    using var bm = new Bitmap(pathToImage);

                    // Square the image so it can be used as a single pixel
                    using var squareBm = ImageProcessing.SquareImage(bm);

                    // Resize image
                    //using var resizedBm = ImageProcessing.ResizeImage(bm, storage.TileSize, storage.TileSize);

                    // Get the average color and convert to CIELAB
                    var imgCol = ColorConversion.RGBtoCIELAB(ImageProcessing.AverageImageColor(squareBm));

                    // Lock the array and save the result
                    lock (storage.TilesColors)
                    {
                        // Save the color in a shared array
                        storage.TilesColors.Add(new ImagePathColor(pathToImage, imgCol));
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void ProcessImage(ThreadedStorage storage, ref Queue<PixelColorAndPosition> ImageColorQueue, Graphics g)
        {
            ImagePathColor[] localList = storage.TilesColors.ToArray();
            while (ImageColorQueue.Count > 0)
            {
                // Get next pixel to process
                PixelColorAndPosition pixel;
                lock (ImageColorQueue)
                {
                    pixel = ImageColorQueue.Dequeue();
                }

                // Get closest image
                ImagePathColor pixelImage = Misc.FindClosesColor(pixel.color, localList);

                // Draw image
                using Bitmap bm = new Bitmap(pixelImage.ImagePath);
                using Bitmap sBm = ImageProcessing.SquareImage(bm);
                using Bitmap smallSBm = ImageProcessing.ResizeImage(sBm, storage.TileSize, storage.TileSize);
                lock (g)
                {
                    g.DrawImage(smallSBm, pixel.position[0] * storage.TileSize, pixel.position[1] * storage.TileSize, storage.TileSize, storage.TileSize);
                    //g.FillRectangle(new SolidBrush(ColorConversion.CIELABtoRGB(pixel.color)), pixel.position[0] * storage.TileSize, pixel.position[1] * storage.TileSize, storage.TileSize, storage.TileSize);
                }
            }
        }

        // This function simply waits until all threads have ended their task
        private static void WaitForThreadsToFinish(IReadOnlyList<Thread> threadList)
        {
            while (true)
            {
                // Counter to keep track of how many threads are still alive
                var totalAlive = 0;
                
                // Loop through all threads and check if alive
                foreach (var t in threadList)
                    if (t.IsAlive)
                        totalAlive++;

                // If none are alive, exit the loop
                if (totalAlive == 0)
                    break;
                
                Thread.Sleep(10);
            }
        }
    }
}