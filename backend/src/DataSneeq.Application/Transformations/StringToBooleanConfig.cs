namespace DataSneeq.Application.Transformations;

public class StringToBooleanConfig
{
    public List<StringBooleanMapping> Mappings { get; set; } = new();
    public bool DefaultValue { get; set; }
    public bool UseDefaultWhenNoMatch { get; set; } = true;
}

public class StringBooleanMapping
{
    public string ExcelValue { get; set; } = string.Empty;
    public bool BooleanValue { get; set; }
}
