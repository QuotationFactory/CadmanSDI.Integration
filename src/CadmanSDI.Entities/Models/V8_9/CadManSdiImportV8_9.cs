using System.Xml.Serialization;

namespace CadmanSDI.Entities.Models._8_9;

[XmlRoot(ElementName="CADMAN")]
public class CadManSdiImportV8_9 : ICadmanSdiImport {
    [XmlElement(ElementName="PARTTYPEIMPORT")]
    public PartTypeImport PartTypeImport { get; set; }

    [XmlElement(ElementName="BATCH")]
    public Batch Batch { get; set; }
}

[XmlRoot(ElementName="PARTTYPEIMPORT")]
public class PartTypeImport {
    [XmlElement(ElementName="PARTTYPELIST")]
    public PartTypeList PartTypeList { get; set; }
}

[XmlRoot(ElementName="BATCH")]
public class Batch
{
    [XmlElement(ElementName = "COMMAND")]
    public List<Command> Commands { get; set; } = new();
}

[XmlRoot(ElementName="TAGS")]
public class Tags
{
    [XmlElement(ElementName = "TAG")]
    public List<Tag> TagsList { get; set; } = new();
}

[XmlRoot(ElementName="TAG")]
public class Tag {
    [XmlAttribute(AttributeName="Type")]
    public string Type { get; set; }

    [XmlText]
    public string Text { get; set; }
}

[XmlRoot(ElementName="DOCUMENTATION")]
public class Documentation {
    [XmlElement(ElementName="DOCUMENT")]
    public Document Document { get; set; }
}

[XmlRoot(ElementName="DOCUMENT")]
public class Document {
	[XmlElement(ElementName="CATEGORY")]
	public string Category { get; set; }

	[XmlElement(ElementName="NAME")]
	public string Name { get; set; }

	[XmlElement(ElementName="FILE")]
	public string File { get; set; }

	[XmlElement(ElementName="LINK")]
	public string Link { get; set; }
}

[XmlRoot(ElementName="PARTTYPELIST")]
public class PartTypeList
{
    [XmlElement(ElementName = "PARTTYPE")]
    public List<PartType> PartTypes { get; set; } = new();
}

[XmlRoot(ElementName="PARTTYPE")]
public class PartType {
    [XmlAttribute(AttributeName="ID")]
    public string Id { get; set; }

	[XmlElement(ElementName="MaterialID")]
	public string MaterialId { get; set; }

	[XmlElement(ElementName="THICKNESS")]
	public string Thickness { get; set; }

	[XmlElement(ElementName="UNIT")]
	public string Unit { get; set; }

	[XmlElement(ElementName="IMPORTFILENAME")]
	public string Importfilename { get; set; }

	[XmlElement(ElementName="TAGS")]
	public Tags Tags { get; set; }

	[XmlElement(ElementName="COMMENT")]
	public string Comment { get; set; }

	[XmlElement(ElementName="DESCRIPTION")]
	public string Description { get; set; }

	[XmlElement(ElementName="DOCUMENTATION")]
	public Documentation Documentation { get; set; }
}

[XmlRoot(ElementName="COMMAND")]
public class Command {
	[XmlArray(ElementName="PARAMETERS")]
    [XmlArrayItem(ElementName = "PARAMETER")]
	public Parameter[] Parameters { get; set; }

	[XmlAttribute(AttributeName="Command")]
	public string CommandString { get; set; }
}

public class Parameter {
    [XmlAttribute(AttributeName="ID")]
    public string Id { get; set; }

    [XmlAttribute(AttributeName="Value")]
    public string Value { get; set; }
}
