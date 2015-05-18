﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Cyber.CItems
{
    class DisplayMessage
    {
        public string Message;
        public TimeSpan DisplayTime;
        public int CurrentIndex;
        public Vector2 Position;
        public string DrawnMessage;
        public Color DrawColor;
        
        
        public DisplayMessage(string message, TimeSpan displayTime, Vector2 position, Color color)
        {
            Message = message;
            DisplayTime = displayTime;
            CurrentIndex = 0;
            Position = position;
            DrawnMessage = string.Empty;
            DrawColor = color;
        }
    }
}
