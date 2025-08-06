
namespace Tactics;

using Godot;
using Godot.Collections;

using System.Text.RegularExpressions;

public static class Extension_GodotObject
{
	#region Public Methods
	
	public static void TransformMeta(this GodotObject obj)
	{
		Array<StringName> keys = obj.GetMetaList();
		
		foreach(StringName key in keys)
		{
			if(key.IsEmpty) { continue; }
			
			string sk = key.ToString();
			Variant meta = obj.GetMeta(sk);
			
			if(meta.VariantType == Variant.Type.String && meta.AsString().StartsWith('@'))
			{
				Variant evaluatedMeta = obj.Query(meta.AsString());
				
				if(evaluatedMeta.VariantType != Variant.Type.Nil)
				{
					obj.SetMeta(key, evaluatedMeta);
				}
			}
		}
	}
	
	public static Variant Evaluate(this GodotObject obj, string expression)
	{
		string evaluated = Regex.Replace(expression, @"((?:@SB|@GM|@MD)[\.a-zA-Z_]+(?:\([^\)]+\))?)", match => {
			return Query(obj, match.Value).ToString();
		}).Trim();
		System.Data.DataTable table = new System.Data.DataTable();
		object computed = table.Compute(evaluated, "");
		
		if(computed is int) { return Variant.From((int)computed); }
		if(computed is float || computed is double || computed is decimal) { return Variant.From(((float)((decimal)computed))); }
		
		return new Variant();
	}
	
	public static Variant Query(this GodotObject obj, string query)
	{
		if(!query.StartsWith('@')) { return new Variant(); }
		
		Array<string> parts = new Array<string>();
		int prevIndex = 0;
		int scope = 0;
		
		for(int i = 1; i < query.Length; ++i)
		{
			if(query[i] == '(') { ++scope; }
			if(query[i] == ')') { --scope; }
			if(scope == 0 && query[i] == '.')
			{
				parts.Add(query.Substring(prevIndex, i - prevIndex));
				prevIndex = i + 1;
			}
		}
		parts.Add(query.Substring(prevIndex));
		
		if(parts.Count < 2) { return new Variant(); }
		if(parts[0] == "@MD")
		{
			Variant data = obj.GetMeta(parts[1]);
			
			if(data.VariantType == Variant.Type.String && data.AsString().StartsWith('@'))
			{
				return Query(obj, data.AsString());
			}
			return data;
		}
		
		GodotObject inquirer;
		
		switch(parts[0])
		{
			default: inquirer = (Engine.GetMainLoop() as SceneTree).Root.GetNode(parts[0].Substring(1)); break;
			case "@GM": inquirer = GM.Singleton; break;
			case "@SB": inquirer = GM.Sheet; break;
		}
		
		if(inquirer == null) { return new Variant(); }
		
		int indexOpen = parts[1].IndexOf('(');
		int indexClose = parts[1].LastIndexOf(')') + 1;
		bool isMethod = indexOpen > -1;
		string parameters = isMethod ? parts[1].Substring(indexOpen, indexClose - indexOpen) : "";
		// TODO: Parse parameters here.
		Variant response = isMethod
			? inquirer.Call(parts[1].Remove(indexOpen, indexClose - indexOpen))
			: inquirer.Get(parts[1]);
		
		for(int i = 2; i < parts.Count; ++i)
		{
			if(response.VariantType == Variant.Type.Nil) { return new Variant(); }
			// TODO: Evaluate primitive types here.
			if(response.VariantType != Variant.Type.Object) { break; }
			
			indexOpen = parts[i].IndexOf('(');
			indexClose = parts[i].LastIndexOf(')') + 1;
			isMethod = indexOpen > -1;
			parameters = isMethod ? parts[i].Substring(indexOpen, indexClose - indexOpen) : "";
			// TODO: Parse parameters here.
			response = isMethod
				? response.AsGodotObject().Call(parts[i].Remove(indexOpen, indexClose - indexOpen))
				: response.AsGodotObject().Get(parts[i]);
		}
		
		return response;
	}
	
	#endregion // Public Methods
}
