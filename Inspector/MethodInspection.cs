
namespace DocNET.Inspections;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>All the information relevant to methods</summary>
public partial class MethodInspection : BaseInspection
{
	#region Properties
	
	// The partial name of the method, used to check for base type duplicates
	private string partialFullName;
	
	/// <summary>The name of the method</summary>
	public string Name { get; set; }
	
	/// <summary>The accessor of the method (such as internal, private, protected, public)</summary>
	public string Accessor { get; set; }
	
	/// <summary>Any modifiers of the method (such as static, virtual, override, etc.)</summary>
	public string Modifier { get; set; }
	
	/// <summary>Set to true if the method is abstract</summary>
	public bool IsAbstract { get; set; }
	
	/// <summary>Set to true if the method is a constructor</summary>
	public bool IsConstructor { get; set; }
	
	/// <summary>Set to true if the method is a conversion operator</summary>
	public bool IsConversionOperator { get; set; }
	
	/// <summary>Set to true if the method is an extension</summary>
	public bool IsExtension { get; set; }
	
	/// <summary>Set to true if the method is an operator</summary>
	public bool IsOperator { get; set; }
	
	/// <summary>Set to true if the method is overridden</summary>
	public bool IsOverridden { get; set; }
	
	/// <summary>Set to true if the method is static</summary>
	public bool IsStatic { get; set; }
	
	/// <summary>Set to true if the method is virtual</summary>
	public bool IsVirtual { get; set; }
	
	/// <summary>The type that the method is implemented in</summary>
	public QuickTypeInspection ImplementedType { get; set; }
	
	/// <summary>The type that the method returns</summary>
	public QuickTypeInspection ReturnType { get; set; }
	
	/// <summary>The attributes of the methods</summary>
	public List<AttributeInspection> Attributes { get; set; } = new List<AttributeInspection>();
	
	/// <summary>The parameters that the methods contains</summary>
	public List<ParameterInspection> Parameters { get; set; } = new List<ParameterInspection>();
	
	/// <summary>The generic parameters that the method uses</summary>
	public List<GenericParameterInspection> GenericParameters { get; set; } = new List<GenericParameterInspection>();
	
	/// <summary>The partial declaration of the method (without parameters) that can be found in the code</summary>
	public string Declaration { get; set; }
	
	/// <summary>The partial declaration of the generics that can be found in the code</summary>
	public string GenericDeclaration { get; set; }
	
	/// <summary>The partial declaration of the parameters that can be found in the code</summary>
	public string ParameterDeclaration { get; set; }
	
	/// <summary>The full declaration of the method that can be found in the code</summary>
	public string FullDeclaration { get; set; }
	
	// Tells if the method is a property, used to remove it when it's irrelevant
	private bool IsProperty { get; set; }
	
	// Tells if the method is an event, used to remove it when it's irrelevant
	private bool IsEvent { get; set; }
	
	/// <summary>Set to true if the method should be deleted / ignored.</summary>
	public bool ShouldIgnore { get; private set; } = false;
	
	
	/// <summary>A constructor to generates the data from just the method itself.</summary>
	/// <param name="type">The type that the method is originating from.</param>
	/// <param name="boundType">The type that the method is created from and bound to.</param>
	/// <param name="boundTypeRef">The reference to the type that the method is created from and bound to.</param>
	/// <param name="method">The method definition to look into.</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	public MethodInspection(
		TypeDefinition type, TypeDefinition boundType,
		TypeReference boundTypeRef, MethodDefinition method, bool ignorePrivate = true
	)
	{
		
		if(boundTypeRef.IsGenericInstance)
		{
			GenericInstanceType baseInstance = boundTypeRef as GenericInstanceType;
			MethodReference methodRef = type.Module.ImportReference(method);
			
			methodRef = methodRef.MakeGeneric(baseInstance.GenericArguments.ToArray());
			this.Construct(method, methodRef, ignorePrivate);
			return;
		}
		
		this.Construct(method, ignorePrivate);
	}
	
	/// <summary>A constructor to generates the data from just the method itself.</summary>
	/// <param name="method">The method definition to look into.</param>
	/// <param name="methodRef">The reference of the method</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	public MethodInspection(MethodDefinition method, MethodReference methodRef, bool ignorePrivate = true) => this.Construct(method, methodRef, ignorePrivate);
	
	/// <summary>A constructor to generates the data from just the method itself.</summary>
	/// <param name="method">The method definition to look into.</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	public MethodInspection(MethodDefinition method, bool ignorePrivate = true) => this.Construct(method, ignorePrivate);
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of method informations from the given type and booleans</summary>
	/// <param name="type">The type definition to look into</param>
	/// <param name="recursive">Set to true to recursively look through the base types of the method</param>
	/// <param name="isStatic">Set to true to generate only static methods</param>
	/// <param name="isConstructor">Set to true to generate only constructors. Defaults to false</param>
	/// <param name="isOperator">Set to true to generate only operators. Defaults to false</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	/// <returns>Returns an array of method informations</returns>
	public static List<MethodInspection> CreateArray(
		TypeDefinition type, bool recursive, bool isStatic,
		bool isConstructor = false, bool isOperator = false, bool ignorePrivate = true
	)
	{
		if(!recursive)
		{
			List<MethodInspection> results = CreateArray(type.Methods, ignorePrivate);
			
			RemoveUnwanted(results, isStatic, isConstructor, isOperator, true);
			
			return results;
		}
		
		List<MethodInspection> methods = new List<MethodInspection>();
		List<MethodInspection> temp;
		TypeDefinition boundType = type;
		TypeReference boundTypeRef = type.Resolve();
		TypeReference baseType;
		bool isOriginal = true;
		
		while(boundType != null)
		{
			temp = CreateArray(type, boundType, boundTypeRef, boundType.Methods);
			RemoveUnwanted(temp, isStatic, isConstructor, isOperator, isOriginal);
			if(boundType != type)
			{
				RemoveDuplicates(temp, methods);
			}
			methods.AddRange(temp);
			baseType = boundType.BaseType;
			
			if(baseType == null) { break; }
			
			boundTypeRef = baseType;
			boundType = baseType.Resolve();
			isOriginal = false;
		}
		
		return methods;
	}
	
	/// <summary>Generates an info array from a generic instanced type</summary>
	/// <param name="type">The parent type to look into</param>
	/// <param name="boundType">The current (base) type to look into</param>
	/// <param name="boundTypeRef">The current (base) type reference to look into</param>
	/// <param name="methods">The collections of methods to look into</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	/// <returns>Returns an array of method informations</returns>
	public static List<MethodInspection> CreateArray(
		TypeDefinition type, TypeDefinition boundType,
		TypeReference boundTypeRef, Collection<MethodDefinition> methods, bool ignorePrivate = true
	)
	{
		List<MethodInspection> results = new List<MethodInspection>();
		
		foreach(MethodDefinition method in methods)
		{
			MethodInspection data = new MethodInspection(type, boundType, boundTypeRef, method, ignorePrivate);
			
			if(data.ShouldIgnore) { continue; }
			
			results.Add(data);
		}
		
		return results;
	}
	
	public override string GetXmlNameID()
	{
		string name = this.Name;
		
		if(this.IsConstructor) { name = "#ctor"; }
		if(this.IsConversionOperator)
		{
			name = this.Modifier.IndexOf("implicit") == -1
				? "op_Explicit"
				: "op_Implicit";
		}
		else if(this.IsOperator && !name.StartsWith("op_")) { name = $"op_{name}"; }
		
		string typePath = $"{this.ImplementedType.UnlocalizedName}.{name}";
		List<string> parameters = new List<string>();
		
		if(this.GenericParameters.Count > 0)
		{
			typePath += $"``{this.GenericParameters.Count}";
		}
		
		foreach(ParameterInspection parameter in this.Parameters)
		{
			string paramResult = parameter.TypeInfo.NonInstancedFullName;
			string temp;
			
			for(int i = 0; i < this.ImplementedType.GenericParameters.Count; ++i)
			{
				temp = this.ImplementedType.GenericParameters[i].UnlocalizedName;
				if(paramResult == temp)
				{
					paramResult = $"`{i}";
					break;
				}
				paramResult = Regex.Replace(paramResult, @$"([\\(<,]){temp}([\\)\\[>,])", $"$1`{i}$2");
				paramResult = Regex.Replace(paramResult, @$"{temp}((?:\\[,*\\])+)", $"`{i}$1");
			}
			
			for(int i = 0; i < this.GenericParameters.Count; ++i)
			{
				temp = this.GenericParameters[i].UnlocalizedName;
				if(paramResult == temp)
				{
					paramResult = $"``{i}";
					break;
				}
				paramResult = Regex.Replace(paramResult, @$"([\\(<,]){temp}([\\)\\[>,])", $"$1``{i}$2");
				paramResult = Regex.Replace(paramResult, @$"{temp}((?:\\[,*\\])+)", $"``{i}$1");
			}
			
			paramResult = paramResult.Replace('<', '{').Replace('>', '}');
			
			if(parameter.Modifier != "") { paramResult += "@"; }
			
			parameters.Add(paramResult);
		}
		
		if(parameters.Count > 0)
		{
			string methodPath = $"{typePath}({string.Join(',', parameters)})";
			
			if(this.IsConversionOperator)
			{
				return $"M:{methodPath}~{this.ReturnType.NonInstancedFullName}";
			}
			return $"M:{methodPath}";
		}
		return $"M:{typePath}";
	}
	
	public string GetTypePath()
	{
		string name = this.Name;
		
		if(this.IsConstructor) { name = "#ctor"; }
		if(this.IsConversionOperator)
		{
			name = this.Modifier.Contains("implicit")
				? "op_Implicit"
				: "op_Explicit";
		}
		else if(this.IsOperator && !name.StartsWith("op_"))
		{
			name = $"op_{name}";
		}
		
		string typePath = this.ImplementedType.GetTypePath(name);
		List<string> parameters = new List<string>();
		
		if(this.GenericParameters.Count > 0)
		{
			typePath += $"``{this.GenericParameters.Count}";
		}
		foreach(ParameterInspection parameter in this.Parameters)
		{
			string paramResult = parameter.TypeInfo.NonInstancedFullName;
			string temp;
			
			for(int i = 0; i < this.ImplementedType.GenericParameters.Count; ++i)
			{
				temp = this.ImplementedType.GenericParameters[i].UnlocalizedName;
				
				if(paramResult == temp)
				{
					paramResult = $"`{i}";
					break;
				}
				paramResult = Regex.Replace(paramResult, $@"`([\(<,]){temp}([\)\[>,])", $"$1`{i}$2");
				paramResult = Regex.Replace(paramResult, $@"{temp}((?:\[,*\])+)", $"`{i}$1");
			}
			for(int i = 0; i < this.GenericParameters.Count; ++i)
			{
				temp = this.GenericParameters[i].UnlocalizedName;
				if(paramResult == temp)
				{
					paramResult = $"``{i}";
					break;
				}
				paramResult = Regex.Replace(paramResult, $@"([\(<,]){temp}([\)\[>,])", $"$1``{i}$2");
				paramResult = Regex.Replace(paramResult, $@"{temp}((?:\[,*\])+)", $"``{i}$1");
			}
			paramResult = Regex.Replace(Regex.Replace(paramResult, "<", "{"), ">", "}");
			if(!string.IsNullOrEmpty(parameter.Modifier) && parameter.Modifier != "params") { paramResult += "@"; }
			parameters.Add(paramResult);
		}
		
		if(parameters.Count > 0)
		{
			string methodPath = $"{typePath}({string.Join(',', parameters)})";
			
			if(this.IsConversionOperator)
			{
				return $"{methodPath}~{this.ReturnType.NonInstancedFullName}";
			}
			return methodPath;
		}
		return typePath;
	}
	
	/// <summary>Gets the generic instanced type information</summary>
	/// <param name="hash">The hashtable to change the generics into the instanced generics</param>
	/// <param name="info">The information of the type to look into, should be the original</param>
	/// <param name="typeInfo">The information of the type to look into, should be generic instanced</param>
	/// <param name="isGeneric">Set to true if anything was changed by this method</param>
	/// <returns>Returns a quick look into the typing</returns>
	public QuickTypeInspection GetGenericInstanceTypeInfo(
		Dictionary<string, QuickTypeInspection> hash, QuickTypeInspection info,
		QuickTypeInspection typeInfo, out bool isGeneric
	)
	{
		isGeneric = false;
		
		foreach(string key in hash.Keys)
		{
			string arrayPattern = $@"{key}((?:\[,*\])+)";
			
			if(info.UnlocalizedName == key)
			{
				info.UnlocalizedName = hash[key].UnlocalizedName;
			}
			if(info.UnlocalizedName.Contains('.'))
			{
				info.NamespaceName = InspectionRegex.NamespaceName().Replace(info.UnlocalizedName, "$1");
			}
			else
			{
				info.NamespaceName = "";
			}
			if(info.FullName == key)
			{
				info.FullName = hash[key].FullName;
				isGeneric = true;
			}
			else if(info.FullName.Contains(key))
			{
				string genericPattern = $@"([<,]){key}([>,])";
				
				info.FullName = Regex.Replace(
					info.FullName,
					genericPattern,
					$"$1{hash[key].FullName}$2"
				);
				info.FullName = Regex.Replace(
					info.FullName,
					arrayPattern,
					$"{hash[key].FullName}$1"
				);
				isGeneric = true;
			}
		}
		
		if(isGeneric)
		{
			info.Name = InspectorUtility.RemoveNamespaceFromType(InspectorUtility.MakeNameFriendly(info.FullName));
		}
		
		if(info.FullName == info.Name)
		{
			info.FullName = InspectionRegex.Array().Replace(
				info.FullName,
				$"{info.UnlocalizedName}$1"
			);
		}
		info.IsGenericType = isGeneric && (info.UnlocalizedName == info.NonInstancedFullName);
		
		return info;
	}
	
	/// <summary>Generates an array of method informations from the given collection of method definitions</summary>
	/// <param name="methods">The collection of methods to look into</param>
	/// <param name="ignorePrivate">Set to true to ignore this field if it's private within the declaring type</param>
	/// <returns>Returns an array of method informations</returns>
	public static List<MethodInspection> CreateArray(Collection<MethodDefinition> methods, bool ignorePrivate = true)
	{
		List<MethodInspection> results = new List<MethodInspection>();
		
		foreach(MethodDefinition method in methods)
		{
			MethodInspection data = new MethodInspection(method, ignorePrivate);
			
			if(data.ShouldIgnore) { continue; }
			
			results.Add(data);
		}
		
		return results;
	}
	
	/// <summary>Gets the generic parameter declarations for the method</summary>
	/// <returns>Returns a list of the generic parameter declarations used</returns>
	public List<string> GetGenericParameterDeclaration()
	{
		List<string> results = new List<string>();
		
		for(int i = 0; i < this.GenericParameters.Count; i++)
		{
			results.Add(this.GenericParameters[i].UnlocalizedName);
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void Construct(MethodDefinition method, MethodReference methodRef, bool ignorePrivate = true)
	{
		QuickTypeInspection typeInfo = new QuickTypeInspection(methodRef.DeclaringType);
		Dictionary<string, QuickTypeInspection> hash = new Dictionary<string, QuickTypeInspection>();
		bool isGeneric = false;
		int i = 0;
		
		this.Construct(method);
		
		foreach(GenericParameterInspection parameter in this.ImplementedType.GenericParameters)
		{
			QuickTypeInspection temp = new QuickTypeInspection();
			
			temp.UnlocalizedName = typeInfo.GenericParameters[i].UnlocalizedName;
			temp.FullName = typeInfo.GenericParameters[i].Name;
			temp.Name = InspectorUtility.RemoveNamespaceFromType(InspectorUtility.MakeNameFriendly(temp.FullName));
			temp.NamespaceName = temp.UnlocalizedName.Contains('.')
				? InspectionRegex.NamespaceName().Replace(temp.UnlocalizedName, "$1")
				: "";
			hash.Add(parameter.Name, temp);
			++i;
		}
		foreach(ParameterInspection parameter in this.Parameters)
		{
			parameter.TypeInfo = this.GetGenericInstanceTypeInfo(hash, parameter.TypeInfo, typeInfo, out isGeneric);
			
			if(isGeneric)
			{
				parameter.TypeInfo.GenericParameters = GenericParameterInspection.CreateArray(
					methodRef.Resolve().DeclaringType.GenericParameters
				);
				parameter.GenericParameterDeclarations = InspectorUtility.GetGenericParametersAsStrings(parameter.TypeInfo.FullName);
				parameter.FullDeclaration = parameter.GetFullDeclaration();
			}
		}
		this.ReturnType = this.GetGenericInstanceTypeInfo(hash, this.ReturnType, new QuickTypeInspection(methodRef.ReturnType), out isGeneric);
		this.Declaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{(
			this.IsConstructor && !this.IsConversionOperator
				? $"{this.ReturnType.Name} "
				: ""
		)}{(
			!this.IsConversionOperator
				? this.Name
				: this.ReturnType.Name
		)}";
		this.GenericDeclaration = this.GenericParameters.Count > 0 && !method.IsGenericInstance
			? $"<{string.Join(',', this.GetGenericParameterDeclaration())}>"
			: "";
		this.ParameterDeclaration = string.Join(", ", this.GetParameterDeclaration());
		this.FullDeclaration = $"{this.Declaration}{this.GenericDeclaration}({this.ParameterDeclaration})";
		this.FullDeclaration += InspectorUtility.GetGenericParameterConstraints(this.GenericParameters);
	}
	
	private void Construct(MethodDefinition method, bool ignorePrivate = true)
	{
		this.IsStatic = method.IsStatic;
		this.IsAbstract = method.IsAbstract;
		this.IsVirtual = method.IsVirtual;
		this.IsConstructor = method.IsConstructor;
		this.IsOverridden = method.IsReuseSlot;
		
		if(method.IsAssembly) { this.Accessor = "internal"; }
		else if(method.IsFamily) { this.Accessor = "protected"; }
		else if(method.IsPublic) { this.Accessor = "public"; }
		else { this.Accessor = "private"; }
		if(ignorePrivate && InspectorUtility.GetAccessorId(this.Accessor, ignorePrivate) == 0)
		{
			this.ShouldIgnore = true;
			return;
		}
		
		this.IsProperty = method.IsGetter || method.IsSetter;
		this.IsEvent = method.IsAddOn || method.IsRemoveOn;
		this.IsOperator = method.Name.StartsWith("op_");
		this.IsConversionOperator = (
			method.Name == "op_Explicit"
			|| method.Name == "op_Implicit"
		);
		this.ImplementedType = new QuickTypeInspection(method.DeclaringType);
		this.ReturnType = new QuickTypeInspection(method.ReturnType);
		
		if(this.IsConstructor)
		{
			this.Name = this.ImplementedType.Name;
			
			int index = this.Name.IndexOf('<');
			
			if(index != -1)
			{
				this.Name = this.Name.Substring(0, index);
			}
		}
		else if(this.IsConversionOperator)
		{
			this.Name = $"{method.Name}__{this.ReturnType.Name}";
		}
		else if(this.IsOperator)
		{
			this.Name = method.Name.Substring(3);
		}
		else { this.Name = method.Name; }
		
		this.partialFullName = method.FullName.Split("::")[1].Replace(",", ", ");
		if(this.IsOperator)
		{
			this.partialFullName = this.Name;
		}
		this.Parameters = ParameterInspection.CreateArray(method.Parameters);
		this.GenericParameters = GenericParameterInspection.CreateArray(method.GenericParameters);
		this.Attributes = AttributeInspection.CreateArray(method.CustomAttributes);
		
		if(this.IsConversionOperator) { this.Modifier = $"static {method.Name.Substring(3).ToLower()} operator"; }
		else if(this.IsOperator) { this.Modifier = "static operator"; }
		else if(this.IsStatic) { this.Modifier = "static"; }
		else if(this.IsAbstract) { this.Modifier = "abstract"; }
		else if(this.IsVirtual && this.IsOverridden) { this.Modifier = "override"; }
		else if(this.IsVirtual) { this.Modifier = "virtual"; }
		else { this.Modifier = ""; }
		
		this.IsExtension = InspectorUtility.HasExtensionAttribute(this.Attributes);
		this.Declaration = $"{this.Accessor} {(
			this.Modifier != ""
				? $"{this.Modifier} "
				: ""
		)}{(
			!this.IsConstructor && !this.IsConversionOperator
				? $"{this.ReturnType.Name} "
				: ""
		)}{(
			!this.IsConversionOperator
				? this.Name
				: this.ReturnType.Name
		)}";
		this.GenericDeclaration = this.GenericParameters.Count > 0 && !method.IsGenericInstance
			? $"<{string.Join(',', this.GetGenericParameterDeclaration())}"
			: "";
		this.ParameterDeclaration = string.Join(", ", this.GetParameterDeclaration());
		if(this.IsExtension)
		{
			this.ParameterDeclaration = $"this {this.ParameterDeclaration}";
		}
		this.FullDeclaration = $"{this.Declaration}{this.GenericDeclaration}({this.ParameterDeclaration})";
		this.FullDeclaration += InspectorUtility.GetGenericParameterConstraints(this.GenericParameters);
	}
	
	/// <summary>Removes any unwanted methods from the given types of booleans</summary>
	/// <param name="methods">The list of methods to remove from</param>
	/// <param name="isStatic">Set to true if non-static methods should be removed</param>
	/// <param name="isConstructor">Set to false if constructors should be removed</param>
	/// <param name="isOperator">Set to false if operators should be removed</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(
		List<MethodInspection> methods, bool isStatic,
		bool isConstructor, bool isOperator, bool isOriginal
	)
	{
		for(int i = methods.Count - 1; i >= 0; i--)
		{
			if(methods[i].ShouldIgnore)
			{
				methods.RemoveAt(i);
			}
			else if(methods[i].Name == ".cctor")
			{
				methods.RemoveAt(i);
			}
			else if(methods[i].IsProperty || methods[i].IsEvent)
			{
				methods.RemoveAt(i);
			}
			else if(methods[i].IsStatic != isStatic)
			{
				methods.RemoveAt(i);
			}
			else if(methods[i].IsConstructor != isConstructor)
			{
				methods.RemoveAt(i);
			}
			else if(methods[i].IsOperator != isOperator)
			{
				methods.RemoveAt(i);
			}
			else if(!isOriginal && methods[i].Accessor == "private")
			{
				methods.RemoveAt(i);
			}
		}
	}
	
	/// <summary>Removes all the duplicates from the list of methods</summary>
	/// <param name="methods">The list of methods to remove duplicates from</param>
	/// <param name="listMethods">The list of recursive-ordered methods to reference which ones are duplicates</param>
	private static void RemoveDuplicates(List<MethodInspection> methods, List<MethodInspection> listMethods)
	{
		for(int i = methods.Count - 1; i >= 0; i--)
		{
			foreach(MethodInspection method in listMethods)
			{
				if(methods[i].partialFullName == method.partialFullName)
				{
					methods.RemoveAt(i);
					break;
				}
			}
		}
	}
	
	/// <summary>Generates the parameter declaration from the given method</summary>
	/// <returns>Returns an array of parameter declaration</returns>
	private List<string> GetParameterDeclaration()
	{
		List<string> declarations = new List<string>();
		
		foreach(ParameterInspection parameter in this.Parameters)
		{
			declarations.Add(parameter.FullDeclaration);
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
}
