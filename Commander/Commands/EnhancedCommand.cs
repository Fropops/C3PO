using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public abstract class EnhancedCommand<T> : ExecutorCommand
    {
        private RootCommand _command;
        public abstract RootCommand Command { get; }
        protected Executor Executor { get; set; }


        public override void Execute(Executor executor, string parms)
        {
            InnerExecute(executor, parms);
        }

        protected override void InnerExecute(Executor executor, string parms)
        {
            this.Executor = executor;

            if (this._command == null)
            {
                this._command = this.Command;
                this._command.Name = this.Name;
                this._command.Handler = CommandHandler.Create<T>(HandleCommandWrapper);
            }

            var res = _command.Invoke(parms);
            if (res > 0)
                this.Executor.InputHandled(this, false);
        }

        private async void HandleCommandWrapper(T options)
        {
            bool result = await this.HandleCommand(options);
            this.Executor.InputHandled(this, result);
        }

        protected abstract Task<bool> HandleCommand(T options);
    }


    //public abstract class EnhancedCommand : ExecutorCommand
    //{
    //    protected RootCommand RootCommand { get; set; }

    //    protected abstract RootCommand DeclareCommand();

    //    protected Executor Executor { get; set; }

    //    public override void Execute(Executor executor, string parms)
    //    {
    //            InnerExecute(executor, parms);
    //    }

    //    protected override void InnerExecute(Executor executor, string parms)
    //    {
    //        this.Executor = executor;
    //        if (this.RootCommand == null)
    //        {
    //            this.RootCommand = this.DeclareCommand();
    //            this.RootCommand.Name = this.Name;
    //        }

    //        var res = RootCommand.Invoke(parms);
    //        if (res > 0)
    //            this.Executor.InputHandled(this, false);
    //    }
    //}
}
