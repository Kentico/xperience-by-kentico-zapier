using System.Xml.Serialization;

namespace Kentico.Xperience.Zapier.Actions;

[XmlRoot("form")]
public class Form
{
    [XmlElement("field")]
    public List<Field>? Fields { get; set; }
}

public class Field
{
    [XmlAttribute("column")]
    public string? Column { get; set; }

    [XmlAttribute("columntype")]
    public string? ColumnType { get; set; }
}
