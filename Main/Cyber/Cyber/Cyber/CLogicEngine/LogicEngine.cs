﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Cyber.CollisionEngine;
using Cyber.GraphicsEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyber.CGameStateEngine
{
    class LogicEngine : Game
    {
        private GameState gameState;
        private GameStateMainMenu gameStateMainMenu;
        private GameStateMainGame gameStateMainGame;
        private GameStatePauseMenu gameStatePauseMenu;
        private List<GameState> menus;

        public GameState GameState
        {
            get { return gameState; }
            set { gameState = value; }
        }

        public LogicEngine(List<GameState> menus)
        {
            this.menus = menus;
            gameState = new GameState();
            gameStateMainMenu = (GameStateMainMenu)menus[0];
            gameStateMainGame = (GameStateMainGame) menus[1];
            gameStatePauseMenu = (GameStatePauseMenu)menus[2];
        }

        #region MAIN MENU LOGIC
        public void LogicMenu()
        {
            MouseState mouse;
            mouse = Mouse.GetState();
            //Debug.WriteLine(mouse.ToString());
            MouseState oldMouseState = new MouseState();
            for (int i = 0; i < gameStateMainMenu.SpriteAnimationList.Length; i++)
            {
                if (new Rectangle(mouse.X, mouse.Y, 1, 1).Intersects(gameStateMainMenu.SpriteAnimationList[i].GetFrameRectangle()))
                {
                    if (gameStateMainMenu.SpriteAnimationList[i].Clicked)
                    {
                        gameStateMainMenu.SpriteAnimationList[i].UpdateClickFrame();
                        if (gameStateMainMenu.SpriteAnimationList[i].ClickCurrentFrameAccessor == gameStateMainMenu.SpriteAnimationList[i].ClickTextureList.Length - 1)
                        {
                            switch (i)
                            {
                                case 0:
                                    gameState.State = GameState.States.mainGame;
                                    break;
                                case 1:
                                case 2:
                                //    base.StateGame = "settings";
                                //    break;
                                //gameState.State = GameState.States.startMenu;
                                //break;                                
                                case 3:
                                    gameState.State = GameState.States.exit;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        gameStateMainMenu.SpriteAnimationList[i].UpdateAnimation();
                    }
                    if (mouse.LeftButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                    {
                        gameStateMainMenu.SpriteAnimationList[i].UpdateClickAnimation(true);
                    }
                }
                else
                {
                    gameStateMainMenu.SpriteAnimationList[i].UpdateReverse();
                    gameStateMainMenu.SpriteAnimationList[i].ResetClickFrame();
                    gameStateMainMenu.SpriteAnimationList[i].UpdateClickAnimation(false);
                }
            }
        }
        #endregion

        #region PAUSE MENU LOGIC

        public void LogicPauseMenu()
        {
            MouseState mouse;
            mouse = Mouse.GetState();
            //Debug.WriteLine(mouse.ToString());
            MouseState oldMouseState = new MouseState();
            for (int i = 0; i < gameStatePauseMenu.SpriteAnimationList.Length; i++)
            {
                if (new Rectangle(mouse.X, mouse.Y, 1, 1).Intersects(gameStatePauseMenu.SpriteAnimationList[i].GetFrameRectangle()))
                {
                    if (gameStatePauseMenu.SpriteAnimationList[i].Clicked)
                    {
                        gameStatePauseMenu.SpriteAnimationList[i].UpdateClickFrame();
                        if (gameStatePauseMenu.SpriteAnimationList[i].ClickCurrentFrameAccessor == gameStatePauseMenu.SpriteAnimationList[i].ClickTextureList.Length - 1)
                        {
                            switch (i)
                            {
                                case 0:
                                    gameState.State = GameState.States.mainGame;
                                    break;
                                case 1:
                                case 2:
                                //    base.StateGame = "settings";
                                //    break;
                                //gameState.State = GameState.States.startMenu;
                                //break;                                
                                case 3:
                                    Thread.Sleep(300);
                                    gameState.State = GameState.States.startMenu;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        gameStatePauseMenu.SpriteAnimationList[i].UpdateAnimation();
                    }
                    if (mouse.LeftButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                    {
                        gameStatePauseMenu.SpriteAnimationList[i].UpdateClickAnimation(true);
                    }
                }
                else
                {
                    gameStatePauseMenu.SpriteAnimationList[i].UpdateReverse();
                    gameStatePauseMenu.SpriteAnimationList[i].ResetClickFrame();
                    gameStatePauseMenu.SpriteAnimationList[i].UpdateClickAnimation(false);
                }
            }
        }
        #endregion
        #region GAME LOGIC
        /*
         * Tutaj powinny być wszelkie reakcje na przyciski, zakończenie gry etc
         * 
         */
        public void LogicGame()
        {
            gameStateMainGame.Keys();
            gameStateMainGame.Update();
        }

        #endregion

        public GameState.States GetState()
        {
            return gameState.State;
        }
    }
}