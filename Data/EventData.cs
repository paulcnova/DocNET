
namespace DocNET.Inspections;

using DocNET.Utilities;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

/// <summary>All the information relevant to events</summary>
public class EventData : BaseData
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
	public List<AttributeData> Attributes { get; set; } = new List<AttributeData>();
	
	/// <summary>Gets and sets the information of the event's type</summary>
	public QuickTypeData TypeInfo { get; set; }
	
	/// <summary>Gets and sets the type the event is implemented in</summary>
	public QuickTypeData ImplementedType { get; set; }
	
	/// <summary>Gets and sets the information of the event's adding method</summary>
	public MethodData Adder { get; set; }
	
	/// <summary>Gets and sets the information of the event's removing method</summary>
	public MethodData Remover { get; set; }
	
	/// <summary>Gets and sets the declaration of the event as it would be found in the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>Set to true to delete the event when looking to be removed</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	/// <summary>Generates an event information from the given event definition</summary>
	/// <param name="ev">The event definition to gather information from</param>
	public EventData(EventDefinition ev, bool ignorePrivate = true)
	{
		this.Name = ev.Name;
		this.TypeInfo = new QuickTypeData(ev.EventType);
		this.ImplementedType = new QuickTypeData(ev.DeclaringType);
		this.Adder = new MethodData(ev.AddMethod);
		this.Remover = new MethodData(ev.RemoveMethod);
		
		if(ignorePrivate && Utility.GetAccessorId(this.Adder.Accessor, ignorePrivate) == 0)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.Accessor = this.Adder.Accessor;
		this.Modifier = this.Adder.Modifier;
		this.IsStatic = this.Adder.IsStatic;
		this.Attributes = AttributeData.CreateArray(ev.CustomAttributes);
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
	public static List<EventData> CreateArray(TypeDefinition type, bool recursive, bool isStatic, bool ignorePrivate = true)
	{
		if(!recursive)
		{
			List<EventData> results = CreateArray(type.Events, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, true);
			
			return results;
		}
		
		List<EventData> events = new List<EventData>();
		List<EventData> temp;
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
	public static List<EventData> CreateArray(Collection<EventDefinition> events, bool ignorePrivate = true)
	{
		List<EventData> results = new List<EventData>();
		
		foreach(EventDefinition ev in events)
		{
			EventData info = new EventData(ev, ignorePrivate);
			
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
	public static void RemoveUnwanted(List<EventData> events, bool isStatic, bool isOriginal)
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
	public static void RemoveDuplicates(List<EventData> events, List<EventData> listEvents)
	{
		for(int i = events.Count - 1; i >= 0; i--)
		{
			foreach(EventData ev in listEvents)
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
