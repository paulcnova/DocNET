
namespace Taco.DocNET.Inspector;

using System.Collections.Generic;

/// <summary>A managed version of the input arguments</summary>
public class InputArguments
{
	#region Properties
	
	/// <summary>Set to true if the user is querying for the program to produce the help menu</summary>
	public bool IsHelp { get; set; } = false;
	/// <summary>Set to true if the user is querying for the program to produce a list</summary>
	public bool IsList { get; set; } = false;
	/// <summary>Set to true to include private members</summary>
	public bool IncludePrivate { get; set; } = false;
	/// <summary>The output file that the program will save to</summary>
	public string Output { get; set; } = null;
	/// <summary>The path to look into the type</summary>
	public string TypePath { get; set; } = "";
	/// <summary>The list of assemblies that the program should look into</summary>
	public List<string> Assemblies { get; private set; } = new List<string>();
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Creates a managed version of input arguments from the array of arguments</summary>
	/// <param name="args">The arguments the program started with</param>
	/// <returns>Returns a managed version of the input arguments</returns>
	public static InputArguments Create(string[] args)
	{
		InputArguments input = new InputArguments();
		
		for(int i = 0; i < args.Length; i++)
		{
			switch(args[i].ToLower())
			{
				case "-h": case "--help": { input.IsHelp = true; } break;
				case "-l": case "--list": { input.IsList = true; } break;
				case "-o": case "--out": { input.Output = args[++i]; } break;
				case "-p": case "--include-private": { input.IncludePrivate = true; } break;
				default:
				{
					if(input.TypePath == "" && !input.IsList)
					{
						input.TypePath = args[i];
					}
					else
					{
						input.Assemblies.Add(args[i]);
					}
				} break;
			}
		}
		
		if(input.Output == null)
		{
			input.Output = (input.IsList ? "listTypes.json" : "type.json");
		}
		input.IsHelp = (input.IsHelp || (!input.IsList && input.TypePath == ""));
		if(input.IsList && input.TypePath != "")
		{
			input.Assemblies.Add(input.TypePath);
			input.TypePath = "";
		}
		
		return input;
	}
	
	#endregion // Public Methods
}
