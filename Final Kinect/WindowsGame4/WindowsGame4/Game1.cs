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
using System.Threading;
using System.IO;

//Checking
namespace hatgame
{
   
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Video clouds;
        int endVar = 0;

        VideoPlayer videoPlayer = new VideoPlayer();
        float sloperight = 0, slopeleft = 0,slopeRightWrist=0,slopeLeftWrist=0;
        byte[] pixelsFromFrame = new byte[1228800];
        short[] pixelsFromFrameD = new short[307200];
        string connectedStatus = "Not connected";
        GraphicsDeviceManager graphics;
        KinectSensor kinectSensor;
        SpriteBatch spriteBatch;
        Texture2D kinectRGBVideo,stage;//stores frames from camera
        Texture2D hand, troll, hat;
        Model[] model = new Model[10];    //For 3d model,currently unused in this program
        SpriteFont font;
        Vector2 rightHandPosition = new Vector2(), headPosition = new Vector2(),
                leftHandPosition = new Vector2(), rightWristPosition = new Vector2(), leftWristPosition = new Vector2(),
                rightElbowPosition = new Vector2(),leftElbowPosition=new Vector2();
        Vector2 leftShoulderP = new Vector2();
         Vector2 rightShoulderP = new Vector2();
         Vector2 centerShoulderP = new Vector2();
         Vector2 hipCenterP = new Vector2();
         Vector2 rightKneeP = new Vector2();
         Vector2 leftKneeP = new Vector2();            
        //Currenly tracking these 5 joints only
        int RightHandDepth, LeftHandDepth, headDepth;//Contain Depths of right and left hands in Centimetres

        Boolean[] closeHands = { false, false };//For the clapping
        Boolean[] closeLeftHandHead = { false, false };
        Boolean[] closeRightHandHead = { false, false };
        Boolean head = false;
        Boolean background = false;
        int count = 0, placedJointNum;  //For Counting,num=0 for head,1 for left,2 for right 
        float x = 0, y = 0;
        Boolean spin; float handDepthPrev=0f, handDepthCur=0f;
        float theta = 0.0f;
        float thetaEarth = 0.0f, thetaMoon = 0.0f,thetaMercury=0.0f,thetaVenus=0.0f;
        int showIndex = -2;
        
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 50.0f), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix view2 = Matrix.CreateLookAt(new Vector3(0, 0, 100.0f), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(57), 640f / 480f, 0.1f, 500f);
        private Matrix projection2 = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(57), 640f / 480f, 0.1f, 1000f);
        private Matrix[] world = new Matrix[10];
        private float[] depth = new float[10];
        private int[] index = new int[10];
        int i = 0;

        public Texture2D[] CloudTexture=new Texture2D[6];
        public Vector2[] CloudPosition=new Vector2[6];
        public Vector2[] CloudSpeed=new Vector2[3];
        public SoundEffect CloudPopSound;
        public Boolean[] burst = { false, false, false, false, false, false };
        static Random rand = new Random();

        bool isNear(Joint first, Joint second)
        {
            //The following if statement could also be
            //if (Math.Abs(first.Position.X - second.Position.X) <= 0.12 && Math.Abs(first.Position.Y - second.Position.Y) <= 0.12 && Math.Abs(first.Position.Z - second.Position.Z) <= 0.1&&Math.Abs(first.Position.Y - second.Position.Y) >= 0.06 )
            //their should be difference in y coordinates for isNear to return true in this case
            if (Math.Abs(first.Position.X - second.Position.X) <= 0.12 && Math.Abs(first.Position.Y - second.Position.Y) <= 0.12 && Math.Abs(first.Position.Z - second.Position.Z) <= 0.1)
                return true;
            else
                return false;
        }
        bool isNearH(Joint first, Joint second)
        {
            //The following if statement could also be
            //if (Math.Abs(first.Position.X - second.Position.X) <= 0.12 && Math.Abs(first.Position.Y - second.Position.Y) <= 0.12 && Math.Abs(first.Position.Z - second.Position.Z) <= 0.1&&Math.Abs(first.Position.Y - second.Position.Y) >= 0.06 )
            //their should be difference in y coordinates for isNear to return true in this case
            if (Math.Abs(first.Position.X - second.Position.X) <= 0.15 && Math.Abs(first.Position.Y - second.Position.Y) <= 0.2)//&& Math.Abs(first.Position.Z - second.Position.Z) <= 0.4)
                return true;
            else
                return false;
        }

        //-------------------------------------------Not Edited Functions Start---------------------------------------------------------//
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;

        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this.kinectSensor == e.Sensor)
            {
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this.kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }

        private void DiscoverKinectSensor()
        {
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Found one, set our sensor to this
                    kinectSensor = sensor;
                    break;
                }
            }

            if (this.kinectSensor == null)
            {
                connectedStatus = "Found none Kinect Sensors connected to USB";
                return;
            }

            // You can use the kinectSensor.Status to check for status
            // and give the user some kind of feedback
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        connectedStatus = "Status: Error";
                        break;
                    }
            }

            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        protected override void Initialize()
        {
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();

            base.Initialize();
        }

        protected override void UnloadContent()
        {
            kinectSensor.Stop();
            kinectSensor.Dispose();
        }

        //-------------------------------------------Not Edited Functions END---------------------------------------------------------//

        private bool InitializeKinect()
        {
            for (int i = 0; i < 3; i++)
            {
                CloudSpeed[i] = new Vector2((i + 1), 0f);
                CloudPosition[i] = new Vector2(10, 100 * i);
            }
            for (int i = 3; i < 6; i++)
            {
                CloudPosition[i] = new Vector2(500, 100 * i + 10);
            }
            for (int i = 0; i < 10; i++)
            {
                world[i] = Matrix.CreateTranslation(0, 0, 0);
            }
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kinectSensor.AllFramesReady += this.SensorAllFramesReady;
            // kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinectSensor_DepthFrameReady);
            // Color stream
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            //kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
            
            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()//EDIT SMOOTHING PARAMETERS OF SKELETON STREAM
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.03f
            });
            //kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            try
            {
                kinectSensor.Start();
            }
            catch
            {
                connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }
            return true;
        }
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {

                if (depthImageFrame != null)
                {
                    depthImageFrame.CopyPixelDataTo(pixelsFromFrameD);
                    //byte[] convertedPixels = ConvertDepthFrame(pixelsFromFrameD, ((KinectSensor)sender).DepthStream, 640 * 480 * 4);

                    //Color[] color = new Color[depthImageFrame.Height * depthImageFrame.Width];
                    //kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, depthImageFrame.Width, depthImageFrame.Height);

                    // Set convertedPixels from the DepthImageFrame to a the datasource for our Texture2D
                    //kinectRGBVideo.SetData<byte>(convertedPixels);
                }
            }

            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {


                    colorImageFrame.CopyPixelDataTo(pixelsFromFrame);

                    Color[] color = new Color[colorImageFrame.Height * colorImageFrame.Width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, colorImageFrame.Width, colorImageFrame.Height);

                    if (background==true||showIndex==3)
                    {
                        int index = 0,headX=(int)headPosition.X,headY=(int)(headPosition.Y);
                        for (int y = 0; y < colorImageFrame.Height; y++)
                        {
                            for (int x = 0; x < colorImageFrame.Width; x++, index += 4)
                            {

                                int player;
                                if (y > 40 &&  x> 40)
                                    player = pixelsFromFrameD[(y - 30) * colorImageFrame.Width + x -16] & DepthImageFrame.PlayerIndexBitmask;
                                else
                                    player = pixelsFromFrameD[y * colorImageFrame.Width + x] & DepthImageFrame.PlayerIndexBitmask;

                                if (player > 0)
                                    color[y * colorImageFrame.Width + x] = new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);
                                else
                                    color[y * colorImageFrame.Width + x] = Color.Transparent;
                            }
                        }
                    }
                    else
                    {
                        int index = 0;
                        for (int y = 0; y < colorImageFrame.Height; y++)
                        {
                            for (int x = 0; x < colorImageFrame.Width; x++, index += 4)
                            {
                                    color[y * colorImageFrame.Width + x] = new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);
                    
                            }
                        }
                    }
                    kinectRGBVideo.SetData(color);

                }
            } 
           
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {

                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                  
                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    Skeleton playerSkeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (playerSkeleton != null)
                    {

                        Joint rightHand = playerSkeleton.Joints[JointType.HandRight];
                        Joint head = playerSkeleton.Joints[JointType.Head];
                        Joint leftHand = playerSkeleton.Joints[JointType.HandLeft];
                        Joint rightElbow = playerSkeleton.Joints[JointType.ElbowRight];
                        Joint leftElbow = playerSkeleton.Joints[JointType.ElbowLeft];
                        Joint leftWrist = playerSkeleton.Joints[JointType.WristLeft];
                        Joint rightWrist = playerSkeleton.Joints[JointType.WristRight];
                        Joint leftShoulder = playerSkeleton.Joints[JointType.ShoulderLeft];
                        Joint rightShoulder = playerSkeleton.Joints[JointType.ShoulderRight];
                        Joint centerShoulder = playerSkeleton.Joints[JointType.ShoulderCenter];
                        Joint hipCenter = playerSkeleton.Joints[JointType.HipCenter];
                        Joint rightKnee = playerSkeleton.Joints[JointType.KneeRight];
                        Joint leftKnee = playerSkeleton.Joints[JointType.KneeLeft];
                        

                        sloperight = (rightHand.Position.Y - rightElbow.Position.Y) / (rightHand.Position.X - rightElbow.Position.X);
                        slopeleft = (leftHand.Position.Y - leftElbow.Position.Y) / (leftHand.Position.X - leftElbow.Position.X);
                        slopeLeftWrist = (leftHand.Position.Y - leftWrist.Position.Y) / (leftHand.Position.X - leftWrist.Position.X);
                        slopeRightWrist = (rightHand.Position.Y - rightWrist.Position.Y) / (rightHand.Position.X - rightWrist.Position.X);

                        rightHandPosition = new Vector2((((0.5f * rightHand.Position.X) + 0.5f) * (640)), (((-0.5f * rightHand.Position.Y) + 0.5f) * (480)));
                        leftHandPosition = new Vector2((((0.5f * leftHand.Position.X) + 0.5f) * (640)), (((-0.5f * leftHand.Position.Y) + 0.5f) * (480)));
                        headPosition = new Vector2((((0.5f * head.Position.X) + 0.5f) * (640)), (((-0.5f * head.Position.Y) + 0.5f) * (480)));
                        rightElbowPosition = new Vector2((((0.5f * rightElbow.Position.X) + 0.5f) * (640)), (((-0.5f * rightElbow.Position.Y) + 0.5f) * (480)));
                        leftElbowPosition = new Vector2((((0.5f * leftElbow.Position.X) + 0.5f) * (640)), (((-0.5f * leftElbow.Position.Y) + 0.5f) * (480)));
                        leftWristPosition = new Vector2((((0.5f * leftWrist.Position.X) + 0.5f) * (640)), (((-0.5f * leftWrist.Position.Y) + 0.5f) * (480)));
                        rightWristPosition=new Vector2((((0.5f * rightWrist.Position.X) + 0.5f) * (640)), (((-0.5f * rightWrist.Position.Y) + 0.5f) * (480)));
                        leftShoulderP = new Vector2((((0.5f * leftShoulder.Position.X) + 0.5f) * (640)), (((-0.5f * leftShoulder.Position.Y) + 0.5f) * (480)));
                        rightShoulderP = new Vector2((((0.5f * rightShoulder.Position.X) + 0.5f) * (640)), (((-0.5f * rightShoulder.Position.Y) + 0.5f) * (480)));
                        centerShoulderP=new Vector2((((0.5f *centerShoulder.Position.X) + 0.5f) * (640)), (((-0.5f * centerShoulder.Position.Y) + 0.5f) * (480)));
                        //hipCenterP
                        //Similiar code for others

                        RightHandDepth = (int)(100 * rightHand.Position.Z);//Gives depth in cm
                        LeftHandDepth = (int)(100 * leftHand.Position.Z);
                        headDepth = (int)(100 * head.Position.Z);


                        switch (showIndex)
                        {
                            case -2:
                                if (isNear(leftHand, rightWrist))
                                {
                                    burst[0] = true;
                                }
                                if (isNear(rightHand, leftWrist))
                                {
                                    burst[1] = true;
                                }
                                if (isNear(leftHand, rightShoulder))
                                {
                                    burst[2] = true;
                                }
                                if (isNear(rightHand, leftShoulder))
                                {
                                    burst[3] = true;
                                }
                                if (isNear(rightHand, head))
                                {
                                    burst[4] = true;
                                }
                                if (burst[0] == true && burst[0] == true && burst[0] == true && burst[0] == true && burst[0] == true)
                                {
                                    count = 1;
                                    
                                }
                                if (isNear(rightHand, leftHand)&&count==1)
                                {
                                    showIndex=-1;
                                    for (int i = 0; i < 5; i++)
                                        burst[i] = false;
                                    count = 0;
                                }
                                break;
                            case -1:
                                if (isNear(rightHand, leftHand))
                                {
                                    background = true;

                                    
                                }
                                if (background == true)
                                {
                                    if (isNearH(rightHand, head))
                                    {
                                      

                                        showIndex++;
                                    } 
 
                                }
                                break;
                            case 0: background = true;
                                if (isNear(rightHand, leftHand))
                                {
                                    closeHands[1] = closeHands[0];
                                    closeHands[0] = true;
                                    if (rightHand.Position.Y > leftHand.Position.Y)
                                        placedJointNum = 1;
                                    else
                                        placedJointNum = 2;
                                }

                                else
                                {
                                    closeHands[1] = closeHands[0];
                                    closeHands[0] = false;
                                }
                               if (closeHands[1] == true && closeHands[0] == false)//Here Narendra argues,false and true should be interchanged
                                {
                                    count++;
                                  
                                }
                                if (count == 3)
                                { showIndex = 1; count = 0; }
                                break;
                            case 1:
                                if (isNear(rightHand, leftHand))
                                {
                                    closeHands[1] = closeHands[0];
                                    closeHands[0] = true;
                                }
                                else
                                {
                                    closeHands[1] = closeHands[0];
                                    closeHands[0] = false;
                                }


                                if (isNearH(rightHand, head))
                                {
                                    closeRightHandHead[1] = closeRightHandHead[0];
                                    closeRightHandHead[0] = true;

                                }
                                else
                                {
                                    closeRightHandHead[1] = closeRightHandHead[0];
                                    closeRightHandHead[0] = false;
                                }

                                if (isNearH(leftHand, head))
                                {
                                    closeLeftHandHead[1] = closeLeftHandHead[0];
                                    closeLeftHandHead[0] = true;

                                }
                                else
                                {
                                    closeLeftHandHead[1] = closeLeftHandHead[0];
                                    closeLeftHandHead[0] = false;
                                }

                                if (closeHands[1] == true && closeHands[0] == false && (placedJointNum == 2 || placedJointNum == 1))
                                {
                                    spin = true;
                                    if (placedJointNum == 1)
                                        placedJointNum = 2;
                                    else
                                        placedJointNum = 1;
                                }

                                if (closeRightHandHead[1] == true && closeRightHandHead[0] == false && (placedJointNum == 2 || placedJointNum == 0))
                                {
                                    spin = true;
                                    if (placedJointNum == 0)
                                    {
                                        count++;
                                        placedJointNum = 2;
                                    }
                                    else
                                        placedJointNum = 0;
                                }
                                if (closeLeftHandHead[1] == true && closeLeftHandHead[0] == false && (placedJointNum == 1 || placedJointNum == 0))
                                {
                                    spin = true;
                                    if (placedJointNum == 0)
                                    {
                                        count++;
                                        placedJointNum = 1;
                                    }
                                    else
                                        placedJointNum = 0;
                                }
                                if (count == 1)
                                {
                                    count = 0;
                                    showIndex++;
                                }
                                break;
                            case 2:
                                if(count<4)
                                {
                                    if (isNearH(rightHand, leftHand))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                    }
                                    else
                                    {
                                         closeHands[1] = closeHands[0];
                                         closeHands[0] = false;
                                    }
                                     if (closeHands[1] == true && closeHands[0] == false)
                                    {
                                        count++;
                                    
                                    }
                                }
                                else
                                {
                                    if (placedJointNum == 1)
                                    { 
                                    if(isNearH(leftHand,head))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;

                                     }
                                    else
                                    {
                                          closeHands[1] = closeHands[0];
                                         closeHands[0] = false;
                                    }
                                    }
                                    else
                                         { 
                                    if(isNearH(rightHand,head))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;

                                     }
                                    else
                                    {
                                          closeHands[1] = closeHands[0];
                                         closeHands[0] = false;
                                    }
                                    }
                                    if(closeHands[1] == true && closeHands[0] == false)
                                    {
                                        count=0;
                                        placedJointNum=0;
                                        showIndex = 3;
                                        
                                        videoPlayer.Play(clouds);
                                    }
                                }
                                
                                break;
                            case 3:
                                if (count == 0)
                                {
                                    if (isNearH(rightHand, leftHand))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                    }
                                    else
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = false;
                                    }
                                    if (closeHands[1] == true && closeHands[0] == false)
                                    {
                                        count++;
                                        background = false;
                                    }
                                }
                                if (count == 1)
                                {
                                    if (isNearH(rightHand, leftShoulder))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                    }
                                    else
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = false;
                                    }
                                    if (closeHands[1] == true && closeHands[0] == false)
                                    {
                                        count++;
                                        
                                    }
                                }
                                if (count == 2)
                                {
                                    int c1;
                                    for (c1 = 0; c1 < 6; c1++)
                                    {
                                        if (burst[c1] == false)
                                        {
                                            if (rightHandPosition.X < CloudPosition[c1].X || rightHandPosition.X > (CloudPosition[c1].X + CloudTexture[c1].Width) || (rightHandPosition.Y < CloudPosition[c1].Y) || (rightHandPosition.Y > (CloudPosition[c1].Y + CloudTexture[c1].Height)))
                                                burst[c1] = false;

                                            else
                                            {
                                                CloudPosition[c1].X = -CloudTexture[c1].Width;
                                                CloudPosition[c1].Y = rand.Next(GraphicsDevice.Viewport.Height - CloudTexture[c1].Height);
                                                //CloudPopSound.Play();
                                            }
                                        }


                                    }
                                    if (isNearH(leftHand,rightShoulder))
                                    {
                                        count++ ;
                                        
                                    }
                                }
                                if (count == 3)
                                {
                                    if (isNearH(rightHand, leftShoulder))
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                    }
                                    else
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = false;
                                    }
                                    if (closeHands[1] == true && closeHands[0] == false)
                                    {
                                        count=-1;
                                        videoPlayer.Stop();
                                        showIndex++;
                                        background = false;
                                    }
                                    
 
                                }
                                break;
                            case 4:
                                    if(placedJointNum==0)
                                    {
                                    if (isNearH(rightHand, head))
                                {
                                    closeRightHandHead[1] = closeRightHandHead[0];
                                    closeRightHandHead[0] = true;

                                }
                                else
                                {
                                    closeRightHandHead[1] = closeRightHandHead[0];
                                    closeRightHandHead[0] = false;
                                }

                                if (isNearH(leftHand, head))
                                {
                                    closeLeftHandHead[1] = closeLeftHandHead[0];
                                    closeLeftHandHead[0] = true;

                                }
                                else
                                {
                                    closeLeftHandHead[1] = closeLeftHandHead[0];
                                    closeLeftHandHead[0] = false;
                                }

                                if (closeRightHandHead[1] == true && closeRightHandHead[0] == false && (placedJointNum == 2 || placedJointNum == 0))
                                {
                                        placedJointNum = 2;
                                   
                                }
                                if (closeLeftHandHead[1] == true && closeLeftHandHead[0] == false && (placedJointNum == 1 || placedJointNum == 0))
                                {
                                   
                                        placedJointNum = 1;
                                  
                                }}
                                    else
                                    {
                                        if(count==-1)
                                        {
                                        if(isNear(leftHand,rightHand))
                                        {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                        }
                                        else
                                        {
                                         closeHands[1] = closeHands[0];
                                         closeHands[0] = false;
                                        }
                                            
                                if (closeHands[1] == true && closeHands[0] == false)
                                {
                                    count++;
                                    if(placedJointNum==2)
                                        placedJointNum=1;
                                    else
                                        placedJointNum=2;
                                    
                                }

                                    }
                                        else
                                        {
                                    if ((handDepthCur-handDepthPrev)<2)
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = true;
                                    }
                                    else
                                    {
                                        closeHands[1] = closeHands[0];
                                        closeHands[0] = false;
                                    }
                                
                               

                                if (closeHands[1] == true && closeHands[0] == false)
                                {
                                    count++;

                                    
                                }
                                if (rightHandPosition.Y < headPosition.Y && leftHandPosition.Y < headPosition.Y)
                                {
                                    count = 0;
                                    showIndex++;
                                }
                                        }
                                    }
                                break;
                            case 5:
                                if(background==false)
                                {
                                    if(isNear(leftHand,rightHand))
                                    background=true;
                                }
                                else
                                {
                                    if (centerShoulderP.Y > 250)
                                        count = 1;
                                }
                                break;
                        }
                    }
                }
            }
        }

       

        protected override void LoadContent()
        {
            //kinectSensor.ElevationAngle =18;

            CloudTexture[0] = Content.Load<Texture2D>("Cloud");
            CloudTexture[1] = Content.Load<Texture2D>("c2");
            CloudTexture[2] = Content.Load<Texture2D>("c3");
            CloudTexture[4] = Content.Load<Texture2D>("Cloud");
            CloudTexture[5] = Content.Load<Texture2D>("c2");
            CloudTexture[3] = Content.Load<Texture2D>("c3");
            showIndex = -2;
            placedJointNum=0;
            SoundEffect cloudPop = Content.Load<SoundEffect>("Pop");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);
            stage = Content.Load<Texture2D>("stage");
            clouds = Content.Load<Video>("Clouds2");
            hand = Content.Load<Texture2D>("thanks");
            font = Content.Load<SpriteFont>("font");
            model[0] = Content.Load<Model>("hat5");
            model[1] = Content.Load<Model>("BeachBall");
            for (int i = 8; i < 10; i++)
                model[i] = Content.Load<Model>("BeachBall");
            model[2] = Content.Load<Model>("sun1");
            model[3] = Content.Load<Model>("earth1");
            model[4] = Content.Load<Model>("moon2");
            model[5] = Content.Load<Model>("mercury1");
            model[6] = Content.Load<Model>("venus2");
            model[7] = Content.Load<Model>("wand");
            model[8] = Content.Load<Model>("rabbit");
           // hat = Content.Load<Texture2D>("hat5");
            troll = Content.Load<Texture2D>("hand");
        }
        KeyboardState state = Keyboard.GetState();
        protected override void Update(GameTime gameTime)
        {
            //if (endVar == 5)
            //    showIndex = 5;
            
             state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Q))
            {
                showIndex = 0;
                if (placedJointNum == 0)
                    placedJointNum = 2;
                count = 0;
            }
            if (state.IsKeyDown(Keys.W))
            {
                showIndex = 1;
                count = 0;
            }
            if (state.IsKeyDown(Keys.E))
            {
                showIndex = 2;
                if (placedJointNum == 0)
                    placedJointNum = 2;
                count = 0;
            }
            if (state.IsKeyDown(Keys.R))
            {
                showIndex = 3;
                for (int i = 0; i < 6; i++)
                    burst[i] = false;
                count = 0;
            }
            if (state.IsKeyDown(Keys.T))
            {
                showIndex = 4;
                if (placedJointNum == 0)
                    placedJointNum = 2;
                count = 0;
            }
            if (state.IsKeyDown(Keys.A))//for turning on backgrounds
            {
                background = true;
            }
            if (state.IsKeyDown(Keys.S))//for turning off backgrounds
            {
                background = false;
            }
            
            
            
            
            if (LeftHandDepth == 0 || RightHandDepth == 0)
                LeftHandDepth = RightHandDepth = 100000;


            view = Matrix.CreateLookAt(new Vector3(0, 0, 50.0f), new Vector3(0, 0, 0), Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(57), 640f / 480f, 0.1f, 100f);

            if (showIndex == 4||showIndex==3)
            {
                if (spin)
                {
                    theta += 0.1f;
                }
                if (theta >= 6.3f)
                {
                    spin = false;
                    theta = 0.0f;
                }

                {
                    thetaEarth += 0.01f;
                    thetaMoon += 0.1f;
                    thetaMercury += 0.08f;
                    thetaVenus += 0.04f;
                    if (thetaEarth >= 6.28)
                        thetaEarth = 0;
                    if (thetaMoon >= 6.28)
                        thetaMoon = 0;
                    if (thetaMercury >= 6.28)
                        thetaMercury = 0;
                    if (thetaVenus >= 6.28)
                        thetaVenus = 0;
                }

                depth[2] = 0;
                depth[3] = (float)(35.0 * Math.Sin(thetaEarth));
                depth[4] = depth[3] + (float)(10.0 * Math.Cos(thetaMoon));
                depth[5] = (float)(10.0 * Math.Cos(thetaMercury));
                depth[6] = (float)(25.0 * Math.Sin(thetaVenus));
                int c1, c2, small1, c = 2;
                float small = depth[2];
                for (int i = 2; i < 2 + count; i++)
                    if (depth[i] < small)
                        small = depth[i];
                for (int i = 2; i < 2 + count; i++)
                    if (depth[i] > depth[c])
                        c = i;
                for (c1 = 0; c1 < 10; c1++)
                    index[c1] = 0;
                small -= 1;
                for (c1 = 2; c1 < 2 + count; c1++)
                {
                    small1 = c;

                    for (c2 = 2; c2 < 2 + count; c2++)
                    {
                        if (depth[c2] < depth[small1] && depth[c2] > small)
                        {
                            small1 = c2;
                        }
                    }
                    index[small1] = c1;
                    small = depth[small1];
                }

            }
            int n;
            for (n = 0; n < 3; n++)
            {
                CloudPosition[n] += CloudSpeed[n];

                if (CloudPosition[n].X > GraphicsDevice.Viewport.Width)
                {
                    CloudPosition[n].X = -CloudTexture[n].Width;
                    CloudPosition[n].Y = rand.Next(GraphicsDevice.Viewport.Height - CloudTexture[n].Height);
                }
            }

            for (n = 3; n < 6; n++)
            {
                CloudPosition[n] += CloudSpeed[n - 3];

                if (CloudPosition[n].X > GraphicsDevice.Viewport.Width)
                {
                    CloudPosition[n].X = -CloudTexture[n].Width;
                    CloudPosition[n].Y = rand.Next(GraphicsDevice.Viewport.Height - CloudTexture[n].Height);
                }
            }

            world[2] = Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(0, 0, 0);
            world[3] = Matrix.CreateScale(0.06f) * Matrix.CreateTranslation((float)(45.0 * Math.Cos(thetaEarth)), 0f, (float)(45.0 * Math.Sin(thetaEarth)));
            world[4] = Matrix.CreateScale(0.02f) * Matrix.CreateTranslation((float)(45.0 * Math.Cos(thetaEarth) + 10.0 * Math.Sin(thetaMoon) * Math.Cos(5.12)), (float)(10.0 * Math.Sin(thetaMoon) * Math.Sin(5.12)), (float)(45.0 * Math.Sin(thetaEarth)+ 10.0 * Math.Cos(thetaMoon))) ;
            world[5] = Matrix.CreateScale(0.02f) * Matrix.CreateTranslation((float)(18.0f * Math.Sin(thetaMercury) * Math.Cos(MathHelper.ToRadians(7.0f))),(float)(18.0f * Math.Sin(thetaMercury) * Math.Sin(MathHelper.ToRadians(7.0f))), (float)(10.0 * Math.Cos(thetaMercury)));
            world[6] = Matrix.CreateScale(0.04f) * Matrix.CreateTranslation((float)(25.0 * Math.Sin(thetaVenus)), 0f, (float)(25.0 * Math.Cos(thetaVenus)));
            
            world[0] = Matrix.CreateScale(0.003f) *( Matrix.CreateRotationY(theta) * world[0]);
            //for (c1 = 2; c1 < 7; c1++)
            //    world[c1] *= Matrix.CreateTranslation(0, -4, 0);

            switch (showIndex)
            {
                case 2:

                    if (placedJointNum == 1)
                    {
                        world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240, 0);
                        world[1] = Matrix.CreateScale(200.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 255) / 240, -10.0f);
                       // world[7] = Matrix.CreateScale(16.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240 + 2f, 0);
                       
                    }
                    if (placedJointNum == 2)
                    {
                        world[1] =Matrix.CreateScale(200.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 255) / 240, -10.0f);
                        world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240, 0);
                       // world[7] = Matrix.CreateScale(16.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240 + 2f, 0);
                       
                    }
                    if (count == 1)
                    {
                        world[1] = Matrix.CreateScale(0.05f)*world[1];
                       // world[1] = Matrix.CreateTranslation(0, -3, 1);
                        world[1] *= Matrix.CreateRotationX(1.57f);
                        world[1] *= Matrix.CreateRotationY(1.0f);
                    }
                    break;
                case 3:
                    if(count>1)
                    videoPlayer.Play(clouds);
                    world[0] = Matrix.CreateRotationZ(0.4f) * Matrix.CreateRotationX(MathHelper.ToRadians(-20.0f)) * Matrix.CreateScale(19.0f / (headDepth ^ 2)) * Matrix.CreateTranslation(28.0f * (headPosition.X - 300) / 240, -27.2f * (headPosition.Y - 250) / 240 - 2.2f, 0f);
                    break;
                
                case 4:
                    if (placedJointNum == 0)
                    {
                        world[0] = Matrix.CreateRotationZ(0.4f) * Matrix.CreateRotationX(MathHelper.ToRadians(-20.0f)) * Matrix.CreateScale(9.0f / (headDepth ^ 2)) * Matrix.CreateTranslation(28.0f * (headPosition.X - 300) / 240, -27.2f * (headPosition.Y - 250) / 240 - 2.2f, 0f);
                   
                    }
                    if (placedJointNum == 1)
                    {
                        
                           if(count==-1)
                               world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240, 0);
                           else
                               world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240, 0);
                      
                            world[7] = Matrix.CreateScale(16.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240 + 2f, 0);
                        
                        handDepthPrev = handDepthCur;
                        handDepthCur = LeftHandDepth;
                    }
                    if (placedJointNum == 2)
                    {
                        if(count==-1)
                            world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240, 0);
                        else
                            world[0] = Matrix.CreateRotationX(-0.3f) * Matrix.CreateScale(8.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240, 0);
                         
                            world[7] = Matrix.CreateScale(16.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240 + 2f, 0);
                        
                        handDepthPrev = handDepthCur;
                        handDepthCur = RightHandDepth;
                    }
                    //videoPlayer.Play(myVideoFile);
                    //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    //    this.Exit();
                    break;
                case 0:
                    {
                        if (placedJointNum == 1)
                            world[0] = Matrix.CreateScale(16.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240, 0);
                        if (placedJointNum == 2)
                            world[0] = Matrix.CreateScale(16.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240, 0);

                    }break;
                case 1:
                     {
                        if (placedJointNum == 0)
                            world[0] = Matrix.CreateRotationZ(0.4f)* Matrix.CreateRotationX(MathHelper.ToRadians(-5)) * Matrix.CreateScale(19.0f / (headDepth ^ 2)) * Matrix.CreateTranslation(28.0f * (headPosition.X - 300) / 240 , -27.2f * (headPosition.Y - 250) / 240 -2.2f, 0f);
                        if (placedJointNum == 1)
                            world[0] = Matrix.CreateScale(16.0f / (LeftHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (leftHandPosition.X - 320) / 240, -27.0f * (leftHandPosition.Y - 240) / 240, 0);
                        if (placedJointNum == 2)
                            world[0] = Matrix.CreateScale(16.0f / (RightHandDepth ^ 2)) * Matrix.CreateTranslation(27.0f * (rightHandPosition.X - 320) / 240, -27.0f * (rightHandPosition.Y - 240) / 240, 0);

                    }break;
            }

           
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            
            if (background == true)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(stage, new Rectangle(0, 0, 640, 480), Color.White);
                spriteBatch.End();
            }
            if (showIndex == 5)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
                if (count == 1)
                    spriteBatch.Draw(hand, new Rectangle(0, 0, 640, 480), Color.White);
                spriteBatch.End();
            }
            if (showIndex == -2||showIndex==-1)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
                if (showIndex == -2)
                {
                    if(burst[0]==false)
                    spriteBatch.Draw(troll, new Rectangle((int)rightWristPosition.X, (int)rightWristPosition.Y, 20, 20), Color.White);
                    if (burst[1] == false)
                    spriteBatch.Draw(troll, new Rectangle((int)leftWristPosition.X, (int)leftWristPosition.Y, 20, 20), Color.White);
                    if (burst[2] == false)
                    spriteBatch.Draw(troll, new Rectangle((int)rightShoulderP.X, (int)rightShoulderP.Y, 20, 20), Color.White);
                    if (burst[3] == false)
                    spriteBatch.Draw(troll, new Rectangle((int)leftShoulderP.X, (int)leftShoulderP.Y, 20, 20), Color.White);
                    if (burst[4] == false)
                    spriteBatch.Draw(troll, new Rectangle((int)headPosition.X, (int)headPosition.Y, 20, 20), Color.White);
               
                }
                spriteBatch.End();
            }
            
            if (showIndex == 0)
            {
                
                    if (count % 2 == 1)
                    {
                        DrawModel2(model[0], world[0], view, projection);
                    }
                    spriteBatch.Begin();
                    spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
                    spriteBatch.End();
                
                
            }
            if (showIndex == 3)
            {
                spriteBatch.Begin();
                if (count ==1||count==2||count==3)
                {
                    spriteBatch.Draw(videoPlayer.GetTexture(), new Rectangle(0, 0, 640, 480), Color.White);

                }
                if (count == 2)
                {
                    if (burst[0] == false)
                        spriteBatch.Draw(CloudTexture[0], CloudPosition[0], Color.White);
                    if (burst[1] == false )
                        spriteBatch.Draw(CloudTexture[1], CloudPosition[1], Color.White);
                    if (burst[2] == false)
                        spriteBatch.Draw(CloudTexture[2], CloudPosition[2], Color.White);
                    if (burst[3] == false )
                        spriteBatch.Draw(CloudTexture[3], CloudPosition[3], Color.White);
                    if (burst[4] == false )
                        spriteBatch.Draw(CloudTexture[4], CloudPosition[4], Color.White);
                    if (burst[5] == false )
                        spriteBatch.Draw(CloudTexture[5], CloudPosition[5], Color.White);
                    
                }
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
                spriteBatch.End();
                DrawModel2(model[0], world[0], view, projection);
            }

            if(showIndex==1||showIndex==2||showIndex==4)
            {

                spriteBatch.Begin();
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);

                //spriteBatch.Draw(hand, new Rectangle((int)rightHandPosition.X - rh / 2, (int)rightHandPosition.Y - rh / 2, rh, rh), Color.White);
                //spriteBatch.Draw(hand, new Rectangle((int)leftHandPosition.X - lh / 2, (int)leftHandPosition.Y - lh / 2, lh, lh), Color.White);
                //spriteBatch.Draw(hand, new Rectangle((int)rightElbowPosition.X - rh / 2, (int)rightElbowPosition.Y - rh / 2, rh, rh), Color.White);
                //spriteBatch.Draw(hand, new Rectangle((int)headPosition.X, (int)headPosition.Y, 15000 / RightHandDepth, 15000 / RightHandDepth), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(burst[0]), new Vector2(20, 20), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(burst[1]), new Vector2(20, 40), Color.White); 
                //spriteBatch.DrawString(font, Convert.ToString(burst[2]), new Vector2(20, 60), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(index[3]), new Vector2(20, 80), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(index[4]), new Vector2(20, 100), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(index[5]), new Vector2(20, 120), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(index[6]), new Vector2(20, 140), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(count), new Vector2(20, 160), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(LeftHandDepth), new Vector2(20, 180), Color.White);
                //spriteBatch.DrawString(font, Convert.ToString(slopeLeftWrist), new Vector2(20, 200), Color.White);


                spriteBatch.End();
                switch (showIndex)
                {
                    //case 0:
                    //    if (count % 2 == 1)
                    //    {
                    //        DrawModel2(model[0], world[0], view, projection);
                    //    }
                    //    break;
                    case 1:
                        DrawModel2(model[0], world[0], view, projection);
                        break;
                    case 2:

                        if (count == 1)
                            DrawModel2(model[8], world[1], view, projection);
                        if (count == 3)
                            DrawModel2(model[1], world[1], view, projection);
                        if (count > 4)
                            DrawModel2(model[7], world[7], view, projection);
                        DrawModel2(model[0], Matrix.CreateScale(2.0f) * world[0], view, projection);
                        break;
                    //case 3:

                    //    DrawModel2(model[0], world[0], view, projection);
                    //    break;
                    case 4:
                        if (count > 6)
                        {
                            endVar++;
                            count = 6;
                        }
                        if (count < 2)
                        {
                            DrawModel2(model[0], Matrix.CreateScale(2.0f) * world[0], view, projection);
                        }
                        int i;
                        if (count  >=0)
                        {
                            if (placedJointNum == 2)
                            {
                                if (slopeRightWrist > 0)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeRightWrist)) - 1.57f) * world[7];
                                if (slopeRightWrist < 0 && rightWristPosition.Y < rightHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeRightWrist)) - 1.57f) * world[7];
                                if (slopeRightWrist < 0 && rightWristPosition.Y > rightHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeRightWrist)) + 1.57f) * world[7];

                            }
                            else
                            {
                                if (slopeLeftWrist > 0 && leftWristPosition.Y < leftHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeLeftWrist)) + 1.57f) * world[7];
                                if (slopeLeftWrist > 0 && leftWristPosition.Y > leftHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeLeftWrist)) - 1.57f) * world[7];

                                if (slopeLeftWrist < 0 && leftWristPosition.Y < leftHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeLeftWrist)) - 1.57f) * world[7];
                                if (slopeLeftWrist < 0 && leftWristPosition.Y > leftHandPosition.Y)
                                    world[8] = Matrix.CreateTranslation(0, -12, 0) * Matrix.CreateRotationZ((float)(Math.Atan(slopeLeftWrist)) + 1.57f) * world[7];
                            }
                            DrawModel(model[7], world[8], view, projection);
                            for (i = 0; i < count; i++)
                            {
                                for (int j = 2; j < 7; j++)
                                {

                                    if (index[j] == i + 2)
                                        DrawModel(model[j], world[j], view2, projection2);
                                }
                            }
                        }

                        break;

                }
            }
            //spriteBatch.Begin();
            //spriteBatch.DrawString(font, Convert.ToString(showIndex), new Vector2(20, 60), Color.White);
            //spriteBatch.DrawString(font, Convert.ToString(centerShoulderP.Y), new Vector2(20, 80), Color.White);
            //spriteBatch.End();
            base.Draw(gameTime);
        }

      

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }

        private void DrawModel2(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * world;

                   
                        if (placedJointNum == 2)
                        {
                            if (sloperight > 0)
                                effect.World = (Matrix.CreateRotationY((float)Math.Atan(1 / sloperight) + MathHelper.ToRadians(90)) * effect.World);
                            else
                            {
                                if (rightHandPosition.Y > rightElbowPosition.Y)
                                    effect.World = (Matrix.CreateRotationY(-(float)Math.Atan(sloperight) + MathHelper.ToRadians(180)) * effect.World);
                                else
                                    effect.World = (Matrix.CreateRotationY((float)Math.Atan(1 / sloperight) + MathHelper.ToRadians(90)) * effect.World);
                            }
                        }
                        if (placedJointNum == 1)
                        {
                            if (slopeleft < 0)
                                effect.World = (Matrix.CreateRotationY((float)Math.Atan(1 / slopeleft) + MathHelper.ToRadians(-90)) * effect.World);
                            else
                            {
                                if (leftHandPosition.Y > leftElbowPosition.Y)
                                    effect.World = (Matrix.CreateRotationY(-(float)Math.Atan(slopeleft) + MathHelper.ToRadians(180)) * effect.World);
                                else
                                    effect.World = (Matrix.CreateRotationY((float)Math.Atan(1 / slopeleft) + MathHelper.ToRadians(-90)) * effect.World);
                            }
                        }
                        if (placedJointNum == 0)
                        {
                            effect.World = Matrix.CreateRotationX(MathHelper.ToRadians(30)) * effect.World;
                        }
                    
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}