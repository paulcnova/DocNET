
namespace Tactics;

using Godot;

using Nova;

public enum Attribute
{
	Free,
	Strength,
	Dexterity,
	Constitution,
	Intelligence,
	Wisdom,
	Charisma,
}

internal static class Extension_Attribute
{
	public static Control Instantiate(this Attribute attr)
	{
		return GDX.Instantiate<Control>(attr switch
		{
			Attribute.Free => "res://interface/components/attributes/free_attribute.tscn",
			Attribute.Strength => "res://interface/components/attributes/str_attribute.tscn",
			Attribute.Dexterity => "res://interface/components/attributes/dex_attribute.tscn",
			Attribute.Constitution => "res://interface/components/attributes/con_attribute.tscn",
			Attribute.Intelligence => "res://interface/components/attributes/int_attribute.tscn",
			Attribute.Wisdom => "res://interface/components/attributes/wis_attribute.tscn",
			Attribute.Charisma => "res://interface/components/attributes/cha_attribute.tscn",
			_ => "res://interface/components/attributes/unknown_attribute.tscn"
		});
	}
}