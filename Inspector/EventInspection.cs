
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
	public AttributeInspection[] Attributes { get; set; }
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
	internal bool ShouldDelete = false;
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of event informations from the given type and booleans</summary>
	/// <param name="type">The type to look into</param>
	/// <param name="recursive">Set to true to recursively look into the base type of the type</param>
	/// <param name="isStatic">Set to true to look for only static members</param>
	/// <returns>Returns the array of event informations</returns>
	public static EventInspection[] GenerateInfoArray(TypeDefinition type, bool recursive, bool isStatic)
	{
		if(!recursive)
		{
			EventInspection[] results = GenerateInfoArray(type.Events);
			
			RemoveUnwanted(ref results, isStatic, true);
			
			return results;
		}
		
		List<EventInspection> events = new List<EventInspection>();
		EventInspection[] temp;
		TypeDefinition currType = type;
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			temp = GenerateInfoArray(currType.Events);
			RemoveUnwanted(ref temp, isStatic, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(ref temp, events);
			}
			events.AddRange(temp);
			baseType = currType.BaseType;
			
			if(baseType == null) { break; }
			
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return events.ToArray();
	}
	
	/// <summary>Generates an array of event informations from the given collection of event definitions</summary>
	/// <param name="events">The collection of event definitions</param>
	/// <returns>Returns an array of event informations generated</returns>
	public static EventInspection[] GenerateInfoArray(Collection<EventDefinition> events)
	{
		List<EventInspection> results = new List<EventInspection>();
		EventInspection info;
		
		foreach(EventDefinition ev in events)
		{
			info = GenerateInfo(ev);
			if(info.ShouldDelete)
			{
				continue;
			}
			results.Add(info);
		}
		
		return results.ToArray();
	}
	
	/// <summary>Generates an event information from the given event definition</summary>
	/// <param name="ev">The event definition to gather information from</param>
	/// <returns>Returns the event information generated</returns>
	public static EventInspection GenerateInfo(EventDefinition ev)
	{
		EventInspection info = new EventInspection();
		
		info.Name = ev.Name;
		info.TypeInfo = QuickTypeInspection.GenerateInfo(ev.EventType);
		info.ImplementedType = QuickTypeInspection.GenerateInfo(ev.DeclaringType);
		info.Adder = MethodInspection.GenerateInfo(ev.AddMethod);
		info.Remover = MethodInspection.GenerateInfo(ev.RemoveMethod);
		info.Accessor = info.Adder.Accessor;
		info.Modifier = info.Adder.Modifier;
		info.IsStatic = info.Adder.IsStatic;
		info.Attributes = AttributeInspection.GenerateInfoArray(ev.CustomAttributes);
		info.FullDeclaration = (
			info.Accessor + " " +
			(info.Modifier != "" ? info.Modifier + " " : "") +
			info.TypeInfo.Name + " " +
			info.Name
		);
		
		if(Inspections.TypeInspection.ignorePrivate && PropertyInspection.GetAccessorId(info.Accessor) == 0)
		{
			info.ShouldDelete = true;
		}
		
		return info;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Removes any unwanted elements from the array of event informations</summary>
	/// <param name="temp">The array of event informations to remove from</param>
	/// <param name="isStatic">Set to true if non-static members should be removed</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	public static void RemoveUnwanted(ref EventInspection[] temp, bool isStatic, bool isOriginal)
	{
		List<EventInspection> events = new List<EventInspection>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
		{
			if(events[i].ShouldDelete)
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
		
		temp = events.ToArray();
	}
	
	/// <summary>Removes the duplicates from the given array of events</summary>
	/// <param name="temp">The array of event informations that will be removed from</param>
	/// <param name="listEvents">The list of recursive-ordered event informations to determine if there is any duplicates</param>
	public static void RemoveDuplicates(ref EventInspection[] temp, List<EventInspection> listEvents)
	{
		List<EventInspection> events = new List<EventInspection>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
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
		
		temp = events.ToArray();
	}
	
	#endregion // Private Methods
}
