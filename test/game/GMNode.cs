
using Godot;
using Godot.Collections;

using Nova;

namespace Tactics.Internal
{
	public partial class GMNode : Node
	{
		#region Properties
		
		[Export] public CreatureSheet Sheet { get; set; }
		
		public static GMNode Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
		public override void _EnterTree()
		{
			if(Instance == null)
			{
				Instance = this;
			}
			else
			{
				this.QueueFree();
				return;
			}
			base._EnterTree();
		}
		
		public override void _ExitTree()
		{
			if(Instance == this)
			{
				Instance = null;
			}
			base._ExitTree();
		}
		
		#endregion // Godot Methods
	}
}

namespace Tactics
{
	using Tactics.Internal;
	
	public static class GM
	{
		#region Properties
		
		public static GMNode Singleton => GMNode.Instance;
		
		public static CreatureSheet Sheet
		{
			get => GMNode.Instance.Sheet;
			set => GMNode.Instance.Sheet = value;
		}
		
		#endregion // Properties
	}
}
