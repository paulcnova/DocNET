
namespace Nova.Boot;

using Nova.UI;

using Godot;
using Godot.Collections;
using System.Linq;


/// <summary>A node used for startup to load resources in on boot.</summary>
public partial class BootLoader : Node
{
	#region Properties
	
	/// <summary>The loading screen being used</summary>
	private LoadingScreen loadingScreen;
	
	/// <summary>Gets the starting scene to load in after the boot load has finished.</summary>
	[Export] public PackedScene StartScene { get; private set; }
	
	/// <summary>Gets the default loading screen used when loading.</summary>
	[Export] public PackedScene DefaultLoadScreen { get; private set; }
	
	/// <summary>Gets the node that will contain the loading screen and current scene.</summary>
	[Export] public Node SceneContainer { get; private set; }
	
	[ExportGroup("Debug")]
	[Export] public Array<Node> SceneStack { get; private set; } = new Array<Node>();
	
	[Signal] public delegate void OnScenePushedEventHandler(Node scene);
	[Signal] public delegate void OnScenePoppedEventHandler(Node scene);
	
	public static BootLoader Instance { get; private set; }
	
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
	
	/// <inheritdoc/>
	public override void _Ready()
	{
		LoadingScreen loading = this.DefaultLoadScreen.Instantiate<LoadingScreen>();
		
		this.loadingScreen = loading;
		this.SceneContainer.AddChild(loading);
		this.SceneStack.Add(loading);
		
		DRML.ContentLoaded += this.OnContentLoaded;
		DRML.LoadingCompleted += this.OnLoadingCompleted;
		DRML.LoadAllContent();
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void PushScene(string path) => this.PushScene(ResourceLoader.Load<PackedScene>(path));
	public void PushScene(PackedScene scene)
	{
		Node node = scene.Instantiate<Node>();
		
		if(this.SceneStack.Count > 0)
		{
			Node prev = this.SceneStack[this.SceneStack.Count - 1];
			
			if(prev is CanvasItem n2d) { n2d.SetActive(false); }
			else if(prev is Node3D n3d) { n3d.SetActive(false); }
		}
		this.SceneContainer.AddChild(node);
		this.SceneStack.Add(node);
		this.EmitSignalOnScenePushed(node);
	}
	
	public void PopScene()
	{
		if(this.SceneStack.Count == 0) { return; }
		
		Node node = this.SceneStack[this.SceneStack.Count - 1];
		
		this.SceneContainer.RemoveChild(node);
		this.SceneStack.RemoveAt(this.SceneStack.Count - 1);
		if(this.SceneStack.Count > 0)
		{
			Node prev = this.SceneStack[this.SceneStack.Count - 1];
			
			if(prev is CanvasItem n2d) { n2d.SetActive(true); }
			else if(prev is Node3D n3d) { n3d.SetActive(true); }
			this.EmitSignalOnScenePopped(prev);
		}
	}
	
	/// <summary>Loads the scene from the given resource path.</summary>
	/// <param name="path">The path to the scene in the resources.</param>
	/// <remarks>Unloads the current scene.</remarks>
	public void SetScene(string path) => this.SetScene(ResourceLoader.Load<PackedScene>(path));
	
	/// <summary>Loads the scene from the given packed scene.</summary>
	/// <param name="scene">The scene to instantiate and load.</param>
	/// <remarks>Unloads the current scene.</remarks>
	public void SetScene(PackedScene scene)
	{
		Node node = scene.Instantiate<Node>();
		
		this.SceneContainer.QueueFreeChildren();
		this.SceneStack.Clear();
		this.SceneContainer.AddChild(node);
		this.SceneStack.Add(node);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Called when content is being loaded.</summary>
	/// <param name="resource">The resource being loaded in.</param>
	/// <param name="current">The current count of resources being loaded in.</param>
	/// <param name="max">The maximum count of resources being loaded in.</param>
	private void OnContentLoaded(DisplayableResource resource, int current, int max)
	{
		if(this.loadingScreen == null) { return; }
		
		this.loadingScreen.UpdateLoadingBar(resource, current, max);
	}
	
	/// <summary>Called when the content has done being loaded.</summary>
	private void OnLoadingCompleted()
	{
		if(this.loadingScreen == null) { return; }
		
		this.loadingScreen.LoadingIsCompleted(Callable.From(this.ChangeSceneToStart));
	}
	
	/// <summary>Changes the scene to the <see cref="StartScene"/>.</summary>
	private void ChangeSceneToStart()
	{
		this.SceneContainer.RemoveChild(this.loadingScreen);
		this.loadingScreen = null;
		
		Node start = this.StartScene.Instantiate<Node>();
		
		this.SceneContainer.AddChild(start);
		this.SceneStack.Add(start);
	}
	
	#endregion // Private Methods
}
