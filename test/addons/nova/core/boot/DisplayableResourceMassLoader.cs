
using Godot;
using Godot.Collections;

using Nova.Boot;

using System.Collections.Generic;

namespace Nova.Internal
{
	/// <summary>A global node that helps mass load in displayable resources used on boot.</summary>
	public partial class DisplayableResourceMassLoader : Node
	{
		#region Properties
		
		/// <summary>The default path to load the displayable resources from.</summary>
		private const string GameDataPath = "res://content/game_data/";
		
		/// <summary>An event for when content gets loaded.</summary>
		/// <param name="resource">The resource being loaded in.</param>
		/// <param name="current">The current count of resources being loaded in.</param>
		/// <param name="max">The maximum count of resources being loaded in.</param>
		[Signal] public delegate void ContentLoadedEventHandler(DisplayableResource resource, int current, int max);
		
		/// <summary>An event for when the loading has been completed.</summary>
		[Signal] public delegate void LoadingCompletedEventHandler();
		
		/// <summary>Gets the static instance for this class</summary>
		public static DisplayableResourceMassLoader Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
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
		public override void _Ready()
		{
			BootLoader boot = this.GetTree().Root.GetNodeOrNull<BootLoader>("Boot");
			
			if(boot == null)
			{
				this.LoadAllContent(100, GameDataPath);
			}
			base._Ready();
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
		
		/// <summary>Loads in all the content from the given paths asynchronously. Calls  when the resource has been loaded in.</summary>
		/// <param name="batchLoad">The amount of content to load in batches for the loading screen to update in between.</param>
		/// <param name="paths">The array of paths to load from.</param>
		public void LoadAllContent(int batchLoad = 100, params string[] paths)
		{
			Timing.RunCoroutine(this.LoadAllContentAsync(batchLoad, paths));
		}
		
		#endregion // Public Methods
		
		#region Private Methods
		
		/// <summary>Loads in all the content from the given paths asynchronously. Calls  when the resource has been loaded in.</summary>
		/// <param name="batchLoad">The amount of content to load in batches for the loading screen to update in between.</param>
		/// <param name="paths">The array of paths to load from.</param>
		public IEnumerator<double> LoadAllContentAsync(int batchLoad, string[] paths)
		{
			Array<DisplayableResource> resources = new Array<DisplayableResource>();
			
			if(paths.Length == 0)
			{
				paths = new string[] { GameDataPath };
			}
			
			foreach(string path in paths)
			{
				resources.AddRange(ResourceLocator.LoadAll<DisplayableResource>(path));
			}
			yield return Timing.WaitForOneFrame;
			
			int current = 0;
			int max = resources.Count;
			
			foreach(DisplayableResource resource in resources)
			{
				this.EmitSignal(SignalName.ContentLoaded, resource, current++, max);
				if(current % batchLoad == 0)
				{
					yield return Timing.WaitForOneFrame;
				}
			}
			this.EmitSignal(SignalName.LoadingCompleted);
			yield return Timing.WaitForOneFrame;
		}
		
		#endregion // Private Methods
	}
}

namespace Nova
{
	using Nova.Internal;
	
	/// <summary>A singleton to the <see cref="DisplayableResourceMassLoader"/> class that helps mass load in displayable resources on boot.</summary>
	public static class DRML
	{
		#region Properties
		
		/// <inheritdoc cref="DisplayableResourceMassLoader.ContentLoaded"/>
		public static event DisplayableResourceMassLoader.ContentLoadedEventHandler ContentLoaded
		{
			add
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not listen to load content");
					return;
				}
				DisplayableResourceMassLoader.Instance.ContentLoaded += value;
			}
			remove
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not stop listening to load content");
					return;
				}
				DisplayableResourceMassLoader.Instance.ContentLoaded -= value;
			}
		}
		
		/// <inheritdoc cref="DisplayableResourceMassLoader.LoadingCompleted"/>
		public static event DisplayableResourceMassLoader.LoadingCompletedEventHandler LoadingCompleted
		{
			add
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not listen to completed loading");
					return;
				}
				DisplayableResourceMassLoader.Instance.LoadingCompleted += value;
			}
			remove
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not stop listening to completed loading");
					return;
				}
				DisplayableResourceMassLoader.Instance.LoadingCompleted -= value;
			}
		}
		
		#endregion // Properties
		
		#region Public Methods
		
		/// <summary>Loads in all the content from the given paths. Calls <see cref="ContentLoaded"/> when the resource has been loaded in.</summary>
		/// <param name="batchLoad">The amount of content to load in batches for the loading screen to update in between.</param>
		/// <param name="paths">The array of paths to load from.</param>
		public static void LoadAllContent(int batchLoad = 100, params string[] paths)
		{
			if(DisplayableResourceMassLoader.Instance == null)
			{
				GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not load all content");
				return;
			}
			DisplayableResourceMassLoader.Instance.LoadAllContent(batchLoad, paths);
		}
		
		#endregion // Public Methods
	}
}
