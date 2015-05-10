﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cyber.AudioEngine;
using Cyber.CItems;
using Cyber.CItems.CStaticItem;
using Cyber.CollisionEngine;
using Cyber.GraphicsEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Cyber.CLogicEngine;
using Cyber.CStageParsing;

namespace Cyber.CGameStateEngine
{
    class GameStateMainGame : GameState
    {
        private int i = 0;
        private int angle = 0;
        private float value = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        
        private KeyboardState oldState;
        private KeyboardState newState;
        private AudioController audio;

        public AudioController Audio
        {
            get { return audio; }
            set { audio = value; }
        }

        //Load Models        
        private ColliderController colliderController;
        private List<StaticItem> wallList;
        private StageParser stageParser;
        private StaticItem WallConcave;
        private StaticItem WallConvex;

        private float przesuniecie;
        StageStructure stageStructure;

        //Barriers for clock manipulation
        Boolean addPushed = false;
        Boolean subPushed = false;

        //Matrix view = Matrix.CreateLookAt(new Vector3(500, 500, 700), new Vector3(5, 5, 5), Vector3.UnitZ);
        Matrix view = Matrix.CreateLookAt(new Vector3(200, 200, 0), new Vector3(0.1f, 0.1f, 0.1f), Vector3.UnitZ);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 10000f);


        //TESTOWANE
        private StaticItem samantha;

        public void LoadContent(ContentManager theContentManager)
        {
            wallList = new List<StaticItem>();

            samantha = new StaticItem("Assets/3D/Characters/Ally_Bunker", new Vector3(20, -100, 0));
            samantha.LoadItem(theContentManager);
            samantha.Type = StaticItemType.none;

            // DODAWANIE NAROŻNIKÓW
            // na razie bez kolizji
            WallConcave = new StaticItem("Assets/3D/Interior/Interior_Wall_Concave");
            WallConvex = new StaticItem("Assets/3D/Interior/Interior_Wall_Convex");
            WallConcave.LoadItem(theContentManager);
            WallConvex.LoadItem(theContentManager);


            stageParser = new StageParser();
            Stage stage = stageParser.ParseBitmap("../../../CStageParsing/stage2.bmp");
            stageStructure = new StageStructure(stage);
            
            Debug.WriteLine("Ilość górnych ścianek to: " + stageStructure.Walls.WallsUp.Count);

            ////Ładowanie przykładowych ścianek
            for (int i = 0; i < stageStructure.Walls.Count; i++)
            {
                wallList.Add(new StaticItem("Assets/3D/Interior/Interior_Wall_Base"));
                wallList[i].LoadItem(theContentManager);
                wallList[i].Type = StaticItemType.wall;
            }
            for (int i = 0; i < stageStructure.ConcaveCorners.Count; i++)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Wall_Concave");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.wall;
                wallList.Add(item);
            }
            for (int i = 0; i < stageStructure.ConvexCorners.Count; i++)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Wall_Convex");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.wall;
                wallList.Add(item);
            }

            Debug.WriteLine("End of Loading");
        }

        public void SetUpScene()
        {
            ////Setup them position on the world at the start, then recreate cage. Order is necessary!
            #region Walls setups
            int i = 0;
            float mnoznikPrzesuniecaSciany = 19.5f;
            float wallOffset = 9.75f;
            #endregion

            samantha.FixCollider(new Vector3(0.75f, 0.75f, 1f), new Vector3(-15f, -15f, 10f));

            #region WallsUp
            for (int j = 0; j < stageStructure.Walls.WallsUp.Count; i++, j++)
            {
            wallList[i].Rotation = -90;
            Vector3 move = new Vector3(stageStructure.Walls.WallsUp[j].X * mnoznikPrzesuniecaSciany,
                                        stageStructure.Walls.WallsUp[j].Y * mnoznikPrzesuniecaSciany - wallOffset,
                                        0.0f);
            wallList[i].Position = move;
            wallList[i].FixCollider(new Vector3(0.2f, 0.1f, 1.4f), new Vector3(-7, -5, 15f));
            }
            WallConcave.Position = new Vector3(-100, 40, 0);
            WallConvex.Position = new Vector3(-140, 80, 0);

            #endregion
            #region WallsDown
            for (int j = 0; j < stageStructure.Walls.WallsDown.Count; i++, j++)
            {
                wallList[i].Rotation = 90;
                Vector3 move = new Vector3(stageStructure.Walls.WallsDown[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.Walls.WallsDown[j].Y * mnoznikPrzesuniecaSciany + wallOffset, 
                                            0.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.2f, 0.1f, 1.4f), new Vector3(-7, -5f, 15f));
            }
            #endregion
            #region WallsLeft
            for (int j = 0; j < stageStructure.Walls.WallsLeft.Count; i++, j++)
            {
                wallList[i].Rotation = 180;
                Vector3 move = new Vector3(stageStructure.Walls.WallsLeft[j].X * mnoznikPrzesuniecaSciany - wallOffset,
                                            stageStructure.Walls.WallsLeft[j].Y * mnoznikPrzesuniecaSciany, 
                                            0.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-5f, -7f, 15f));
            }
            #endregion
            #region WallsRight
            for (int j = 0; j < stageStructure.Walls.WallsRight.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.Walls.WallsRight[j].X * mnoznikPrzesuniecaSciany + wallOffset,
                                            stageStructure.Walls.WallsRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConcaveCornersLowerLeft
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersLowerLeft.Count; i++, j++ )
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersLowerLeft[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConcaveCorners.ConcaveCornersLowerLeft[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConcaveCornersLowerRight
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersLowerRight.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersLowerRight[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConcaveCorners.ConcaveCornersLowerRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConcaveCornersUpperLeft
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersUpperLeft.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersUpperLeft[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConcaveCorners.ConcaveCornersUpperLeft[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConcaveCornersUpperRight
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersUpperRight.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersUpperRight[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConcaveCorners.ConcaveCornersUpperRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConvexCornersLowerLeft
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersLowerLeft.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersLowerLeft[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConvexCorners.ConvexCornersLowerLeft[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConvexCornersLowerRight
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersLowerRight.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersLowerRight[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConvexCorners.ConvexCornersLowerRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConvexCornersUpperLeft
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersUpperLeft.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersUpperLeft[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConvexCorners.ConvexCornersUpperLeft[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConvexCornersUpperRight
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersUpperRight.Count; i++, j++)
            {
                wallList[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersUpperRight[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.ConvexCorners.ConvexCornersUpperRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                wallList[i].Position = move;
                wallList[i].FixCollider(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            colliderController = new ColliderController(wallList);
        }


        public void SetUpClock()
        {
            //Clock clock = Clock.Instance;
            //clock.RemainingSeconds = /*4 * 60*/20;
            //clock.AddEvent(Clock.BEFOREOVER, 0, TimePassed);
            //clock.Pause();
        }

        private void TimePassed(object sender, int time)
        {
            Debug.WriteLine("TIMEOUT");
        }

        public override void Draw(GraphicsDevice device, GameTime gameTime)
        {
            view = Matrix.CreateLookAt(new Vector3(samantha.Position.X + 200,
                samantha.Position.Y + 200,
                samantha.Position.Z + 400), new Vector3(
                samantha.Position.X,
                samantha.Position.Y,
                samantha.Position.Z),
                Vector3.UnitZ);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            
            Matrix samanthaView = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) *
                       Matrix.CreateTranslation(samantha.Position);
            Matrix samanthaColliderView = Matrix.CreateTranslation(samantha.ColliderInternal.Position);
            samantha.DrawItem(device, samanthaView, view, projection);
            samantha.ColliderExternal.DrawBouding(device, samanthaColliderView, view, projection);

            for (int i = 0; i < wallList.Count; i++)
            {
                //TUTEJ SIĘ MNOŻY MACIERZE W ZALEŻNOŚCI OD OBROTU
                Matrix wallView = Matrix.Identity *
                                    Matrix.CreateRotationZ(MathHelper.ToRadians(wallList[i].Rotation)) *
                                    Matrix.CreateTranslation(wallList[i].Position);
                Matrix wallColliderView = Matrix.CreateTranslation(wallList[i].ColliderInternal.Position);
                wallList[i].DrawItem(device, wallView, view, projection);
                //wallList[i].ColliderInternal.DrawBouding(device, wallColliderView, view, projection);
            }

            Matrix concaveView = Matrix.Identity *
                                    Matrix.CreateRotationZ(MathHelper.ToRadians(WallConcave.Rotation)) *
                                    Matrix.CreateTranslation(WallConcave.Position);
            Matrix convexView = Matrix.Identity *
                                    Matrix.CreateRotationZ(MathHelper.ToRadians(WallConvex.Rotation)) *
                                    Matrix.CreateTranslation(WallConvex.Position);

            WallConcave.DrawItem(device, concaveView, view, projection);
            WallConvex.DrawItem(device, convexView, view, projection);

            base.Draw(gameTime);
        }


        public override void Update()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.T))
            {
                if (Clock.Instance.CanResume())
                {
                    Clock.Instance.Resume();
                    Debug.WriteLine("Starting clock...");
                }
                if (newState.IsKeyDown(Keys.Add))
                {
                    addPushed = true;
                }
                if (newState.IsKeyUp(Keys.Add) && addPushed)
                {
                    addPushed = false;
                    Clock.Instance.AddSeconds(60);
                    Debug.WriteLine("ADDED +1");
                }
                if (newState.IsKeyDown(Keys.Subtract))
                {
                    subPushed = true;
                }
                if (newState.IsKeyUp(Keys.Subtract) && subPushed)
                {
                    subPushed = false;
                    Clock.Instance.AddSeconds(-60);
                    Debug.WriteLine("ADDED -1");
                }
            }
            if (newState.IsKeyDown(Keys.Y))
            {
                if (Clock.Instance.CanPause())
                {
                    Clock.Instance.Pause();
                    Debug.WriteLine("Stopped clock");
                }
            }

            Vector3 move = new Vector3(0, 0, 0);
            colliderController.PlayAudio = audio.Play0;
            if (newState.IsKeyDown(Keys.W))
            {
                move = new Vector3(0, -1f, 0);
                colliderController.CheckCollision(samantha, move);
            }
            if (newState.IsKeyDown(Keys.S)) { 
	            move = new Vector3(0, 1f, 0);
                colliderController.CheckCollision(samantha, move);
            }
            if (newState.IsKeyDown(Keys.A)) { 
                move = new Vector3(1f, 0, 0);
                colliderController.CheckCollision(samantha, move);
            }
            if (newState.IsKeyDown(Keys.D)) { 
	            move = new Vector3(-1f, 0, 0);
                colliderController.CheckCollision(samantha, move);
            }
            oldState = newState;
        }
    }
}
