using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expose
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SetConsoleColours();
            args = Settings.Cleanse(args);
            var command = App.GetCommand(args);
            if (command == null)
            {
                App.ShowMenu();
                Wait();
                return;
            }
            try
            {
                await command(args);
            }
            catch (Exception ex)
            {
                Output.WriteError(ex.ToString());
                Wait();
                return;
            }

            if (Output.HadError)
            {
                Wait();
            }
        }

        [Conditional("DEBUG")]
        private static void Wait()
        {
            Output.WriteLine("Done!");
            Console.ReadLine();
        }

        private static void SetConsoleColours()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }
    }
}
