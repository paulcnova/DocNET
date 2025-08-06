
namespace DocNET.Generators;

using DocNET.Information;
using DocNET.Inspections;
using DocNET.Linking;

public sealed class StaticHTMLGenerator : IGenerator
{
	#region Public Methods
	
	public GeneratedDocumentation Generate(LinkedMember member)
	{
		switch(member.MemberType)
		{
			case LinkedMember.Type.Type: return this.GenerateType(member);
		}
		
		return null;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private GeneratedDocumentation GenerateType(LinkedMember member)
	{
		TypeInspection details = member.TypeInspection;
		InformationElement info = member.Info;
		NodeFlattener flattener = new NodeFlattener(member.Document, member.SiteMap);
		
		if(info == null)
		{
			return new GeneratedDocumentation()
			{
				Content = "",
				FileExtension = ".html",
				FileName = details.Info.FullName,
			};
		}
		
		return new GeneratedDocumentation()
		{
			Content = $"""
			<div class="type member">
				<h1>{details.Info.FullName}</h1>
				<p>{info.StringifySummary(flattener)}</p>
			</div>
			""",
			FileName = details.Info.FullName,
			FileExtension = ".html",
		};
	}
	
	#endregion // Private Methods
}
