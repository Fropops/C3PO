using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Agent.Service
{
    internal interface IFrameService
    {
        NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data);
        NetFrame CreateFrame(string source, NetFrameType typ, byte[] data);

        byte[] GetData(NetFrame frame);
    }
    internal class FrameService : IFrameService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IConfigService _configService;
        public FrameService(ICryptoService cryptoService, IConfigService configService)
        {
            _cryptoService = cryptoService;
            _configService = configService;
        }

        public NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data)
        {
            var newData = this._configService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
            var frame = new NetFrame(source, destination, typ, newData);
            return frame;
        }

        public NetFrame CreateFrame(string source, NetFrameType typ, byte[] data)
        {
            return this.CreateFrame(source, String.Empty, typ, data);
        }

        public byte[] GetData(NetFrame frame)
        {
            var data = frame.Data;
            return this._configService.EncryptFrames ? this._cryptoService.Decrypt(data) : data;
        }
    }

}
