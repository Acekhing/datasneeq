namespace DataSneeq.Application.Transformations;

public class ListPickFirstConfig
{
    public static readonly string[] DefaultDelimiters = { ",", ";", "|", "\n", "\r\n" };
    public List<string> Delimiters { get; set; } = new(DefaultDelimiters);
}
