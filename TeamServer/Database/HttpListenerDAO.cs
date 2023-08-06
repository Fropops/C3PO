using Common.Payload;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("http_handlers")]
public sealed class HttpListenerDAO : TeamServerDAO
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("port")]
    public int BindPort { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("secure")]
    public bool Secure { get; set; }

    public static implicit operator HttpListenerDAO(HttpListener handler)
    {
        return new HttpListenerDAO
        {
            Id = handler.Id,
            Name = handler.Name,
            BindPort = handler.BindPort,
            Address = handler.Ip,
            Secure = handler.Secured,
        };
    }

    public static implicit operator HttpListener(HttpListenerDAO dao)
    {
        return new HttpListener(dao.Id, dao.Name, dao.BindPort, dao.Address, dao.Secure);
    }
}
