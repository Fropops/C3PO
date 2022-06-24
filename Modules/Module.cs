using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace ModuleBase
{
	public abstract class Module
	{
		public abstract string Name { get; }

        public AgentTaskResult Result { get; set; } = new AgentTaskResult();
		public string ServerProtocol { get; set; }
		public string ServerIp { get; set; }
		public int ServerPort { get; set; }


        private HttpClient _client;

        public static bool verbose = true;
        public static void Log(string message)
        {
            if (!verbose)
                return;
            File.AppendAllText("module.log" ,message+ Environment.NewLine);
        }

        public virtual void Execute(string executionContextB64)
        {
            Module.Log($"Starting {this.Name}");
            var context = Convert.FromBase64String(executionContextB64).Deserialize<ExecutionContext>();
            this.Execute(context);
        }

        public virtual void Execute(ExecutionContext context)
        {
            this.Result.Id = context.TaskId;
            this.ServerProtocol = context.ServerProtocol == "y" ? "https" : "http";
            this.ServerIp = context.ServerIp;
            this.ServerPort = context.ServerPort;
            


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback
            (
               delegate {
                   return true;
               }
            );

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            _client.BaseAddress = new Uri($"{this.ServerProtocol}://{this.ServerIp}:{this.ServerPort}");
            //Console.WriteLine(_client.BaseAddress);

            this.Result.Result += $"Starting Module {this.Name}" + Environment.NewLine;
            this.Result.Status = AgentResultStatus.Running;
            this.Notify();
            try
            {
                this.InnerExecute(context.Parameters);
            }
            catch(Exception ex)
            {
                Module.Log(ex.ToString());
            }
            this.Result.Result += $"Module {this.Name} Complete";
            this.Result.Status = AgentResultStatus.Completed;
            this.Notify();
            Module.Log($"{this.Name} Completed");

            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        public void Notify(string notif = null)
        {
            try
            {
                this.Result.Info = notif;
                var json = this.Result.SerializeAsString();
               
                var content = new StringContent(this.Result.SerializeAsString(), Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/ModuleInfo", content).Result;
            }
            catch(Exception ex)
            {
                Module.Log(ex.ToString());
            }
        }

        public void AppendResult(string res, bool notifAlso = false)
        {
            try
            {
                this.Result.Result += res + Environment.NewLine;
                if (notifAlso)
                    this.Result.Info = res;
                var json = this.Result.SerializeAsString();

                var content = new StringContent(this.Result.SerializeAsString(), Encoding.UTF8, "application/json");
                var response = _client.PostAsync("/ModuleInfo", content).Result;
            }
            catch (Exception ex)
            {
                Module.Log(ex.ToString());
            }
        }

        public abstract void InnerExecute(string parameters);
	}

    [DataContract]
    public class ExecutionContext
    {
        [DataMember(Name = "id")]
        public string TaskId { get; set; }
        [DataMember(Name = "i")]
        public string ServerIp { get; set; }
        [DataMember(Name = "p")]
        public int ServerPort { get; set; }
        [DataMember(Name = "s")]
        public string ServerProtocol { get; set; }
        [DataMember(Name = "a")]
        public string Parameters { get; set; }
    }

    [DataContract]
    public class AgentTask
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "command")]
        public string Command { get; set; }
        [DataMember(Name = "arguments")]
        public string Arguments { get; set; }

        [DataMember(Name = "fileId")]
        public string FileId { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }


        public string[] SplittedArgs
        {
            get
            {
                return (this.Arguments ?? string.Empty).GetArgs();
            }

        }
    }

    public enum AgentResultStatus
    {
        Queued = 0,
        Running = 1,
        Completed = 2
    }

    public class AgentTaskResult
    {
        public string Id { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Info { get; set; }
        public AgentResultStatus Status { get; set; }

        public List<TaskFileResult> Files { get; set; } = new List<TaskFileResult>();
    }

    public class TaskFileResult
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
    }

}

namespace ModuleBase
{
    public static class Extensions
    {
        public static string SerializeAsString<T>(this T obj)
        {
            byte[] bytes = null;
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                bytes = ms.ToArray();
            }
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static T Deserialize<T>(this string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(bytes))
            {
                return (T)serializer.ReadObject(ms);
            }
        }



        public static byte[] Serialize<T>(this T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(json))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        public static string[] GetArgs(this string src)
        {
            var res = new List<string>();
            src = src.Trim();

            bool inQuotes = false;
            bool inDoubleQuotes = false;

            string currentValue = string.Empty;
            foreach (var c in src)
            {
                if (c == '\"')
                {
                    if (!inQuotes)
                    {
                        if (inDoubleQuotes)
                        {
                            //end of params => 
                            inDoubleQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inDoubleQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c== '\'')
                {
                    if (!inDoubleQuotes)
                    {
                        if (inQuotes)
                        {
                            //end of params => 
                            inQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c == ' ')
                {
                    if (!inQuotes && !inDoubleQuotes)
                    {
                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                    }
                    else
                        currentValue += c;
                    continue;
                }

                currentValue += c;
            }


            if (!string.IsNullOrEmpty(currentValue))
            {
                res.Add(currentValue);
                currentValue = string.Empty;
            }

            return res.ToArray();
        }
    }


}

