
namespace Taco.DocNET;

using System.Collections.Generic;

using Taco.DocNET.Generators;
using Taco.DocNET.Utilities;

/// <summary>A static class where the program starts from</summary>
public static class Program
{
	#region Public Methods
	
	/// <summary>The starting point for the program</summary>
	/// <param name="args">The list of arguments put in by the user</param>
	public static void Main(string[] args)
	{
		try
		{
			List<DllXmlPair> documents = DotNETBuilder.InjectBuildRestore();
			
			foreach(DllXmlPair document in documents)
			{
				System.Console.WriteLine("XML: " + document.XmlAbsolutePath);
				System.Console.WriteLine("DLL: " + document.DllAbsolutePath);
				DocumentationGenerator generator = new DocumentationGenerator();
				
				generator.Generate("example", "docs", document);
			}
			
			// System.Console.WriteLine(DotNETBuilder.BuildProject());
		}
		catch(System.Exception e)
		{
			System.Console.WriteLine(e);
		}
		// System.Console.WriteLine(args[0]);
		
		// string cwd = System.Environment.CurrentDirectory;
		// string content = File.ReadAllText(Path.Combine(cwd, args[0]));
		
		// System.Console.WriteLine(content);
		
		// string html = Markdown.ToHtml(content, pipeline);
		
		// System.Console.WriteLine(html);
		// html = @$"
		// <!DOCTYPE html>
		// <html>
		// <head>
		// <link href=""prism.css"" rel=""stylesheet""/>
		// </head>
		// <body>
		// {html}
		// <script src=""prism.js""></script>
		// </body>
		// </html>";
		// File.WriteAllText("test.html", html);
	}
	
	#endregion // Public Methods
}
