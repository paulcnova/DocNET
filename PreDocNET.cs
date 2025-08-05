
namespace DocNET.Tasks;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using System.IO;

public class PreDocNET : Task
{
	#region Properties
	
	[Required] public string OutputPath { get; set; }
	[Required] public string AssemblyName { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override bool Execute()
	{
		System.Console.WriteLine("ASDF");
		this.BuildEngine4.RegisterTaskObject(
			"GenerateDocumentationFile",
			"true",
			RegisteredTaskObjectLifetime.Build,
			allowEarlyCollection: false
		);
		this.BuildEngine4.RegisterTaskObject(
			"DocumentationFile",
			$"{this.OutputPath}{this.AssemblyName}.xml",
			RegisteredTaskObjectLifetime.Build,
			allowEarlyCollection: false
		);
		return true;
	}
	
	#endregion // Public Methods
}
