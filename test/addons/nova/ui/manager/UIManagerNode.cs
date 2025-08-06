
using Godot;

using System.Collections.Generic;
using System.Reflection;

namespace Nova.UI
{
	/// <summary>A node that manager the UI</summary>
	[SceneLocation("res://addons/nova/ui/manager/ui_manager_template.tscn")]
	public sealed partial class UIManagerNode : CanvasLayer
	{
		#region Properties
		
		private Dictionary<System.Type, UIControl> dataConnections = new Dictionary<System.Type, UIControl>();
		/// <summary>The mapping of types into pages.</summary>
		private Dictionary<System.Type, Page> pages = new Dictionary<System.Type, Page>();
		/// <summary>The mapping of types into widgets.</summary>
		private Dictionary<System.Type, Widget> widgets = new Dictionary<System.Type, Widget>();
		/// <summary>The set of widgets currently being shown on screen.</summary>
		private HashSet<System.Type> widgetsShown = new HashSet<System.Type>();
		/// <summary>The stack of previous pages visited.</summary>
		private Stack<System.Type> history = new Stack<System.Type>();
		/// <summary>The stack of next pages that the user went back from.</summary>
		private Stack<System.Type> future = new Stack<System.Type>();
		/// <summary>Set to true to ignore adding pages into the history stack.</summary>
		private bool ignoreAddingToHistory = false;
		
		/// <summary>Gets and sets if the UI Manager should dynamically instantiate pages and widgets when retrieving the page or the widget.</summary>
		[Export] public bool DynamicallyInstantiate { get; set; } = true;
		
		/// <summary>Gets the starting page that first appears on start up.</summary>
		[Export] public Page StartingPage { get; private set; }
		
		/// <summary>Gets the container where the pages are located.</summary>
		[Export] public Control PagesContainer { get; private set; }
		
		/// <summary>Gets the container where the widgets are located.</summary>
		[Export] public Control WidgetsContainer { get; private set; }
		
		/// <summary>Gets the current view type.</summary>
		[Export] public ViewType ViewType { get; private set; }= ViewType.Keyboard;
		
		/// <summary>Gets the current page being displayed.</summary>
		[ExportGroup("Debug")]
		[Export] public Page CurrentPage { get; private set; }
		
		/// <summary>Gets the previous page from the history.</summary>
		public Page PreviousPage => this.history.Count > 0
			? this.GetPage(this.history.Peek())
			: null;
		
		/// <summary>Gets the next page from the future.</summary>
		public Page NextPage => this.future.Count > 0
			? this.GetPage(this.future.Peek())
			: null;
		
		/// <summary>Gets the instance of the UI Manager</summary>
		public static UIManagerNode Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
		/// <inheritdoc/>
		public override void _EnterTree()
		{
			if(Instance == null)
			{
				Instance = this;
				this.dataConnections = new Dictionary<System.Type, UIControl>();
			}
			else
			{
				this.QueueFree();
				return;
			}
			this.OnEnterTree();
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
		
		/// <inheritdoc/>
		public override void _Input(InputEvent ev)
		{
			if(ev is InputEventJoypadButton)
			{
				this.UpdateAllViews(ViewType.Gamepad);
			}
			else if(ev is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) >= 0.24f)
			{
				this.UpdateAllViews(ViewType.Gamepad);
			}
			else if(ev is InputEventKey || ev is InputEventMouse)
			{
				this.UpdateAllViews(ViewType.Keyboard);
			}
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Call(UIControl.MethodName.OnInput, ev);
			}
			foreach(System.Type type in this.widgetsShown)
			{
				Widget widget = this.GetWidget(type);
				
				if(widget == null) { continue; }
				widget.Call(UIControl.MethodName.OnInput, ev);
			}
		}
		
		#endregion // Godot Methods
		
		#region Public Methods
		
		#region Pages Methods
		
		/// <summary>Adds the given page to the UI Manager.</summary>
		/// <param name="page">The page to add.</param>
		public void AddPage(Page page)
		{
			if(page == null) { return; }
			if(this.ContainsPage(page.GetType()))
			{
				return;
			}
			this.PagesContainer.AddChild(page);
			this.AddPageToManagerData(page);
		}
		
		/// <summary>Changes the current page's view to the given view type.</summary>
		/// <param name="viewType">The view type to change to.</param>
		/// <returns>Returns the current page.</returns>
		public Page ChangeCurrentPageView(ViewType viewType) => this.CurrentPage != null ? this.ChangePageView(this.CurrentPage.GetType(), viewType) : null;
		
		/// <summary>Changes the page's view to the given view type.</summary>
		/// <param name="viewType">The view type to change to.</param>
		/// <typeparam name="T">The type of page to change views on.</typeparam>
		/// <returns>Returns the page.</returns>
		public T ChangePageView<T>(ViewType viewType) where T : Page => this.ChangePageView(typeof(T), viewType) as T;
		
		/// <summary>Changes the page's view to the given view type.</summary>
		/// <param name="type">The type of page to change views on.</param>
		/// <param name="viewType">The view type to change to.</param>
		/// <returns>Returns the page.</returns>
		public Page ChangePageView(System.Type type, ViewType viewType)
		{
			Page page = this.GetPage(type);
			
			if(page == null)
			{
				return null;
			}
			
			page.ChangeView(viewType);
			
			return page;
		}
		
		/// <summary>Opens the page.</summary>
		/// <param name="transition">The transition to open the page on.</param>
		/// <typeparam name="T">The type of page to open.</typeparam>
		/// <returns>Returns the page being opened.</returns>
		public T OpenPage<T>(UITransition transition = null) where T : Page => this.OpenPage(typeof(T), transition) as T;
		
		/// <summary>Opens the page.</summary>
		/// <param name="type">The type of page to open.</param>
		/// <param name="transition">The transition to open the page on.</param>
		/// <returns>Returns the page being opened.</returns>
		public Page OpenPage(System.Type type, UITransition transition = null)
		{
			Page page = this.GetPage(type);
			
			if(page == null) { return null; }
			
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(this.ViewType, false, transition);
				if(!this.ignoreAddingToHistory)
				{
					this.history.Push(this.CurrentPage.GetType());
				}
			}
			
			this.CurrentPage = page;
			page.Toggle(this.ViewType, true, transition);
			if(!this.ignoreAddingToHistory && this.future.Count > 0)
			{
				this.future.Clear();
			}
			
			return page;
		}
		
		/// <summary>Open the page.</summary>
		/// <param name="updateData">Updates the data before opening the page.</param>
		/// <param name="transition">The transition to open the page on.</param>
		/// <typeparam name="T">The type of page to open.</typeparam>
		/// <typeparam name="TData">The data to update with.</typeparam>
		/// <returns>Returns the page being opened.</returns>
		public T OpenPage<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Page
			where TData : PageData => this.OpenPage<TData>(typeof(T), updateData, transition) as T;
		
		/// <summary>Open the page.</summary>
		/// <param name="type">The type of page to open.</param>
		/// <param name="updateData">Updates the data before opening the page.</param>
		/// <param name="transition">The transition to open the page on.</param>
		/// <typeparam name="TData">The data to update with.</typeparam>
		/// <returns>Returns the page being opened.</returns>
		public Page OpenPage<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : PageData
		{
			Page page = this.GetPage(type);
			
			if(page == null) { return null; }
			
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(this.ViewType, false, transition);
				if(!this.ignoreAddingToHistory)
				{
					this.history.Push(this.CurrentPage.GetType());
				}
			}
			
			this.CurrentPage = page;
			if(page.Data.GetType() == typeof(TData) && updateData != null)
			{
				updateData(page.Data as TData);
			}
			page.Toggle(this.ViewType, true, transition);
			if(!this.ignoreAddingToHistory && this.future.Count > 0)
			{
				this.future.Clear();
			}
			
			return page;
		}
		
		/// <summary>Closes the page.</summary>
		/// <param name="transition">The transition to close the page on.</param>
		public void ClosePage(UITransition transition = null)
		{
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(this.ViewType, false, transition);
				this.history.Push(this.CurrentPage.GetType());
				if(this.future.Count > 0)
				{
					this.future.Clear();
				}
				this.CurrentPage = null;
			}
		}
		
		/// <summary>Closes the page if the page is the given type.</summary>
		/// <param name="transition">The transition to close the page on.</param>
		/// <typeparam name="T">The type of page to check before closing.</typeparam>
		public void ClosePageIf<T>(UITransition transition = null) where T : Page
		{
			if(this.CurrentPage != null && this.CurrentPage.GetType() == typeof(T))
			{
				this.ClosePage(transition);
			}
		}
		
		/// <summary>Finds if the page is contained within the UI Manager.</summary>
		/// <typeparam name="T">The type of the page to search for.</typeparam>
		/// <returns>Returns true if the page is found within the UI Manager.</returns>
		public bool ContainsPage<T>() where T : Page => this.ContainsPage(typeof(T));
		
		/// <summary>Finds if the page is contained within the UI Manager.</summary>
		/// <param name="type">The type of the page to search for.</param>
		/// <returns>Returns true if the page is found within the UI Manager.</returns>
		public bool ContainsPage(System.Type type) => this.pages.ContainsKey(type);
		
		/// <summary>Goes back one page in the history.</summary>
		/// <param name="transition">The transition to open the page on.</param>
		/// <returns>Returns the previous page, or null if there is no previous page.</returns>
		public Page GoBack(UITransition transition = null)
		{
			if(this.history.Count == 0)
			{
				GDX.PrintWarning("No page to go back to");
				return null;
			}
			
			System.Type prevType = this.history.Pop();
			
			if(this.CurrentPage != null)
			{
				System.Type currType = this.CurrentPage.GetType();
				
				this.future.Push(currType);
			}
			
			this.ignoreAddingToHistory = true;
			
			Page page = this.OpenPage(prevType, transition);
			
			this.ignoreAddingToHistory = false;
			
			return page;
		}
		
		/// <summary>Goes forward one page in the future.</summary>
		/// <param name="transition">The transition to open the page on.</param>
		/// <returns>Returns the next page, or null if there is no next page.</returns>
		public Page GoForward(UITransition transition = null)
		{
			if(this.future.Count == 0)
			{
				GDX.PrintWarning("No page to go forward to");
				return null;
			}
			
			System.Type nextType = this.future.Pop();
			
			if(this.CurrentPage != null)
			{
				System.Type currType = this.CurrentPage.GetType();
				
				this.history.Push(currType);
			}
			
			this.ignoreAddingToHistory = true;
			
			Page page = this.OpenPage(nextType, transition);
			
			this.ignoreAddingToHistory = false;
			
			return page;
		}
		
		#endregion // Pages Methods
		
		#region Widget Methods
		
		/// <summary>Adds the widget to the UI Manager.</summary>
		/// <param name="widget">The widget to add.</param>
		public void AddWidget(Widget widget)
		{
			if(this.ContainsWidget(widget.GetType()))
			{
				return;
			}
			
			int priority = widget.Priority;
			Godot.Collections.Array<Node> children = this.WidgetsContainer.GetChildren();
			
			this.WidgetsContainer.AddChild(widget);
			for(int i = 0; i < children.Count; ++i)
			{
				if(children[i] is Widget child)
				{
					if(priority <= child.Priority)
					{
						this.WidgetsContainer.MoveChild(widget, i);
						break;
					}
				}
			}
			this.AddWidgetToManagerData(widget);
		}
		
		/// <summary>Changes the widget's view to the given view type.</summary>
		/// <param name="viewType">The view type to change to.</param>
		/// <typeparam name="T">The type of widget to change views on.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T ChangeWidgetView<T>(ViewType viewType) where T : Widget => this.ChangeWidgetView(typeof(T), viewType) as T;
		
		/// <summary>Changes the widget's view to the given view type.</summary>
		/// <param name="type">The type of widget to change views on.</param>
		/// <param name="viewType">The view type to change to.</param>
		/// <returns>Returns the widget.</returns>
		public Widget ChangeWidgetView(System.Type type, ViewType viewType)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null)
			{
				return null;
			}
			
			widget.ChangeView(viewType);
			
			return widget;
		}
		
		/// <summary>Shows the widget.</summary>
		/// <param name="transition">The transition to show the widget.</param>
		/// <typeparam name="T">The type of widget to show.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T ShowWidget<T>(UITransition transition = null) where T : Widget => this.ShowWidget(typeof(T), transition) as T;
		
		/// <summary>Shows the widget.</summary>
		/// <param name="type">The type of widget to show.</param>
		/// <param name="transition">The transition to show the widget.</param>
		/// <returns>Returns the widget.</returns>
		public Widget ShowWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			widget.Toggle(this.ViewType, true, transition);
			this.widgetsShown.Add(type);
			
			return widget;
		}
		
		/// <summary>Shows the widget.</summary>
		/// <param name="updateData">Updates the data before showing the widget.</param>
		/// <param name="transition">The transition to show the widget.</param>
		/// <typeparam name="T">The type of widget to show.</typeparam>
		/// <typeparam name="TData">The type of data to update.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T ShowWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData => this.ShowWidget<TData>(typeof(T), updateData, transition) as T;
		
		/// <summary>Shows the widget.</summary>
		/// <param name="type">The type of widget to show.</param>
		/// <param name="updateData">Updates the data before showing the widget.</param>
		/// <param name="transition">The transition to show the widget.</param>
		/// <typeparam name="TData">The type of data to update.</typeparam>
		/// <returns>Returns the widget.</returns>
		public Widget ShowWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			if(widget.Data.GetType() == typeof(TData) && updateData != null)
			{
				updateData(widget.Data as TData);
			}
			widget.Toggle(this.ViewType, true, transition);
			this.widgetsShown.Add(type);
			
			return widget;
		}
		
		/// <summary>Hides the widget.</summary>
		/// <param name="transition">The transition to hide the widget.</param>
		/// <typeparam name="T">The type of widget to hide.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T HideWidget<T>(UITransition transition = null) where T : Widget => this.HideWidget(typeof(T), transition) as T;
		
		/// <summary>Hides the widget.</summary>
		/// <param name="type">The type of widget to hide.</param>
		/// <param name="transition">The transition to hide the widget.</param>
		/// <returns>Returns the widget.</returns>
		public Widget HideWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			widget.Toggle(this.ViewType, false, transition);
			this.widgetsShown.Remove(type);
			
			return widget;
		}
		
		/// <summary>Toggles the widget on or off. If it's on, then it's toggled off. If it's off, then it's toggled on.</summary>
		/// <param name="transition">The transition to toggle the widget.</param>
		/// <typeparam name="T">The type of widget to toggle.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T ToggleWidget<T>(UITransition transition = null) where T : Widget => this.ToggleWidget(typeof(T), transition) as T;
		
		/// <summary>Toggles the widget on or off. If it's on, then it's toggled off. If it's off, then it's toggled on.</summary>
		/// <param name="type">The type of widget to toggle.</param>
		/// <param name="transition">The transition to toggle the widget.</param>
		/// <returns>Returns the widget.</returns>
		public Widget ToggleWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			widget.Toggle(this.ViewType, !widget.IsOn, transition);
			
			if(widget.IsOn) { this.widgetsShown.Add(type); }
			else { this.widgetsShown.Remove(type); }
			
			return widget;
		}
		
		/// <summary>Toggles the widgets on or off. If it's on, then it's toggled off. If it's off, then it's toggled on.</summary>
		/// <param name="updateData">Updates the data before the widget gets toggled.</param>
		/// <param name="transition">The transition to toggle the widget.</param>
		/// <typeparam name="T">The type of widget to toggle.</typeparam>
		/// <typeparam name="TData">The type of data to update.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T ToggleWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData => this.ToggleWidget<TData>(typeof(T), updateData, transition) as T;
		
		/// <summary>Toggles the widget on or off. If it's on, then it's toggled off. If it's off, then it's toggled on.</summary>
		/// <param name="type">The type of widget to toggle.</param>
		/// <param name="updateData">Updates the data before the widget gets toggled.</param>
		/// <param name="transition">The transition to toggle the widget.</param>
		/// <typeparam name="TData">The type of data to update.</typeparam>
		/// <returns>Returns the widget.</returns>
		public Widget ToggleWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			if(widget.Data.GetType() == typeof(TData) && updateData != null)
			{
				updateData(widget.Data as TData);
			}
			widget.Toggle(this.ViewType, !widget.IsOn, transition);
			
			if(widget.IsOn) { this.widgetsShown.Add(type); }
			else { this.widgetsShown.Remove(type); }
			
			return widget;
		}
		
		
		/// <summary>Hides all the currently shown widgets.</summary>
		/// <param name="transition">The transition to hide all the widgets.</param>
		public void HideAllWidgets(UITransition transition = null)
		{
			foreach(KeyValuePair<System.Type, Widget> pair in this.widgets)
			{
				this.HideWidget(pair.Key, transition);
			}
		}
		
		/// <summary>Gets the list of currently shown widgets.</summary>
		/// <returns>Returns the list of currently shown widgets.</returns>
		public List<Widget> GetAllShownWidgets()
		{
			List<Widget> widgets = new List<Widget>();
			
			foreach(System.Type type in this.widgetsShown)
			{
				widgets.Add(this.widgets[type]);
			}
			
			return widgets;
		}
		
		/// <summary>Finds if the widget is contained within the UI Manager.</summary>
		/// <typeparam name="T">The type of widget to search with.</typeparam>
		/// <returns>Returns true if the widget is found within the UI Manager.</returns>
		public bool ContainsWidget<T>() where T : Widget => this.ContainsWidget(typeof(T));
		
		/// <summary>Finds if the widget is contained within the UI Manager.</summary>
		/// <param name="type">The type of widget to search with.</param>
		/// <returns>Returns true if the widget is found within the UI Manager.</returns>
		public bool ContainsWidget(System.Type type) => this.widgets.ContainsKey(type);
		
		#endregion // Widget Methods
		
		#region Getter Methods
		
		/// <summary>Gets the page.</summary>
		/// <typeparam name="T">The type of page to get.</typeparam>
		/// <returns>Returns the page.</returns>
		public T GetPage<T>() where T : Page => this.GetPage(typeof(T)) as T;
		
		/// <summary>Gets the page.</summary>
		/// <param name="type">The type of page to get.</param>
		/// <returns>Returns the page.</returns>
		public Page GetPage(System.Type type)
			=> type != null
				? (this.pages.TryGetValue(type, out Page page) ? page : this.InstantiatePage(type))
				: null;
		
		/// <summary>Gets the widget.</summary>
		/// <typeparam name="T">The type of widget to get.</typeparam>
		/// <returns>Returns the widget.</returns>
		public T GetWidget<T>() where T : Widget => this.GetWidget(typeof(T)) as T;
		
		/// <summary>Gets the widget.</summary>
		/// <param name="type">The type of widget to get.</param>
		/// <returns>Returns the widget.</returns>
		public Widget GetWidget(System.Type type)
			=> type != null
				? (this.widgets.TryGetValue(type, out Widget widget) ? widget : this.InstantiateWidget(type))
				: null;
		
		/// <summary>Gets the data node.</summary>
		/// <typeparam name="T">The type of data to get.</typeparam>
		/// <returns>Returns the data node. Returns null if nothing is found.</returns>
		/// <remarks>It's more optimal to get the data node straight from the page or widget.</remarks>
		public T GetData<T>() where T : UIData => this.GetData(typeof(T)) as T;
		
		/// <summary>Gets the data node.</summary>
		/// <param name="type">The type of data to get.</param>
		/// <returns>Returns the data node. Returns null if nothing is found.</returns>
		/// <remarks>It's more optimal to get the data node straight from the page or widget.</remarks>
		public UIData GetData(System.Type type)
		{
			if(type == null) { return null; }
			
			if(this.dataConnections.TryGetValue(type, out UIControl control))
			{
				return control.Data;
			}
			
			if(type.IsSubclassOf(typeof(PageData)))
			{
				TiedToAttribute attribute = type.GetCustomAttribute<TiedToAttribute>();
				
				if(attribute != null)
				{
					Page page = this.GetPage(attribute.LinkedType);
					
					if(page != null)
					{
						return page.Data;
					}
				}
				else
				{
					foreach(Page page in this.pages.Values)
					{
						if(page.Data.GetType() == type)
						{
							return page.Data;
						}
					}
				}
			}
			else if(type.IsSubclassOf(typeof(WidgetData)))
			{
				TiedToAttribute attribute = type.GetCustomAttribute<TiedToAttribute>();
				
				if(attribute != null)
				{
					Widget widget = this.GetWidget(attribute.LinkedType);
					
					return widget.Data;
				}
				else
				{
					foreach(Widget widget in this.widgets.Values)
					{
						if(widget.Data.GetType() == type)
						{
							return widget.Data;
						}
					}
				}
			}
			
			return null;
		}
		
		/// <summary>Instantiates the page from the given type.</summary>
		/// <param name="type">The type of page to instantiate.</param>
		/// <returns>Returns the instantiated page, returns null if nothing got instantiated.</returns>
		private Page InstantiatePage(System.Type type)
		{
			if(!this.DynamicallyInstantiate) { return null; }
			
			if(type == null || !type.IsSubclassOf(typeof(Page)))
			{
				return null;
			}
			
			SceneLocationAttribute location = type.GetCustomAttribute<SceneLocationAttribute>();
			
			if(location == null) { return null; }
			
			Page page = GDX.Instantiate<Page>(location.ScenePath);
			
			if(page == null) { return null; }
			
			this.AddPage(page);
			return page;
		}
		
		/// <summary>Instantiates the widget from the given type.</summary>
		/// <param name="type">The type of widget to instantiate.</param>
		/// <returns>Returns the instantiated widget, returns null if nothing got instantiated.</returns>
		private Widget InstantiateWidget(System.Type type)
		{
			if(!this.DynamicallyInstantiate) { return null; }
			
			if(type == null || !type.IsSubclassOf(typeof(Widget)))
			{
				return null;
			}
			
			SceneLocationAttribute location = type.GetCustomAttribute<SceneLocationAttribute>();
			
			if(location == null) { return null; }
			
			Widget widget = GDX.Instantiate<Widget>(location.ScenePath);
			
			if(widget == null) { return null; }
			
			this.AddWidget(widget);
			
			return widget;
		}
		
		#endregion // Getter Methods
		
		#endregion // Public Methods
		
		#region Private Methods
		
		/// <inheritdoc cref="Godot.Node._EnterTree"/>
		private void OnEnterTree()
		{
			this.FindUIElements();
			this.AwakenUIElements();
			this.CurrentPage = this.StartingPage;
			if(this.StartingPage != null)
			{
				this.StartingPage.Toggle(this.ViewType, true);
			}
		}
		
		/// <summary>Finds all the widgets and pages that the UI Manager holds.</summary>
		private void FindUIElements()
		{
			List<Widget> sortedWidgets = new List<Widget>();
			
			foreach(Page page in this.GetChildrenRecursively<Page>())
			{
				page.Call(UIControl.MethodName.SetupFocus);
				this.AddPageToManagerData(page);
			}
			
			foreach(Widget widget in this.GetChildrenRecursively<Widget>())
			{
				this.AddWidgetToManagerData(widget);
				sortedWidgets.Add(widget);
			}
			
			sortedWidgets.Sort((left, right) => left.Priority.CompareTo(right.Priority));
			
			foreach(Widget widget in sortedWidgets)
			{
				widget.Call(UIControl.MethodName.SetupFocus);
			}
		}
		
		/// <summary>Awakens all the widgets and pages that the UI Manager holds.</summary>
		private void AwakenUIElements()
		{
			foreach(Page page in this.pages.Values)
			{
				page.Call(UIControl.MethodName.HideAway);
			}
			
			foreach(Page page in this.pages.Values)
			{
				page.Call(UIControl.MethodName.OnEnterTree);
				page.ViewType = this.ViewType;
				page.KeyboardView?.SetActive(this.ViewType == ViewType.Keyboard);
				page.GamepadView?.SetActive(this.ViewType == ViewType.Gamepad);
				page.MobileView?.SetActive(this.ViewType == ViewType.Mobile);
			}
			
			foreach(Widget widget in this.widgets.Values)
			{
				widget.Call(UIControl.MethodName.HideAway);
				if(widget.ShowOnStartup)
				{
					widget.Toggle(this.ViewType, widget.IsOn, new FadeTransition(true));
					this.widgetsShown.Add(widget.GetType());
				}
			}
			
			foreach(Widget widget in this.widgets.Values)
			{
				widget.Call(UIControl.MethodName.OnEnterTree);
				widget.ViewType = this.ViewType;
				widget.KeyboardView.SetActive(this.ViewType == ViewType.Keyboard);
				widget.GamepadView.SetActive(this.ViewType == ViewType.Gamepad);
				widget.MobileView.SetActive(this.ViewType == ViewType.Mobile);
			}
		}
		
		/// <summary>Updates all the views for anything currently being shown.</summary>
		/// <param name="nextViewType">The view type to change to.</param>
		private void UpdateAllViews(ViewType nextViewType)
		{
			if(this.ViewType == nextViewType) { return; }
			
			this.ViewType = nextViewType;
			if(this.CurrentPage != null)
			{
				this.CurrentPage.ChangeView(this.ViewType);
			}
			foreach(System.Type type in this.widgetsShown)
			{
				Widget widget = this.GetWidget(type);
				
				if(widget == null) { continue; }
				widget.ChangeView(this.ViewType);
			}
		}
		
		/// <summary>Adds the page to the data the manager uses to regulate pages.</summary>
		/// <param name="page">The page to add to the data set.</param>
		private void AddPageToManagerData(Page page)
		{
			if(page == null) { return; }
			if(this.ContainsPage(page.GetType()))
			{
				return;
			}
			this.pages.Add(page.GetType(), page);
			this.dataConnections.Add(page.Data.GetType(), page);
		}
		
		/// <summary>Adds the widget to the data the manager uses to regulate widgets.</summary>
		/// <param name="widget">The widget to add to the data set.</param>
		private void AddWidgetToManagerData(Widget widget)
		{
			if(widget == null) { return; }
			if(this.ContainsWidget(widget.GetType()))
			{
				return;
			}
			this.widgets.Add(widget.GetType(), widget);
			this.dataConnections.Add(widget.Data.GetType(), widget);
		}
		
		#endregion // Private Methods
	}
}

namespace Nova
{
	using Nova.UI;
	
	/// <summary>A static class that manages the UI.</summary>
	public static class UIManager
	{
		#region Properties
		
		/// <inheritdoc cref="UIManagerNode.CurrentPage"/>
		public static Page CurrentPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve current page");
					return null;
				}
				return UIManagerNode.Instance.CurrentPage;
			}
		}
		
		/// <inheritdoc cref="UIManagerNode.PreviousPage"/>
		public static Page PreviousPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve previous page");
					return null;
				}
				return UIManagerNode.Instance.PreviousPage;
			}
		}
		
		/// <inheritdoc cref="UIManagerNode.NextPage"/>
		public static Page NextPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve next page");
					return null;
				}
				return UIManagerNode.Instance.NextPage;
			}
		}
		
		/// <inheritdoc cref="UIManagerNode.ViewType"/>
		public static ViewType ViewType
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning($"UI Manager is not instantiated! Could not retrieve view type");
					return ViewType.Keyboard;
				}
				return UIManagerNode.Instance.ViewType;
			}
		}
		
		#endregion // Properties
		
		#region Public Methods
		
		#region Pages Methods
		
		/// <inheritdoc cref="UIManagerNode.AddPage(Page)"/>
		public static void AddPage(Page page)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not add page {page.GetType()}");
				return;
			}
			UIManagerNode.Instance.AddPage(page);
		}
		
		/// <inheritdoc cref="UIManagerNode.ChangeCurrentPageView(ViewType)"/>
		public static Page ChangeCurrentPageView(ViewType viewType)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not change current page's view to {viewType}");
				return null;
			}
			return UIManagerNode.Instance.ChangeCurrentPageView(viewType);
		}
		
		/// <inheritdoc cref="UIManagerNode.ChangePageView{T}(ViewType)"/>
		public static T ChangePageView<T>(ViewType viewType) where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not change page's view: {typeof(T)}; to {viewType}");
				return null;
			}
			return UIManagerNode.Instance.ChangePageView<T>(viewType);
		}
		
		/// <inheritdoc cref="UIManagerNode.ChangePageView(System.Type, ViewType)"/>
		public static Page ChangePageView(System.Type type, ViewType viewType)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not change page's view: {type}; to {viewType}");
				return null;
			}
			return UIManagerNode.Instance.ChangePageView(type, viewType);
		}
		
		/// <inheritdoc cref="UIManagerNode.OpenPage{T}(UITransition)"/>
		public static T OpenPage<T>(UITransition transition = null) where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<T>(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.OpenPage(System.Type, UITransition)"/>
		public static Page OpenPage(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {type}");
				return null;
			}
			return UIManagerNode.Instance.OpenPage(type, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.OpenPage{T, TData}(System.Action{TData}, UITransition)"/>
		public static T OpenPage<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Page
			where TData : PageData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {typeof(T)}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<T, TData>(updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.OpenPage{TData}(System.Type, System.Action{TData}, UITransition)"/>
		public static Page OpenPage<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : PageData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {type}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<TData>(type, updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ClosePage(UITransition)"/>
		public static void ClosePage(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not close page");
				return;
			}
			UIManagerNode.Instance.ClosePage(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ClosePageIf{T}(UITransition)"/>
		public static void ClosePageIf<T>(UITransition transition = null) where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not close page");
				return;
			}
			UIManagerNode.Instance.ClosePageIf<T>(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ContainsPage{T}()"/>
		public static bool ContainsPage<T>() where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not check if page [{typeof(T)}] is contained within the manager.");
				return false;
			}
			return UIManagerNode.Instance.ContainsPage<T>();
		}
		
		/// <inheritdoc cref="UIManagerNode.ContainsPage(System.Type)"/>
		public static bool ContainsPage(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not check if page [{type}] is contained within the manager.");
				return false;
			}
			return UIManagerNode.Instance.ContainsPage(type);
		}
		
		/// <inheritdoc cref="UIManagerNode.GoBack(UITransition)"/>
		public static Page GoBack(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not go back");
				return null;
			}
			return UIManagerNode.Instance.GoBack(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.GoForward(UITransition)"/>
		public static Page GoForward(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not go forward");
				return null;
			}
			return UIManagerNode.Instance.GoForward(transition);
		}
		
		#endregion // Pages Methods
		
		#region Widget Methods
		
		/// <inheritdoc cref="UIManagerNode.AddWidget(Widget)"/>
		public static void AddWidget(Widget widget)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not add widget {widget.GetType()}");
				return;
			}
			UIManagerNode.Instance.AddWidget(widget);
		}
		
		/// <inheritdoc cref="UIManagerNode.ChangeWidgetView{T}(ViewType)"/>
		public static T ChangeWidgetView<T>(ViewType viewType) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not change widget's view type: {typeof(T)}; to {viewType}");
				return null;
			}
			return UIManagerNode.Instance.ChangeWidgetView<T>(viewType);
		}
		
		/// <inheritdoc cref="UIManagerNode.ChangeWidgetView(System.Type, ViewType)"/>
		public static Widget ChangeWidgetView(System.Type type, ViewType viewType)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not change widget's view type: {type}; to {viewType}");
				return null;
			}
			return UIManagerNode.Instance.ChangeWidgetView(type, viewType);
		}
		
		/// <inheritdoc cref="UIManagerNode.ShowWidget{T}(UITransition)"/>
		public static T ShowWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<T>(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ShowWidget(System.Type, UITransition)"/>
		public static Widget ShowWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget(type, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ShowWidget{T, TData}(System.Action{TData}, UITransition)"/>
		public static T ShowWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {typeof(T)}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<T, TData>(updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ShowWidget{TData}(System.Type, System.Action{TData}, UITransition)"/>
		public static Widget ShowWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {type}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<TData>(type, updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.HideWidget{T}(UITransition)"/>
		public static T HideWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not hide widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.HideWidget<T>(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.HideWidget(System.Type, UITransition)"/>
		public static Widget HideWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not hide widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.HideWidget(type, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ToggleWidget{T}(UITransition)"/>
		public static T ToggleWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget<T>(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ToggleWidget(System.Type, UITransition)"/>
		public static Widget ToggleWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget(type, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ToggleWidget{T, TData}(System.Action{TData}, UITransition)"/>
		public static T ToggleWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget<T, TData>(updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.ToggleWidget{TData}(System.Type, System.Action{TData}, UITransition)"/>
		public static Widget ToggleWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget<TData>(type, updateData, transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.HideAllWidgets(UITransition)"/>
		public static void HideAllWidgets(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Hide all widgets");
				return;
			}
			UIManagerNode.Instance.HideAllWidgets(transition);
		}
		
		/// <inheritdoc cref="UIManagerNode.GetAllShownWidgets"/>
		public static List<Widget> GetAllShownWidgets()
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get all shown widgets");
				return new List<Widget>();
			}
			return UIManagerNode.Instance.GetAllShownWidgets();
		}
		
		/// <inheritdoc cref="UIManagerNode.ContainsWidget{T}()"/>
		public static bool ContainsWidget<T>() where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not find if widget [{typeof(T)}] is contained within the UI Manager.");
				return false;
			}
			return UIManagerNode.Instance.ContainsWidget<T>();
		}
		
		/// <inheritdoc cref="UIManagerNode.ContainsWidget(System.Type)"/>
		public static bool ContainsWidget(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not find if widget [{type}] is contained within the UI Manager.");
				return false;
			}
			return UIManagerNode.Instance.ContainsWidget(type);
		}
		
		#endregion // Widget Methods
		
		#region Getter Methods
		
		/// <inheritdoc cref="UIManagerNode.GetPage{T}()"/>
		public static T GetPage<T>() where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get page: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetPage<T>();
		}
		
		/// <inheritdoc cref="UIManagerNode.GetPage(System.Type)"/>
		public static Page GetPage(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get page: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetPage(type);
		}
		
		/// <inheritdoc cref="UIManagerNode.GetWidget{T}()"/>
		public static T GetWidget<T>() where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetWidget<T>();
		}
		
		/// <inheritdoc cref="UIManagerNode.GetWidget(System.Type)"/>
		public static Widget GetWidget(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetWidget(type);
		}
		
		/// <inheritdoc cref="UIManagerNode.GetData{T}()"/>
		public static T GetData<T>() where T : UIData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get data: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetData<T>();
		}
		
		/// <inheritdoc cref="UIManagerNode.GetData(System.Type)"/>
		public static UIData GetData(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get data: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetData(type);
		}
		
		#endregion // Getter Methods
		
		#endregion // Public Methods
	}
}
