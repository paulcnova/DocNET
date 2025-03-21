
namespace DocNET.Utilities;

using Mono.Cecil;

using System.Collections.Generic;

public static class Settings
{
	#region Properties
	
	public static bool IgnorePrivate { get; set; } = false;
	public static string CWD { get; set; } = System.Environment.CurrentDirectory;
	public static string Output { get; set; } = $"{System.Environment.CurrentDirectory}/bin/docs";
	public static string ProjectName { get; set; }
	public static IUtilitySet UtilitySet { get; set; } = new XmlUtilitySet();
	
	public static string CSProjectFile => $"{Settings.CWD}/{ProjectName}.csproj";
	
	#endregion // Properties
	
	#region Public Methods
	
	public static void FindTemplate(string fileName)
	{
		string absolute = System.IO.Path.Combine(CWD, fileName);
		AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(absolute);
		
		foreach(ModuleDefinition module in asm.Modules)
		{
			foreach(TypeDefinition type in module.GetTypes())
			{
				foreach(InterfaceImplementation iFace in type.Interfaces)
				{
					if(iFace.InterfaceType.FullName == "DocNET.Utilities.IUtilitySet")
					{
						string binPath = absolute.Substring(0, System.Math.Max(absolute.LastIndexOf('/'), absolute.LastIndexOf('\\')));
						List<string> assemblies = new List<string>(FileUtility.GetAllBinaries(binPath));
						
						for(int i = assemblies.Count - 1; i >= 0; ++i)
						{
							if(assemblies[i].EndsWith("DocNET.dll"))
							{
								assemblies.RemoveAt(i);
								break;
							}
						}
						
						System.Type util = System.Reflection.Assembly.LoadFrom(absolute).GetType(type.FullName);
						
						foreach(string asmPath in assemblies)
						{
							System.Reflection.Assembly.LoadFrom(asmPath);
						}
						
						System.Reflection.ConstructorInfo ctor = util.GetConstructor(new System.Type[0]);
						
						if(ctor != null)
						{
							UtilitySet = ctor.Invoke(null) as IUtilitySet;
						}
						return;
					}
				}
			}
		}
	}
	
	#endregion // Public Methods
}
