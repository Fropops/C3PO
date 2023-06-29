using System.Collections.Generic;
using System.Linq;
using Common.APIModels.WebHost;

namespace TeamServer.Services;

public interface IWebHostService
{
    void Add(string path, FileWebHost file);
    void Remove(string path);
    byte[] GetFile(string path);

    FileWebHost Get(string path);

    List<FileWebHost> GetAll();

    void Clear();

    List<WebHostLog> GetLogs();
    void ClearLogs();

    void Addlog(WebHostLog log);

}

public class WebHostService : IWebHostService
{
    private Dictionary<string, FileWebHost> files = new Dictionary<string, FileWebHost>();
    private List<WebHostLog> logs = new List<WebHostLog>();

    public void Add(string path, FileWebHost file)
    {
        if (!this.files.ContainsKey(path))
            files.Add(path, file);
        else
            files[path] = file;
    }


    public void Remove(string path)
    {
        if (this.files.ContainsKey(path))
            this.files.Remove(path);
    }

    public byte[] GetFile(string path)
    {
        if (this.files.ContainsKey(path))
            return this.files[path].Data;
        return null;
    }

    public FileWebHost Get(string path)
    {
        if (this.files.ContainsKey(path))
            return this.files[path];
        return null;
    }

    public List<FileWebHost> GetAll()
    {
        return this.files.Values.ToList();
    }

    public void Clear()
    {
        this.files.Clear();
    }

    public List<WebHostLog> GetLogs()
    {
        return logs;
    }
    public void ClearLogs()
    {
        this.logs.Clear();
    }

    public void Addlog(WebHostLog log)
    {
        this.logs.Add(log);
    }

}