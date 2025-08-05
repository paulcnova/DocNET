
namespace DocNET.Tasks;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using System.IO;

public class DocNET : Task
{
	#region Properties
	
	[Required] public string AssemblyDir { get; set; }
	[Required] public string OriginalAssemblyDir { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override bool Execute()
	{
		string[] files = Directory.GetFiles(this.AssemblyDir, "*.dll", SearchOption.AllDirectories);
		
		foreach(string file in files)
		{
			System.Console.WriteLine(file);
		}
		System.Console.WriteLine(this.OriginalAssemblyDir);
		
		return true;
	}
	
	#endregion // Public Methods
}
