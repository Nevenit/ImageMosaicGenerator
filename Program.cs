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

namespace ImageMosaicGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                throw new Exception("Missing arguments");
            }
            
            string imagePath = args[0];
            string tilesPath = args[1];
            int threadCount = Int32.Parse(args[2]);
            
            var ext = new List<string> { ".jpg", ".png" };
            string[] tileImages = Directory.GetFiles(tilesPath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s))).ToArray();

            SharedIncrementalArray sharedArray = new SharedIncrementalArray(tileImages);
            ThreadedStorage storage = new ThreadedStorage(tileImages);
            
            Thread[] threadList = new Thread[threadCount];
            
            for (int i = 0; i < threadCount; i++)
            {
                int temp = i;
                threadList[i] = new Thread(() => ProcessImages(temp, sharedArray, storage));
                threadList[i].Start();
            }

            WaitForThreadsToFinish(threadList);

        }

        static void ProcessImages(int threadId, SharedIncrementalArray sharedArray, ThreadedStorage storage)
        {
            int index = 0;
            while (true)
            {
                string pathToImage = sharedArray.GetNext(out index);
                
                if (index == -1)
                    break;

                using (Bitmap bm = new Bitmap(pathToImage))
                {
                    Color imgCol = ImageProcessing.AverageImageColor(bm);
                }
                Console.WriteLine(index);
            }
        }

        static void WaitForThreadsToFinish(Thread[] threadList)
        {
            while (true)
            {
                int totalAlive = 0;
                for (int i = 0; i < threadList.Length; i++)
                    if (threadList[i].IsAlive)
                        totalAlive++;
                
                if (totalAlive == 0)
                    break;
            }
        }
    }
}