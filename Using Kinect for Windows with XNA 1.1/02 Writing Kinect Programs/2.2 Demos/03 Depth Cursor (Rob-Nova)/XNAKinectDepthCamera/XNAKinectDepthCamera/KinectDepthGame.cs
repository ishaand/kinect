using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Microsoft.Kinect;

using System.IO;

namespace XNAKinectDepthCamera
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectDepthGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont messageFont;
        string errorMessage = "";

        Rectangle videoDisplayRectangle;
        KinectSensor myKinect;
        Texture2D kinectDepthTexture;

        Rectangle ballRectangle;
        Texture2D ballTexture;

        // Depth cursor positions
        float cursorX;
        float cursorY;

        protected bool setupKinect()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                errorMessage = "No Kinects detected";
                return false;
            }

            myKinect = KinectSensor.KinectSensors[0];

            try
            {
                myKinect.DepthStream.Enable();
            }
            catch
            {
                errorMessage = "Kinect initialise failed";
                return false;
            }

            myKinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(myKinect_DepthFrameReady);

            try
            {
                myKinect.Start();
            }
            catch
            {
                errorMessage = "Camera start failed";
                return false;
            }

            return true;
        }


        short[] depthData = null;
        byte[] depthBytes = null;

        void myKinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null) return;

                if (depthData == null)
                    depthData = new short[depthFrame.Width * depthFrame.Height];

                depthFrame.CopyPixelDataTo(depthData);

                byte closestByte = 255;

                if (depthBytes == null)
                    depthBytes = new byte[depthFrame.Width * depthFrame.Height];

                for (int i = 0; i < depthData.Length; i++)
                {
                    int depth = depthData[i] >> 3;
                    if (depth == myKinect.DepthStream.UnknownDepth ||
                        depth == myKinect.DepthStream.TooFarDepth ||
                        depth == myKinect.DepthStream.TooNearDepth)
                    {
                        // Mark as an invalid value
                        depthBytes[i] = 255;
                    }
                    else
                    {
                        byte depthByte = (byte)(depth >> 4);
                        depthBytes[i] = depthByte;

                        if (depthByte < closestByte)
                            closestByte = depthByte;
                    }
                }

                Color[] bitmap = new Color[depthFrame.Width * depthFrame.Height];

                int closestXTotal = 0;
                int closestYTotal = 0;
                int closestCount = 0;

                for (int i = 0; i < depthBytes.Length; i++)
                {
                    byte colorValue = (byte)(255 - depthBytes[i]);

                    if (depthBytes[i] == closestByte)
                    {
                        bitmap[i] = new Color(colorValue, 0, 0, 255);
                        // Calculate X and Y
                        int X = i % depthFrame.Width;
                        int Y = i / depthFrame.Width;
                        // Add these to the total
                        closestXTotal += X;
                        closestYTotal += Y;
                        closestCount++;
                    }
                    else
                        bitmap[i] = new Color(colorValue, colorValue, colorValue, 255);
                }

                kinectDepthTexture = new Texture2D(GraphicsDevice, depthFrame.Width, depthFrame.Height);
                kinectDepthTexture.SetData(bitmap);

                // Get the average values
                float closestX = (float)closestXTotal / closestCount;
                float closestY = (float)closestYTotal / closestCount;

                // work out the cursor positions as a fraction of the screen size
                cursorX = closestX / depthFrame.Width;
                cursorY = closestY / depthFrame.Height;

            }
        }


        public KinectDepthGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            messageFont = Content.Load<SpriteFont>("MessageFont");

            setupKinect();

            videoDisplayRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            ballRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width / 15, GraphicsDevice.Viewport.Width / 15);
            ballTexture = Content.Load<Texture2D>("ball");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            ballRectangle.X = (int)((cursorX * GraphicsDevice.Viewport.Width) - ballRectangle.Width / 2);
            ballRectangle.Y = (int)((cursorY * GraphicsDevice.Viewport.Height) - ballRectangle.Height / 2);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            if (kinectDepthTexture != null)
            {
                spriteBatch.Draw(kinectDepthTexture, videoDisplayRectangle, Color.White);
                spriteBatch.Draw(ballTexture, ballRectangle, Color.Blue);
            }

            if (errorMessage.Length > 0)
            {
                spriteBatch.DrawString(messageFont, errorMessage,
                                       Vector2.Zero, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
