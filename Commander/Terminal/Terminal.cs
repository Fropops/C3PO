using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Commander
{
    public partial class Terminal
    {
        public static Terminal Instance { get; } = new Terminal();

        public event EventHandler<string> InputValidated;
        public ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;

        CancellationTokenSource _token = new CancellationTokenSource();

        public bool CanHandleInput { get; set; } = true;
        public async Task Start()
        {
            while (!_token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    try
                    {
                        this.HandleKey(key);
                    }
                    catch (Exception e)
                    {
                        Terminal.WriteLine();
                        Terminal.WriteError("Terminal Error :");
                        Terminal.WriteError("----------------");
                        Terminal.WriteError(e.ToString());
                        this.CanHandleInput = true;
                    }
                }

                await Task.Delay(10);
            }
        }

        private int _positionInLine = 0;
        private string _currentLine = string.Empty;

        private List<string> _history = new List<string>();
        private int _historyPosition = 0;

        private Terminal()
        {
            Console.TreatControlCAsInput = true;
        }

        public string Prompt { get; set; } = "$> ";

        protected int CursorLeft
        {
            get
            {
                return this.Prompt.Length + this._positionInLine;
            }
        }
        protected void HandleKey(ConsoleKeyInfo key)
        {
            if (!this.CanHandleInput)
                return;
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    {
                        if (_positionInLine > 0)
                        {
                            Console.CursorLeft -= 1;
                            _positionInLine -= 1;
                        }
                    }
                    break;
                case ConsoleKey.RightArrow:
                    {
                        if (_positionInLine < this._currentLine.Length)
                        {
                            Console.CursorLeft += 1;
                            _positionInLine += 1;
                        }
                    }
                    break;
                case ConsoleKey.Home:
                    {
                        _positionInLine = 0;
                        Console.CursorLeft = this.CursorLeft;
                    }
                    break;
                case ConsoleKey.End:
                    {
                        _positionInLine = this._currentLine.Length;
                        Console.CursorLeft = this.CursorLeft;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    {
                        if (_historyPosition < _history.Count -1)
                        {
                            _historyPosition++;
                            LoadHistory();
                        }
                    }
                    break;
                case ConsoleKey.DownArrow:
                    {
                        if (_historyPosition > 0)
                        {
                            _historyPosition--;
                            LoadHistory();
                        }
                    }
                    break;
                case ConsoleKey.Enter:
                    {
                        //Save to history
                        string line = this._currentLine.Trim();
                        Console.WriteLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            this.RefreshCurrentHistory();
                            this.InputValidated?.Invoke(this, line);
                        }
                        else
                        {
                            this._history.RemoveAt(0);
                            this.NewLine(false);
                        }
                    }
                    break;
                case ConsoleKey.Backspace:
                    {
                        if (_positionInLine > 0)
                        {
                            Console.CursorLeft -= 1;
                            _positionInLine -= 1;
                            this._currentLine = this._currentLine.Remove(this._positionInLine, 1);
                            Console.CursorLeft = this.CursorLeft;
                            Console.Write(LineRightPart + " ");
                            Console.CursorLeft = this.CursorLeft;
                            this.RefreshCurrentHistory();
                        }
                    }
                    break;
                case ConsoleKey.Delete:
                    {
                        if (_positionInLine < this._currentLine.Length)
                        {
                            this._currentLine = this._currentLine.Remove(this._positionInLine, 1);
                            Console.Write(LineRightPart + " ");
                            Console.CursorLeft = this.CursorLeft;
                            this._history.RemoveAt(0);
                            this.RefreshCurrentHistory();
                        }
                    }
                    break;
                default:
                    {
                        if (key.Key == ConsoleKey.C && (key.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            this._history.RemoveAt(0);
                            this.NewLine();
                            break;
                        }


                        var c = key.KeyChar;
                        string str = string.Empty;
                        str += c;

                        this._currentLine = this._currentLine.Insert(this._positionInLine, str);
                        WriteAndResetCursor(this.LineRightPart);
                        _positionInLine += 1;
                        Console.CursorLeft = this.CursorLeft;

                        this.RefreshCurrentHistory();
                    }
                    break;


            }
        }

        protected string LineRightPart
        {
            get
            {
                return this._currentLine.Substring(this._positionInLine, this._currentLine.Length - this._positionInLine);
            }
        }

        protected void WriteAndResetCursor(string str)
        {
            Console.Write(str);
            Console.CursorLeft = this.CursorLeft;
        }

        public void NewLine(bool lineBreak = true)
        {
            if (lineBreak)
                Console.WriteLine();
            this.WritePrompt();
            _positionInLine = 0;
            _currentLine = string.Empty;
            this._history.Insert(0, this._currentLine);
            this._historyPosition = 0;
        }
        public void stop()
        {
            _token.Cancel();
        }

        private void LoadHistory()
        {
            var history = _history[_historyPosition];
            int max = Math.Max(history.Length, this._currentLine.Length);
            this._currentLine = history;
            this._positionInLine = 0;
            Console.CursorLeft = CursorLeft;
            Console.Write(this._currentLine.PadRight(max));
            this._positionInLine = this._currentLine.Length;
            Console.CursorLeft = CursorLeft;
        }

        private void RefreshCurrentHistory()
        {
            if (this._history.Count > 0)
                this._history.RemoveAt(0);
            this._history.Insert(0, this._currentLine);
        }

        public void Interrupt()
        {
            Console.CursorLeft = 0;
            Console.Write(string.Empty.PadRight(this.Prompt.Length + this._currentLine.Length));
            Console.CursorLeft = 0;
        }

        public void Restore()
        {
            this.WritePrompt();
            Console.Write(this._currentLine);
            Console.CursorLeft = this.CursorLeft;
        }

       

        
    }
}
