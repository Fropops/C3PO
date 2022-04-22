using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
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

        private bool _isCommandCorrectlyExecuting = false;
        public abstract RootCommand Command { get; }

        public override void Execute(string parms)
        {
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();
            InnerExecute(terminal, executor, comm, parms);
        }

        protected async override void InnerExecute(ITerminal terminal, IExecutor executor, ICommModule comm, string parms)
        {
            if (this._command == null)
            {
                this._command = this.Command;
                this._command.Name = this.Name;
                this._command.Handler = CommandHandler.Create<T>(HandleCommandWrapper);
            }

            var res = _command.Invoke(parms);
            if (res > 0)
            {
                executor.InputHandled(this, false);
                return;
            }

            await Task.Delay(2000);
            if (!this._isCommandCorrectlyExecuting) //prevent blocking when Handler is not called
                executor.InputHandled(this, false);
        }

        private async void HandleCommandWrapper(T options)
        {
            this._isCommandCorrectlyExecuting = true;
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();
            bool result = await this.HandleCommand(options, terminal, executor, comm);
            executor.InputHandled(this, result);
        }

        protected abstract Task<bool> HandleCommand(T options, ITerminal terminal, IExecutor executor, ICommModule comm);
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
