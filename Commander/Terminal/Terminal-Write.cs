using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Terminal
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
        private void InnerWriteLine(params string[] strs)
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
                    Console.ForegroundColor = TerminalConstants.SuccessColor;
                    break;
                case TerminalMessageType.Error:
                    Console.ForegroundColor = TerminalConstants.ErrorColor;
                    break;
                case TerminalMessageType.Info:
                    Console.ForegroundColor = TerminalConstants.InfoColor;
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
            Console.ForegroundColor = TerminalConstants.PromptColor;
            Console.Write(this.Prompt);
            Console.ForegroundColor = this.DefaultColor;

        }

        public void WriteSuccess(params string[] parm)
        {
            this.WriteLine(TerminalMessageType.Success, parm);
        }
        public void WriteError(params string[] parm)
        {
            this.WriteLine(TerminalMessageType.Error, parm);
        }
        public void WriteInfo(params string[] parm)
        {
            this.WriteLine(TerminalMessageType.Info, parm);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void Write(string text)
        {
            Console.Write(text);
        }

        public void WriteLine(params string[] parm)
        {
            this.InnerWriteLine(parm);
        }

        private int CursorLeftSave;
        private int CursorTopSave;

        public void SaveCursorPosition()
        {
            CursorLeftSave = Console.GetCursorPosition().Left;
            CursorTopSave = Console.GetCursorPosition().Top;
        }

        public void ResetCursorPosition()
        {
            Console.SetCursorPosition(CursorLeftSave, CursorTopSave);
        }

        public void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, Console.WindowTop + top);
        }

        public void DrawBackGround(ConsoleColor color, int height)
        {
            var consoleWidth = Console.WindowWidth;
            Console.BackgroundColor = color;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < consoleWidth; ++x)
                    Console.Write(' ');
                Console.WriteLine();
            }
            Console.BackgroundColor = TerminalConstants.DefaultBackGroundColor;
        }

        public void SetForeGroundColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public void SetBackGroundColor(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }



    }
}
