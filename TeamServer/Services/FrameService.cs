using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Linq;
using System;
using Shared;

namespace TeamServer.Services;

public interface IFrameService
{
    byte[] GetData(NetFrame frame);
    NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data);
    NetFrame CreateFrame(string destination, NetFrameType typ, byte[] data);
}

public class FrameService : IFrameService
{
    private readonly ICryptoService _cryptoService;
    public string Key { get; private set; }
    public FrameService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public NetFrame CreateFrame(string source, string destination, NetFrameType typ, byte[] data)
    {
        var newData = this._cryptoService.EncryptFrames ? this._cryptoService.Encrypt(data) : data;
        var frame = new NetFrame(source, destination, typ, newData);
        return frame;
    }

    public NetFrame CreateFrame(string destination, NetFrameType typ, byte[] data)
    {
        return this.CreateFrame(null, destination, typ, data);
    }

    public byte[] GetData(NetFrame frame)
    {
        var data = frame.Data;
        return this._cryptoService.EncryptFrames ? this._cryptoService.Decrypt(data) : data;
    }
}