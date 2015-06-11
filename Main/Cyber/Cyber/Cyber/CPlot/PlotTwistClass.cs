﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Cyber.CLogicEngine;
using Microsoft.Xna.Framework.Input;

namespace Cyber
{
    public class PlotTwistClass
    {
        public bool loaded { get; set; }
        private List<string> dialogsList;
        public List<int> BreakPoints { get; set; }
        public int dialogNumber { get; set; }
        public List<string> BreakPointsText { get; set; }
        private string line;

        private bool getTime;
        private bool passedThroughGate;
        private bool gate1Opened;
        private bool allyChecked;
        private bool allyHacked;
        private bool levelCleared;
        private bool generatorFound;
        private bool generatorAccess;
        private bool generatorOn;

        //Czy wykonujemy zadanie, czy opowiadamy fabułę
        public bool action { get; set; }


        #region Aksesory do elementów fabuły
        public bool GetTime1
        {
            get { return getTime; }
            set { getTime = value; }
        }

        public bool PassedThroughGate
        {
            get { return passedThroughGate; }
            set { passedThroughGate = value; }
        }

        public bool AllyChecked
        {
            get { return allyChecked; }
            set { allyChecked = value; }
        }
        public bool Gate1Opened
        {
            get { return gate1Opened; }
            set { gate1Opened = value; }
        }
        public bool LevelCleared
        {
            get { return levelCleared; }
            set { levelCleared = value; }
        }
        public bool GeneratorAccess
        {
            get { return generatorAccess; }
            set { generatorAccess = value; }
        }
        public bool GeneratorOn
        {
            get { return generatorOn; }
            set { generatorOn = value; }
        }
        public bool AllyHacked
        {
            get { return allyHacked; }
            set { allyHacked = value; }
        }

        public bool GeneratorFound
        {
            get { return generatorFound; }
            set { generatorFound = value; }
        }
        #endregion

        public PlotTwistClass()
        {
            loaded = false;
            passedThroughGate = false;
            gate1Opened = false;
            allyChecked = false;
            allyHacked = false;
            generatorFound = false;
            generatorAccess = false;
            generatorOn = false;
            dialogNumber = 1;
        }

        public void PrintStatus()
        {
            Debug.WriteLine("Status loaded: " + loaded + "\n"+
                "Status Gate: " + gate1Opened + "\n"+
                "Status Ally: " + allyHacked + "\n" + 
                "Status generatorAccess");    
        }

        public void Initialize()
        {
            dialogsList = new List<string>();
            StreamReader file = new StreamReader("...//..//..//..//Cyber//CPlot//mainDialogsTranslated.txt");
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace(System.Environment.NewLine, "");
                dialogsList.Add(line);
            }
            BreakPoints = new List<int>();
            BreakPoints.Add(7);
            BreakPoints.Add(12);

            BreakPoints.Add(14);
            BreakPoints.Add(20);
            BreakPoints.Add(21);
            BreakPoints.Add(22);
            BreakPoints.Add(23);
            BreakPoints.Add(24);

            BreakPointsText = new List<string>();
            //Dla linijki 7
            BreakPointsText.Add("Use 'GetTime' to know up how much time you have to finish operation.");
            //Dla linijki 13
            BreakPointsText.Add("Samantha: Well... It's hard to don't know where is teleporter here...");

            //Druga plansza
            //Dla linijki 15
            BreakPointsText.Add("Use 'OpenGate ID' where ID is id of gate to open them.");

            //Dla linijki 21
            BreakPointsText.Add("Samantha: I should do something quickly. Let's see those robots.");
            //Dla linijki 22
            BreakPointsText.Add("Use 'AllySleep ID' to screw the corpo-robot up. ID is his unique identifier.");
            //Dla linijki 23
            BreakPointsText.Add("Find the generator here.");
            //Dla linijki 24
            BreakPointsText.Add("Use 'AccessGenerator' to get the generator ID.");
            //Dla linijki 25
            BreakPointsText.Add("Use 'Free ID to release oxygen.");


            file.Close();
            loaded = true;
        }

        public void GetTime()
        {
            BreakPoints.RemoveAt(0);
            BreakPointsText.RemoveAt(0);
            action = false;
            getTime = true;
        }

        public void ThroughGate()
        {
            if (!passedThroughGate) { 
                BreakPoints.RemoveAt(0);
                BreakPointsText.RemoveAt(0);
                dialogNumber++;
                action = false;
                passedThroughGate = true;
            }
        }

        public void OpenGate1()
        {
            BreakPoints.RemoveAt(0);
            BreakPointsText.RemoveAt(0);
            action = false;
            gate1Opened = true;
        }

        public void CheckAlly()
        {
            if (!AllyChecked) {
                BreakPoints.RemoveAt(0);
                BreakPointsText.RemoveAt(0);
                action = true;
                AllyChecked = true;
            }
        }

        public void FoundGenerator()
        {
            if (!generatorFound)
            {
                BreakPoints.RemoveAt(0);
                BreakPointsText.RemoveAt(0);
                action = true;
                generatorFound = true;
            }
        }

        public void HackAlly()
        {
            if (!allyHacked)
            {
                dialogNumber--;
                BreakPoints.RemoveAt(0);
                BreakPointsText.RemoveAt(0);
                action = true;
                allyHacked = true;
            }
        }

        public void AccessGenerator()
        {
            if (!generatorAccess) {
                BreakPoints.RemoveAt(0);
                BreakPointsText.RemoveAt(0);
                action = true;
                generatorAccess = true;
                dialogNumber--;
            }
        }

        public void RunGenerator()
        {
            BreakPoints.RemoveAt(0);
            BreakPointsText.RemoveAt(0);
            action = true;
            generatorOn = true;
        }
        

        public string getActualDialog()
        {
            if (BreakPoints.Contains(dialogNumber))
            {
                if (!GetTime1           ||
                    !passedThroughGate  ||
                    !gate1Opened        ||
                    !allyChecked        ||
                    !allyHacked         ||
                    !GeneratorFound     ||
                    !generatorFound     ||
                    !generatorAccess    ||
                    !generatorOn        
                    )
                    return BreakPointsText[0];
            }
            if (dialogNumber < dialogsList.Count)
            {
                dialogNumber++;
                if (allyChecked)
                    dialogNumber++;
                if(dialogNumber < 25)
                return dialogsList[dialogNumber];
            }
            return "";
        }
    }
}
