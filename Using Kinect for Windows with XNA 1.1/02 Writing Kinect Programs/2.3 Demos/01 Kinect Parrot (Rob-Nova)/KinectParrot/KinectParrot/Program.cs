using System;

namespace KinectParrot
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KinectParrotGame game = new KinectParrotGame())
            {
                game.Run();
            }
        }
    }
#endif
}

