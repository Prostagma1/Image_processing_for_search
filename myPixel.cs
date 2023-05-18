namespace LABA3
{
    public class myPixel
    {
        public int x;
        public int y;
        public int cluster;
        public static int CountClusters = -1;
        public static int CountPixels = 0;
        public myPixel(int xInput, int yInput)
        {
            x = xInput;
            y = yInput;
            cluster = -1;
            CountPixels++;
        }
        public int GetCurrentClusterId(bool add = false)
        {
            if(add) CountClusters++;
            return CountClusters;
        }
    }
}
