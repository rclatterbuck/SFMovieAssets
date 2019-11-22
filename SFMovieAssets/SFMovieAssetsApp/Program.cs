namespace SFMovieAssets.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var manager = new Manager.AssetPropertyRetriever();

            manager.Run(args);
        }
    }
}
