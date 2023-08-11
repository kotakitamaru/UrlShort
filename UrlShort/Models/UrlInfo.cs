namespace UrlShort.Models;

public class UrlInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public uint Number { get; set; }
    public string FullUrl { get; set; }
    public string ShortUrl { get; set; }
    public string Author { get; set; }
    public string CreatedDate { get; set; } = DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString();
}