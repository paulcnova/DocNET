
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;

public class TypeInspection : BaseInspection
{
	#region Properties
	
	/// <summary>The quick look at the information of the type (including name, namespace, generic parameters)</summary>
	public QuickTypeInspection Info { get; set; }
	
	/// <summary>The name of the assembly where the type is found in</summary>
	public string AssemblyName { get; set; }
	
	/// <summary>Set to true if the type is a delegate declaration</summary>
	public bool IsDelegate { get; set; }
	
	/// <summary>Set to true if the type is a nested type</summary>
	public bool IsNested { get; set; }
	
	/// <summary>Set to true if the type is static and cannot have any instances only static members</summary>
	public bool IsStatic { get; set; }
	
	/// <summary>Set to true if the type is abstract and needs to be inherited to be used as an instance</summary>
	public bool IsAbstract { get; set; }
	
	/// <summary>Set to true if the type is sealed and cannot be inherited from</summary>
	public bool IsSealed { get; set; }
	
	/// <summary>The accessor of the type (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Any modifiers that the type contains (such as static, sealed, abstract, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>The object type of the type (such as class, struct, enum, or interface)</summary>
	public string ObjectType { get; set; }
	
	/// <summary>Set to true if the type is nested and has a parent type</summary>
	public bool HasDeclaringType { get; set; }
	
	/// <summary>Gets the parent type in which this type is nested under. If it is not a nested type, then it will be null. Check hasDeclaringType to see if it exists to begin with</summary>
	public QuickTypeInspection DeclaringType { get; set; }
	
	/// <summary>The partial declaration of the class within the inheritance declaration that can be found within the code</summary>
	public string Declaration { get; set; }
	
	/// <summary>The full declaration of the type as it would be found within the code</summary>
	public string FullDeclaration { get; set; }
	
	/// <summary>The information of the base type that the type inherits</summary>
	public QuickTypeInspection BaseType { get; set; }
	
	/// <summary>The array of attributes that the type contains</summary>
	public List<AttributeInspection> Attributes { get; private set; } = new List<AttributeInspection>();
	
	/// <summary>The array of type information of interfaces that the type implements</summary>
	public List<QuickTypeInspection> Interfaces { get; private set; } = new List<QuickTypeInspection>();
	
	public List<FieldInspection> Fields { get; private set; } = new List<FieldInspection>();
	public List<FieldInspection> StaticFields { get; private set; } = new List<FieldInspection>();
	public List<PropertyInspection> Properties { get; private set; } = new List<PropertyInspection>();
	public List<PropertyInspection> StaticProperties { get; private set; } = new List<PropertyInspection>();
	public List<EventInspection> Events { get; private set; } = new List<EventInspection>();
	public List<EventInspection> StaticEvents { get; private set; } = new List<EventInspection>();
	public List<MethodInspection> Constructors { get; private set; } = new List<MethodInspection>();
	public List<MethodInspection> Methods { get; private set; } = new List<MethodInspection>();
	public List<MethodInspection> StaticMethods { get; private set; } = new List<MethodInspection>();
	public List<MethodInspection> Operators { get; private set; } = new List<MethodInspection>();
	
	/// <summary>Gets if the type should be ignored since it is private</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	public TypeInspection(string typeName, SiteMap map, ProjectEnvironment environment) : this(
		map.GetAssemblyDefinition(typeName),
		map.TypeDefinitions[typeName],
		environment.Assemblies.ToArray(),
		!environment.IncludePrivate
	) {}
	
	public TypeInspection(AssemblyDefinition asm, TypeDefinition type, string[] assemblies, bool ignorePrivate = true)
	{
		if(type.IsPublic || type.IsNestedPublic) { this.Accessor = "public"; }
		else if(type.IsNestedAssembly) { this.Accessor = "internal"; }
		else if(type.IsNestedFamily) { this.Accessor = "protected"; }
		else if(type.IsNestedPrivate) { this.Accessor = "private"; }
		else { this.Accessor = "internal"; }
		
		if(ignorePrivate && InspectorUtility.GetAccessorId(this.Accessor, ignorePrivate) == 0)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.Info = new QuickTypeInspection(type);
		this.HasDeclaringType = type.DeclaringType != null;
		this.DeclaringType = this.HasDeclaringType ? new QuickTypeInspection(type.DeclaringType) : null;
		this.AssemblyName = asm.Name.Name;
		if(type.BaseType != null)
		{
			switch(type.BaseType.FullName)
			{
				case "System.Enum":
				case "System.ValueType":
				case "System.Object":
					this.BaseType = new QuickTypeInspection();
					break;
				default:
					this.BaseType = new QuickTypeInspection(type.BaseType);
					break;
			}
		}
		else
		{
			this.BaseType = new QuickTypeInspection();
		}
		this.IsDelegate = this.BaseType != null && this.BaseType.FullName == "System.MulticastDelegate";
		this.IsNested = type.IsNested;
		
		// ObjectType
		if(this.IsDelegate) { this.ObjectType = "delegate"; }
		else if(type.IsEnum) { this.ObjectType = "enum"; }
		else if(type.IsValueType) { this.ObjectType = "struct"; }
		else if(type.IsInterface) { this.ObjectType = "interface"; }
		else { this.ObjectType = "class"; }
		
		// Modifier
		if(this.IsDelegate || type.IsValueType || type.IsInterface) { this.Modifier = ""; }
		else if(type.IsSealed && type.IsAbstract) { this.Modifier = "static"; }
		else
		{
			this.Modifier = type.IsSealed
				? "sealed"
				: type.IsAbstract
					? "abstract"
					: "";
		}
		this.IsStatic = this.Modifier == "static";
		this.IsAbstract = this.Modifier == "abstract";
		this.IsSealed = this.Modifier == "sealed";
		this.Attributes = AttributeInspection.CreateArray(type.CustomAttributes);
		this.Interfaces = this.GetInterfaceData(type.Interfaces, assemblies, ignorePrivate);
		this.Declaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{this.ObjectType} {(
			this.IsDelegate
				? $"{this.GetDelegateReturnType(type)} "
				: ""
		)}{this.Info.Name}";
		this.FullDeclaration = this.GetFullDeclaration(type);
		
		// TODO: Have an option to not be recursive.
		this.Fields = FieldInspection.CreateArray(type, true, false, ignorePrivate);
		this.StaticFields = FieldInspection.CreateArray(type, true, true, ignorePrivate);
		
		this.Properties = PropertyInspection.CreateArray(type, true, false, ignorePrivate);
		this.StaticProperties = PropertyInspection.CreateArray(type, true, true, ignorePrivate);
		
		this.Events = EventInspection.CreateArray(type, true, false, ignorePrivate);
		this.StaticEvents = EventInspection.CreateArray(type, true, true, ignorePrivate);
		
		this.Constructors = MethodInspection.CreateArray(type, false, false, true, ignorePrivate: ignorePrivate);
		this.Methods = MethodInspection.CreateArray(type, true, false, ignorePrivate: ignorePrivate);
		this.StaticMethods = MethodInspection.CreateArray(type, true, true, ignorePrivate: ignorePrivate);
		this.Operators = MethodInspection.CreateArray(type, true, true, false, true, ignorePrivate: ignorePrivate);
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public override string GetXmlNameID() => $"T:{this.Info.UnlocalizedName}";
	
	public static TypeDefinition SearchDefinition(string typePath, string[] assemblies, bool ignorePrivate = true)
	{
		foreach(string assembly in assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				TypeDefinition type = module.GetType(typePath);
				
				if(type != null)
				{
					return type;
				}
			}
		}
		try
		{
			System.Type sysType = System.Type.GetType(typePath, true);
			AssemblyDefinition _asm = AssemblyDefinition.ReadAssembly(
				sysType.Assembly.Location.Replace("file:///", "")
			);
			
			foreach(ModuleDefinition _module in _asm.Modules)
			{
				TypeDefinition _type = _module.GetType(typePath);
				
				if(_type != null)
				{
					return _type;
				}
			}
		}
		catch
		{
			foreach(string assembly in assemblies)
			{
				AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
				
				foreach(ModuleDefinition module in asm.Modules)
				{
					foreach(TypeDefinition type in module.GetTypes())
					{
						string strType = type.FullName.Replace("/", ".");
						
						if(typePath == strType)
						{
							return type;
						}
					}
				}
			}
		}
		
		return null;
	}
	
	public static TypeInspection Search(string typePath, string[] assemblies, bool ignorePrivate = true)
	{
		foreach(string assembly in assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				TypeDefinition type = module.GetType(typePath);
				
				if(type != null)
				{
					return new TypeInspection(asm, type, assemblies, ignorePrivate);
				}
			}
		}
		try
		{
			System.Type sysType = System.Type.GetType(typePath, true);
			AssemblyDefinition _asm = AssemblyDefinition.ReadAssembly(
				sysType.Assembly.Location.Replace("file:///", "")
			);
			
			foreach(ModuleDefinition _module in _asm.Modules)
			{
				TypeDefinition _type = _module.GetType(typePath);
				
				if(_type != null)
				{
					return new TypeInspection(_asm, _type, assemblies, ignorePrivate);
				}
			}
		}
		catch
		{
			foreach(string assembly in assemblies)
			{
				AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
				
				foreach(ModuleDefinition module in asm.Modules)
				{
					foreach(TypeDefinition type in module.GetTypes())
					{
						string strType = type.FullName.Replace("/", ".");
						
						if(typePath == strType)
						{
							return new TypeInspection(asm, type, assemblies, ignorePrivate);
						}
					}
				}
			}
		}
		
		return null;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private MethodInspection FindInvokeMethod(TypeDefinition type)
	{
		foreach(MethodDefinition method in type.Methods)
		{
			if(method.Name == "Invoke")
			{
				return new MethodInspection(method, false);
			}
		}
		return null;
	}
	
	/// <summary>Gets the full declaration of the type</summary>
	/// <param name="type">The type definition to look into</param>
	/// <returns>Returns the full declaration of the type</returns>
	private string GetFullDeclaration(TypeDefinition type)
	{
		if(this.IsDelegate)
		{
			MethodInspection invoke = this.FindInvokeMethod(type);
			
			if(invoke != null)
			{
				return $"{this.Declaration}({invoke.ParameterDeclaration}){InspectorUtility.GetGenericParameterConstraints(this.Info.GenericParameters)}";
			}
		}
		
		bool hasInheritance = !string.IsNullOrEmpty(this.BaseType.FullName) || this.Interfaces.Count > 0;
		string decl;
		
		if(this.Info.GenericParameters.Count > 0)
		{
			decl = $"{this.Declaration}<{string.Join(", ", this.Info.GenericParameters.ConvertAll(gp => gp.UnlocalizedName))}>";
		}
		else
		{
			decl = this.Declaration;
		}
		decl += hasInheritance ? " : " : "";
		
		if(this.BaseType.FullName != "")
		{
			decl += $"{this.BaseType.FullName}{(this.Interfaces.Count > 0 ? ", " : "")}";
		}
		if(this.Interfaces.Count > 0)
		{
			for(int i = 0; i < this.Interfaces.Count; ++i)
			{
				decl += $"{this.Interfaces[i].Name}{(i != this.Interfaces.Count - 1 ? ", " : "")}";
			}
			decl += InspectorUtility.GetGenericParameterConstraints(this.Info.GenericParameters);
		}
		
		return decl;
	}
	
	private string GetDelegateReturnType(TypeDefinition type)
	{
		MethodInspection invoke = this.FindInvokeMethod(type);
		
		if(invoke == null) { return ""; }
		
		return invoke.ReturnType.Name;
	}
	
	/// <summary>Generates an array of interface informations</summary>
	/// <param name="interfaces">The collection of interface implementations</param>
	/// <param name="assemblies">The assemblies to get the interface data from.</param>
	/// <param name="ignorePrivate">Set to false to include all the private properties</param>
	/// <returns>Returns an array of interface informations</returns>
	private List<QuickTypeInspection> GetInterfaceData(Collection<InterfaceImplementation> interfaces, string[] assemblies, bool ignorePrivate = true)
	{
		List<QuickTypeInspection> results = new List<QuickTypeInspection>();
		
		foreach(InterfaceImplementation iFace in interfaces)
		{
			QuickTypeInspection info = new QuickTypeInspection(iFace.InterfaceType);
			
			if(ignorePrivate && !this.IsTypePublic(info.UnlocalizedName, assemblies))
			{
				continue;
			}
			results.Add(info);
		}
		
		return results;
	}
	
	/// <summary>Finds if the type is a public type</summary>
	/// <param name="typePath">The type path to look into</param>
	/// <param name="assemblies">The list of assemblies to look into</param>
	/// <returns>Returns true if the type is public</returns>
	private bool IsTypePublic(string typePath, string[] assemblies)
	{
		foreach(string assembly in assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				TypeDefinition type = module.GetType(typePath);
				
				if(type != null)
				{
					return type.IsPublic;
				}
			}
		}
		try
		{
			System.Type sysType = System.Type.GetType(typePath, true);
			AssemblyDefinition _asm = AssemblyDefinition.ReadAssembly(
				sysType.Assembly.Location.Replace("file:///", "")
			);
			
			foreach(ModuleDefinition _module in _asm.Modules)
			{
				TypeDefinition _type = _module.GetType(typePath);
				
				if(_type != null)
				{
					return _type.IsPublic;
				}
			}
		}
		catch(System.Exception e)
		{
			System.Console.WriteLine(e);
		}
		
		return false;
	}
	
	#endregion // Private Methods
}
