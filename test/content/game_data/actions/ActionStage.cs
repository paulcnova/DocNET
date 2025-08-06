
namespace Tactics.Actions;

using Godot;
using Godot.Collections;

using Nova;

[GlobalClass, Tool] public partial class ActionStage : Resource
{
	#region Properties
	
	[Export] public string Name { get; set; }
	[Export(PropertyHint.TypeString)] public Array<string> FeaturesToEnable { get; private set; } = new Array<string>();
	
	#endregion // Properties
	
	#region Godot methods
	
	public override void _ValidateProperty(Dictionary property)
	{
		if(property["name"].AsString() == nameof(this.FeaturesToEnable))
		{
			property["hint"] = (int)PropertyHint.TypeString;
			property["hint_string"] = $"{Variant.Type.String:D}/{PropertyHint.TypeString:D}:ActionControllerFeature";
		}
	}
	
	#endregion // Godot methods
	
	#region Public Methods
	
	public virtual string GetDefaultName() => "";
	
	public virtual Array<string> GetFeaturesToEnable()
	{
		Array<string> features = this.FeaturesToEnable;
		
		return features;
	}
	
	#endregion // Public Methods
}
