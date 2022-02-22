using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageMosaicGenerator
{
    public class ThreadedStorage
    {
        // This stores the paths to all tile images
        private string[] TilePaths;
        
        // This stores the ImagePaths as a queue used for multi threading
        public readonly Queue<string> ImagePathsQueue;
        
        // This stores both the path to the tile images and its colors
        public List<ImagePathColor> TilesColors = new List<ImagePathColor>();
        
        //This just stores the image
        public string ImagePath;

        public int TileSize;

        public ThreadedStorage(string[] imagePaths, string image, int tileSize)
        {
            TilePaths = imagePaths;
            ImagePathsQueue = new Queue<string>(TilePaths);
            ImagePath = image;
            TileSize = tileSize;
        }
    }
}