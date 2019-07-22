using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expose
{
    internal static class Output
    {
        internal static bool HadError = false;

        internal static void WriteLine(string message)
        {
            Write($"{message}{Environment.NewLine}");
        }

        internal static void Write(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            string messageLeft = message;
            while (messageLeft.Length > 0)
            {
                switch (Console.ForegroundColor)
                {
                    case ConsoleColor.Cyan:
                        WriteToCharacter(ref messageLeft, new Dictionary<string, ConsoleColor>
                        {
                            { ">", ConsoleColor.White } ,
                        });
                        break;
                    case ConsoleColor.Yellow:
                        WriteToCharacter(ref messageLeft, new Dictionary<string, ConsoleColor>
                        {
                            { "]", ConsoleColor.White } ,
                        });
                        break;
                    case ConsoleColor.DarkGray:
                        WriteToCharacter(ref messageLeft, new Dictionary<string, ConsoleColor>
                        {
                            { "*__", ConsoleColor.White } ,
                        });
                        break;
                    case ConsoleColor.Red:
                        WriteToCharacter(ref messageLeft, new Dictionary<string, ConsoleColor>
                        {
                            { "%$#", ConsoleColor.White } ,
                        });
                        break;
                    case ConsoleColor.White:
                        WriteToCharacter(ref messageLeft, new Dictionary<string, ConsoleColor>
                        {
                            { "<", ConsoleColor.Cyan } ,
                            { "[", ConsoleColor.Yellow } ,
                            { "__*", ConsoleColor.DarkGray } ,
                            { "#$%", ConsoleColor.Red } ,
                        });
                        break;
                    case ConsoleColor.Black:
                    case ConsoleColor.DarkBlue:
                    case ConsoleColor.DarkGreen:
                    case ConsoleColor.DarkCyan:
                    case ConsoleColor.DarkRed:
                    case ConsoleColor.DarkMagenta:
                    case ConsoleColor.DarkYellow:
                    case ConsoleColor.Gray:
                    case ConsoleColor.Blue:
                    case ConsoleColor.Green:
                    case ConsoleColor.Magenta:
                    default:
                        throw new NotImplementedException();
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal static void WriteError(string message)
        {
            HadError = true;
            WriteLine($@"

#$%{message}%$#

");
        }

        private static bool WriteToCharacter(ref string messageLeft, Dictionary<string, ConsoleColor> config)
        {
            var currentLowestIndex = messageLeft.Length;
            string splitter = null;
            ConsoleColor? colour = null;
            foreach (var key in config.Keys)
            {
                var index = messageLeft.IndexOf(key);
                if (index != -1 && index < currentLowestIndex)
                {
                    currentLowestIndex = index;
                    splitter = key;
                    colour = config[key];
                }
            }
            if (colour.HasValue)
            {
                var messageToWrite = messageLeft.Remove(messageLeft.IndexOf(splitter));
                messageLeft = messageLeft.Remove(0, messageLeft.IndexOf(splitter) + splitter.Length);
                Console.Write(messageToWrite);
                Console.ForegroundColor = colour.Value;
                return true;
            }
            else
            {
                Console.Write(messageLeft);
                Console.ForegroundColor = ConsoleColor.White;
                messageLeft = string.Empty;
                return false;
            }
        }
    }
}
