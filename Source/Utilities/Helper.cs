
namespace Taco.DocNET.Utilities;

using HandlebarsDotNet;

using System.Text.RegularExpressions;

using Taco.DocNET.Generators;
using Taco.DocNET.Inspector;

/// <summary>A class used for helping build html using handlebars</summary>
public class Helper
{
	#region Properties
	
	/// <summary>Gets the handlebars context</summary>
	public IHandlebars Handlebars { get; private set; }
	
	/// <summary>Gets and sets the properties for the project</summary>
	public Project Project { get; set; }
	
	/// <summary>Gets the information for the type currently being helped</summary>
	public TypeInfo Info { get; private set; }
	
	/// <summary>Gets and sets the theme used for generation</summary>
	public string Theme { get; set; }
	
	/// <summary>Gets and sets the type's path</summary>
	public string TypePath { get; set; }
	
	/// <summary>Gets and sets the type of view for the current page being rendered</summary>
	public string ViewType { get; set; }
	
	/// <summary>Gets the generator used to generate HTML</summary>
	public HtmlGenerator Generator { get; internal set; }
	
	/// <summary>A base constructor that sets up the helped within the HTML generator</summary>
	/// <param name="hbs">The handlebars context used</param>
	/// <param name="info">The information for the type being rendered</param>
	/// <param name="theme">The theme of the site</param>
	public Helper(IHandlebars hbs, TypeInfo info, string theme)
	{
		this.Handlebars = hbs;
		this.Info = info;
		this.Theme = theme;
		
		hbs.RegisterHelper("Func_Link", this.Link);
		hbs.RegisterHelper("Func_ToLower", this.ToLower);
		hbs.RegisterHelper("Func_RenderView", this.RenderView);
		hbs.RegisterHelper("Func_RenderContent", this.RenderContent);
		hbs.RegisterHelper("Func_LinkFavicon", this.SetFavicon);
		hbs.RegisterHelper("Func_SetupTitle", this.SetupTitle);
		hbs.RegisterHelper("Func_Replace", this.RegexReplace);
		hbs.RegisterHelper("Func_Equals", this.IfEquals);
		hbs.RegisterHelper("Func_NotEquals", this.IfNotEquals);
	}
	
	// TODO: Change this into a `.Clone(string)`
	/// <summary>A constructor that clones the helper</summary>
	/// <param name="original">The original helper class to clone from</param>
	/// <param name="viewType">The type of view the helper is trying to create</param>
	public Helper(Helper original, string viewType)
	{
		this.Handlebars = original.Handlebars;
		this.Info = original.Info;
		this.Theme = original.Theme;
		this.TypePath = original.TypePath;
		this.Project = original.Project;
		this.ViewType = viewType;
		this.Generator = original.Generator;
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>A helper function to correctly link a type path into an href link</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments put in. There are a total of 1 argument:
	/// 
	/// 0. `string` typePath: The type's path (i.e. System.Collection.Generic.List-1) that is used to link
	/// </param>
	public void Link(EncodedTextWriter output, Context context, Arguments arguments)
	{
		string newTypePath = arguments[0].ToString();
		string[] oldBreadcrumbs = this.TypePath.Split('.');
		string[] newBreadcrumbs = newTypePath.Split('.');
		int backSpaces = newBreadcrumbs.Length - 1;
		int size = oldBreadcrumbs.Length < newBreadcrumbs.Length
			? oldBreadcrumbs.Length
			: newBreadcrumbs.Length;
		
		for(int i = 0; i < size; ++i)
		{
			if(oldBreadcrumbs[i] == newBreadcrumbs[i])
			{
				--backSpaces;
			}
			else
			{
				break;
			}
		}
		
		newTypePath = "";
		
		for(int i = newBreadcrumbs.Length - backSpaces - 1; i < newBreadcrumbs.Length; ++i)
		{
			newTypePath += newBreadcrumbs[i];
		}
		
		if(newTypePath == ".html")
		{
			newTypePath = "index.html";
		}
		
		output.Write(newTypePath);
	}
	
	/// <summary>A helper function to make a string to lowercase</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments put in. There are a total of 1 argument:
	/// 
	/// 0. `string` value: The string used to convert into lower case
	/// </param>
	public void ToLower(EncodedTextWriter output, Context context, Arguments arguments) => output.Write(arguments[0].ToString().ToLower());
	
	/// <summary>A helper function to render other views of the generated HTML page</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments put in. There are a total of 1 argument:
	/// 
	/// 0. `string` view: The view type used to render specific views
	/// </param>
	public void RenderView(EncodedTextWriter output, Context context, Arguments arguments)
	{
		switch(arguments[0].ToString())
		{
			default: break;
			// default: case "type": output.Write(this.Generator.GenerateView(this.Theme, "type", this, this.ViewType), false); break;
		}
	}
	
	/// <summary>A helper function to render the `main-content` without having to create a mess in the handlebars file</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">The list of arguments to put in. There are a total of 0 arguments</param>
	public void RenderContent(EncodedTextWriter output, Context context, Arguments arguments)
	{
		if(this.ViewType == "type")
		{
			this.RenderView(output, context, new Arguments("type"));
		}
	}
	
	/// <summary>A helper function to setup the title without having to create a mess in the handlebars file</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">The list of arguments to put in. There are a total of 0 arguments</param>
	public void SetupTitle(EncodedTextWriter output, Context context, Arguments arguments)
	{
		if(this.ViewType == "type")
		{
			string path = Regex.Replace(this.TypePath, @".*[\/\.]([^\.\/]+)$", "$1");
			
			output.Write($"<title>{path} | {this.Project.Name} API</title>", false);
		}
		else if(this.ViewType == "namespace")
		{
			output.Write($"<title>{this.TypePath} | {this.Project.Name} API</title>", false);
		}
		else
		{
			output.Write($"<title>{this.Project.Name} API</title>", false);
		}
	}
	
	/// <summary>A helper function to setup the favicon</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">The list of arguments to put in. There are a total of 0 arguments</param>
	public void SetFavicon(EncodedTextWriter output, Context context, Arguments arguments)
	{
		if(!string.IsNullOrEmpty(this.Project.FavIcon))
		{
			output.Write(
				@$"<link rel=""icon"" href=""{this.Project.FavIcon}""/>",
				false
			);
		}
	}
	
	/// <summary>A helper function used to replace a string using regex</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments to put in. There are a total of 3 arguments:
	/// 
	/// 0. `string` input: The input string to replace from.
	/// 1. `string` pattern: The pattern string to use to replace.
	/// 2. `string` replacement: The replacement string
	/// </param>
	public void RegexReplace(EncodedTextWriter output, Context context, Arguments arguments)
	{
		output.Write(Regex.Replace(
			arguments[0].ToString(),
			arguments[1].ToString(),
			arguments[2].ToString()
		), true);
	}
	
	/// <summary>A helper function used to check if two contents are equal</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="options">The options used to complete or inverse</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments to put in. There are a total of 2 arguments:
	/// 
	/// 0. `object` left: The left-hand side to check with
	/// 1. `object` right: The right-hand side to check with
	/// </param>
	public void IfEquals(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
	{
		if(arguments[0] == arguments[1])
		{
			options.Template(in output, in context);
		}
		else
		{
			options.Inverse(in output, in context);
		}
	}
	
	/// <summary>A helper function used to check if two contents are not equal</summary>
	/// <param name="output">The output to write the html file</param>
	/// <param name="options">The options used to complete or inverse</param>
	/// <param name="context">The context from coming from handlebars, this would be the helper class</param>
	/// <param name="arguments">
	/// The list of arguments to put in. There are a total of 2 arguments:
	/// 
	/// 0. `object` left: The left-hand side to check with
	/// 1. `object` right: The right-hand side to check with
	/// </param>
	public void IfNotEquals(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
	{
		if(arguments[0] != arguments[1])
		{
			options.Template(in output, in context);
		}
		else
		{
			options.Inverse(in output, in context);
		}
	}
	
	#endregion // Public Methods
}
