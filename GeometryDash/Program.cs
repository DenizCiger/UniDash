﻿/*		          HTBLA Leonding / CABS Project
 *----------------------------------------------------------------
 *		                   Deniz Ciger
 *----------------------------------------------------------------
 *	Description: Recreation of Geometry Dash made for the Console.
 *	             Geometry Dash is a rythm based platforming game.	        
 *----------------------------------------------------------------
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection.Emit;
using Test;

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen

namespace GeometryDash
{
    public enum Speeds { Slow = 0, Normal = 1, Fast = 2, Faster = 3, Fastest = 4 };

    internal class Program
    {

        //GamePlay Config
        static LevelGrid[,] level = new LevelGrid[10, 10];
        static readonly string[] UI_PATHS = { "Logo.txt" };
        static readonly string[] LEVEL_PATHS = { "Data\\Levels\\StereoMadness.dat", "Data\\Levels\\BackOnTrack.dat", "Data\\Levels\\Polargeist.dat" };
        static readonly string[] SONG_PATHS = { "Data\\Songs\\StereoMadness.wav", "Data\\Songs\\BackOnTrack.wav", "Data\\Songs\\Polargeist.wav" };
        static readonly float[] speedValues = { 8.4f, 10.41667f, 12.91667f, 15.667f, 19.2f };
        static SoundPlayer gameSound;
        static ConsoleColor backgroundStartColor = new ConsoleColor();
        static ConsoleColor floorStartColor = new ConsoleColor();
        static int startGameMode = 0;
        static int startSpeed = 0;

        const int MILLIS_PER_TICK = 50;

        const int VIEWABLE_UP = 5;
        const int VIEWABLE_DOWN = 5;
        const int VIEWABLE_LEFT = 7;
        const int VIEWABLE_RIGHT = 11;

        const float JUMP_FORCE = 12.65795f / 2;
        const float FALLING_SPEED = 12.63245f/2;

        const char BLOCK_CHAR = '\u25A0';
        const char PLAYER_CHAR = '\u25A0';//'\u25A1';
        const char SPIKE_CHAR = '\u25B2';
        const char ORB_CHAR = '\u25c9';
        const char COIN_CHAR = '\u235f';
        const char JUMP_PAD_CHAR = '\u235f';
        const char PORTAL_CHAR = '\u0029';

        //Needed Gameplay variables
        static bool playerJumping = false;
        static bool playerJumpPad = false;
        static float oldYPos = 0;
        static float playerX = 0;
        static float playerY = 0;
        static Stopwatch time = new Stopwatch();
        static Stopwatch triggerDuration = new Stopwatch();
        static float deltaTime = 1000f;

        static int currentGamemode = 1; // 0 cube 1 ship 2 ball 3 ufo 4 wave 5 robot 6 spider
        static ConsoleColor currentBackgroundColor = new ConsoleColor();
        static ConsoleColor currentFloorColor = ConsoleColor.White;
        static readonly int[] blockList = { 1, 2, 3, 4/*, 5*/, 6, 7, 40 }; //commenteds have no hitboxes
        static readonly int[] spikeList = { 8, 9, 39 };
        static readonly int[] portalList = { 10, 11, 12, 13 }; //Gravity Down, Gravity Up, Cube,Ship
        static readonly int[] objectList = { 16, 17, 18, 19, 20, 21, 41 };
        static readonly int[] triggerList = { 29, 30, 104, 105, 221 /*...*/ };
        static int levelNumb = 0;
        static byte redDifference = 0;
        static byte greenDifference = 0;
        static byte blueDifference = 0;

        //Hacks
        static bool noClip = false;

        //Debug Config
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
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.SetWindowSize(Console.LargestWindowWidth / 2, Console.LargestWindowHeight / 2);
            Console.Title = "UniDash";

            LevelSelect();

            gameSound.Play();

            while (true)
            {
                time.Start();

                UpdateGame(gameSound);
                Thread.Sleep(MILLIS_PER_TICK);

                time.Stop();
                deltaTime = time.ElapsedMilliseconds;
                time.Reset();
            }
        }

        private static int GetLevelNumb()
        {
            bool isValid = false;
            int input = 0;

            while (!isValid)
            {
                Console.Write("Level Number: ");
                isValid = Int32.TryParse(Console.ReadLine()!, out input);

                if (isValid && (input <= 0 || input > LEVEL_PATHS.Length))
                {
                    isValid = false;
                }

                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Only Levels 1 - {LEVEL_PATHS.Length} are available!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            return input;
        }

        private static void UpdateGame(SoundPlayer gameSound)
        {
            int roundedPlayerY = (int)Math.Round(playerY);
            int roundedPlayerX = (int)Math.Round(playerX);
            int currentTile;
            Console.CursorVisible = false;



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
            else if (IsInPortal(roundedPlayerX, roundedPlayerY))
            {
                switch (currentTile)
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
            else if (triggersTrigger(roundedPlayerX))
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

                            currentBackgroundColor = RGBToConsoleColor(r, g, b);

                            if (level[y, x].GetTintGround())
                            {
                                currentFloorColor = RGBToConsoleColor(r, g, b);
                            }
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
                        playerJumpPad = true;
                        oldYPos = playerY;
                        break;
                    default:
                        break;
                }
            }

            playerX += (float)speedValues[startSpeed] * (deltaTime / 1100f);

            if (playerJumping)
            {

                //JumpForce is 12.65795 Units per 0.2secs
                playerY += JUMP_FORCE * (deltaTime / 500f);

                if (playerY > oldYPos + 2f)
                {
                    playerJumping = false;
                    playerY = oldYPos + 2f;
                }
            }
            else if (playerJumpPad)
            {
                playerY += JUMP_FORCE * 3 * ((float)MILLIS_PER_TICK / 600f);

                if (playerY > oldYPos + 4.5f)
                {
                    playerJumpPad = false;
                    playerY = oldYPos + 4.5f;
                }
            }
            else
            {
                if (playerY > 0 && !IsStandingOnBlock((int)Math.Round(playerY), (int)Math.Round(playerX)))
                {
                    float distanceToFall = 0;

                    if (currentGamemode == 0)
                    {
                        //FallSpeed is 14.63245 Units per 0.5secs
                        distanceToFall = FALLING_SPEED * ((float)MILLIS_PER_TICK / 500f);
                    }
                    else if (currentGamemode == 1)
                    {
                        distanceToFall = 2f * ((float)MILLIS_PER_TICK / 500f);
                    }

                    bool isOkPos = false;

                    while (!isOkPos)
                    {
                        if (distanceToFall > 0)
                        {
                            if ((int)Math.Round(playerY - distanceToFall) >= 0 && level[(int)Math.Round(playerY - distanceToFall), roundedPlayerX] != null && blockList.Contains(level[(int)Math.Round(playerY - distanceToFall), roundedPlayerX].GetObjectID()))
                            {
                                distanceToFall -= 1;
                            }
                            else
                            {
                                isOkPos = true;
                            }
                        }
                        else
                        {
                            isOkPos = true;
                            distanceToFall = 0;
                        }
                    }
                    playerY -= distanceToFall;
                }
            }

            if (playerY < 0)
            {
                playerY = 0;
            }

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
                        playerY += 6f * ((float)MILLIS_PER_TICK / 500f);
                    }
                }
                else if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    PauseGame(gameSound);
                }
            }
            PrintState();
        }

        private static void PauseGame(SoundPlayer gameSound)
        {
            Console.Clear();
            gameSound.
            
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

        private static bool triggersTrigger(int roundedPlayerX)
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

        private static bool IsInPortal(int roundedPlayerX, int roundedPlayerY)
        {
            bool isInPortal = false;

            for (int i = -1; i < 2; i++)
            {
                int checkPosY = i + roundedPlayerY;
                if (checkPosY < level.GetLength(0) && checkPosY >= 0 && level[checkPosY, roundedPlayerX] != null && portalList.Contains(level[checkPosY, roundedPlayerX].GetObjectID()))
                {
                    isInPortal = true;
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
            Console.WriteLine(File.ReadAllText(UI_PATHS[0]));

            levelNumb = GetLevelNumb() - 1;

            gameSound = new(SONG_PATHS[levelNumb]);
            Configurelevel();

            playerX = 0;
            playerY = 0;
        }

        private static void PlayerDieEvent(SoundPlayer gameSound)
        {
            playerY = 0; playerX = 0;
            currentGamemode = startGameMode;
            currentBackgroundColor = backgroundStartColor;
            currentFloorColor = floorStartColor;
            gameSound.Stop();
            gameSound.Play();
        }

        private static void PlayerJump()
        {
            int roundedPlayerY = (int)Math.Round(playerY);
            int roundedPlayerX = (int)Math.Round(playerX);

            if (playerY < 0.05f || IsStandingOnBlock(roundedPlayerY, roundedPlayerX))
            {
                playerJumping = true;
                oldYPos = playerY;
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
                            if (level[(roundedPlayerY + i), roundedPlayerX + j] != null && level[(roundedPlayerY + i), roundedPlayerX + j].GetObjectID() == 36)
                            {
                                playerJumping = true;
                                oldYPos = playerY;
                                foundJumpOrb = true;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsStandingOnBlock(int roundedPlayerY, int roundedPlayerX)
        {
            bool isOnBlock = false;

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
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(PLAYER_CHAR);
                        }
                        else
                        {
                            if (level[yPos + roundedPlayerY, xPos + roundedPlayerX] != null)
                                currentTile = level[yPos + roundedPlayerY, xPos + roundedPlayerX].GetObjectID();
                            else
                                currentTile = 0;

                            Console.BackgroundColor = currentBackgroundColor;

                            //Printing Objects
                            if (currentTile == 0) // Is Air
                            {
                                Console.Write(" ");
                            }
                            else if (blockList.Contains(currentTile)) // Is Block
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(BLOCK_CHAR);
                            }
                            else if (spikeList.Contains(currentTile)) // Is Spike
                            {
                                Console.ForegroundColor = ConsoleColor.Black;
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

                                Console.Write(PORTAL_CHAR);
                            }
                            else if (currentTile == 35) // Is Jump Pad
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(JUMP_PAD_CHAR);
                            }
                            else if (currentTile == 36) // Is Jump Orb
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(ORB_CHAR);
                            }
                            else
                            {
                                Console.ForegroundColor = GetRandomConsoleColor();
                                Console.Write(new Random().Next(0, 10));
                            }

                        }
                        cursorX++;
                    }
                    else
                    {
                        Console.ForegroundColor = currentFloorColor;

                        if (currentFloorColor == currentBackgroundColor)
                        {
                            if (currentBackgroundColor != ConsoleColor.White)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                            }
                        }

                        Console.Write(BLOCK_CHAR);
                    }
                }
                cursorX = defaultCursorX;
                cursorY++;
            }

            Console.SetCursorPosition(0, cursorY + 5);
            Console.WriteLine($"X: {playerX,-10:f2} Y: {playerY,-10:f2}");
        }

        private static void Configurelevel()
        {
            string text = File.ReadAllText(LEVEL_PATHS[levelNumb]);
            string[] lines = text.Split(';');
            level = new LevelGrid[10, 10];

            GetStartData(lines[0]);

            for (int i = 1; i < lines.Length; i++)
            {

                GetObjectData(lines[i].Split(','), i, lines.Length);
            }

            OutputIDs();

            currentFloorColor = floorStartColor;
            currentBackgroundColor = backgroundStartColor;
            ResetBuffer();
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
            string[] colorChannels = colorInfos.Split('|');

            for (int i = 0; i < colorChannels.Length; i++)
            {
                int colorProperty = 0;

                byte red = 0;
                byte green = 0;
                byte blue = 0;
                int channelID = 0;

                string[] colorChannelInfo = colorChannels[i].Split('_');

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

                switch (channelID)
                {
                    case 1000:
                        backgroundStartColor = RGBToConsoleColor(red, green, blue);
                        break;
                    case 1001:
                        floorStartColor = RGBToConsoleColor(red, green, blue);
                        break;
                }
            }
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
            float floatValue = 0;
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
                    if (/*levelObjectID != 10 && levelObjectID != 35*/true)
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
                                            Console.WriteLine($"Setting TouchTriggered in trigger to {level[yPos, xPos].GetTriggerDuration()}");
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
                                    // Undiscovered Feature
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
                    else
                    {
                        isID = float.TryParse(numbs[i], out floatValue);
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

        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(_random.Next(consoleColors.Length));
        }

        static ConsoleColor RGBToConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}