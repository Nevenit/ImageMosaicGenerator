using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageMosaicGenerator
{
    public class ThreadedStorage
    {
        // This stores the paths to all tile images
        private string[] ImagePaths;
        
        // This stores the ImagePaths as a queue used for multi threading
        public readonly Queue<string> ImagePathsQueue;
        
        // This stores both the path to the tile images and its colors
        public List<ImagePathColor> TilesColors = new List<ImagePathColor>();
        
        //This just stores the image
        private Bitmap Image;
        
        // This stores the Image as a color array in a queue used for multi threading
        private Queue<Color> ImageColorQueue;

        public int TileSize;

        public ThreadedStorage(string[] imagePaths, Bitmap image, int tileSize)
        {
            ImagePaths = imagePaths;
            ImagePathsQueue = new Queue<string>(ImagePaths);
            Image = image;
            ImageColorQueue = new Queue<Color>(Misc.BitmapToColorList(image));
            TileSize = tileSize;
        }

    }
}