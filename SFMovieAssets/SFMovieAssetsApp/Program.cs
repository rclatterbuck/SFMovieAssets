using System;

namespace SFMovieAssets.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new Manager.AssetPropertyRetriever();

            manager.Run(args);
        }
    }
}
