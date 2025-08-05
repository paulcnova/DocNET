
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to events</summary>
public class EventInspection : BaseInspection
{
	#region Properties
	
	/// <summary>Gets and sets the name of the event</summary>
	public string Name { get; set; }
	
	/// <summary>Gets and sets if the event is static</summary>
	public bool IsStatic { get; set; }
	
	/// <summary>Gets and sets the accessor of the event (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Gets and sets any modifiers of the event (such as static, virtual, override, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>Gets and sets the attributes associated with the event</summary>
	public List<AttributeInspection> Attributes { get; set; } = new List<AttributeInspection>();
	
	/// <summary>Gets and sets the information of the event's type</summary>
	public QuickTypeInspection TypeInfo { get; set; }
	
	/// <summary>Gets and sets the type the event is implemented in</summary>
	public QuickTypeInspection ImplementedType { get; set; }
	
	/// <summary>Gets and sets the information of the event's adding method</summary>
	public MethodInspection Adder { get; set; }
	
	/// <summary>Gets and sets the information of the event's removing method</summary>
	public MethodInspection Remover { get; set; }
	
	/// <summary>Gets and sets the declaration of the event as it would be found in the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>Set to true to delete the event when looking to be removed</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	/// <summary>Generates an event information from the given event definition</summary>
	/// <param name="ev">The event definition to gather information from</param>
	public EventInspection(EventDefinition ev, bool ignorePrivate = true)
	{
		this.Name = ev.Name;
		this.TypeInfo = new QuickTypeInspection(ev.EventType);
		this.ImplementedType = new QuickTypeInspection(ev.DeclaringType);
		this.Adder = new MethodInspection(ev.AddMethod);
		this.Remover = new MethodInspection(ev.RemoveMethod);
		
		if(ignorePrivate && InspectorUtility.GetAccessorId(this.Adder.Accessor, ignorePrivate) == 0)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.Accessor = this.Adder.Accessor;
		this.Modifier = this.Adder.Modifier;
		this.IsStatic = this.Adder.IsStatic;
		this.Attributes = AttributeInspection.CreateArray(ev.CustomAttributes);
		this.FullDeclaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{this.TypeInfo.Name} {this.Name}";
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of event informations from the given type and booleans</summary>
	/// <param name="type">The type to look into</param>
	/// <param name="recursive">Set to true to recursively look into the base type of the type</param>
	/// <param name="isStatic">Set to true to look for only static members</param>
	/// <returns>Returns the array of event informations</returns>
	public static List<EventInspection> CreateArray(TypeDefinition type, bool recursive, bool isStatic, bool ignorePrivate = true)
	{
		if(!recursive)
		{
			List<EventInspection> results = CreateArray(type.Events, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, true);
			
			return results;
		}
		
		List<EventInspection> events = new List<EventInspection>();
		List<EventInspection> temp;
		TypeDefinition currType = type;
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			temp = CreateArray(currType.Events, ignorePrivate);
			RemoveUnwanted(temp, isStatic, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(temp, events);
			}
			events.AddRange(temp);
			baseType = currType.BaseType;
			
			if(baseType == null) { break; }
			
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return events;
	}
	
	/// <summary>Generates an array of event informations from the given collection of event definitions</summary>
	/// <param name="events">The collection of event definitions</param>
	/// <returns>Returns an array of event informations generated</returns>
	public static List<EventInspection> CreateArray(Collection<EventDefinition> events, bool ignorePrivate = true)
	{
		List<EventInspection> results = new List<EventInspection>();
		
		foreach(EventDefinition ev in events)
		{
			EventInspection info = new EventInspection(ev, ignorePrivate);
			
			if(info.ShouldIgnore) { continue; }
			results.Add(info);
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Removes any unwanted elements from the array of event informations</summary>
	/// <param name="events">The array of event informations to remove from</param>
	/// <param name="isStatic">Set to true if non-static members should be removed</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	public static void RemoveUnwanted(List<EventInspection> events, bool isStatic, bool isOriginal)
	{
		for(int i = events.Count - 1; i >= 0; i--)
		{
			if(events[i].ShouldIgnore)
			{
				events.RemoveAt(i);
			}
			else if(events[i].IsStatic != isStatic)
			{
				events.RemoveAt(i);
			}
			else if(!isOriginal && events[i].Accessor == "private")
			{
				events.RemoveAt(i);
			}
		}
	}
	
	/// <summary>Removes the duplicates from the given array of events</summary>
	/// <param name="events">The array of event informations that will be removed from</param>
	/// <param name="listEvents">The list of recursive-ordered event informations to determine if there is any duplicates</param>
	public static void RemoveDuplicates(List<EventInspection> events, List<EventInspection> listEvents)
	{
		for(int i = events.Count - 1; i >= 0; i--)
		{
			foreach(EventInspection ev in listEvents)
			{
				if(events[i].Name == ev.Name)
				{
					events.RemoveAt(i);
					break;
				}
			}
		}
	}
	
	#endregion // Private Methods
}
