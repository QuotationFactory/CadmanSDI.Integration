using System.Xml.Serialization;

namespace CadmanSDI.Entities.Models.V89;

[XmlRoot(ElementName="OSMCONVERTORDER")]
public class OSMCONVERTORDER {
	[XmlElement(ElementName="PartID")]
	public string PartID { get; set; }
	[XmlElement(ElementName="MaterialID")]
	public string MaterialID { get; set; }
	[XmlElement(ElementName="Thickness")]
	public string Thickness { get; set; }
	[XmlElement(ElementName="Unit")]
	public string Unit { get; set; }
	[XmlElement(ElementName="OrigPath")]
	public string OrigPath { get; set; }
	[XmlElement(ElementName="Quantity")]
	public string Quantity { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
}

[XmlRoot(ElementName="OSMCONVERTRESULT")]
public class OSMCONVERTRESULT {
	[XmlElement(ElementName="PartTypeID")]
	public string PartTypeID { get; set; }
	[XmlElement(ElementName="PartTypeRev")]
	public string PartTypeRev { get; set; }
	[XmlElement(ElementName="PartTypeSubRev")]
	public string PartTypeSubRev { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
}

[XmlRoot(ElementName="JOBOSMCONVERT")]
public class JOBOSMCONVERT {
	[XmlElement(ElementName="JobStatus")]
	public string JobStatus { get; set; }
	[XmlElement(ElementName="JobStatusReason")]
	public string JobStatusReason { get; set; }
	[XmlElement(ElementName="OSMCONVERTORDER")]
	public OSMCONVERTORDER OSMCONVERTORDER { get; set; }
	[XmlElement(ElementName="OSMCONVERTRESULT")]
	public OSMCONVERTRESULT OSMCONVERTRESULT { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
	[XmlAttribute(AttributeName="OrderID")]
	public string OrderID { get; set; }
}

[XmlRoot(ElementName="JOBOSMCONVERTLIST")]
public class JOBOSMCONVERTLIST {
	[XmlElement(ElementName="JOBOSMCONVERT")]
	public JOBOSMCONVERT JOBOSMCONVERT { get; set; }
}

[XmlRoot(ElementName="ToolCombination")]
public class ToolCombination {
	[XmlElement(ElementName="PunchID")]
	public string PunchID { get; set; }
	[XmlElement(ElementName="PunchHolderID")]
	public string PunchHolderID { get; set; }
	[XmlElement(ElementName="DieID")]
	public string DieID { get; set; }
	[XmlElement(ElementName="DieHolderID")]
	public string DieHolderID { get; set; }
}

[XmlRoot(ElementName="ToolCombinationList")]
public class ToolCombinationList {
	[XmlElement(ElementName="ToolCombination")]
	public ToolCombination ToolCombination { get; set; }
}

[XmlRoot(ElementName="ToolCombinationGroup")]
public class ToolCombinationGroup {
	[XmlElement(ElementName="ToolCombinationList")]
	public ToolCombinationList ToolCombinationList { get; set; }
	[XmlElement(ElementName="NumberOfBends")]
	public string NumberOfBends { get; set; }
}

[XmlRoot(ElementName="PossibleToolCombinationGroupList")]
public class PossibleToolCombinationGroupList {
	[XmlElement(ElementName="ToolCombinationGroup")]
	public ToolCombinationGroup ToolCombinationGroup { get; set; }
}

[XmlRoot(ElementName="JOBPARTPREPARATION")]
public class JOBPARTPREPARATION {
	[XmlElement(ElementName="MachineTypeID")]
	public string MachineTypeID { get; set; }
	[XmlElement(ElementName="Method")]
	public string Method { get; set; }
	[XmlElement(ElementName="JobStatus")]
	public string JobStatus { get; set; }
	[XmlElement(ElementName="JobStatusReason")]
	public string JobStatusReason { get; set; }
	[XmlElement(ElementName="UnfoldingID")]
	public string UnfoldingID { get; set; }
	[XmlElement(ElementName="PossibleToolCombinationGroupList")]
	public PossibleToolCombinationGroupList PossibleToolCombinationGroupList { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
	[XmlAttribute(AttributeName="OrderID")]
	public string OrderID { get; set; }
	[XmlAttribute(AttributeName="PartID")]
	public string PartID { get; set; }
}

[XmlRoot(ElementName="JOBPARTPREPARATIONLIST")]
public class JOBPARTPREPARATIONLIST {
	[XmlElement(ElementName="JOBPARTPREPARATION")]
	public List<JOBPARTPREPARATION> JOBPARTPREPARATION { get; set; }
}

[XmlRoot(ElementName="TAG")]
public class TAG {
	[XmlAttribute(AttributeName="Type")]
	public string Type { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot(ElementName="TAGS")]
public class TAGS {
	[XmlElement(ElementName="TAG")]
	public List<TAG> TAG { get; set; } = new();
}

[XmlRoot(ElementName="PARTSHEETMETALMODEL")]
public class PARTSHEETMETALMODEL {
	[XmlElement(ElementName="Description")]
	public string Description { get; set; }
	[XmlElement(ElementName="Comment")]
	public string Comment { get; set; }
	[XmlElement(ElementName="TAGS")]
	public TAGS TAGS { get; set; }
	[XmlElement(ElementName="UnfoldedBoundHeight")]
	public string UnfoldedBoundHeight { get; set; }
	[XmlElement(ElementName="UnfoldedBoundWidth")]
	public string UnfoldedBoundWidth { get; set; }
	[XmlElement(ElementName="MinUnfoldedBoundHeight")]
	public string MinUnfoldedBoundHeight { get; set; }
	[XmlElement(ElementName="MinUnfoldedBoundWidth")]
	public string MinUnfoldedBoundWidth { get; set; }
	[XmlElement(ElementName="UnfoldedArea")]
	public string UnfoldedArea { get; set; }
	[XmlElement(ElementName="UnfoldedBrutoArea")]
	public string UnfoldedBrutoArea { get; set; }
	[XmlElement(ElementName="BoundingBox3DHeight")]
	public string BoundingBox3DHeight { get; set; }
	[XmlElement(ElementName="BoundingBox3DWidth")]
	public string BoundingBox3DWidth { get; set; }
	[XmlElement(ElementName="BoundingBox3DDepth")]
	public string BoundingBox3DDepth { get; set; }
	[XmlElement(ElementName="TotalContourLength")]
	public string TotalContourLength { get; set; }
	[XmlElement(ElementName="NumberOfHinges")]
	public string NumberOfHinges { get; set; }
	[XmlElement(ElementName="NumberOfContours")]
	public string NumberOfContours { get; set; }
	[XmlElement(ElementName="NumberOfBends")]
	public string NumberOfBends { get; set; }
	[XmlElement(ElementName="NumberOfBendSequences")]
	public string NumberOfBendSequences { get; set; }
	[XmlElement(ElementName="LengthLongestBend")]
	public string LengthLongestBend { get; set; }
	[XmlElement(ElementName="ForceLongestBend")]
	public string ForceLongestBend { get; set; }
	[XmlElement(ElementName="Mass")]
	public string Mass { get; set; }
	[XmlElement(ElementName="Folded3DImage_Base64")]
	public string Folded3DImage_Base64 { get; set; }
	[XmlElement(ElementName="Unfolded3DImage_Base64")]
	public string Unfolded3DImage_Base64 { get; set; }
	[XmlElement(ElementName="NumberOfSOEVEREINs")]
	public string NumberOfSOEVEREINs { get; set; }
	[XmlElement(ElementName="NumberOfRELIEFs")]
	public string NumberOfRELIEFs { get; set; }
	[XmlElement(ElementName="NumberOfUNKNOWNs")]
	public string NumberOfUNKNOWNs { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
	[XmlAttribute(AttributeName="Rev")]
	public string Rev { get; set; }
	[XmlAttribute(AttributeName="SubRev")]
	public string SubRev { get; set; }
}

[XmlRoot(ElementName="PARTSHEETMETALMODELLIST")]
public class PARTSHEETMETALMODELLIST {
	[XmlElement(ElementName="PARTSHEETMETALMODEL")]
	public PARTSHEETMETALMODEL PARTSHEETMETALMODEL { get; set; }
}

[XmlRoot(ElementName="FEATURE")]
public class FEATURE {
	[XmlElement(ElementName="Name")]
	public string Name { get; set; }
	[XmlElement(ElementName="FeatureTypeCode")]
	public string FeatureTypeCode { get; set; }
	[XmlElement(ElementName="FeatureTypeName")]
	public string FeatureTypeName { get; set; }
}

[XmlRoot(ElementName="FEATURELIST")]
public class FEATURELIST {
	[XmlElement(ElementName="FEATURE")]
	public List<FEATURE> FEATURE { get; set; }
}

[XmlRoot(ElementName="PARTSHEETMETALTYPE")]
public class PARTSHEETMETALTYPE {
	[XmlElement(ElementName="Unit")]
	public string Unit { get; set; }
	[XmlElement(ElementName="MaterialID")]
	public string MaterialID { get; set; }
	[XmlElement(ElementName="MaterialRev")]
	public string MaterialRev { get; set; }
	[XmlElement(ElementName="MaterialSubRev")]
	public string MaterialSubRev { get; set; }
	[XmlElement(ElementName="Thickness")]
	public string Thickness { get; set; }
	[XmlElement(ElementName="SPECIALSIDE")]
	public string SPECIALSIDE { get; set; }
	[XmlElement(ElementName="PARTSHEETMETALTYPECONFLIST")]
	public string PARTSHEETMETALTYPECONFLIST { get; set; }
	[XmlElement(ElementName="PARTSHEETMETALMODELLIST")]
	public PARTSHEETMETALMODELLIST PARTSHEETMETALMODELLIST { get; set; }
	[XmlElement(ElementName="FEATURELIST")]
	public FEATURELIST FEATURELIST { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="Kind")]
	public string Kind { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
	[XmlAttribute(AttributeName="Rev")]
	public string Rev { get; set; }
	[XmlAttribute(AttributeName="SubRev")]
	public string SubRev { get; set; }
}

[XmlRoot(ElementName="PARTTYPELIST")]
public class PARTTYPELIST {
	[XmlElement(ElementName="PARTSHEETMETALTYPE")]
	public PARTSHEETMETALTYPE PARTSHEETMETALTYPE { get; set; }
}

[XmlRoot(ElementName="SUMMARY")]
public class SUMMARY {
	[XmlElement(ElementName="MachineTypeID")]
	public string MachineTypeID { get; set; }
	[XmlElement(ElementName="Valid")]
	public string Valid { get; set; }
	[XmlElement(ElementName="TotalTime")]
	public string TotalTime { get; set; }
	[XmlElement(ElementName="Reason")]
	public string Reason { get; set; }
}

[XmlRoot(ElementName="JOBTYPE")]
public class JOBTYPE {
	[XmlElement(ElementName="SUMMARY")]
	public SUMMARY SUMMARY { get; set; }
	[XmlAttribute(AttributeName="JobTypeID")]
	public string JobTypeID { get; set; }
}

[XmlRoot(ElementName="JOBTYPELIST")]
public class JOBTYPELIST {
	[XmlElement(ElementName="JOBTYPE")]
	public List<JOBTYPE> JOBTYPE { get; set; }
}

[XmlRoot(ElementName="STEP")]
public class STEP {
	[XmlElement(ElementName="JOBTYPELIST")]
	public JOBTYPELIST JOBTYPELIST { get; set; }
	[XmlAttribute(AttributeName="Kind")]
	public string Kind { get; set; }
	[XmlAttribute(AttributeName="Valid")]
	public string Valid { get; set; }
}

[XmlRoot(ElementName="STEPLIST")]
public class STEPLIST {
	[XmlElement(ElementName="STEP")]
	public List<STEP> STEP { get; set; }
}

[XmlRoot(ElementName="PATH")]
public class PATH {
	[XmlElement(ElementName="STEPLIST")]
	public STEPLIST STEPLIST { get; set; }
	[XmlAttribute(AttributeName="Name")]
	public string Name { get; set; }
	[XmlAttribute(AttributeName="IsDefault")]
	public string IsDefault { get; set; }
	[XmlAttribute(AttributeName="PartTypeID")]
	public string PartTypeID { get; set; }
}

[XmlRoot(ElementName="PATHLIST")]
public class PATHLIST {
	[XmlElement(ElementName="PATH")]
	public PATH PATH { get; set; }
}

[XmlRoot(ElementName="EXECUTIONPLANPROPOSAL")]
public class EXECUTIONPLANPROPOSAL {
	[XmlElement(ElementName="PATHLIST")]
	public PATHLIST PATHLIST { get; set; }
	[XmlAttribute(AttributeName="OrderID")]
	public string OrderID { get; set; }
	[XmlAttribute(AttributeName="PartID")]
	public string PartID { get; set; }
	[XmlAttribute(AttributeName="Name")]
	public string Name { get; set; }
	[XmlAttribute(AttributeName="Valid")]
	public string Valid { get; set; }
}

[XmlRoot(ElementName="EXECUTIONPLANPROPOSALLIST")]
public class EXECUTIONPLANPROPOSALLIST {
	[XmlElement(ElementName="EXECUTIONPLANPROPOSAL")]
	public EXECUTIONPLANPROPOSAL EXECUTIONPLANPROPOSAL { get; set; }
}

[XmlRoot(ElementName="DateCreated")]
public class DateCreated {
	[XmlElement(ElementName="YEAR")]
	public string YEAR { get; set; }
	[XmlElement(ElementName="MONTH")]
	public string MONTH { get; set; }
	[XmlElement(ElementName="DAY")]
	public string DAY { get; set; }
	[XmlElement(ElementName="HOUR")]
	public string HOUR { get; set; }
	[XmlElement(ElementName="MIN")]
	public string MIN { get; set; }
	[XmlElement(ElementName="SEC")]
	public string SEC { get; set; }
	[XmlElement(ElementName="MSEC")]
	public string MSEC { get; set; }
	[XmlElement(ElementName="TZ")]
	public string TZ { get; set; }
}

[XmlRoot(ElementName="DateUpdated")]
public class DateUpdated {
	[XmlElement(ElementName="YEAR")]
	public string YEAR { get; set; }
	[XmlElement(ElementName="MONTH")]
	public string MONTH { get; set; }
	[XmlElement(ElementName="DAY")]
	public string DAY { get; set; }
	[XmlElement(ElementName="HOUR")]
	public string HOUR { get; set; }
	[XmlElement(ElementName="MIN")]
	public string MIN { get; set; }
	[XmlElement(ElementName="SEC")]
	public string SEC { get; set; }
	[XmlElement(ElementName="MSEC")]
	public string MSEC { get; set; }
	[XmlElement(ElementName="TZ")]
	public string TZ { get; set; }
}

[XmlRoot(ElementName="Comment")]
public class Comment {
	[XmlElement(ElementName="L")]
	public List<string> L { get; set; }
	[XmlAttribute(AttributeName="Type")]
	public string Type { get; set; }
}

[XmlRoot(ElementName="Color")]
public class Color {
	[XmlElement(ElementName="T")]
	public string T { get; set; }
	[XmlElement(ElementName="R")]
	public string R { get; set; }
	[XmlElement(ElementName="G")]
	public string G { get; set; }
	[XmlElement(ElementName="B")]
	public string B { get; set; }
}

[XmlRoot(ElementName="SideColor")]
public class SideColor {
	[XmlElement(ElementName="T")]
	public string T { get; set; }
	[XmlElement(ElementName="R")]
	public string R { get; set; }
	[XmlElement(ElementName="G")]
	public string G { get; set; }
	[XmlElement(ElementName="B")]
	public string B { get; set; }
}

[XmlRoot(ElementName="MATERIAL")]
public class MATERIAL {
	[XmlElement(ElementName="DateCreated")]
	public DateCreated DateCreated { get; set; }
	[XmlElement(ElementName="DateUpdated")]
	public DateUpdated DateUpdated { get; set; }
	[XmlElement(ElementName="Description")]
	public string Description { get; set; }
	[XmlElement(ElementName="Comment")]
	public Comment Comment { get; set; }
	[XmlElement(ElementName="Unit")]
	public string Unit { get; set; }
	[XmlElement(ElementName="Color")]
	public Color Color { get; set; }
	[XmlElement(ElementName="SideColor")]
	public SideColor SideColor { get; set; }
	[XmlElement(ElementName="Density")]
	public string Density { get; set; }
	[XmlElement(ElementName="YieldStrength")]
	public string YieldStrength { get; set; }
	[XmlElement(ElementName="TensileStrength")]
	public string TensileStrength { get; set; }
	[XmlElement(ElementName="MaxStrain")]
	public string MaxStrain { get; set; }
	[XmlElement(ElementName="RSRatio")]
	public string RSRatio { get; set; }
	[XmlElement(ElementName="Type")]
	public string Type { get; set; }
	[XmlElement(ElementName="MaterialNorm")]
	public string MaterialNorm { get; set; }
	[XmlElement(ElementName="MaterialNormRev")]
	public string MaterialNormRev { get; set; }
	[XmlElement(ElementName="MaterialNormSubRev")]
	public string MaterialNormSubRev { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ID")]
	public string ID { get; set; }
	[XmlAttribute(AttributeName="Rev")]
	public string Rev { get; set; }
	[XmlAttribute(AttributeName="SubRev")]
	public string SubRev { get; set; }
}

[XmlRoot(ElementName="MATERIALLIST")]
public class MATERIALLIST {
	[XmlElement(ElementName="MATERIAL")]
	public MATERIAL MATERIAL { get; set; }
}

[XmlRoot(ElementName="CADMAN")]
public class CadManSdiExportV89 : ICadmanSdiExport {
	[XmlElement(ElementName="JOBOSMCONVERTLIST")]
	public JOBOSMCONVERTLIST JOBOSMCONVERTLIST { get; set; }
	[XmlElement(ElementName="JOBPARTPREPARATIONLIST")]
	public JOBPARTPREPARATIONLIST JOBPARTPREPARATIONLIST { get; set; }
	[XmlElement(ElementName="PARTTYPELIST")]
	public PARTTYPELIST PARTTYPELIST { get; set; }
	[XmlElement(ElementName="EXECUTIONPLANPROPOSALLIST")]
	public EXECUTIONPLANPROPOSALLIST EXECUTIONPLANPROPOSALLIST { get; set; }
	[XmlElement(ElementName="MATERIALLIST")]
	public MATERIALLIST MATERIALLIST { get; set; }
	[XmlAttribute(AttributeName="Version")]
	public string Version { get; set; }
	[XmlAttribute(AttributeName="ExportApplication")]
	public string ExportApplication { get; set; }
	[XmlAttribute(AttributeName="ExportVersion")]
	public string ExportVersion { get; set; }
}
