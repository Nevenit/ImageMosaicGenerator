using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using NDesk.Options;

namespace ImageMosaicGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define variables for arguments
            bool showHelp = false;
            string imagePath = "";
            string tilesPath = "";
            int threadCount = 0;
            
            
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
            string[] tileImages = Directory.GetFiles(tilesPath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s))).ToArray();

            // Define arrays objects to be shared between classes
            SharedIncrementalArray sharedArray = new SharedIncrementalArray(tileImages);
            ThreadedStorage storage = new ThreadedStorage(tileImages);
            
            Thread[] threadList = new Thread[threadCount];
            
            // Start the threads to process images
            for (int i = 0; i < threadCount; i++)
            {
                int temp = i;
                threadList[i] = new Thread(() => ProcessImages(temp, sharedArray, storage));
                threadList[i].Start();
            }

            // Wait for the threads to finish
            WaitForThreadsToFinish(threadList);
            
        }

        /*
         * This function shows the user how to use the program
         */
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine ("Usage: dotnet ImageMosaicGenerator.dll [OPTIONS]");
            Console.WriteLine ("Convert the 'image' to a mosaic using 'tiles'");
            Console.WriteLine ();
            Console.WriteLine ("Options:");
            p.WriteOptionDescriptions (Console.Out);
        }

        /*
         * This function goes through all of the images and calculates their averago color
         */
        static void ProcessImages(int threadId, SharedIncrementalArray sharedArray, ThreadedStorage storage)
        {
            int index = 0;
            while (true)
            {
                // Get next image to process
                string pathToImage = sharedArray.GetNext(out index);

                // Verify an image got returned
                if (pathToImage == null || index == -1)
                    break;
                
                // Load the image
                using (Bitmap bm = new Bitmap(pathToImage))
                {
                    // Get the average color and convert to CIELAB
                    double[] imgCol = ColorConversion.RGBtoCIELAB(ImageProcessing.AverageImageColor(bm));
                    
                    // Lock the array and save the result
                    lock (storage.ImageColors)
                    {
                        // Save the color in a shared array
                        storage.ImageColors[index] = imgCol;
                    }
                }
            }
        }

        // This function simply waits until all threads have ended their task
        static void WaitForThreadsToFinish(Thread[] threadList)
        {
            while (true)
            {
                // Counter to keep track of how many threads are still alive
                int totalAlive = 0;
                
                // Loop through all threads and check if alive
                for (int i = 0; i < threadList.Length; i++)
                    if (threadList[i].IsAlive)
                        totalAlive++;
                
                // If none are alive, exit the loop
                if (totalAlive == 0)
                    break;
                
                Thread.Sleep(10);
            }
        }
    }
}