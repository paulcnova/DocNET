
namespace DocNET.Generators;

using System.IO;

public class GeneratedDocumentation
{
	#region Properties
	
	public string Content { get; set; }
	public string FileName { get; set; }
	public string FileExtension { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual void Save(ProjectEnvironment environment)
	{
		string saveFileName = Path.Combine(environment.OutputDirectory, $"{this.FileName}{this.FileExtension}");
		
		if(!Directory.Exists(environment.OutputDirectory))
		{
			Directory.CreateDirectory(environment.OutputDirectory);
		}
		File.WriteAllText(saveFileName, this.Content);
	}
	
	#endregion // Public Methods
}
