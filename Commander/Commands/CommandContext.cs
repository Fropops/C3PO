using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Common.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Shared;
using BinarySerializer;

namespace Commander.Commands
{
    public class CommandContext
    {
        public IExecutor Executor { get; set; }

        public ITerminal Terminal { get; set; }

        public ICommModule CommModule { get; set; }

        public CommanderConfig Config { get; set; }

        public string CommandLabel { get; set; }

        public string CommandParameters { get; set; }

        public ParameterDictionary Parameters = null;

        public void AddParameter<T>(ParameterId id, T item)
        {
            if (this.Parameters == null)
                this.Parameters = new ParameterDictionary();
            this.Parameters.Add(id, item.BinarySerializeAsync().Result);
        }
    }

    public class CommandContext<T> : CommandContext
    {
        public T Options { get; set; }
    }

    public static class Extenstions
    {
        //internal static async Task<string> UploadAndDisplay(this CommandContext context, byte[] data, string fileName, string description = "Uploading")
        //{
        //    string fileId = null;
        //    await AnsiConsole.Progress().Columns(new ProgressColumn[] { new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn(Spinner.Known.Default).Style(Style.Parse("cyan")) })
        //               .StartAsync(async ctx =>
        //               {
        //                   var task = ctx.AddTask($"[cyan]{description}[/]");
        //                   task.MaxValue = 100;
        //                   fileId = await context.CommModule.Upload(data, fileName, a => { task.Increment(1); });
        //                   task.Value = task.MaxValue;
        //               });
        //    return fileId;
        //}

        internal static byte[] GeneratePayloadAndDisplay(this CommandContext context, PayloadGenerationOptions options, bool verbose = false)
        {
            byte[] pay = null;
            AnsiConsole.Status()
                    .Start($"[olive]Generating Payload {options.Type} for Endpoint {options.Endpoint} (arch = {options.Architecture}).[/]", ctx =>
                    {
                        var generator = new PayloadGenerator(context.Config.PayloadConfig, context.Config.SpawnConfig);
                        generator.MessageSent += (object sender, string msg) => { if (verbose) context.Terminal.WriteLine(msg); };
                        pay = generator.GeneratePayload(options);
                    });

            return pay;
        }
    }
}
