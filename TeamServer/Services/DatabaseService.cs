using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeamServer.Database;

namespace TeamServer.Service;
public interface IDatabaseService
{
    public Task<List<T>> Load<T>() where T : TeamServerDAO, new();

    public Task Insert<T>(T item) where T : TeamServerDAO, new();
}

public class DatabaseService : IDatabaseService
{
    private readonly SQLiteConnection _connection;
    private readonly SQLiteAsyncConnection _asyncConnection;

    public DatabaseService()
    {
        var directory = Directory.GetCurrentDirectory();

        var path = Path.Combine(directory, "data.db");

        using (var conn = new SQLiteConnection(path))
        {
            conn.CreateTable<HttpListenerDAO>();
            //conn.CreateTable<HttpHandlerDao>();
            //conn.CreateTable<SmbHandlerDao>();
            //conn.CreateTable<TcpHandlerDao>();
            //conn.CreateTable<ExtHandlerDao>();
            //conn.CreateTable<WebLogDao>();
            //conn.CreateTable<HostedFileDao>();
            //conn.CreateTable<CryptoDao>();
            //conn.CreateTable<DroneDao>();
            //conn.CreateTable<TaskRecordDao>();
            //conn.CreateTable<ReversePortForwardDao>();
            //conn.CreateTable<SocksDao>();
            //conn.CreateTable<SlackWebhookDao>();
            //conn.CreateTable<CustomWebhookDao>();
        }

        // open connections
        _connection = new SQLiteConnection(path);
        _asyncConnection = new SQLiteAsyncConnection(path);
    }

    //public SQLiteConnection GetConnection()
    //    => _connection;

    //public SQLiteAsyncConnection GetAsyncConnection()
    //    => _asyncConnection;

    async Task<List<T>> IDatabaseService.Load<T>()
    {
        var list = await this._asyncConnection.Table<T>().ToListAsync();
        return list;
    }

    async Task IDatabaseService.Insert<T>(T item)
    {
        await this._asyncConnection.InsertAsync(item);
    }
}