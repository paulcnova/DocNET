
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>All the information relevant to types</summary>
public partial class TypeInspection
{
	#region Properties
	
	// The name of the assembly used
	internal static string assemblyUsed = "";
	// The array of assemblies that the user wanted to look into
	internal static string[] assembliesUsed;
	// Set to true to ignore all private members
	internal static bool ignorePrivate = true;
	
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
	/// <summary>
	/// Gets the parent type in which this type is nested under. If it is not a nested type,
	/// then it will be null. Check hasDeclaringType to see if it exists to begin with
	/// </summary>
	public QuickTypeInspection DeclaringType { get; set; }
	/// <summary>The partial declaration of the class within the inheritance declaration that can be found within the code</summary>
	public string Declaration { get; set; }
	/// <summary>The full declaration of the type as it would be found within the code</summary>
	public string FullDeclaration { get; set; }
	/// <summary>The information of the base type that the type inherits</summary>
	public QuickTypeInspection BaseType { get; set; }
	/// <summary>The array of attributes that the type contains</summary>
	public AttributeInspection[] Attributes { get; set; }
	/// <summary>The array of type information of interfaces that the type implements</summary>
	public QuickTypeInspection[] Interfaces { get; set; }
	/// <summary>The array of constructors that the type contains</summary>
	public MethodInspection[] Constructors { get; set; }
	/// <summary>The array of fields that the type contains</summary>
	public FieldInspection[] Fields { get; set; }
	/// <summary>The array of static fields that the type contains</summary>
	public FieldInspection[] StaticFields { get; set; }
	/// <summary>The array of properties that the type contains</summary>
	public PropertyInspection[] Properties { get; set; }
	/// <summary>The array of static properties that the type contains</summary>
	public PropertyInspection[] StaticProperties { get; set; }
	/// <summary>The array of events that the type contains</summary>
	public EventInspection[] Events { get; set; }
	/// <summary>The array of static events that the type contains</summary>
	public EventInspection[] StaticEvents { get; set; }
	/// <summary>The array of methods that the type contains</summary>
	public MethodInspection[] Methods { get; set; }
	/// <summary>The array of static methods that the type contains</summary>
	public MethodInspection[] StaticMethods { get; set; }
	/// <summary>The array of operators that the type contains</summary>
	public MethodInspection[] Operators { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Sets whether the program should ignore private methods</summary>
	/// <param name="isPrivate">Set to true to ignore all private members</param>
	public static void SetIgnorePrivate(bool isPrivate) { ignorePrivate = isPrivate; }
	
	/// <summary>Generates the type information from a list of assemblies with a safe check</summary>
	/// <param name="typePath">The type path to look into</param>
	/// <param name="info">The resulting type information that is generated</param>
	/// <param name="assemblies">The list of assemblies to look into</param>
	/// <returns>Returns true if the type information is found</returns>
	public static bool GenerateTypeInfo(string typePath, out TypeInspection info, params string[] assemblies)
	{
		assembliesUsed = assemblies;
		foreach(string assembly in assemblies)
		{
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assembly);
			
			foreach(ModuleDefinition module in asm.Modules)
			{
				TypeDefinition type = module.GetType(typePath);
				
				if(type != null)
				{
					assemblyUsed = assembly;
					info = GenerateInfo(asm, type);
					return true;
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
					info = GenerateInfo(_asm, _type);
					return true;
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
							assemblyUsed = assembly;
							info = GenerateInfo(asm, type);
							return true;
						}
					}
				}
			}
		}
		
		info = null;
		return false;
	}
	
	/// <summary>Generates a type information from the given type definition</summary>
	/// <param name="asm">The assembly definition where the type came from</param>
	/// <param name="type">The type definition to look into</param>
	/// <returns>Returns the type information</returns>
	public static TypeInspection GenerateInfo(AssemblyDefinition asm, TypeDefinition type)
	{
		TypeInspection info = new TypeInspection();
		string[] generics = GetGenericParametersString(type.GenericParameters.ToArray());
		
		if(type.IsPublic || type.IsNestedPublic) { info.Accessor = "public"; }
		else if(type.IsNestedAssembly) { info.Accessor = "internal"; }
		else if(type.IsNestedFamily) { info.Accessor = "protected"; }
		else if(type.IsNestedPrivate) { info.Accessor = "private"; }
		else { info.Accessor = "internal"; }
		
		info.Info = QuickTypeInspection.GenerateInfo(type);
		info.HasDeclaringType = (type.DeclaringType != null);
		if(info.HasDeclaringType)
		{
			info.DeclaringType = QuickTypeInspection.GenerateInfo(type.DeclaringType);
		}
		else
		{
			info.DeclaringType = null;
		}
		info.AssemblyName = asm.Name.Name;
		if(type.BaseType != null)
		{
			switch(type.BaseType.FullName)
			{
				case "System.Enum":
				case "System.ValueType":
				case "System.Object":
				{
					info.BaseType = new QuickTypeInspection();
					info.BaseType.UnlocalizedName = "";
					info.BaseType.Name = "";
					info.BaseType.FullName = "";
					info.BaseType.NamespaceName = "";
					info.BaseType.GenericParameters = new GenericParametersInspection[0];
				} break;
				default:
				{
					info.BaseType = QuickTypeInspection.GenerateInfo(type.BaseType);
				} break;
			}
		}
		else
		{
			info.BaseType = new QuickTypeInspection();
			info.BaseType.UnlocalizedName = "";
			info.BaseType.Name = "";
			info.BaseType.FullName = "";
			info.BaseType.NamespaceName = "";
			info.BaseType.GenericParameters = new GenericParametersInspection[0];
		}
		info.IsDelegate = (info.BaseType != null && info.BaseType.FullName == "System.MulticastDelegate");
		info.IsNested = type.IsNested;
		// ObjectType
		if(info.IsDelegate) { info.ObjectType = "delegate"; }
		else if(type.IsEnum) { info.ObjectType = "enum"; }
		else if(type.IsValueType) { info.ObjectType = "struct"; }
		else if(type.IsInterface) { info.ObjectType = "interface"; }
		else { info.ObjectType = "class"; }
		// Modifier
		if(info.IsDelegate || type.IsValueType || type.IsInterface) { info.Modifier = ""; }
		else if(type.IsSealed && type.IsAbstract) { info.Modifier = "static"; }
		else
		{
			info.Modifier = (type.IsSealed
				? "sealed"
				: type.IsAbstract
					? "abstract"
					: ""
			);
		}
		info.IsStatic = (info.Modifier == "static");
		info.IsAbstract = (info.Modifier == "abstract");
		info.IsSealed = (info.Modifier == "sealed");
		info.Attributes = AttributeInspection.GenerateInfoArray(type.CustomAttributes);
		info.Interfaces = GenerateInterfaceInfoArray(type.Interfaces);
		info.Constructors = MethodInspection.GenerateInfoArray(type, false, false, true);
		info.Fields = FieldInspection.GenerateInfoArray(type, true, false);
		info.StaticFields = FieldInspection.GenerateInfoArray(type, false, true);
		info.Properties = PropertyInspection.GenerateInfoArray(type, true, false);
		info.StaticProperties = PropertyInspection.GenerateInfoArray(type, false, true);
		info.Events = EventInspection.GenerateInfoArray(type, true, false);
		info.StaticEvents = EventInspection.GenerateInfoArray(type, false, true);
		info.Methods = MethodInspection.GenerateInfoArray(type, true, false);
		info.StaticMethods = MethodInspection.GenerateInfoArray(type, false, true);
		info.Operators = MethodInspection.GenerateInfoArray(type, true, true, false, true);
		
		System.Array.Sort(info.Constructors);
		System.Array.Sort(info.Fields);
		System.Array.Sort(info.StaticFields);
		System.Array.Sort(info.Properties);
		System.Array.Sort(info.StaticProperties);
		System.Array.Sort(info.Events);
		System.Array.Sort(info.StaticEvents);
		System.Array.Sort(info.Methods);
		System.Array.Sort(info.StaticMethods);
		System.Array.Sort(info.Operators);
		
		info.Declaration = (
			$"{ info.Accessor } " +
			$"{ (info.Modifier != "" ? info.Modifier + " " : "") }" +
			$"{ info.ObjectType } " +
			(info.IsDelegate ? GetDelegateReturnType(info) + " " : "") +
			info.Info.Name
		);
		info.FullDeclaration = GetFullDeclaration(info, type);
		
		return info;
	}
	
	/// <summary>Gets the generic parameter constraints (if any)</summary>
	/// <param name="generics">The generic parameter to look into</param>
	/// <returns>Returns the string of the generic parameter constraints</returns>
	public static string GetGenericParameterConstraints(GenericParametersInspection[] generics)
	{
		string results = "";
		
		foreach(GenericParametersInspection generic in generics)
		{
			if(generic.Constraints.Length == 0) { continue; }
			
			results += $" where { generic.Name } : ";
			for(int i = 0; i < generic.Constraints.Length; i++)
			{
				results += generic.Constraints[i].Name + (i != generic.Constraints.Length - 1 ? "," : "");
			}
		}
		
		return results;
	}
	
	/// <summary>Gets the list of generics from the given name</summary>
	/// <param name="name">The name to get the generics from</param>
	/// <returns>Returns the list of generics</returns>
	public static string[] GetGenerics(string name)
	{
		int left = name.IndexOf("<");
		int scope = 0;
		string temp = "";
		List<string> generics = new List<string>();
		
		for(int i = left + 1; i < name.Length; i++)
		{
			if(name[i] == '<') { scope++; }
			else if(name[i] == '>') { scope--; }
			if(scope < 0) { break; }
			if(scope == 0 && name[i] == ',')
			{
				generics.Add(temp);
				temp = "";
			}
			else { temp += name[i]; }
		}
		
		generics.Add(temp);
		
		return generics.ToArray();
	}
	
	/// <summary>Localizes the name using the list of generic parameter names</summary>
	/// <param name="name">The name of the type</param>
	/// <param name="generics">The array of generic parameter names</param>
	/// <returns>Returns the localized name</returns>
	public static string LocalizeName(string name, string[] generics)
	{
		if(generics.Length == 0 && name.LastIndexOf('<') == -1)
		{
			return name;
		}
		
		if(name.LastIndexOf('<') != -1)
		{
			generics = GetGenerics(name);
			name = name.Substring(0, name.IndexOf('<'));
		}
		
		int index = 0;
		string newName = InspectionRegex.GenericNotation().Replace(name, match => {
			int count;
			string result = "";
			
			if(int.TryParse(match.Groups[1].Value, out count))
			{
				string[] localGenerics = new string[count];
				
				for(int i = 0; i < count; i++)
				{
					localGenerics[i] = generics[index++];
				}
				
				result = $"<{ string.Join(",", localGenerics) }>";
			}
			
			return result;
		});
		
		return newName;
	}
	
	/// <summary>Gets an array of generic parameter names from the given array of generic parameters</summary>
	/// <param name="generics">The array of generic parameters</param>
	/// <returns>Returns an array of generic parameter names</returns>
	public static string[] GetGenericParametersString(GenericParameter[] generics)
	{
		if(generics == null) { return new string[0]; }
		
		string[] results = new string[generics.Length];
		
		for(int i = 0; i < generics.Length; i++)
		{
			results[i] = generics[i].Name;
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the return type of the delegate in string form</summary>
	/// <param name="info">The type info to look into</param>
	/// <returns>Returns the delegate return type in string form</returns>
	private static string GetDelegateReturnType(TypeInspection info)
	{
		foreach(MethodInspection method in info.Methods)
		{
			if(method.Name == "Invoke")
			{
				return method.ReturnType.Name;
			}
		}
		
		return "";
	}
	
	/// <summary>Gets the full declaration of the type</summary>
	/// <param name="info">The type information to look into</param>
	/// <param name="type">The type definition to look into</param>
	/// <returns>Returns the full declaration of the type</returns>
	private static string GetFullDeclaration(TypeInspection info, TypeDefinition type)
	{
		if(info.IsDelegate)
		{
			foreach(MethodInspection method in info.Methods)
			{
				if(method.Name == "Invoke")
				{
					string results = $"{ info.Declaration }({ method.ParameterDeclaration })";
					
					return results + GetGenericParameterConstraints(info.Info.GenericParameters);
				}
			}
		}
		
		bool hasInheritance = (info.BaseType.FullName != "" || info.Interfaces.Length > 0);
		string decl = info.Declaration + (hasInheritance ? " : " : "");
		
		if(info.BaseType.FullName != "")
		{
			decl += info.BaseType.Name + (info.Interfaces.Length > 0 ? ", " : "");
		}
		if(info.Interfaces.Length > 0)
		{
			for(int i = 0; i < info.Interfaces.Length; i++)
			{
				decl += info.Interfaces[i].Name + (i != info.Interfaces.Length - 1 ? ", " : "");
			}
			decl += GetGenericParameterConstraints(info.Info.GenericParameters);
		}
		
		return decl;
	}
	
	/// <summary>Generates an array of interface informations</summary>
	/// <param name="interfaces">The collection of interface implementations</param>
	/// <returns>Returns an array of interface informations</returns>
	private static QuickTypeInspection[] GenerateInterfaceInfoArray(Collection<InterfaceImplementation> interfaces)
	{
		List<QuickTypeInspection> results = new List<QuickTypeInspection>();
		QuickTypeInspection info;
		
		foreach(InterfaceImplementation iFace in interfaces)
		{
			info = QuickTypeInspection.GenerateInfo(iFace.InterfaceType);
			if(ignorePrivate && !IsTypePublic(info.UnlocalizedName, assembliesUsed))
			{
				continue;
			}
			results.Add(QuickTypeInspection.GenerateInfo(iFace.InterfaceType));
		}
		
		return results.ToArray();
	}
	
	/// <summary>Finds if the type is a public type</summary>
	/// <param name="typePath">The type path to look into</param>
	/// <param name="assemblies">The list of assemblies to look into</param>
	/// <returns>Returns true if the type is public</returns>
	private static bool IsTypePublic(string typePath, string[] assemblies)
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
