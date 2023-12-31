﻿/*		          HTBLA Leonding / CABS Project
 *----------------------------------------------------------------
 *		                   Deniz Ciger
 *----------------------------------------------------------------
 *	Description: Recreation of Geometry Dash made for the Console.
 *	             Geometry Dash is a rythm based platforming game.	        
 *----------------------------------------------------------------
 */

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Media;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;
using Test;

#pragma warning disable CS8605 // Unboxing a possibly null value.
#pragma warning disable CA1416 // Validate platform compatibility

namespace GeometryDash
{
    public enum Speeds { Slow = 0, Normal = 1, Fast = 2, Faster = 3, Fastest = 4 };

    internal class Program
    {
        //Compatability Variables
        static int osVersion = Convert.ToInt32(Environment.OSVersion.Version.Major);
        static int osBuildNumb = Convert.ToInt32(Environment.OSVersion.Version.Build.ToString());
        static int winVersion = 0;

        //GamePlay Config
        static LevelGrid[,] level = new LevelGrid[10, 10];
        static LevelColor[] startColorChannel = new LevelColor[1011];
        static LevelColor[] colorChannel = new LevelColor[1011];
        static readonly string[] UI_PATHS = { "UI\\Logo.txt", "UI\\Pause11.txt", "UI\\Pause10.txt", "UI\\Level.txt" };
        static readonly string[] LEVEL_PATHS = { "Data\\Levels\\StereoMadness.dat", "Data\\Levels\\BackOnTrack.dat", "Data\\Levels\\Polargeist.dat" };
        static readonly string[] SONG_PATHS = { "Data\\Songs\\StereoMadness.wav", "Data\\Songs\\BackOnTrack.wav", "Data\\Songs\\Polargeist.wav" };
        static readonly string[] LEVEL_NAMES = { "Stereo Madness", "Back On Track", "Polargeist" };
        static int[] LEVEL_ATTEMPTS = { 0, 0, 0 };
        static readonly float[] speedValues = { 8.4f, 10.41667f, 12.91667f, 15.667f, 19.2f };
        static SoundPlayer gameSound = new SoundPlayer();
        //static ConsoleColor backgroundStartColor = new ConsoleColor();
        //static ConsoleColor groundStartColor1 = new ConsoleColor();
        //static ConsoleColor groundStartColor2 = new ConsoleColor();
        static ConsoleColor player_color;
        static int startGameMode = 0;
        static int startSpeed = 0;

        const int PAUSE_BUTTONS = 10; //Practice, Reset, Main Menu

        const int MILLIS_PER_TICK = 10;

        const int VIEWABLE_UP = 5;
        const int VIEWABLE_DOWN = 5;
        const int VIEWABLE_LEFT = 7;
        const int VIEWABLE_RIGHT = 11;

        const float JUMP_FORCE = 12.65795f / 2;
        const float FALLING_SPEED = 12.63245f / 2;

        //Characters for Windows 11
        const char PLAYER_CHAR_11 = '\u25A1';
        const char ORB_CHAR_11 = '\u25c9';
        const char COIN_CHAR_11 = '\u235f';
        const char JUMP_PAD_CHAR_11 = '\u2313';

        //Characters for Windows 10
        const char PLAYER_CHAR_10 = '\u25A0';
        const char ORB_CHAR_10 = 'O';
        const char COIN_CHAR_10 = '*';
        const char JUMP_PAD_CHAR_10 = '_';

        //Characters good with both Win10 & Win11
        const char BLOCK_CHAR = '\u25A0';
        const char SPIKE_CHAR = '\u25B2';
        const char PORTAL_CHAR = '\u0029';

        //Needed Gameplay variables
        static float playerX = 0;
        static float playerY = 0;
        static float playerYVelocity = 0;
        static float gravity = 0;
        static Stopwatch time = new Stopwatch();
        static float deltaTime = 1000f;
        static bool died = false;
        static bool isJumping = false;
        static long attempts = 0;

        static int currentGamemode = 1; // 0 cube 1 ship 2 ball 3 ufo 4 wave 5 robot 6 spider
        static readonly int[] blockList = { 1, 2, 3, 4/*, 5*/, 6, 7, 40 }; //commenteds have no hitboxes
        static readonly int[] spikeList = { 8, 9, 39 };
        static readonly int[] portalList = { 10, 11, 12, 13 }; //Gravity Down, Gravity Up, Cube,Ship
        static readonly int[] objectList = { 16, 17, 18, 19, 20, 21, 41 };
        static readonly int[] triggerList = { 29, 30, 104, 105, 221 /*...*/ };
        static int levelNumb = 0;
        static ConsoleColorConverter rgbToColor = new ConsoleColorConverter();

        //Hacks
        static bool noClip = false;

        //Debug Configs
        static bool showDebugPercentage = false;
        static bool debugObjID = false;
        static bool debugXPos = false;
        static bool debugObjPlace = false;
        static bool debugResize = false;
        static bool debugHorizontalFlip = false;
        static bool debugVerticalFlip = false;
        static bool debugRotation = false;
        static bool debugTriggers = false;
        static bool debugNotImplemented = false;


        static void Main(string[] args)
        {

            if (OperatingSystem.IsWindows() && osVersion >= 10)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.SetWindowSize(Console.LargestWindowWidth / 2, Console.LargestWindowHeight / 2);
                Console.Title = "UniDash";
                player_color = GetRandomConsoleColor();

                if (osBuildNumb >= 22000)
                {
                    winVersion = 11;
                }
                else
                {
                    winVersion = 10;
                }

                if (File.Exists("attempts.csv"))
                {
                    string[] attempts = File.ReadAllLines("attempts.csv")[0].Split(';');

                    for (int i = 0; i < attempts.Length; i++)
                    {
                        LEVEL_ATTEMPTS[i] = int.Parse(attempts[i]);
                    }
                }

                LevelSelect();

                while (true)
                {
                    if (!died)
                    {
                        deltaTime = time.ElapsedMilliseconds;
                        time.Restart();
                    }
                    else
                    {
                        died = false;
                    }

                    UpdateGame(gameSound);
                    //Thread.Sleep(MILLIS_PER_TICK);
                }
            }
            else
            {
                Console.WriteLine("This project is only made for Windows version 10 or higher!");
                Console.WriteLine("Press any key to end the program...");
                Console.ReadKey();
            }
        }


        static void SaveAttempts()
        {
            string contentToWrite = "";

            for (int i = 0; i < LEVEL_ATTEMPTS.Length; i++)
            {
                contentToWrite += LEVEL_ATTEMPTS[i];

                if (i < LEVEL_ATTEMPTS.Length - 1)
                {
                    contentToWrite += ';';
                }
            }

            File.WriteAllText("attempts.csv", contentToWrite);
        }

        private static void UpdateGame(SoundPlayer gameSound)
        {
            int roundedPlayerY = (int)Math.Round(playerY);
            int roundedPlayerX = (int)Math.Round(playerX);
            int currentTile;
            int portalTile = 0;
            Console.CursorVisible = false;

            for (int i = 0; i < colorChannel.Length; i++)
            {
                if (colorChannel[i].GetLeftFadeTime() > 0f)
                {
                    colorChannel[i].UpdateRGBFade(deltaTime);
                }
            }

            if (roundedPlayerX >= level.GetLength(1))
            {
                FinishLevel();
                roundedPlayerY = (int)Math.Round(playerY);
                roundedPlayerX = (int)Math.Round(playerX);
            }

            if (roundedPlayerY < level.GetLength(0) && roundedPlayerY >= 0 && level[roundedPlayerY, roundedPlayerX] != null)
                currentTile = level[roundedPlayerY, roundedPlayerX].GetObjectID();
            else
                currentTile = 0;

            if (roundedPlayerY >= level.GetLength(0) || blockList.Contains(currentTile) || spikeList.Contains(currentTile))
            {
                if (!noClip)
                {
                    PlayerDieEvent(gameSound);
                }
            }
            else if (IsInPortal(roundedPlayerX, roundedPlayerY, ref portalTile))
            {
                switch (portalTile)
                {
                    case 13: //Ship Portal
                        currentGamemode = 1;
                        break;
                    case 12: //Cube Portal
                        currentGamemode = 0;
                        break;
                    default:
                        break;
                }
            }
            else if (TriggersTrigger(roundedPlayerX))
            {
                string[] triggerLocations = GetTriggerLocations(roundedPlayerX);

                for (int i = 0; i < triggerLocations.Length; i++)
                {
                    string[] values = triggerLocations[i].Split(';');
                    int y = int.Parse(values[0]);
                    int x = int.Parse(values[1]);


                    int triggerID = level[y, x].GetObjectID();
                    float triggerDuration = level[y, x].GetTriggerDuration();

                    switch (triggerID)
                    {
                        case 29: //Background Color

                            byte r = level[y, x].GetColorRed();
                            byte g = level[y, x].GetColorGreen();
                            byte b = level[y, x].GetColorBlue();

                            byte[] rgb = new byte[3];

                            rgb[0] = r;
                            rgb[1] = g;
                            rgb[2] = b;

                            float fadeTime = level[y, x].GetTriggerDuration();

                            colorChannel[1000].SetRGBFade(rgb, fadeTime);

                            //if (level[y, x].GetTintGround())
                            //{
                            //    currentGroundColor1 = rgbToColor.GetClosestConsoleColor(r, g, b);
                            //}
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                switch (currentTile)
                {
                    case 35: //Jump Pad
                        playerYVelocity = 2.77f;
                        break;
                    default:
                        break;
                }
            }

            if (!died)
            {
                playerX += (float)speedValues[startSpeed] * (deltaTime / 1000f);

                if (!IsStandingOnBlock(playerY, playerX))
                {
                    if (playerYVelocity > -2.6)
                    {
                        gravity = 0.876f;
                        playerYVelocity -= gravity * deltaTime * 0.01f;
                    }
                    else
                    {
                        gravity = 0;
                        playerYVelocity = 0;
                    }
                }
                else
                {
                    playerYVelocity = 0;
                }

                //Getting Inputs
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        if (currentGamemode == 0)
                        {
                            PlayerJump();
                        }
                        else if (currentGamemode == 1)
                        {
                            playerY += 6f * (deltaTime / 500f);
                        }
                    }
                    else if (Console.ReadKey(true).Key == ConsoleKey.Backspace)
                    {
                        PauseGame(gameSound);
                    }
                }

                playerY += playerYVelocity * deltaTime * 0.01f;

                // Check if yPos is below ground level
                if (playerY <= 0)
                {
                    playerY = 0;
                    isJumping = false; // Reset jumping state when landing
                }
                PrintState();
            }
        }

        private static void PauseGame(SoundPlayer gameSound)
        {
            int currentButtonSelected = 1;
            int previousButton = 1;
            bool pressedEnter = false;
            int cursorX = (currentButtonSelected * 8) + 5;

            Console.ResetColor();
            Console.Clear();
            gameSound.Stop();
            string pauseMenu = File.ReadAllText(winVersion >= 11 ? UI_PATHS[1] : UI_PATHS[2]);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(pauseMenu);

            Console.SetCursorPosition(cursorX, 5);
            Console.Write(">>");
            Console.SetCursorPosition(cursorX + 5, 5);
            Console.Write("<<");

            while (!pressedEnter)
            {

                if (previousButton != currentButtonSelected)
                {
                    Console.SetCursorPosition(cursorX, 5);
                    Console.Write("  ");
                    Console.SetCursorPosition(cursorX + 5, 5);
                    Console.Write("  ");

                    cursorX = (currentButtonSelected * 8) + 5;

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(pauseMenu);

                    Console.SetCursorPosition(cursorX, 5);
                    Console.Write(">>");
                    Console.SetCursorPosition(cursorX + 5, 5);
                    Console.Write("<<");

                    previousButton = currentButtonSelected;
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKey pressedKey = Console.ReadKey(true).Key;

                    switch (pressedKey)
                    {
                        case ConsoleKey.Enter:
                            pressedEnter = true;
                            break;
                        case ConsoleKey.RightArrow:
                            currentButtonSelected++;

                            if (currentButtonSelected >= PAUSE_BUTTONS)
                            {
                                currentButtonSelected = 0;
                            }

                            break;
                        case ConsoleKey.LeftArrow:
                            currentButtonSelected--;

                            if (currentButtonSelected < 0)
                            {
                                currentButtonSelected = PAUSE_BUTTONS - 1;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }

            Console.Clear();

            switch (currentButtonSelected)
            {
                case 0:
                    Console.WriteLine("Not implemented yet!\n Press any key to continue to the Menu...");
                    Console.ReadKey();
                    LevelSelect();
                    break;
                case 1:
                    PlayerDieEvent(gameSound);
                    break;
                case 2:
                    LevelSelect();
                    break;
            }

        }

        private static string[] GetTriggerLocations(int roundedPlayerX)
        {
            string[] triggerLocs = new string[0];

            for (int i = 0; i < level.GetLength(0); i++)
            {
                if (roundedPlayerX < level.GetLength(1) && level[i, roundedPlayerX] != null && triggerList.Contains(level[i, roundedPlayerX].GetObjectID()))
                {
                    string[] newTriggerLocs = new string[triggerLocs.Length + 1];
                    Array.Copy(triggerLocs, newTriggerLocs, triggerLocs.Length);

                    newTriggerLocs[newTriggerLocs.Length - 1] = $"{i};{roundedPlayerX}"; //row;col(y,x)

                    triggerLocs = newTriggerLocs;
                }
            }

            return triggerLocs;
        }

        private static bool TriggersTrigger(int roundedPlayerX)
        {
            bool triggersTrigger = false;

            for (int i = 0; i < level.GetLength(0); i++)
            {
                if (roundedPlayerX < level.GetLength(1) && level[i, roundedPlayerX] != null && triggerList.Contains(level[i, roundedPlayerX].GetObjectID()))
                {
                    triggersTrigger = true;
                    break;
                }
            }

            return triggersTrigger;
        }

        private static bool IsInPortal(int roundedPlayerX, int roundedPlayerY, ref int portalTile)
        {
            bool isInPortal = false;

            for (int i = -1; i < 2; i++)
            {
                int checkPosY = i + roundedPlayerY;
                if (checkPosY < level.GetLength(0) && checkPosY >= 0 && level[checkPosY, roundedPlayerX] != null && portalList.Contains(level[checkPosY, roundedPlayerX].GetObjectID()))
                {
                    isInPortal = true;
                    portalTile = level[checkPosY, roundedPlayerX].GetObjectID();
                }
            }

            return isInPortal;
        }

        private static void FinishLevel()
        {
            LevelSelect();
        }

        private static void LevelSelect()
        {
            Console.ResetColor();
            Console.Clear();
            Console.CursorVisible = false;

            bool selectedLevel = false;
            int currentShownLevel = 0;

            while (!selectedLevel)
            {
                PrintLevelSelect(currentShownLevel);

                ConsoleKey pressedKey = Console.ReadKey(true).Key;

                if (pressedKey == ConsoleKey.Enter)
                {
                    selectedLevel = true;
                }
                else if (pressedKey == ConsoleKey.LeftArrow)
                {
                    currentShownLevel--;
                }
                else if (pressedKey == ConsoleKey.RightArrow)
                {
                    currentShownLevel++;
                }
                else if (pressedKey == ConsoleKey.NumPad6)
                {
                    noClip = true;
                }
                else if (pressedKey == ConsoleKey.NumPad9)
                {
                    noClip = false;
                }

                if (currentShownLevel >= LEVEL_NAMES.Length)
                {
                    currentShownLevel = 0;
                }
                else if (currentShownLevel < 0)
                {
                    currentShownLevel = LEVEL_NAMES.Length - 1;
                }
            }

            levelNumb = currentShownLevel;

            gameSound = new(SONG_PATHS[levelNumb]);
            Configurelevel();

            playerX = 0;
            playerY = 0;
            attempts = 1;
            LEVEL_ATTEMPTS[levelNumb]++;
            time.Stop();
            time.Reset();
            deltaTime = MILLIS_PER_TICK;
            SaveAttempts();
            gameSound.Play();
        }

        private static void PrintLevelSelect(int currentLevel)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(File.ReadAllText(UI_PATHS[0]));
            Console.SetCursorPosition(0, 10);
            Console.WriteLine(File.ReadAllText(UI_PATHS[3]));
            Console.SetCursorPosition(8, 13);
            Console.WriteLine(LEVEL_NAMES[currentLevel]);
            Console.SetCursorPosition(8, 15);
            Console.WriteLine($"{LEVEL_ATTEMPTS[currentLevel]} Attempts");
        }

        private static void PlayerDieEvent(SoundPlayer gameSound)
        {
            currentGamemode = startGameMode;
            HardCopyColors(startColorChannel, ref colorChannel);

            gameSound.Stop();
            gameSound.Play();

            time.Restart();
            playerY = 0;
            playerX = 0;
            attempts++;
            LEVEL_ATTEMPTS[levelNumb]++;
            deltaTime = MILLIS_PER_TICK;
            SaveAttempts();
            died = true;

        }

        private static void PlayerJump()
        {
            int roundedPlayerY = (int)Math.Round(playerY);
            int roundedPlayerX = (int)Math.Round(playerX);

            if (playerY < 0.05f || IsStandingOnBlock(playerY, playerX))
            {
                playerYVelocity = 1.94f;
                playerY += playerYVelocity * (deltaTime / 1000f);
            }
            else
            {
                bool foundJumpOrb = false;

                for (int i = -1; i < 2 && !foundJumpOrb; i++)
                {
                    for (int j = -1; j < 2 && !foundJumpOrb; j++)
                    {
                        if (roundedPlayerY + i >= 0 && roundedPlayerY + i < level.GetLength(0) && roundedPlayerX + j >= 0 && roundedPlayerX + j < level.GetLength(1))
                        {
                            if (level[(roundedPlayerY + i), roundedPlayerX + j] != null)
                            {
                                foundJumpOrb = true;

                                switch (level[(roundedPlayerY + i), roundedPlayerX + j].GetObjectID())
                                {
                                    case 36: // Yellow Orb
                                        playerYVelocity = 1.91f;
                                        break;
                                    default:
                                        foundJumpOrb = false;
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (playerYVelocity > 0.0f)
            {
                isJumping = true;
            }
        }

        private static bool IsStandingOnBlock(float PlayerY, float PlayerX)
        {
            bool isOnBlock = false;

            int roundedPlayerX = (int)Math.Round(playerX);
            int roundedPlayerY = (int)Math.Round(playerY);

            if (playerY > 0.05f)
            {
                if (roundedPlayerY - 1 >= 0 && roundedPlayerY + 1 < level.GetLength(0))
                {
                    for (int i = -1; i < 2 && !isOnBlock; i++)
                    {
                        if (roundedPlayerX + i >= 0 && roundedPlayerX + i < level.GetLength(1))
                        {
                            if (level[(roundedPlayerY - 1), roundedPlayerX + i] != null && blockList.Contains(level[(roundedPlayerY - 1), roundedPlayerX + i].GetObjectID()))
                            {
                                isOnBlock = true;
                            }
                        }
                    }
                }
            }
            else
            {
                isOnBlock = true;
            }
            return isOnBlock;
        }

        private static void PrintState()
        {
            int defaultCursorX = Console.WindowLeft + 1;
            int defaultCursorY = Console.WindowTop + 1;
            int cursorX = defaultCursorX;
            int cursorY = defaultCursorY;
            int roundedPlayerX = (int)Math.Round(playerX);
            int roundedPlayerY = (int)Math.Round(playerY);
            int currentTile;

            //Viewable Range: (-7,-5) to (11,5) [x, y]


            for (int yPos = VIEWABLE_UP; yPos >= 0 - VIEWABLE_DOWN; yPos--)
            {
                Console.SetCursorPosition(cursorX, cursorY);

                for (int xPos = 0 - VIEWABLE_LEFT; xPos <= VIEWABLE_RIGHT; xPos++)
                {
                    if (level.GetLength(0) - 1 - (yPos + roundedPlayerY) < level.GetLength(0) && xPos + roundedPlayerX < level.GetLength(1) && level.GetLength(0) - 1 - (yPos + roundedPlayerY) >= 0 && xPos + roundedPlayerX >= 0)
                    {

                        if (cursorY == defaultCursorY + VIEWABLE_UP && cursorX == defaultCursorX + VIEWABLE_LEFT)
                        {
                            Console.ForegroundColor = player_color;

                            if (Console.ForegroundColor == rgbToColor.GetClosestConsoleColor(colorChannel[1000].GetCurrRGB()))
                            {
                                Console.ForegroundColor = GetInverseColor(Console.ForegroundColor);
                            }
                            Console.Write((winVersion >= 11) ? PLAYER_CHAR_11 : PLAYER_CHAR_10);
                        }
                        else
                        {
                            if (level[yPos + roundedPlayerY, xPos + roundedPlayerX] != null)
                                currentTile = level[yPos + roundedPlayerY, xPos + roundedPlayerX].GetObjectID();
                            else
                                currentTile = 0;

                            Console.BackgroundColor = rgbToColor.GetClosestConsoleColor(colorChannel[1000].GetCurrRGB());

                            //Printing Objects
                            if (currentTile == 0) // Is Air
                            {
                                Console.Write(" ");
                            }
                            else if (blockList.Contains(currentTile)) // Is Block
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = Console.ForegroundColor;
                                FixSameColor();
                                Console.Write(BLOCK_CHAR);
                            }
                            else if (spikeList.Contains(currentTile)) // Is Spike
                            {
                                Console.ForegroundColor = ConsoleColor.Black;
                                FixSameColor();
                                Console.Write(SPIKE_CHAR);
                            }
                            else if (portalList.Contains(currentTile))
                            {
                                switch (currentTile)
                                {
                                    case 10:
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        break;
                                    case 11:
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        break;
                                    case 12:
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        break;
                                    case 13:
                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                        break;
                                    default:
                                        Console.ForegroundColor = ConsoleColor.White;
                                        break;
                                }

                                FixSameColor();
                                Console.Write(PORTAL_CHAR);
                            }
                            else if (currentTile == 35) // Is Jump Pad
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                FixSameColor();
                                Console.Write((winVersion >= 11) ? JUMP_PAD_CHAR_11 : JUMP_PAD_CHAR_10);
                            }
                            else if (currentTile == 36) // Is Jump Orb
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                FixSameColor();
                                Console.Write((winVersion >= 11) ? ORB_CHAR_11 : ORB_CHAR_10);
                            }
                            else if (currentTile == 1329) // Is Coin
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                FixSameColor();
                                Console.Write((winVersion >= 11) ? COIN_CHAR_11 : COIN_CHAR_10);
                            }
                            else
                            {
                                Console.Write(" ");
                            }

                        }
                        cursorX++;
                    }
                    else
                    {
                        if (xPos + roundedPlayerX >= level.GetLength(1))
                        {
                            Console.ForegroundColor = GetRandomConsoleColor();
                            Console.BackgroundColor = Console.ForegroundColor;
                            FixSameColor();
                            Console.ForegroundColor = Console.BackgroundColor;
                            Console.WriteLine(" ");
                        }
                        else if (level.GetLength(0) - 1 - (yPos + roundedPlayerY) < 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = Console.ForegroundColor;
                            FixSameColor();
                            Console.ForegroundColor = Console.BackgroundColor;
                            Console.Write(BLOCK_CHAR);
                        }
                        else if (yPos + roundedPlayerY == -1)
                        {
                            Console.ForegroundColor = rgbToColor.GetClosestConsoleColor(colorChannel[1001].GetCurrRGB()); //BG1
                            Console.BackgroundColor = Console.ForegroundColor;
                            FixSameColor();
                            Console.ForegroundColor = Console.BackgroundColor;
                            Console.Write(BLOCK_CHAR);
                        }
                        else if (yPos + roundedPlayerY < -1)
                        {
                            Console.ForegroundColor = rgbToColor.GetClosestConsoleColor(colorChannel[1009].GetCurrRGB()); //BG2
                            Console.BackgroundColor = Console.ForegroundColor;
                            Console.Write(' ');
                        }
                        else if (xPos + roundedPlayerX < 0)
                        {
                            Console.ForegroundColor = rgbToColor.GetClosestConsoleColor(colorChannel[1009].GetCurrRGB()); //BG2
                            Console.BackgroundColor = Console.ForegroundColor;
                            Console.Write(' ');
                        }
                    }
                }
                cursorX = defaultCursorX;
                cursorY++;
            }

            Console.SetCursorPosition(0, cursorY + 5);
            Console.BackgroundColor = ConsoleColor.Black;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{((float)playerX / level.GetLength(1) * 100):f2}%    Attempt {attempts} Velocity: {playerYVelocity:f2}");
        }

        private static void FixSameColor()
        {

            if (Console.ForegroundColor == rgbToColor.GetClosestConsoleColor(colorChannel[1000].GetCurrRGB()))
            {
                Console.BackgroundColor = GetInverseColor(Console.ForegroundColor);
            }
        }


        private static void Configurelevel()
        {
            string text = File.ReadAllText(LEVEL_PATHS[levelNumb]);
            string[] lines = text.Split(';');
            level = new LevelGrid[10, 10];
            startColorChannel = new LevelColor[1011];

            for (int i = 0; i < colorChannel.Length; i++)
            {
                startColorChannel[i] = new LevelColor();
            }


            GetStartData(lines[0]);

            for (int i = 1; i < lines.Length; i++)
            {

                GetObjectData(lines[i].Split(','), i, lines.Length);
            }

            OutputIDs();

            HardCopyColors(startColorChannel, ref colorChannel);
            ResetBuffer();
        }

        private static void HardCopyColors(LevelColor[] source, ref LevelColor[] destination)
        {
            destination = new LevelColor[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i].HardClone();
            }
        }

        private static void GetStartData(string dataText)
        {
            string[] startInfos = dataText.Split(',');

            for (int i = 0; i < startInfos.Length - 1; i++)
            {
                switch (startInfos[i])
                {
                    case "kA2":
                        startGameMode = Int32.Parse(startInfos[i + 1]);
                        currentGamemode = startGameMode;
                        break;
                    case "kA4":
                        startSpeed = Int32.Parse(startInfos[i + 1]);
                        currentGamemode = startGameMode;
                        break;
                    case "kS38":
                        GetColors(startInfos[i + 1]);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void GetColors(string colorInfos)
        {
            string[] listOfColorChannels = colorInfos.Split('|');

            for (int i = 0; i < listOfColorChannels.Length; i++)
            {
                int colorProperty = 0;

                byte red = 0;
                byte green = 0;
                byte blue = 0;
                int channelID = 0;

                string[] colorChannelInfo = listOfColorChannels[i].Split('_');

                for (int j = 0; j < colorChannelInfo.Length - 1; j++)
                {
                    int value = int.Parse(colorChannelInfo[j]);

                    if (j % 2 == 0)
                    {
                        colorProperty = value;
                    }
                    else
                    {
                        switch (colorProperty)
                        {
                            case 1:
                                red = (byte)value;
                                break;
                            case 2:
                                green = (byte)value;
                                break;
                            case 3:
                                blue = (byte)value;
                                break;
                            case 6:
                                channelID = value;
                                break;
                        }
                    }
                }
                startColorChannel[channelID].SetChannelColor(red, green, blue);
            }
            startColorChannel[1010].SetChannelColor(0, 0, 0);
        }

        private static void OutputIDs()
        {
            string[] contentToWrite = new string[level.GetLength(0)];

            for (int i = 0; i < level.GetLength(0); i++)
            {
                for (int j = 0; j < level.GetLength(1); j++)
                {
                    if (level[i, j] != null)
                    {
                        contentToWrite[i] += level[i, j].GetObjectID();
                    }
                    else
                        contentToWrite[i] += 0;
                    contentToWrite[i] += " ";
                }
            }

            File.WriteAllLines("StereoMadness.test", contentToWrite);
        }

        private static void GetObjectData(string[] numbs, int objNumb, int maxObj)
        {
            int levelObjectID = 0;
            float objectValue = 0;
            bool isID = false;
            bool isObject = false;

            int objectID = -1;
            int xPos = -1;
            int yPos = -1;

            for (int i = 0; i < numbs.Length && !isObject; i++)
            {
                if (showDebugPercentage)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Loading Object {objNumb + 1,5}/{maxObj}...");
                }

                if (i % 2 == 0)
                {
                    isID = Int32.TryParse(numbs[i], out levelObjectID);
                }
                else
                {

                    isID = float.TryParse(numbs[i], out objectValue);

                    if (isID)
                    {
                        switch (levelObjectID)
                        {
                            case 1:
                                objectID = (int)objectValue;

                                if (objectList.Contains(objectID))
                                {
                                    isObject = true;
                                }

                                if (debugObjID)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Setting ObjectID to {objectValue}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }

                                break;
                            case 2:
                                xPos = (int)objectValue / 30;

                                if (debugXPos)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Setting X Position to {xPos}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }

                                break;
                            case 3:
                                yPos = (int)objectValue / 30;

                                if (xPos >= 0 && yPos >= 0)
                                {
                                    if (xPos >= level.GetLength(1) || yPos >= level.GetLength(0))
                                    {
                                        EnlargeLevel(xPos, yPos);

                                        if (debugResize)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"New Rows: {xPos,5} {yPos,5}");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                    }

                                    if (level[yPos, xPos] == null || !blockList.Contains(level[yPos, xPos].GetObjectID()))
                                    {
                                        level[yPos, xPos] = new LevelGrid();
                                        level[yPos, xPos].SetObjectID(objectID);
                                    }
                                    if (debugObjPlace)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting X{xPos,5} Y{yPos,5} to {objectID}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 4:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetHorizontalFlipped((int)objectValue);

                                    if (debugHorizontalFlip)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Horizontal Flip to {level[yPos, xPos].GetHorizontalFlip()}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 5:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetVerticalFlipped((int)objectValue);

                                    if (debugVerticalFlip)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Vertical Flip to {level[yPos, xPos].GetVerticalFlip()}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 6:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetRotation((int)objectValue);

                                    if (debugRotation)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Rotation to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 7:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetColorTriggerRed((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Red Color in ColorTrigger to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 8:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetColorTriggerGreen((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Green Color in ColorTrigger to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 9:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetColorTriggerBlue((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Blue Color in ColorTrigger to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 10:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetTriggerDuration(objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Duration in trigger to {level[yPos, xPos].GetTriggerDuration()}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 11:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetTouchTriggered((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting TouchTriggered in trigger to {level[yPos, xPos].GetTouchTriggered()}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            //case 13:
                            //    // Not usefull
                            //    break;
                            case 14:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetTintGround((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting TintGround in trigger to {level[yPos, xPos].GetTintGround()}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            //case 17:
                            //    // Not usefull
                            //    break;
                            case 23:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetTriggerTargetColorID((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Target Color in ColorTrigger to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 36:
                                if (xPos >= 0 && yPos >= 0)
                                {
                                    level[yPos, xPos].SetTriggerTargetColorID((int)objectValue);

                                    if (debugTriggers)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Setting Target Color in ColorTrigger to {objectValue}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                break;
                            case 74:
                                // Undiscovered Feature
                                break;
                            default:
                                if (debugTriggers)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"LevelObjectID {levelObjectID,3} not implemented. Wanted Value: {objectValue,5}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (debugNotImplemented)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{numbs[i]} is not an integer or float! {levelObjectID}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }

            }
        }

        private static void EnlargeLevel(int xPos, int yPos)
        {
            int rows;
            int cols;


            if (yPos >= level.GetLength(0))
                rows = yPos + 1;
            else
                rows = level.GetLength(0);

            if (xPos >= level.GetLength(1))
                cols = xPos + 1;
            else
                cols = level.GetLength(1);

            LevelGrid[,] newLevel = new LevelGrid[rows, cols];

            for (int i = 0; i < level.GetLength(0); i++)
                for (int j = 0; j < level.GetLength(1); j++)
                    newLevel[i, j] = level[i, j];

            level = newLevel;
        }

        public static void ResetBuffer()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            string box = File.ReadAllText("border.txt");
            Console.WriteLine(box);
        }

        private static Random _random = new Random();

        private static ConsoleColor GetInverseColor(ConsoleColor color)
        {
            // Convert ConsoleColor to the corresponding RGB value
            int colorValue = (int)color;

            // Calculate the inverted RGB value using XOR operation with 15 (maximum value for ConsoleColor)
            int invertedColorValue = colorValue ^ 15;

            // Convert the inverted RGB value back to ConsoleColor
            ConsoleColor invertedColor = (ConsoleColor)invertedColorValue;

            return invertedColor;
        }
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(_random.Next(consoleColors.Length));
        }
    }
}