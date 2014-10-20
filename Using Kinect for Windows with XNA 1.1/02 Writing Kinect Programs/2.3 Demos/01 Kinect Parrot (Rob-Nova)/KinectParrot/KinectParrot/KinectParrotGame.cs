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
using System.Threading;


namespace KinectParrot
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectParrotGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor myKinect;

        SpriteFont messageFont;

        string message = "Press B to record";

        GamePadState oldPadState;
        GamePadState padState;

        KeyboardState oldKeyState;
        KeyboardState keyState;

        enum ParrotState
        {
            idle,
            recording,
            playing
        };

        ParrotState state;

        const int bufferSize = 50000;

        byte[] soundSampleBuffer = new byte[bufferSize];

        Stream kinectAudioStream;

        protected bool setupKinect()
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                message = "No Kinects detected";
                return false;
            }

            myKinect = KinectSensor.KinectSensors[0];

            try
            {
                myKinect.Start();
            }
            catch
            {
                message = "Kinect start failed";
                return false;
            }

            return true;
        }

        public KinectParrotGame()
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
            // TODO: Add your initialization logic here

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

            setupKinect();

            messageFont = Content.Load<SpriteFont>("MessageFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        void startRecording()
        {
            kinectAudioStream = myKinect.AudioSource.Start();
            state = ParrotState.recording;
            message = "Recording";
        }

        void updateRecording()
        {
            kinectAudioStream.Read(soundSampleBuffer, 0, soundSampleBuffer.Length);
            myKinect.AudioSource.Stop();
            message = "Press A to playback B to record again";
            state = ParrotState.idle;
        }

        DynamicSoundEffectInstance playback = new DynamicSoundEffectInstance(16000, AudioChannels.Mono);

        void startPlayback()
        {
            playback.SubmitBuffer(soundSampleBuffer);
            playback.Play();
            message = "Playing";
            state = ParrotState.playing;
        }

        void updatePlayback()
        {
            if (playback.PendingBufferCount == 0)
            {
                message = "Press A to playback B to record again";
                state = ParrotState.idle;
            }
            else
            {
                playback.Pitch = padState.ThumbSticks.Left.X;
            }
        }

        void updateIdle()
        {
            if ((oldPadState.Buttons.A == ButtonState.Released &&
                    padState.Buttons.A == ButtonState.Pressed) ||
                    (oldKeyState.IsKeyUp(Keys.A) && keyState.IsKeyDown(Keys.A)))
            {
                startPlayback();
            }

            if ((oldPadState.Buttons.B == ButtonState.Released &&
                    padState.Buttons.B == ButtonState.Pressed) ||
                    (oldKeyState.IsKeyUp(Keys.B) && keyState.IsKeyDown(Keys.B)))
            {
                startRecording();
            }
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

            padState = GamePad.GetState(PlayerIndex.One);
            keyState = Keyboard.GetState();

            switch (state)
            {
                case ParrotState.idle:
                    updateIdle();
                    break;

                case ParrotState.recording:
                    updateRecording();
                    break;

                case ParrotState.playing:
                    updatePlayback();
                    break;
            }

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

            if (message.Length > 0)
            {
                spriteBatch.DrawString(messageFont, message, Vector2.Zero, Color.White);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
