
namespace DocNET.Inspections;

using Mono.Cecil;

internal static class Inspection_Extensions
{
	public static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
	{
		if(self.GenericParameters.Count != arguments.Length)
		{
			throw new System.ArgumentException();
		}
		
		GenericInstanceType instance = new GenericInstanceType(self);
		
		foreach(TypeReference arg in arguments)
		{
			instance.GenericArguments.Add(arg);
		}
		
		return instance;
	}
	
	public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
	{
		MethodReference reference = new MethodReference(self.Name, self.ReturnType)
		{
			DeclaringType = self.DeclaringType.MakeGenericType(arguments),
			HasThis = self.HasThis,
			ExplicitThis = self.ExplicitThis,
			CallingConvention = self.CallingConvention
		};
		
		foreach(ParameterDefinition parameter in self.Parameters)
		{
			reference.Parameters.Add(new ParameterDefinition(parameter.Resolve().ParameterType));
		}
		foreach(GenericParameter generic in self.GenericParameters)
		{
			reference.GenericParameters.Add(new GenericParameter(generic.Name, reference));
		}
		
		return reference;
	}
}
