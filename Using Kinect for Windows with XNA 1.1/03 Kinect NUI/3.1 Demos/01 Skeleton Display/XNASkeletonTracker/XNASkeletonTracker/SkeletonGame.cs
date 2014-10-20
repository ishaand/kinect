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

namespace XNASkeletonTracker
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SkeletonGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor myKinect;

        SpriteFont messageFont;

        string errorMessage = "";

        protected bool setupKinect()
        {
            // Check to see if a Kinect is available
            if (KinectSensor.KinectSensors.Count == 0)
            {
                errorMessage = "No Kinects detected";
                return false;
            }

            // Get the first Kinect on the computer
            myKinect = KinectSensor.KinectSensors[0];

            // Start the Kinect running and select the depth camera
            try
            {
                myKinect.SkeletonStream.Enable();
                myKinect.Start();
            }
            catch
            {
                errorMessage = "Kinect initialise failed";
                return false;
            }

            // connect a handler to the event that fires when new frames are available

            myKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

            return true;
        }

        Color boneColor = Color.White;

        Texture2D lineDot;

        void drawLine(Vector2 v1, Vector2 v2, Color col)
        {
            Vector2 origin = new Vector2(0.5f, 0.0f);
            Vector2 diff = v2 - v1;
            float angle;
            Vector2 scale = new Vector2(1.0f, diff.Length() / lineDot.Height);
            angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(lineDot, v1, null, col, angle, origin, scale, SpriteEffects.None, 1.0f);
        }

        void drawBone(Joint j1, Joint j2, Color col)
        {
            ColorImagePoint j1P = myKinect.MapSkeletonPointToColor(
                j1.Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            Vector2 j1V = new Vector2(j1P.X, j1P.Y);

            ColorImagePoint j2P = myKinect.MapSkeletonPointToColor(
                j2.Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            Vector2 j2V = new Vector2(j2P.X, j2P.Y);

            drawLine(j1V, j2V, col);
        }

        void drawSkeleton(Skeleton skel, Color col)
        {
            // Spine
            drawBone(skel.Joints[JointType.Head], skel.Joints[JointType.ShoulderCenter], col);
            drawBone(skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine], col);

            // Left leg
            drawBone(skel.Joints[JointType.Spine], skel.Joints[JointType.HipCenter], col);
            drawBone(skel.Joints[JointType.HipCenter], skel.Joints[JointType.HipLeft], col);
            drawBone(skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], col);
            drawBone(skel.Joints[JointType.KneeLeft], skel.Joints[JointType.AnkleLeft], col);
            drawBone(skel.Joints[JointType.AnkleLeft], skel.Joints[JointType.FootLeft], col);

            // Right leg
            drawBone(skel.Joints[JointType.HipCenter], skel.Joints[JointType.HipRight], col);
            drawBone(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], col);
            drawBone(skel.Joints[JointType.KneeRight], skel.Joints[JointType.AnkleRight], col);
            drawBone(skel.Joints[JointType.AnkleRight], skel.Joints[JointType.FootRight], col);

            // Left arm
            drawBone(skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.ShoulderLeft], col);
            drawBone(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], col);
            drawBone(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft], col);
            drawBone(skel.Joints[JointType.WristLeft], skel.Joints[JointType.HandLeft], col);

            // Right arm
            drawBone(skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.ShoulderRight], col);
            drawBone(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], col);
            drawBone(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight], col);
            drawBone(skel.Joints[JointType.WristRight], skel.Joints[JointType.HandRight], col);
        }

        Skeleton[] skeletons = null;

        void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                }
            }
        }


        public SkeletonGame()
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
            // Create v1 new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            setupKinect();

            messageFont = Content.Load<SpriteFont>("MessageFont");
            lineDot = Content.Load<Texture2D>("whiteDot");
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
        /// <param name="gameTime">Provides v1 snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides v1 snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            if (skeletons != null)
                foreach (Skeleton s in skeletons)
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                        drawSkeleton(s, Color.White);

            if (errorMessage.Length > 0)
            {
                spriteBatch.DrawString(messageFont, errorMessage, Vector2.Zero, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
