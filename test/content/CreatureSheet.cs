
namespace Tactics;

using Godot;
using Godot.Collections;

using Nova;

[GlobalClass] public partial class CreatureSheet : StatBlock
{
	#region Properties
	
	[Export] public int MaxHealth { get; private set; }
	[Export] public Array<Attribute> AttributeBoosts { get; private set; } = new Array<Attribute>();
	[Export] public Array<Attribute> AttributeFlaws { get; private set; } = new Array<Attribute>();
	[Export] public Array<string> Traits { get; private set; } = new Array<string>();
	
	[ExportGroup("Overrides")]
	[ExportSubgroup("Attributes")]
	[Export] private bool OverrideStrengthModifier { get; set; } = false;
	[Export] private int StrengthModifierOverrideValue { get; set; } = 0;
	[Export] private bool OverrideDexterityModifier { get; set; } = false;
	[Export] private int DexterityModifierOverrideValue { get; set; } = 0;
	[Export] private bool OverrideConstitutionModifier { get; set; } = false;
	[Export] private int ConstitutionModifierOverrideValue { get; set; } = 0;
	[Export] private bool OverrideIntelligenceModifier { get; set; } = false;
	[Export] private int IntelligenceModifierOverrideValue { get; set; } = 0;
	[Export] private bool OverrideWisdomModifier { get; set; } = false;
	[Export] private int WisdomModifierOverrideValue { get; set; } = 0;
	[Export] private bool OverrideCharismaModifier { get; set; } = false;
	[Export] private int CharismaModifierOverrideValue { get; set; } = 0;
	
	public int StrengthModifier => !this.OverrideStrengthModifier ? this.GetModifier(Attribute.Strength) : this.StrengthModifierOverrideValue;
	public int DexterityModifier => !this.OverrideDexterityModifier ? this.GetModifier(Attribute.Dexterity) : this.DexterityModifierOverrideValue;
	public int ConstitutionModifier => !this.OverrideConstitutionModifier ? this.GetModifier(Attribute.Constitution) : this.ConstitutionModifierOverrideValue;
	public int IntelligenceModifier => !this.OverrideIntelligenceModifier ? this.GetModifier(Attribute.Intelligence) : this.IntelligenceModifierOverrideValue;
	public int WisdomModifier => !this.OverrideWisdomModifier ? this.GetModifier(Attribute.Wisdom) : this.WisdomModifierOverrideValue;
	public int CharismaModifier => !this.OverrideCharismaModifier ? this.GetModifier(Attribute.Charisma) : this.CharismaModifierOverrideValue;
	
	#endregion // Properties
	
	#region Public Methods
	
	public int GetModifier(Attribute attribute)
	{
		if(attribute == Attribute.Free) { return 0; }
		
		int value = 0;
		
		foreach(Attribute attr in this.AttributeBoosts)
		{
			if(attr == attribute) { ++value; }
		}
		
		foreach(Attribute attr in this.AttributeFlaws)
		{
			if(attr == attribute) { --value; }
		}
		
		return value > 4
			? 4 + ((value - 4) / 2)
			: value;
	}
	
	#endregion // Public Methods
}
