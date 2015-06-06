﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cyber.AudioEngine;
using Cyber.CAdditionalLibs;
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
using Cyber.CItems.CDynamicItem;

namespace Cyber.CGameStateEngine
{
    public class GameStateMainGame : GameState
    {
        private int i = 0;
        private int angle = 0;
        private float value = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ContentManager theContentManager;

        public Level level { get; set; }

        private KeyboardState oldState;
        private KeyboardState newState;
        private AudioController audio;

        public AudioController Audio
        {
            get { return audio; }
            set { audio = value; }
        }

        //2D elements
        private ConsoleSprites console;

        //3D elements
        // TODO: Refactor na private
        public StaticItem samanthaGhostController;
        public DynamicItem samanthaActualPlayer;
        //private StaticItem terminal;
        private ColliderController colliderController;
        private List<StaticItem> stageElements;
        // TODO: Refactor na private
        public List<StaticItem> npcList;
        public List<GateHolder> gateList; 
        private StageParser stageParser;
        private Stage stage;

        private float przesuniecie;
        StageStructure stageStructure;

        //Barriers for clock manipulation
        Boolean addPushed = false;
        Boolean subPushed = false;

        //Spojrzenie postaci tam gdzie zwrot
        float rotateSam = 0.0f;
        Matrix samPointingAtDirection = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f)) * Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(0));
        bool changedDirection = false;
        
        //Escape items
        private ParticleEmitter escapeemitter;
        private StaticItem escapeCollider;
        public bool escaped;
        public StaticItem podjazd;
        private float podjazdStopPoint;
        private float podjazdBefore;


        public void Unload()
        {
            theContentManager.Unload();
        }

        public void LoadContent(ContentManager theContentManager, GraphicsDevice device)
        {
            this.theContentManager = theContentManager;
            #region Load 2D elements
            console = new ConsoleSprites(this, audio);
            console.LoadContent(theContentManager);

            //UWAZAC NA WYMIARY OKNA
            //(1366 - 32) / 2, 768 / 2 - 120, StaticIcon.none
            //iconOverHead = new Icon((device.Viewport.Width - 32) / 2, device.Viewport.Height / 2 - 100, StaticIcon.none);
            //iconOverHead.LoadContent(theContentManager);

            #endregion
            #region Load 3D elements
            samanthaGhostController = new StaticItem("Assets/3D/Characters/Ally_Bunker");
            samanthaGhostController.LoadItem(theContentManager);
            samanthaGhostController.Type = StaticItemType.samantha;

            samanthaActualPlayer = new DynamicItem("Assets/3D/Characters/dude", "Take 001", new Vector3(100, 100, 50));
            samanthaActualPlayer.LoadItem(theContentManager);
            samanthaActualPlayer.Type = DynamicItemType.samantha;

            #region Ładowanie całego stuffu do wychodzenia ze sceny
            escapeemitter = new ParticleEmitter();
            escapeemitter.LoadContent(device, theContentManager, "Assets/2D/blueGlow", 40, 70, 70, 100, new Vector3(-20, 270, 60), 1, 1);

            escapeCollider = new StaticItem("Assets/3D/escapeBoxFBX");
            escapeCollider.LoadItem(theContentManager);
            escapeCollider.Position = new Vector3(0, 0, 0);
            escapeCollider.FixColliderInternal(new Vector3(1, 1, 1), new Vector3(-50, 280, 40));

            podjazd = new StaticItem("Assets/3D/podjazdFBX");
            podjazd.LoadItem(theContentManager);
            podjazd.Position = new Vector3(50, 300, 0);
            podjazd.FixColliderInternal(new Vector3(2,2,2), new Vector3(50,0,0));

            #endregion

            stageElements = new List<StaticItem>();
            npcList = new List<StaticItem>();
            npcList.Clear();
            stageElements.Clear();

            stageParser = new StageParser();
            
            #region ustawianie leveli
            if (level == Level.level1) {
                stage = stageParser.ParseBitmap("../../../CStageParsing/stage4.bmp");
            }
            else if (level == Level.level2)
            {
                stage = stageParser.ParseBitmap("../../../CStageParsing/stage5.bmp");
            }
            else
            {
                stage = stageParser.ParseBitmap("../../../CStageParsing/stage2.bmp");
            }
            #endregion

            stageStructure = new StageStructure(stage, StageStructureGenerationStrategy.GENEROUS);

            foreach (StageObject stageObj in stage.Objects)
            {
                StaticItem item = new StaticItem(stageObj.StaticObjectAsset);
                item.LoadItem(theContentManager);
                if (stageObj is Terminal)
                {
                    item.Type = StaticItemType.terminal;
                }
                else if (stageObj is Column)
                {
                    item.Type = StaticItemType.wall;
                }
                else
                {
                    item.Type = StaticItemType.decoration;
                }
                stageElements.Add(item);
            }

            #endregion

            #region Gates
            gateList = new List<GateHolder>();
            foreach (var gate in stage.Gates)
            {
                GateHolder gateHolder = new GateHolder(gate);
                gateList.Add(gateHolder);
                StaticItem staticItem = new StaticItem(gateHolder.FirstItem.StaticObjectAsset);
                staticItem.LoadItem(theContentManager);
                staticItem.Type = StaticItemType.decoration;
                stageElements.Add(staticItem);
                staticItem = new StaticItem(gateHolder.SecondItem.StaticObjectAsset);
                staticItem.LoadItem(theContentManager);
                staticItem.Type = StaticItemType.decoration;
                stageElements.Add(staticItem);
            }
            #endregion
            #region NPCs
            foreach (StageNPC stageNPC in stage.NPCs)
            {
                NPC npc = new NPC(stageNPC.StaticObjectAsset);
                npc.LoadItem(theContentManager);
                npc.Type = StaticItemType.tank;
                npcList.Add(npc);
                AI.Instance.AddRobot(npc);
            }

            Debug.WriteLine("Ilość narożników to: " + stageStructure.ConcaveCorners.Count + " lub " + stageStructure.ConvexCorners.Count);
            #endregion
            #region Ładowanie ścian
            for (int i = 0; i < stageStructure.Walls.Count; i++)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Wall_Base");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.wall;
                stageElements.Add(item);
            }
            for (int i = 0; i < stageStructure.ConcaveCorners.Count; i++)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Wall_Concave");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.wall;
                stageElements.Add(item);
            }
            for (int i = 0; i < stageStructure.ConvexCorners.Count; i++)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Wall_Convex");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.wall;
                stageElements.Add(item);
            }
            #endregion
            #region Ładowanie podłóg
            foreach (Pair<int, int> point in stageStructure.Floor.Floors)
            {
                StaticItem item = new StaticItem("Assets/3D/Interior/Interior_Floor");
                item.LoadItem(theContentManager);
                item.Type = StaticItemType.none; // TODO: dodać typ floor Dobrotek: Dodane
                stageElements.Add(item);
            }
            #endregion
            Debug.WriteLine("End of Loading");

        }

        public void LookAtSam(ref Vector3 cameraTarget)
        {
            cameraTarget.X = -samanthaGhostController.Position.X;
            cameraTarget.Y = samanthaGhostController.Position.Y;
        }

        public Vector3 returnSamanthaPosition()
        {
            return samanthaGhostController.Position;
        }

        public void SetUpScene(GraphicsDevice device)
        {
            escaped = false;
            ////Setup them position on the world at the start, then recreate cage. Order is necessary!
            #region setups
            int i = 0;
            float mnoznikPrzesuniecaSciany = 19.5f;
            float mnoznikPrzesunieciaOther = mnoznikPrzesuniecaSciany;
            float terminalZ = 50.0f;
            float objectZ = 0.0f;
            float wallOffset = 9.75f;
            float cornerOffset = 5.5f;
            #endregion


            samanthaGhostController.Position = new Vector3(stage.PlayerPosition.X * mnoznikPrzesunieciaOther, 
                                            stage.PlayerPosition.Y * mnoznikPrzesunieciaOther,
                                            0.0f);
            samanthaGhostController.FixColliderInternal(new Vector3(0.75f, 0.75f, 1f), new Vector3(-15f, -15f, 10f));


            #region Objects
            for (int j = 0; j < stage.Objects.Count; i++, j++)
            {
                float z;
                Vector3 move = new Vector3();
                if (stage.Objects[j] is Terminal)
                {
                    z = terminalZ;
                    move = new Vector3(stage.Objects[j].GetBlock().X * mnoznikPrzesunieciaOther,
                                        stage.Objects[j].GetBlock().Y * mnoznikPrzesunieciaOther,
                                        z);
                    stageElements[i].Position = move;
                    stageElements[i].FixColliderExternal(new Vector3(1.5f, 1.5f, 1.5f), new Vector3(15f, 20f, 20f));
                    stageElements[i].FixColliderInternal(new Vector3(0.75f, 0.75f, 0.75f), new Vector3(10, 10, 0));
                    stageElements[i].bilboards = new List<BillboardSystem>();
                    stageElements[i].bilboards.Add(new BillboardSystem(device, theContentManager,
                        theContentManager.Load<Texture2D>("Assets/2D/buttonTab"),
                        new Vector2(60),
                        move + new Vector3(0, 0, 20)
                        ));
                    ParticleEmitter emitter = new ParticleEmitter();
                    emitter.LoadContent(device, theContentManager, "Assets/2D/yellowGlow",
                        40, 20, 20, 100, move - new Vector3(15, 10, 10), 1, 0.005f);
                    stageElements[i].particles = emitter;
                }
                else if (stage.Objects[j] is Column)
                {
                    z = terminalZ;
                    move = new Vector3(stage.Objects[j].GetBlock().X * mnoznikPrzesunieciaOther,
                        stage.Objects[j].GetBlock().Y * mnoznikPrzesunieciaOther,
                        0);
                    stageElements[j].Position = move;
                    stageElements[j].FixColliderInternal(new Vector3(0.2f, 0.2f, 1f), new Vector3(-8,-8, -50));
                }
                else
                {
                    z = objectZ;
                    move = new Vector3(stage.Objects[j].GetBlock().X * mnoznikPrzesunieciaOther,
                                            stage.Objects[j].GetBlock().Y * mnoznikPrzesunieciaOther,
                                            z);
                    stageElements[i].Position = move;
                    stageElements[i].Rotation = stage.Objects[j].Rotation;
                }
                
            }
            #endregion
            #region Gates
            for (int j = 0; j < gateList.Count; j++)
            {
                Vector3 move;
                move = new Vector3(gateList[j].FirstItem.GetBlock().X * mnoznikPrzesunieciaOther,
                                            gateList[j].FirstItem.GetBlock().Y * mnoznikPrzesunieciaOther,
                                            objectZ);
                stageElements[i].Position = move;
                stageElements[i].Rotation = stage.Gates[j].Rotation;
                i++;
                move = new Vector3(gateList[j].SecondItem.GetBlock().X * mnoznikPrzesunieciaOther,
                                            gateList[j].SecondItem.GetBlock().Y * mnoznikPrzesunieciaOther,
                                            objectZ);
                stageElements[i].Position = move;
                stageElements[i].Rotation = stage.Gates[j].Rotation;
                i++;
            }
            foreach (var gateHolder in gateList)
            {
                gateHolder.SetUpCollider(samanthaGhostController);
            }
            #endregion
            #region NPCs
            for (int j = 0; j < stage.NPCs.Count; j++)
            {
                if (npcList[j].Type == StaticItemType.tank) { 
                    Debug.WriteLine("Mamy tutaj przeciwnika typu: Tank");
                }
                Vector3 move = new Vector3(stage.NPCs[j].GetBlock().X * mnoznikPrzesunieciaOther,
                                        stage.NPCs[j].GetBlock().Y * mnoznikPrzesunieciaOther,
                                        0.0f);
                npcList[j].Position = move;
                npcList[j].Rotation = stage.NPCs[j].Rotation;
                npcList[j].FixColliderInternal(new Vector3(0.75f, 0.75f, 1f), new Vector3(-15f, -15f, 10f));
                npcList[j].FixColliderExternal(new Vector3(2,2,2), new Vector3(-15f, -15f, 10f));
                npcList[j].ID = IDGenerator.GenerateID();

                ParticleEmitter emitter = new ParticleEmitter();
                    emitter.LoadContent(device, theContentManager, "Assets/2D/redGlow",
                        30, 20, 20, 100, move - new Vector3(15, 15, 10), 1, 0.005f);
                npcList[j].particles = emitter;
            }

            #endregion
            #region WallsUp
            for (int j = 0; j < stageStructure.Walls.WallsUp.Count; i++, j++)
            {
            stageElements[i].Rotation = -90;
            Vector3 move = new Vector3(stageStructure.Walls.WallsUp[j].X * mnoznikPrzesuniecaSciany,
                                        stageStructure.Walls.WallsUp[j].Y * mnoznikPrzesuniecaSciany - wallOffset,
                                        0.0f);
            stageElements[i].Position = move;
            stageElements[i].FixColliderInternal(new Vector3(0.2f, 0.1f, 1.4f), new Vector3(-7, -5, 15f));
            }

            #endregion
            #region WallsDown
            for (int j = 0; j < stageStructure.Walls.WallsDown.Count; i++, j++)
            {
                stageElements[i].Rotation = 90;
                Vector3 move = new Vector3(stageStructure.Walls.WallsDown[j].X * mnoznikPrzesuniecaSciany,
                                            stageStructure.Walls.WallsDown[j].Y * mnoznikPrzesuniecaSciany + wallOffset, 
                                            0.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.2f, 0.1f, 1.4f), new Vector3(-7, -5f, 15f));
            }
            #endregion
            #region WallsLeft
            for (int j = 0; j < stageStructure.Walls.WallsLeft.Count; i++, j++)
            {
                stageElements[i].Rotation = 180;
                Vector3 move = new Vector3(stageStructure.Walls.WallsLeft[j].X * mnoznikPrzesuniecaSciany - wallOffset,
                                            stageStructure.Walls.WallsLeft[j].Y * mnoznikPrzesuniecaSciany, 
                                            0.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-5f, -7f, 15f));
            }
            #endregion
            #region WallsRight
            for (int j = 0; j < stageStructure.Walls.WallsRight.Count; i++, j++)
            {
                stageElements[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.Walls.WallsRight[j].X * mnoznikPrzesuniecaSciany + wallOffset,
                                            stageStructure.Walls.WallsRight[j].Y * mnoznikPrzesuniecaSciany,
                                            2.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion
            #region ConcaveCornersLowerLeft
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersLowerLeft.Count; i++, j++ )
            {
                stageElements[i].Rotation = 180;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersLowerLeft[j].X * mnoznikPrzesuniecaSciany - cornerOffset,
                                            stageStructure.ConcaveCorners.ConcaveCornersLowerLeft[j].Y * mnoznikPrzesuniecaSciany + cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
            }
            #endregion
            #region ConcaveCornersLowerRight
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersLowerRight.Count; i++, j++)
            {
                stageElements[i].Rotation = 90;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersLowerRight[j].X * mnoznikPrzesuniecaSciany + cornerOffset,
                                            stageStructure.ConcaveCorners.ConcaveCornersLowerRight[j].Y * mnoznikPrzesuniecaSciany + cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
            }
            #endregion
            #region ConcaveCornersUpperLeft
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersUpperLeft.Count; i++, j++)
            {
                stageElements[i].Rotation = 270;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersUpperLeft[j].X * mnoznikPrzesuniecaSciany - cornerOffset,
                                            stageStructure.ConcaveCorners.ConcaveCornersUpperLeft[j].Y * mnoznikPrzesuniecaSciany - cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
            }
            #endregion
            #region ConcaveCornersUpperRight
            for (int j = 0; j < stageStructure.ConcaveCorners.ConcaveCornersUpperRight.Count; i++, j++)
            {
                stageElements[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConcaveCorners.ConcaveCornersUpperRight[j].X * mnoznikPrzesuniecaSciany + cornerOffset,
                                            stageStructure.ConcaveCorners.ConcaveCornersUpperRight[j].Y * mnoznikPrzesuniecaSciany - cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
            }
            #endregion
            #region ConvexCornersLowerLeft
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersLowerLeft.Count; i++, j++)
            {
                stageElements[i].Rotation = 180;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersLowerLeft[j].X * mnoznikPrzesuniecaSciany - cornerOffset,
                                            stageStructure.ConvexCorners.ConvexCornersLowerLeft[j].Y * mnoznikPrzesuniecaSciany + cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.2f, 0.2f, 1.4f), new Vector3(-25f, 5f, 15f));
            }
            #endregion
            #region ConvexCornersLowerRight
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersLowerRight.Count; i++, j++)
            {
                stageElements[i].Rotation = 90;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersLowerRight[j].X * mnoznikPrzesuniecaSciany + cornerOffset,
                                            stageStructure.ConvexCorners.ConvexCornersLowerRight[j].Y * mnoznikPrzesuniecaSciany + cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.2f, 0.2f, 1.4f), new Vector3(-7f, 5f, 15f));
            }
            #endregion
            #region ConvexCornersUpperLeft
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersUpperLeft.Count; i++, j++)
            {
                stageElements[i].Rotation = 270;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersUpperLeft[j].X * mnoznikPrzesuniecaSciany - cornerOffset,
                                            stageStructure.ConvexCorners.ConvexCornersUpperLeft[j].Y * mnoznikPrzesuniecaSciany - cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.2f, 0.2f, 1.4f), new Vector3(-25f, -30f, 15f));
            }
            #endregion
            #region ConvexCornersUpperRight
            for (int j = 0; j < stageStructure.ConvexCorners.ConvexCornersUpperRight.Count; i++, j++)
            {
                stageElements[i].Rotation = 0;
                Vector3 move = new Vector3(stageStructure.ConvexCorners.ConvexCornersUpperRight[j].X * mnoznikPrzesuniecaSciany + cornerOffset,
                                            stageStructure.ConvexCorners.ConvexCornersUpperRight[j].Y * mnoznikPrzesuniecaSciany - cornerOffset,
                                            2.0f);
                stageElements[i].Position = move;
                stageElements[i].FixColliderInternal(new Vector3(0.1f, 0.2f, 1.4f), new Vector3(-7f, -5f, 15f));
            }
            #endregion

            #region Floor setups
            float mnoznikPrzesunieciaPodlogi = mnoznikPrzesuniecaSciany;
            for (int j = 0; j < stageStructure.Floor.Count; i++, j++)
            {
                Vector3 move = new Vector3(stageStructure.Floor.Floors[j].X * mnoznikPrzesunieciaPodlogi,
                                            stageStructure.Floor.Floors[j].Y * mnoznikPrzesunieciaPodlogi,
                                            -5.0f);
                stageElements[i].Position = move;
                //    stageSurroundingsList[i].FixColliderInternal(new Vector3(0.2f, 0.1f, 1.4f), new Vector3(-7, -5, 15f));
            }
            #endregion

            //stageSurroundingsList.Add(terminal);
            colliderController = new ColliderController(console);
            colliderController.samantha = samanthaGhostController;
            colliderController.staticItemList = stageElements;
            colliderController.npcItem = npcList;

            #region Inicjalizacja AI
            AI ai = AI.Instance;
            ai.ColliderController = colliderController;
            ai.FreeSpaceMap = StageUtils.RoomListToFreeSpaceMap(stage.Rooms);
            #endregion
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


        public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, 
            GameTime gameTime, Matrix world, Matrix view, Matrix projection,
            ref float cameraRotation
            )
        {
            #region Przed bilboardingiem
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            Matrix samanthaActualPlayerView = Matrix.CreateRotationY(MathHelper.ToRadians(rotateSam)) * samPointingAtDirection * Matrix.CreateTranslation(samanthaGhostController.Position);

            Matrix samanthaGhostView = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(angle)) *
                      Matrix.CreateTranslation(samanthaGhostController.Position);

            Matrix samanthaColliderView = Matrix.CreateTranslation(samanthaGhostController.ColliderInternal.Position);
            //samanthaGhostController.DrawItem(device, samanthaGhostView, view, projection);
            
            samanthaActualPlayer.DrawItem(gameTime, device, samanthaActualPlayerView, view, projection);

            samanthaGhostController.ColliderInternal.DrawBouding(device, samanthaColliderView, view, projection);

            foreach (var gateHolder in gateList)
            {
                if (gateHolder.Collider != null)
                {
                    gateHolder.Collider.DrawBouding(device, Matrix.CreateTranslation(gateHolder.Collider.Position), view, projection);
                }
            }


            //samanthaGhostController.ColliderInternal.DrawBouding(device, samanthaColliderView, view, projection);
            #endregion
            //Przyda się do testowania pojedynczych elementów, ale foreach coś wydaje się być wydajniejszy, dunno why O.o
            //for (int i = 0; i < 125; i++)
            //{
            //    //TUTEJ SIĘ MNOŻY MACIERZE W ZALEŻNOŚCI OD OBROTU
            //    Matrix stageElementView = Matrix.Identity *
            //                        Matrix.CreateRotationZ(MathHelper.ToRadians(stageElements[i].Rotation)) *
            //                        Matrix.CreateTranslation(stageElements[i].Position);
            //    Matrix stageElementColliderView = Matrix.CreateTranslation(stageElements[i].ColliderInternal.Position);
            //    stageElements[i].DrawItem(device, stageElementView, view, projection);
            //    //stageElements[i].ColliderExternal.DrawBouding(device, stageElementColliderView, view, projection);
            //    //stageElements[i].ColliderInternal.DrawBouding(device, stageElementColliderView, view, projection);
            //}
            #region Rysowanie elementów sceny
            foreach (StaticItem stageElement in stageElements)
            {
                Matrix stageElementView = Matrix.Identity *
                    Matrix.CreateRotationZ(MathHelper.ToRadians(stageElement.Rotation)) *
                    Matrix.CreateTranslation(stageElement.Position);
                if (stageElement.OnOffBilboard)
                {
                    stageElement.DrawItem(device, stageElementView, view, projection, cameraRotation);
                }
                else
                {
                    stageElement.DrawItem(device, stageElementView, view, projection);
                    if (stageElement.particles != null)
                    {
                        stageElement.particles.Update();
                        stageElement.particles.Draw(device, view, projection, cameraRotation, stageElement.Position);
                    }
                }
                if (stageElement is Column)
                {
                    Matrix stageElementColliderView = Matrix.CreateTranslation(stageElement.ColliderInternal.Position);
                    stageElements[i].ColliderExternal.DrawBouding(device, stageElementColliderView, view, projection);
                    stageElements[i].ColliderInternal.DrawBouding(device, stageElementColliderView, view, projection);
                }
            }
            #endregion
            #region Rysowanie NPCów
            foreach (StaticItem item in npcList)
            {
                Matrix stageElementView = Matrix.Identity *
                                          Matrix.CreateRotationZ(MathHelper.ToRadians(item.Rotation)) *
                                          Matrix.CreateTranslation(item.Position);
                item.DrawItem(device, stageElementView, view, projection);

                //Matrix stageElementColliderView = Matrix.CreateTranslation(item.ColliderInternal.Position);
                //item.ColliderExternal.DrawBouding(device, stageElementColliderView, view, projection);
                //item.ColliderInternal.DrawBouding(device, stageElementColliderView, view, projection);

                if (item.particles != null)
                {
                    item.particles.Update();
                    item.particles.Draw(device, view, projection, cameraRotation, item.Position);
                }
            }
            #endregion


            console.Draw(spriteBatch);
            escapeemitter.Draw(device, view, projection, cameraRotation, new Vector3(0, 0, 0));
            //Matrix escapeCollideModel = Matrix.CreateTranslation(escapeCollider.Position);
            //escapeCollider.DrawItem(device, escapeCollideModel, view, projection);
            //Matrix escapeColliderBox = Matrix.CreateTranslation(escapeCollider.ColliderInternal.Position);
            //escapeCollider.ColliderInternal.DrawBouding(device, escapeColliderBox, view, projection);

            Matrix podjazdModel = Matrix.CreateTranslation(podjazd.Position);
            podjazd.DrawItem(device, podjazdModel, view, projection);

            //Matrix podjazdBox = Matrix.CreateTranslation(podjazd.ColliderInternal.Position);
            //podjazd.ColliderInternal.DrawBouding(device, podjazdBox, view, projection);
        }

        public override void Update(GraphicsDevice device, GameTime gameTime, KeyboardState currentKeyboardState, MouseState currentMouseState, ref float cameraArc, ref float cameraRotation, ref float cameraDistance, ref Vector3 cameraTarget, ref float cameraZoom)
        {
            if (samanthaGhostController.ColliderInternal.AABB.Intersects(escapeCollider.ColliderInternal.AABB))
            {
                escaped = true;
            }
            escapeemitter.Update();
            console.Update();
            KeyboardState newState = currentKeyboardState;

            //Kuba edit:
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            samanthaGhostController.SkinnedModel.UpdateCamera(device, gameTime, currentKeyboardState, currentMouseState, ref cameraArc, ref cameraRotation, ref cameraDistance);
            samanthaActualPlayer.SkinnedModel.UpdateCamera(device, gameTime, currentKeyboardState, currentMouseState, ref cameraArc, ref cameraRotation, ref cameraDistance);
            samanthaActualPlayer.SkinnedModel.UpdatePlayer(gameTime);

            #region Sterowanie zegarem
            //if (newState.IsKeyDown(Keys.T))
            //{
            //    if (Clock.Instance.CanResume())
            //    {
            //        Clock.Instance.Resume();
            //        Debug.WriteLine("Starting clock...");
            //    }
            //    if (newState.IsKeyDown(Keys.Add))
            //    {
            //        addPushed = true;
            //    }
            //    if (newState.IsKeyUp(Keys.Add) && addPushed)
            //    {
            //        addPushed = false;
            //        Clock.Instance.AddSeconds(60);
            //        Debug.WriteLine("ADDED +1");
            //    }
            //    if (newState.IsKeyDown(Keys.Subtract))
            //    {
            //        subPushed = true;
            //    }
            //    if (newState.IsKeyUp(Keys.Subtract) && subPushed)
            //    {
            //        subPushed = false;
            //        Clock.Instance.AddSeconds(-60);
            //        Debug.WriteLine("ADDED -1");
            //    }
            //}
            //if (newState.IsKeyDown(Keys.Y))
            //{
            //    if (Clock.Instance.CanPause())
            //    {
            //        Clock.Instance.Pause();
            //        Debug.WriteLine("Stopped clock");
            //    }
            //}
            #endregion
            #region Sterowanie Samanthą i kamerą
            Vector3 move = new Vector3(0, 0, 0);
            colliderController.PlayAudio = audio.Play0;
            if (!console.IsUsed)
            {
                if (newState.IsKeyDown(Keys.W)) { 
                    move = new Vector3(0, 1f, 0);
                    colliderController.CheckCollision(samanthaGhostController, move);
                    podjazdCollision();
                    cameraTarget.Y = samanthaGhostController.Position.Y;
                   Debug.WriteLine("Rotate sam: " + rotateSam);
                    if (rotateSam >= -179.9f && rotateSam < 0.0f || rotateSam > 180.0f)
                    {
                        rotateSam += time * 0.2f;
                      //  Debug.WriteLine("D wins");
                        if(rotateSam >= 360.0f)
                        {
                            rotateSam = 0.0f;
                        }
                    }
                    else if (rotateSam > 0.0f && rotateSam <= 180.0f)
                    {
                        rotateSam -= time * 0.2f;
                      //  Debug.WriteLine("A wins");
                    }
                   

                }
                if (newState.IsKeyDown(Keys.S)) { 
	                move = new Vector3(0, -1f, 0);
                    colliderController.CheckCollision(samanthaGhostController, move);
                    podjazdCollision(); 
                    cameraTarget.Y = samanthaGhostController.Position.Y;
                    Debug.WriteLine("Rotate sam: " + rotateSam);
                    if(rotateSam >= -6.8f && rotateSam <= 180.0f)
                    {
                        rotateSam += time * 0.2f;
                    }
                    if(rotateSam > 180.0f && rotateSam <= 270.0f  || rotateSam <= -86.0f)
                    {
                        rotateSam -= time * 0.2f;
                        if(rotateSam <= -180.0f)
                        {
                            rotateSam = 180.0f;
                        }
                    }
                  //  changedDirection = true;
                   
                }
                if (newState.IsKeyDown(Keys.A)) { 
                    move = new Vector3(-1f, 0, 0);
                    colliderController.CheckCollision(samanthaGhostController, move);
                    podjazdCollision();
                    cameraTarget.X = -samanthaGhostController.Position.X;
                    Debug.WriteLine("Rotate sam: " + rotateSam);
                    if (rotateSam >= -179.9f && rotateSam <= 90.0f)
                    {
                        rotateSam += time * 0.2f;
      
                    }
                    if (rotateSam <= 269.9f && rotateSam > 90.0f)
                    {
                        rotateSam -= time * 0.2f;
                    }
                  
                   // changedDirection = true;
                   // samPointingAtDirection = Matrix.CreateRotationY(MathHelper.ToRadians(rotateSam)) * samPointingAtDirection; 
                }
                if (newState.IsKeyDown(Keys.D))
                {
                    
                    move = new Vector3(1f, 0, 0);
                    colliderController.CheckCollision(samanthaGhostController, move);
                    podjazdCollision();
                    cameraTarget.X = -samanthaGhostController.Position.X;
                    Debug.WriteLine("Rotate sam: " + rotateSam);  
                    if ((rotateSam <= 90.0f) && (rotateSam > -90.0f))
                    {
                        rotateSam -= time * 0.2f;
                    }
                    if (rotateSam >= 170.0f || rotateSam <= -90.0f || (rotateSam > 90.0f && rotateSam < 170.0f))
                    {
                        rotateSam += time * 0.2f;
                        if(rotateSam > 270.0f)
                        {
                            rotateSam = -90.0f;
                        }
                    }
                }
              
            }
            else
            {
                console.Action();
            }
            #endregion

            if (colliderController.CallTerminalAfterCollision(samanthaGhostController))
            {   
                cameraZoom = 2.75f;
            }
            else
            {
                cameraZoom = 1.0f;
            }
           
            if (colliderController.EnemyCollision(samanthaGhostController))
            {
                //Debug.WriteLine("Weszłam w zasięg robota!");
                Debug.WriteLine("Sam zlokalizowana w " + samanthaGhostController.Position.ToString());
                AI.Instance.AlertOthers(samanthaGhostController);
            }
            console.Update();
            oldState = newState;
            AI.Instance.MoveNPCs(null);
        }

        public void podjazdCollision()
        {
            if (podjazd.ColliderInternal.AABB.Intersects(samanthaGhostController.ColliderInternal.AABB))
            {
                Debug.WriteLine("Wlazłem na schody");
                if (podjazdBefore < podjazdStopPoint - samanthaGhostController.Position.X)
                {
                    Debug.WriteLine("Wchodzę");
                    samanthaGhostController.Position += new Vector3(0, 0, 0.5f);
                }
                else if (podjazdBefore > podjazdStopPoint - samanthaGhostController.Position.X)
                {
                    Debug.WriteLine("Schodzę");
                    samanthaGhostController.Position += new Vector3(0, 0, -0.5f);
                }
                podjazdBefore = podjazdStopPoint - samanthaGhostController.Position.X;
            }
        }
    }
}