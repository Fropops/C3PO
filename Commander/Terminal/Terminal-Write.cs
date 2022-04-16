using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{

    public enum TerminalMessageType
    {
        Default,
        Error,
        Info,
        Success,
    }

    public partial class Terminal
    {


        public const ConsoleColor PromptColor = ConsoleColor.Yellow;
        public const ConsoleColor SuccessColor = ConsoleColor.Green;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;
        public const ConsoleColor InfoColor = ConsoleColor.Cyan;

        public static ConsoleColor DefaultBackGroundColor = Console.BackgroundColor;
        public static ConsoleColor DefaultForeGroundColor = Console.ForegroundColor;


        public void InnerWriteLine(params string[] strs)
        {
            foreach (var str in strs)
                Console.Write(str);
            Console.WriteLine();
        }

        public void WriteLine(TerminalMessageType typ, params string[] strs)
        {
            switch (typ)
            {

                case TerminalMessageType.Success:
                    Console.ForegroundColor = SuccessColor;
                    break;
                case TerminalMessageType.Error:
                    Console.ForegroundColor = ErrorColor;
                    break;
                case TerminalMessageType.Info:
                    Console.ForegroundColor = InfoColor;
                    break;
                default:
                    Console.ForegroundColor = this.DefaultColor;
                    break;

            }
            InnerWriteLine(strs);
            Console.ForegroundColor = this.DefaultColor;
        }


        public void WritePrompt()
        {
            Console.ForegroundColor = PromptColor;
            Console.Write(this.Prompt);
            Console.ForegroundColor = this.DefaultColor;

        }

        public static void WriteSuccess(params string[] parm)
        {
            Instance.WriteLine(TerminalMessageType.Success, parm);
        }
        public static void WriteError(params string[] parm)
        {
            Instance.WriteLine(TerminalMessageType.Error, parm);
        }
        public static void WriteInfo(params string[] parm)
        {
            Instance.WriteLine(TerminalMessageType.Info, parm);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void Write(string text)
        {
            Console.Write(text);
        }

        public static void WriteLine(params string[] parm)
        {
            Instance.InnerWriteLine(parm);
        }

        private static int CursorLeftSave;
        private static int CursorTopSave;

        public static void SaveCursorPosition()
        {
            CursorLeftSave = Console.GetCursorPosition().Left;
            CursorTopSave = Console.GetCursorPosition().Top;
        }

        public static void ResetCursorPosition()
        {
            Console.SetCursorPosition(CursorLeftSave, CursorTopSave);
        }

        public static void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, Console.WindowTop + top);
        }

        public static void DrawBackGround(ConsoleColor color, int height)
        {
            var consoleWidth = Console.WindowWidth;
            Console.BackgroundColor = color;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < consoleWidth; ++x)
                    Console.Write(' ');
                Console.WriteLine();
            }
            Console.BackgroundColor = DefaultBackGroundColor;
        }

        public static void SetForeGroundColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void SetBackGroundColor(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }



    }
}
