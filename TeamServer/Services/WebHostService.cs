using System.Collections.Generic;

namespace TeamServer.Services;

public interface IWebHostService
{
    void Add(string path, byte[] fileContent);
    void Remove(string path);
    byte[] Get(string path);
    void Clear();
}

public class WebHostService : IWebHostService
{
    private Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

    public void Add(string path, byte[] fileContent)
    {
        if (!this.files.ContainsKey(path))
            files.Add(path, fileContent);
        else
            files[path] = fileContent;
    }


    public void Remove(string path)
    {
        if (this.files.ContainsKey(path))
            this.files.Remove(path);
    }

    public byte[] Get(string path)
    {
        if (this.files.ContainsKey(path))
            return this.files[path];
        return null;
    }

    public void Clear()
    {
        this.files.Clear();
    }


}