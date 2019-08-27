using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VRCheat.Utils
{
    static class ConsoleUtils
    {
        [DllImport("kernel32.dll")]
        internal static extern int AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public static void ShowConsole() => SetForegroundWindow(GetConsoleWindow());

        public static void Load()
        {
            AllocConsole();

            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);

            StreamReader standardInput = new StreamReader(Console.OpenStandardInput());
            Console.SetIn(standardInput);

            Console.Clear();

            Console.WriteLine("Created by Harekuin");
            ShowConsole();
        }

        public static string AskInput(string question)
        {
            ShowConsole();

            Console.Write(question);

            while (Console.KeyAvailable)
                Console.ReadKey();

            string answer = Console.ReadLine();

            return answer == string.Empty ? null : answer;
        }
    }
}
