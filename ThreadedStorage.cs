namespace ImageMosaicGenerator
{
    public class ThreadedStorage
    {
        public string[] ImagePaths;
        public string[] ImagePaths2;
        
        public ThreadedStorage(string[] imagePaths)
        {
            ImagePaths = imagePaths;
            ImagePaths2 = new string[imagePaths.Length];
        }
    }
}