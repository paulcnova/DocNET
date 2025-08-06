
namespace Tactics.Controls.Features;

using Godot;

using Nova;

using Tactics.Actions;

[GlobalClass] public partial class ActionControllerFeature : ToggleableNode3D
{
	#region Properties
	
	public const string MID_MinRange = "min_range";
	public const string MID_MaxRange = "max_range";
	public const string MID_Speed = "speed";
	
	[Export] public ActionController Controller { get; set; }
	
	[ExportGroup("Debug")]
	[Export] public bool IsValid { get; set; } = true;
	
	public string ID => this.GetType().Name;
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual void Setup(ActionController controller, ActionState state, int stageIndex, Vector3 position) {}
	
	public virtual T GetData<[MustBeVariant] T>(GodotObject obj, int stageIndex, string id, T defaultData = default)
	{
		if(obj == null) { return defaultData; }
		
		string correctID = $"{id}_{stageIndex}";
		
		// If it's not `range_0`, then it could be `range`.
		if(!obj.HasMeta(correctID)) { correctID = id; }
		// If it's not `range`, then the parameter doesn't exist.
		if(!obj.HasMeta(correctID)) { return defaultData; }
		
		Variant data = obj.GetMeta(correctID, Variant.From(defaultData));
		
		if(data.VariantType == Variant.Type.String && data.AsString().StartsWith('@'))
		{
			return obj.Evaluate(data.AsString()).As<T>();
		}
		return data.As<T>();
	}
	
	#endregion // Public Methods
}
