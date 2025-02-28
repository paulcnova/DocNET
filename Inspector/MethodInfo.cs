
namespace DocNET.Inspector;

using Mono.Cecil;
using Mono.Collections.Generic;

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>All the information relevant to methods</summary>
public partial class MethodInfo : BaseInfo
{
	#region Properties
	
	// The partial name of the method, used to check for base type duplicates
	private string partialFullName;
	// Set to true if the method should be deleted / ignored
	internal bool shouldDelete = false;
	
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
	public QuickTypeInfo ImplementedType { get; set; }
	/// <summary>The type that the method returns</summary>
	public QuickTypeInfo ReturnType { get; set; }
	/// <summary>The attributes of the methods</summary>
	public AttributeInfo[] Attributes { get; set; }
	/// <summary>The parameters that the methods contains</summary>
	public ParameterInfo[] Parameters { get; set; }
	/// <summary>The generic parameters that the method uses</summary>
	public GenericParametersInfo[] GenericParameters { get; set; }
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
	
	// TODO: Centralize this into a helper class.
	[GeneratedRegex(@"(.*)\..*$")]
	public static partial Regex NamespaceNameRegex();
	
	// TODO: Centralize this into a helper class.
	[GeneratedRegex(@"[a-zA-Z0-9]+((?:\[,*\])+)")]
	public static partial Regex ArrayRegex();
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Generates an array of method informations from the given type and booleans</summary>
	/// <param name="type">The type definition to look into</param>
	/// <param name="recursive">Set to true to recursively look through the base types of the method</param>
	/// <param name="isStatic">Set to true to generate only static methods</param>
	/// <param name="isConstructor">Set to true to generate only constructors. Defaults to false</param>
	/// <param name="isOperator">Set to true to generate only operators. Defaults to false</param>
	/// <returns>Returns an array of method informations</returns>
	public static MethodInfo[] GenerateInfoArray(
		TypeDefinition type, bool recursive, bool isStatic,
		bool isConstructor = false, bool isOperator = false
	)
	{
		if(!recursive)
		{
			MethodInfo[] results = GenerateInfoArray(type.Methods);
			
			RemoveUnwanted(ref results, isStatic, isConstructor, isOperator, true);
			
			return results;
		}
		
		List<MethodInfo> methods = new List<MethodInfo>();
		MethodInfo[] temp;
		TypeDefinition currType = type;
		TypeReference currTypeRef = type.Resolve();
		TypeReference baseType;
		bool isOriginal = true;
		
		while(currType != null)
		{
			temp = GenerateInfoArray(type, currType, currTypeRef, currType.Methods);
			RemoveUnwanted(ref temp, isStatic, isConstructor, isOperator, isOriginal);
			if(currType != type)
			{
				RemoveDuplicates(ref temp, methods);
			}
			methods.AddRange(temp);
			baseType = currType.BaseType;
			
			if(baseType == null) { break; }
			
			currTypeRef = baseType;
			currType = baseType.Resolve();
			isOriginal = false;
		}
		
		return methods.ToArray();
	}
	
	/// <summary>Generates an info array from a generic instanced type</summary>
	/// <param name="type">The parent type to look into</param>
	/// <param name="currType">The current (base) type to look into</param>
	/// <param name="currTypeRef">The current (base) type reference to look into</param>
	/// <param name="methods">The collections of methods to look into</param>
	/// <returns>Returns an array of method informations</returns>
	public static MethodInfo[] GenerateInfoArray(
		TypeDefinition type, TypeDefinition currType,
		TypeReference currTypeRef, Collection<MethodDefinition> methods
	)
	{
		List<MethodInfo> results = new List<MethodInfo>();
		MethodInfo info;
		
		foreach(MethodDefinition method in methods)
		{
			info = GetGenericMethodInfo(type, currType, currTypeRef, method);
			
			if(info.shouldDelete) { continue; }
			
			results.Add(info);
		}
		
		return results.ToArray();
	}
	
	/// <summary>Gets the generic instance version of the method info</summary>
	/// <param name="type">The parent type to look into</param>
	/// <param name="currType">The current (base) type to look into</param>
	/// <param name="currTypeRef">The current (base) type reference to look into</param>
	/// <param name="method">The method to look into</param>
	/// <returns>Returns the method information</returns>
	public static MethodInfo GetGenericMethodInfo(
		TypeDefinition type, TypeDefinition currType,
		TypeReference currTypeRef, MethodDefinition method
	)
	{
		MethodReference methodRef = type.Module.ImportReference(method);
		
		if(currTypeRef.IsGenericInstance)
		{
			GenericInstanceType baseInstance = currTypeRef as GenericInstanceType;
			
			methodRef = methodRef.MakeGeneric(baseInstance.GenericArguments.ToArray());
			return GenerateInfo(method, methodRef);
		}
		
		return GenerateInfo(method);
	}
	
	/// <summary>Generates a generic instance of the method</summary>
	/// <param name="method">The method to look into</param>
	/// <param name="methodRef">The method reference to look into</param>
	/// <returns>Returns the information into the method</returns>
	public static MethodInfo GenerateInfo(MethodDefinition method, MethodReference methodRef)
	{
		QuickTypeInfo nInfo = QuickTypeInfo.GenerateInfo(methodRef.DeclaringType);
		MethodInfo info = GenerateInfo(method);
		Dictionary<string, QuickTypeInfo> hash = new Dictionary<string, QuickTypeInfo>();
		bool isGeneric = false;
		int i = 0;
		
		foreach(GenericParametersInfo generic in info.ImplementedType.GenericParameters)
		{
			QuickTypeInfo temp = new QuickTypeInfo();
			
			temp.UnlocalizedName = nInfo.GenericParameters[i].UnlocalizedName;
			temp.FullName = nInfo.GenericParameters[i].Name;
			temp.Name = QuickTypeInfo.DeleteNamespaceFromType(QuickTypeInfo.MakeNameFriendly(temp.FullName));
			if(temp.UnlocalizedName.Contains('.'))
			{
				temp.NamespaceName = NamespaceNameRegex().Replace(temp.UnlocalizedName, "$1");
			}
			else
			{
				temp.NamespaceName = "";
			}
			
			hash.Add(generic.Name, temp);
			i++;
		}
		foreach(ParameterInfo parameter in info.Parameters)
		{
			parameter.TypeInfo = GetGenericInstanceTypeInfo(hash, parameter.TypeInfo, nInfo, out isGeneric);
			if(isGeneric)
			{
				parameter.TypeInfo.GenericParameters = GenericParametersInfo.GenerateInfoArray(methodRef.Resolve().DeclaringType.GenericParameters);
				parameter.GenericParameterDeclarations = QuickTypeInfo.GetGenericParametersAsStrings(parameter.TypeInfo.FullName);
				parameter.FullDeclaration = ParameterInfo.GetFullDeclaration(parameter);
			}
		}
		
		info.ReturnType = GetGenericInstanceTypeInfo(
			hash,
			info.ReturnType,
			QuickTypeInfo.GenerateInfo(methodRef.ReturnType),
			out isGeneric
		);
		
		info.Declaration = (
			info.Accessor + " " +
			(info.Modifier != "" ? info.Modifier + " " : "") +
			(!info.IsConstructor && !info.IsConversionOperator ? info.ReturnType.Name + " " : "") +
			(!info.IsConversionOperator ? info.Name : info.ReturnType.Name)
		);
		info.GenericDeclaration = (info.GenericParameters.Length > 0 && !method.IsGenericInstance
			? $"<{ string.Join(',', GetGenericParameterDeclaration(info.GenericParameters)) }>"
			: ""
		);
		info.ParameterDeclaration = string.Join(", ", GetParameterDeclaration(info));
		if(info.IsExtension)
		{
			info.ParameterDeclaration = $"this {info.ParameterDeclaration}";
		}
		info.FullDeclaration = $"{info.Declaration}{info.GenericDeclaration}({info.ParameterDeclaration})";
		info.FullDeclaration += TypeInfo.GetGenericParameterConstraints(info.GenericParameters);
		
		return info;
	}
	
	/// <summary>Gets the generic instanced type information</summary>
	/// <param name="hash">The hashtable to change the generics into the instanced generics</param>
	/// <param name="info">The information of the type to look into, should be the original</param>
	/// <param name="nInfo">The information of the type to look into, should be generic instanced</param>
	/// <param name="isGeneric">Set to true if anything was changed by this method</param>
	/// <returns>Returns a quick look into the typing</returns>
	public static QuickTypeInfo GetGenericInstanceTypeInfo(
		Dictionary<string, QuickTypeInfo> hash, QuickTypeInfo info,
		QuickTypeInfo nInfo, out bool isGeneric
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
				info.NamespaceName = NamespaceNameRegex().Replace(info.UnlocalizedName, "$1");
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
				string genericPattern = $@"([<,]){ key }([>,])";
				
				info.FullName = Regex.Replace(
					info.FullName,
					genericPattern,
					$"$1{ hash[key].FullName }$2"
				);
				info.FullName = Regex.Replace(
					info.FullName,
					arrayPattern,
					$"{ hash[key].FullName }$1"
				);
				isGeneric = true;
			}
		}
		
		if(isGeneric)
		{
			info.Name = QuickTypeInfo.DeleteNamespaceFromType(
				QuickTypeInfo.MakeNameFriendly(info.FullName)
			);
		}
		
		if(info.FullName == info.Name)
		{
			info.FullName = ArrayRegex().Replace(
				info.FullName,
				$"{ info.UnlocalizedName }$1"
			);
		}
		info.IsGenericType = isGeneric && (info.UnlocalizedName == info.NonInstancedFullName);
		
		return info;
	}
	
	/// <summary>Generates an array of method informations from the given collection of method definitions</summary>
	/// <param name="methods">The collection of methods to look into</param>
	/// <returns>Returns an array of method informations</returns>
	public static MethodInfo[] GenerateInfoArray(Collection<MethodDefinition> methods)
	{
		List<MethodInfo> results = new List<MethodInfo>();
		MethodInfo info;
		
		foreach(MethodDefinition method in methods)
		{
			info = GenerateInfo(method);
			
			if(info.shouldDelete) { continue; }
			
			results.Add(info);
		}
		
		return results.ToArray();
	}
	
	/// <summary>Generates the method information from the given method definition</summary>
	/// <param name="method">The method definition to look into</param>
	/// <returns>Returns the method information</returns>
	public static MethodInfo GenerateInfo(MethodDefinition method)
	{
		MethodInfo info = new MethodInfo();
		int index;
		
		info.IsStatic = method.IsStatic;
		info.IsVirtual = method.IsVirtual;
		info.IsConstructor = method.IsConstructor;
		if(method.IsAssembly) { info.Accessor = "internal"; }
		else if(method.IsFamily) { info.Accessor = "protected"; }
		else if(method.IsPublic) { info.Accessor = "public"; }
		else { info.Accessor = "private"; }
		info.IsProperty = method.IsGetter || method.IsSetter;
		info.IsEvent = method.IsAddOn || method.IsRemoveOn;
		info.IsOperator = method.Name.StartsWith("op_");
		info.IsConversionOperator = (
			method.Name == "op_Explicit" ||
			method.Name == "op_Implicit"
		);
		info.ImplementedType = QuickTypeInfo.GenerateInfo(method.DeclaringType);
		info.ReturnType = QuickTypeInfo.GenerateInfo(method.ReturnType);
		if(info.IsConstructor)
		{
			info.Name = info.ImplementedType.Name;
			index = info.Name.IndexOf('<');
			if(index != -1)
			{
				info.Name = info.Name.Substring(0, index);
			}
		}
		else if(info.IsConversionOperator)
		{
			info.Name = method.Name + "__" + info.ReturnType.Name;
		}
		else if(info.IsOperator)
		{
			info.Name = method.Name.Substring(3);
		}
		else
		{
			info.Name = method.Name;
		}
		info.partialFullName = method.FullName.Split("::")[1].Replace(",", ", ");
		if(info.IsOperator)
		{
			info.partialFullName = info.Name;
		}
		info.Parameters = ParameterInfo.GenerateInfoArray(method.Parameters);
		info.GenericParameters = GenericParametersInfo.GenerateInfoArray(method.GenericParameters);
		info.Attributes = AttributeInfo.GenerateInfoArray(method.CustomAttributes);
		if(info.IsConversionOperator) { info.Modifier = $"static { method.Name.Substring(3).ToLower() } operator"; }
		else if(info.IsOperator) { info.Modifier = "static operator"; }
		else if(method.IsStatic) { info.Modifier = "static"; }
		else if(method.IsAbstract) { info.Modifier = "abstract"; }
		else if(method.IsVirtual && method.IsReuseSlot) { info.Modifier = "override"; }
		else if(method.IsVirtual) { info.Modifier = "virtual"; }
		else { info.Modifier = ""; }
		info.IsExtension = HasExtensionAttribute(info);
		info.IsAbstract = method.IsAbstract;
		info.IsOverridden = method.IsReuseSlot;
		info.Declaration = (
			info.Accessor + " " +
			(info.Modifier != "" ? info.Modifier + " " : "") +
			(!info.IsConstructor && !info.IsConversionOperator ? info.ReturnType.Name + " " : "") +
			(!info.IsConversionOperator ? info.Name : info.ReturnType.Name)
		);
		info.GenericDeclaration = (info.GenericParameters.Length > 0 && !method.IsGenericInstance ?
			$"<{ string.Join(',', GetGenericParameterDeclaration(info.GenericParameters)) }>" :
			""
		);
		info.ParameterDeclaration = string.Join(", ", GetParameterDeclaration(info));
		if(info.IsExtension)
		{
			info.ParameterDeclaration = $"this { info.ParameterDeclaration }";
		}
		info.FullDeclaration = $"{ info.Declaration }{ info.GenericDeclaration }({ info.ParameterDeclaration })";
		info.FullDeclaration += TypeInfo.GetGenericParameterConstraints(info.GenericParameters);
		if(TypeInfo.ignorePrivate && PropertyInfo.GetAccessorId(info.Accessor) == 0)
		{
			info.shouldDelete = true;
		}
		
		return info;
	}
	
	/// <summary>Gets the generic parameter declarations for the method</summary>
	/// <param name="generics">The list of generics to look into</param>
	/// <returns>Returns a list of the generic parameter declarations used</returns>
	public static string[] GetGenericParameterDeclaration(GenericParametersInfo[] generics)
	{
		string[] results = new string[generics.Length];
		
		for(int i = 0; i < generics.Length; i++)
		{
			results[i] = generics[i].UnlocalizedName;
		}
		
		return results;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Removes any unwanted methods from the given types of booleans</summary>
	/// <param name="temp">The list of methods to remove from</param>
	/// <param name="isStatic">Set to true if non-static methods should be removed</param>
	/// <param name="isConstructor">Set to false if constructors should be removed</param>
	/// <param name="isOperator">Set to false if operators should be removed</param>
	/// <param name="isOriginal">Set to false if it's a base type, this will remove any private members</param>
	private static void RemoveUnwanted(
		ref MethodInfo[] temp, bool isStatic,
		bool isConstructor, bool isOperator, bool isOriginal
	)
	{
		List<MethodInfo> methods = new List<MethodInfo>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
		{
			if(methods[i].shouldDelete)
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
		
		temp = methods.ToArray();
	}
	
	/// <summary>Removes all the duplicates from the list of methods</summary>
	/// <param name="temp">The list of methods to remove duplicates from</param>
	/// <param name="listMethods">The list of recursive-ordered methods to reference which ones are duplicates</param>
	private static void RemoveDuplicates(ref MethodInfo[] temp, List<MethodInfo> listMethods)
	{
		List<MethodInfo> methods = new List<MethodInfo>(temp);
		
		for(int i = temp.Length - 1; i >= 0; i--)
		{
			foreach(MethodInfo method in listMethods)
			{
				if(methods[i].partialFullName == method.partialFullName)
				{
					methods.RemoveAt(i);
					break;
				}
			}
		}
		
		temp = methods.ToArray();
	}
	
	/// <summary>Finds if the method is an extension</summary>
	/// <param name="method">The method to look into</param>
	/// <returns>Returns true if the method is an extension by having the extension attribute</returns>
	private static bool HasExtensionAttribute(MethodInfo method)
	{
		foreach(AttributeInfo attr in method.Attributes)
		{
			if(attr.TypeInfo.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
			{
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>Generates the parameter declaration from the given method</summary>
	/// <param name="method">The method info to look into</param>
	/// <returns>Returns an array of parameter declaration</returns>
	private static string[] GetParameterDeclaration(MethodInfo method)
	{
		string[] declarations = new string[method.Parameters.Length];
		int i = 0;
		
		foreach(ParameterInfo parameter in method.Parameters)
		{
			declarations[i++] = parameter.FullDeclaration;
		}
		
		return declarations;
	}
	
	#endregion // Private Methods
}
