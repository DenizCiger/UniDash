using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryDash
{
    public class ConsoleColorConverter
    {
        public ConsoleColor GetClosestConsoleColor(byte[] rgbInput)
        {
            int r = (int)rgbInput[0];
            int g = (int)rgbInput[1];
            int b = (int)rgbInput[2];


            ConsoleColor closestColor = ConsoleColor.Black;
            double closestDistance = double.MaxValue;

            foreach (ConsoleColor consoleColor in Enum.GetValues(typeof(ConsoleColor)))
            {
                GetRgbValues(consoleColor, out int consoleR, out int consoleG, out int consoleB);
                double distance = CalculateColorDistance(r, g, b, consoleR, consoleG, consoleB);

                if (distance < closestDistance)
                {
                    closestColor = consoleColor;
                    closestDistance = distance;
                }
            }

            return closestColor;
        }

        private double CalculateColorDistance(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            double deltaR = r1 - r2;
            double deltaG = g1 - g2;
            double deltaB = b1 - b2;

            return Math.Sqrt(deltaR * deltaR + deltaG * deltaG + deltaB * deltaB);
        }

        private void GetRgbValues(ConsoleColor color, out int r, out int g, out int b)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    r = 0;
                    g = 0;
                    b = 0;
                    break;
                case ConsoleColor.DarkBlue:
                    r = 0;
                    g = 0;
                    b = 128;
                    break;
                case ConsoleColor.DarkGreen:
                    r = 0;
                    g = 128;
                    b = 0;
                    break;
                case ConsoleColor.DarkCyan:
                    r = 0;
                    g = 128;
                    b = 128;
                    break;
                case ConsoleColor.DarkRed:
                    r = 128;
                    g = 0;
                    b = 0;
                    break;
                case ConsoleColor.DarkMagenta:
                    r = 128;
                    g = 0;
                    b = 128;
                    break;
                case ConsoleColor.DarkYellow:
                    r = 128;
                    g = 128;
                    b = 0;
                    break;
                case ConsoleColor.Gray:
                    r = 192;
                    g = 192;
                    b = 192;
                    break;
                case ConsoleColor.DarkGray:
                    r = 128;
                    g = 128;
                    b = 128;
                    break;
                case ConsoleColor.Blue:
                    r = 0;
                    g = 0;
                    b = 255;
                    break;
                case ConsoleColor.Green:
                    r = 0;
                    g = 255;
                    b = 0;
                    break;
                case ConsoleColor.Cyan:
                    r = 0;
                    g = 255;
                    b = 255;
                    break;
                case ConsoleColor.Red:
                    r = 255;
                    g = 0;
                    b = 0;
                    break;
                case ConsoleColor.Magenta:
                    r = 255;
                    g = 0;
                    b = 255;
                    break;
                case ConsoleColor.Yellow:
                    r = 255;
                    g = 255;
                    b = 0;
                    break;
                case ConsoleColor.White:
                    r = 255;
                    g = 255;
                    b = 255;
                    break;
                default:
                    r = 255;
                    g = 255;
                    b = 255;
                    break;
            }
        }
    }

}
