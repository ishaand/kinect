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

namespace KinectCam
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectCameraGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor myKinect;
        Texture2D kinectVideoTexture;
        Rectangle videoDisplayRectangle;
        byte[] colorData = null;

        public KinectCameraGame()
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
            videoDisplayRectangle = new Rectangle(
                0, 0,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

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

            myKinect = KinectSensor.KinectSensors[0];

            myKinect.ColorStream.Enable();

            myKinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(myKinect_ColorFrameReady);

            myKinect.Start();

        }


        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;

                if (colorData == null)
                    colorData = new byte[colorFrame.Width * colorFrame.Height * 4];

                colorFrame.CopyPixelDataTo(colorData);

                Color[] bitmap = new Color[colorFrame.Width * colorFrame.Height];

                int sourceOffset = 0;

                for (int i = 0; i < bitmap.Length; i++)
                {
                    bitmap[i] = new Color(colorData[sourceOffset + 2],
                        colorData[sourceOffset + 1], colorData[sourceOffset], 255);
                    sourceOffset += 4;
                }

                kinectVideoTexture = new Texture2D(GraphicsDevice, colorFrame.Width, colorFrame.Height);

                kinectVideoTexture.SetData(bitmap);
            }
        }

        #region Camera angle adjustment

        GamePadState oldPadState;
        GamePadState padState;

        KeyboardState oldKeyState;
        KeyboardState keyState;

        void updateCamera()
        {
            if ((oldPadState.Buttons.Y == ButtonState.Released &&
                    padState.Buttons.Y == ButtonState.Pressed) ||
                    (oldKeyState.IsKeyUp(Keys.U) && keyState.IsKeyDown(Keys.U)))
            {
                myKinect.ElevationAngle += 5;
            }

            if ((oldPadState.Buttons.A == ButtonState.Released &&
                    padState.Buttons.A == ButtonState.Pressed) ||
                    (oldKeyState.IsKeyUp(Keys.D) && keyState.IsKeyDown(Keys.D)))
            {
                myKinect.ElevationAngle -= 5;
            }
        }

        #endregion
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

            // record the keyboard and keypad states for update methods to use
            padState = GamePad.GetState(PlayerIndex.One);
            keyState = Keyboard.GetState();

            updateCamera();

            oldPadState = padState;
            oldKeyState = keyState;

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

            if (kinectVideoTexture != null)
            {
                spriteBatch.Draw(kinectVideoTexture, videoDisplayRectangle,
                                 Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
