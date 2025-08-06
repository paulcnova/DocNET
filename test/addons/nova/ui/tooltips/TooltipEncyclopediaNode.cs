
using Godot;

namespace Nova.Tooltips
{
	/// <summary>A node that holds all the data to displayable resources for use in tooltips.</summary>
	public partial class TooltipEncyclopediaNode : Node
	{
		#region Properties
		
		private const string GameDataPath = "res://content/game_data/";
		
		/// <summary>Gets the static instance to this node.</summary>
		public static TooltipEncyclopediaNode Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
		/// <inheritdoc/>
		public override void _Ready()
		{
			DRML.ContentLoaded += this.FindTooltips;
		}
		
		/// <inheritdoc/>
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
		
		/// <inheritdoc/>
		public override void _ExitTree()
		{
			if(Instance == this)
			{
				Instance = null;
			}
			base._ExitTree();
		}
		
		#endregion // Godot Methods
		
		#region Public Methods
		
		/// <summary>Creates a tooltip.</summary>
		/// <param name="tooltipID">The ID for the tooltip entry.</param>
		/// <returns>Returns the tooltip.</returns>
		public BaseTooltipUI CreateTooltip(string tooltipID)
		{
			DisplayableResource resource = TooltipEncyclopedia.FindEntry(tooltipID);
			
			if(resource == null) { return null; }
			
			BaseTooltipUI tooltip = BaseTooltipUI.Create(
				tooltipID,
				!string.IsNullOrEmpty(resource.OverrideTooltipCategory)
					? resource.OverrideTooltipCategory
					: resource.TooltipCategory
			);
			
			if(tooltip == null) { return null; }
			
			return tooltip;
		}
		
		#endregion // Public Methods
		
		#region Private Methods
		
		private void FindTooltips(DisplayableResource resource, int curr, int max)
		{
			if(string.IsNullOrEmpty(resource.TooltipID)) { return; }
			
			string[] paths = resource.TooltipID.Split('/');
			int i = 0;
			Node current = this;
			
			foreach(string path in paths)
			{
				if(i == paths.Length - 1) { break; }
				
				Node next = current.GetNodeOrNull(path);
				
				if(next == null)
				{
					Node folder = new Node();
					
					folder.Name = path;
					current.AddChild(folder);
					next = folder;
				}
				
				current = next;
				++i;
			}
			
			TooltipEntryNode entryNode = new TooltipEntryNode();
			
			entryNode.TooltipPath = resource.ResourcePath;
			entryNode.Name = paths[paths.Length - 1];
			current.AddChild(entryNode);
		}
		
		#endregion // Private Methods
	}
}

namespace Nova
{
	using Nova.Tooltips;
	
	/// <summary>A static class that holds all the data for tooltips to use.</summary>
	public static class TooltipEncyclopedia
	{
		#region Public Methods
		
		/// <inheritdoc cref="TooltipEncyclopediaNode.CreateTooltip(string)"/>
		public static BaseTooltipUI CreateTooltip(string tooltipID)
		{
			if(TooltipEncyclopediaNode.Instance == null)
			{
				GDX.PrintError($"Tooltip Encyclopedia isn't instantiated! Could not find tooltip {tooltipID}");
				return null;
			}
			
			return TooltipEncyclopediaNode.Instance.CreateTooltip(tooltipID);
		}
		
		/// <summary>Finds the entry given the ID.</summary>
		/// <param name="entryID">The tooltip entry ID.</param>
		/// <returns>Returns the displayable resource that's associated with the ID. If nothing was found, then it returns null.</returns>
		public static DisplayableResource FindEntry(string entryID)
		{
			if(TooltipEncyclopediaNode.Instance == null)
			{
				GDX.PrintError($"Tooltip Encyclopedia isn't instantiated! Could not find entry {entryID}");
				return null;
			}
			
			string correctedID = entryID.Replace("-", "/").ToLower();
			TooltipEntryNode entryNode = TooltipEncyclopediaNode.Instance.GetNodeOrNull<TooltipEntryNode>(correctedID);
			
			if(entryNode == null)
			{
				GDX.PrintError($"Entry {entryID} does not exist. Path {correctedID} does not exist");
				return null;
			}
			
			return ResourceLoader.Load<DisplayableResource>(entryNode.TooltipPath);
		}
		
		#endregion // Public Methods
	}
}
