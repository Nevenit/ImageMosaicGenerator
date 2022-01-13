namespace ImageMosaicGenerator
{
    public class ThreadedStorage
    {
        public string[] ImagePaths;
        public double[][] ImageColors;
        
        public ThreadedStorage(string[] imagePaths)
        {
            ImagePaths = imagePaths;
            ImageColors = new double[imagePaths.Length][];
        }
    }
}