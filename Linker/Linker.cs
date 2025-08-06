
namespace DocNET.Linking;

using DocNET.Generators;
using DocNET.Information;
using DocNET.Inspections;

using System.Collections;
using System.Collections.Generic;

public sealed class Linker : IEnumerable<LinkedMember>
{
	#region Properties
	
	public ProjectEnvironment Environment { get; private set; }
	public InformationDocument Document { get; private set; }
	public SiteMap SiteMap { get; private set; }
	public TypeInspection Inspection { get; private set; }
	public GeneratedDocumentation Documentation { get; private set; }
	
	/// <summary>A constructor that links together the type's inspection as well as it's information</summary>
	/// <param name="inspection">The actual type's inspection (source code).</param>
	/// <param name="document">The documentation tied to the type</param>
	/// <param name="siteMap">The map of types and assemblies.</param>
	/// <param name="environment">The environment meant to keep data types and inspections consistent</param>
	public Linker(TypeInspection inspection, InformationDocument document, SiteMap siteMap, ProjectEnvironment environment)
	{
		this.Environment = environment;
		this.Document = document;
		this.SiteMap = siteMap;
		this.Inspection = inspection;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public void RenderAndSave()
	{
		
		// TODO: Render the tree.
		// TODO: Save the rendered object.
		this.Documentation.Save(this.Environment);
		// throw new System.NotImplementedException();
	}
	
	public void Push(GeneratedDocumentation documentation)
	{
		// TODO: Push the documentation into a tree like structure.
		this.Documentation = documentation;
		// throw new System.NotImplementedException();
	}
	
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	
	public IEnumerator<LinkedMember> GetEnumerator()
	{
		yield return new LinkedMember()
		{
			Document = this.Document,
			SiteMap = this.SiteMap,
			Info = this.Document.Find(this.Inspection),
			MemberType = LinkedMember.Type.Type,
			TypeInspection = this.Inspection,
		};
		
		foreach(MethodInspection method in this.Inspection.Constructors)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(method),
				MemberType = LinkedMember.Type.Method,
				MethodInspection = method,
			};
		}
		
		foreach(FieldInspection field in this.Inspection.Fields)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(field),
				MemberType = LinkedMember.Type.Field,
				FieldInspection = field,
			};
		}
		
		foreach(FieldInspection field in this.Inspection.StaticFields)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(field),
				MemberType = LinkedMember.Type.Field,
				IsStatic = true,
				FieldInspection = field,
			};
		}
		
		foreach(PropertyInspection property in this.Inspection.Properties)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(property),
				MemberType = LinkedMember.Type.Property,
				PropertyInspection = property,
			};
		}
		
		foreach(PropertyInspection property in this.Inspection.StaticProperties)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(property),
				MemberType = LinkedMember.Type.Property,
				IsStatic = true,
				PropertyInspection = property,
			};
		}
		
		foreach(EventInspection @event in this.Inspection.Events)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(@event),
				MemberType = LinkedMember.Type.Event,
				EventInspection = @event,
			};
		}
		
		foreach(EventInspection @event in this.Inspection.StaticEvents)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(@event),
				MemberType = LinkedMember.Type.Event,
				EventInspection = @event,
			};
		}
		
		foreach(MethodInspection method in this.Inspection.Methods)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(method),
				MemberType = LinkedMember.Type.Method,
				MethodInspection = method,
			};
		}
		
		foreach(MethodInspection method in this.Inspection.StaticMethods)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(method),
				MemberType = LinkedMember.Type.Method,
				IsStatic = true,
				MethodInspection = method,
			};
		}
		
		foreach(MethodInspection method in this.Inspection.Operators)
		{
			yield return new LinkedMember()
			{
				Document = this.Document,
				SiteMap = this.SiteMap,
				Info = this.Document.Find(method),
				MemberType = LinkedMember.Type.Method,
				IsStatic = true,
				MethodInspection = method,
			};
		}
	}
	
	#endregion // Public Methods
}
