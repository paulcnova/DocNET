
namespace DocNET.Inspector;

using Newtonsoft.Json;

using System.IO;

/// <summary>A static class used as the entry point for the program</summary>
public static class Start
{
	#region Public Methods
	
	///// <summary>Starts the application</summary>
	///// <param name="args">The arguments to start with</param>
	// public static void Main(string[] args)
	// {
	// 	try
	// 	{
	// 		InputArguments input = InputArguments.Create(args);
			
	// 		TypeInfo.ignorePrivate = !input.IncludePrivate;
			
	// 		if(input.IsList)
	// 		{
	// 			TypeList list = TypeList.GenerateList(input.Assemblies.ToArray());
	// 			string listJson = JsonConvert.SerializeObject(list, Formatting.Indented);
				
	// 			File.WriteAllText(input.Output, listJson);
	// 			System.Console.WriteLine($"JSON file ({ input.Output }) of listing every type successfully created!");
	// 			System.Environment.Exit(0);
	// 		}
	// 		else if(input.IsHelp) { DisplayHelp(); }
			
	// 		string typePath = input.TypePath;
	// 		string[] assemblies = input.Assemblies.ToArray();
	// 		TypeInfo info;
	// 		string json = "";
			
	// 		if(TypeInfo.GenerateTypeInfo(assemblies, typePath, out info))
	// 		{
	// 			json = JsonConvert.SerializeObject(info, Formatting.Indented);
	// 			File.WriteAllText(input.Output, json);
	// 			System.Console.WriteLine($"JSON file ({ input.Output }) of peek into type [{ typePath }] successfully created!");
	// 		}
	// 		else
	// 		{
	// 			System.Console.WriteLine($"Type [{ typePath }] is not found!");
	// 			DisplayHelp();
	// 		}
	// 	}
	// 	catch(System.IndexOutOfRangeException)
	// 	{
	// 		System.Console.WriteLine("Error: Output location not specified");
	// 		DisplayHelp();
	// 	}
	// 	catch(System.Exception e)
	// 	{
	// 		System.Console.WriteLine($"Error: { e.Message }");
	// 		DisplayHelp();
	// 	}
	// }
	
	/// <summary>Displays the help menu</summary>
	public static void DisplayHelp()
	{
		System.Console.WriteLine("Use: SharpChecker [options] <type-path> <list-of-assemblies>");
		System.Console.WriteLine("Note: Instead of using ` you can use -.");
		System.Console.WriteLine("Options:");
		System.Console.WriteLine("--help\t\t\tDisplays the help menu. (Shorthand: -h).");
		System.Console.WriteLine("--list\t\t\tLists all the types of each assembly. (Shorthand: -l).");
		System.Console.WriteLine("--include-private\tInclude private members. (Shorthand: -p).");
		System.Console.WriteLine("--out <output-file>\tThe file to be outputted. (Shorthand: -o).");
		System.Environment.Exit(0);
	}
	
	#endregion // Public Methods
}
