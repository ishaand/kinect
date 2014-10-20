using System;

namespace TextureDemo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TextureDemoGame game = new TextureDemoGame())
            {
                game.Run();
            }
        }
    }
#endif
}

