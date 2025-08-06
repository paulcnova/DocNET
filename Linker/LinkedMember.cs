
namespace DocNET.Linking;

using DocNET.Information;
using DocNET.Inspections;

public sealed class LinkedMember
{
	#region Properties
	
	public required InformationElement Info { get; set; }
	public required Type MemberType { get; set; }
	public bool IsStatic { get; set; } = false;
	public TypeInspection TypeInspection { get; set; }
	public FieldInspection FieldInspection { get; set; }
	public PropertyInspection PropertyInspection { get; set; }
	public EventInspection EventInspection { get; set; }
	public MethodInspection MethodInspection { get; set; }
	
	#endregion // Properties
	
	#region Types
	
	public enum Type
	{
		Type,
		Field,
		Property,
		Event,
		Method,
	}
	
	#endregion // Types
}
