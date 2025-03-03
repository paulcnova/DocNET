
namespace DocNET.Utilities;

using System.IO;

/// <summary>A static class used for utility</summary>
public static class Utility
{
	#region Properties
	
	/// <summary>The markdown pipeline used to render for prism</summary>
	// private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
	// 	.UseAdvancedExtensions()
	// 	.UsePrism()
	// 	.Build();
	
	public static IUtilitySet UtilitySet { get; set; } = new HtmlUtilitySet();
	
	/// <summary>Gets the absolute path this program's place in the user's `APPDATA` folder</summary>
	public static string AppDataPath => Path.Combine(GetAppDataPath(), "DocNET");
	
	/// <summary>Gets the absolute path to the temp folder used by this program</summary>
	public static string TempPath => Path.Combine(AppDataPath, "temp");
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <inheritdoc cref="IUtilitySet.CreateSystemLink(string, string)"/>
	public static string CreateSystemLink(string typePath, string linkName) => UtilitySet?.CreateSystemLink(typePath, linkName) ?? "";
	/// <inheritdoc cref="IUtilitySet.CreateInternalLink(string, string)"/>
	public static string CreateInternalLink(string typePath, string linkName) => UtilitySet?.CreateInternalLink(typePath, linkName) ?? "";
	
	/// <summary>Renders the given markdown</summary>
	/// <param name="markdown">The markdown content to render</param>
	/// <returns>Returns the rendered markdown</returns>
	public static string RenderMarkdown(string markdown) => markdown;//Markdown.ToHtml(markdown, Pipeline);
	
	/// <summary>Ensures the path by making any directories if they don't exist yet</summary>
	/// <param name="path">The folder path to ensure it's path</param>
	public static void EnsurePath(string path)
	{
		if(!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
	
	/// <summary>Writes to the content, making sure it can override it without error</summary>
	/// <param name="path">The file path to the file</param>
	/// <param name="content">The content to write</param>
	public static void Write(string path, string content)
	{
		EnsurePath(path);
		if(File.Exists(path))
		{
			File.Delete(path);
		}
		
		File.WriteAllText(path, content);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the path to the user's `APPDATA` folder</summary>
	/// <returns>Returns the path to the user's `APPDATA` folder</returns>
	private static string GetAppDataPath()
	{
		return System.Environment.GetEnvironmentVariable("APPDATA");
	}
	
	#endregion // Private Methods
}
