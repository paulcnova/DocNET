
namespace Nova;

using Godot;
using Godot.Collections;

using System.Collections.Generic;

/// <summary>An extension for <see cref="Godot.Node"/> to add helpful methods.</summary>
public static class Extension_Node
{
	#region Public Methods
	
	/// <summary>Frees every child from the node.</summary>
	/// <param name="node">The node to free it's children.</param>
	public static void QueueFreeChildren(this Node node)
	{
		Array<Node> children = node.GetChildren();
		
		foreach(Node child in children)
		{
			node.RemoveChild(child);
			child.QueueFree();
		}
	}
	
	/// <summary>Flattens the node's 3D basis into a plane, so that it's basis gets re-oriented.</summary>
	/// <param name="node">The 3D node to flatten.</param>
	/// <param name="planeUp">The direction the plane exists, ignoring the depth aspect of the plane.</param>
	/// <returns>Returns the flattened plane basis.</returns>
	public static PlaneBasis FlattenBasisToPlane(this Node3D node, Vector3 planeUp)
	{
		Vector3 up = planeUp.Normalized();
		Vector3 right = up.Cross(node.GlobalBasis.Z).Normalized();
		Vector3 forward = -right.Cross(up).Normalized();
		
		return new PlaneBasis(forward, right, up);
	}
	
	/// <summary>Gets all the children of the given node.</summary>
	/// <param name="node">The node to get the children from.</param>
	/// <typeparam name="T">The type to search for.</typeparam>
	/// <returns>Returns the array of children that match the type listed.</returns>
	public static Array<T> GetChildren<[MustBeVariant] T>(this Node node) where T : Node
	{
		Array<T> result = new Array<T>();
		
		foreach(Node child in node.GetChildren())
		{
			if(child is T || child.GetType().IsSubclassOf(typeof(T)))
			{
				result.Add(child as T);
			}
		}
		
		return result;
	}
	
	/// <summary>Gets all the children of the given node, recursively.</summary>
	/// <param name="node">The node to get the children from.</param>
	/// <typeparam name="T">The type to search for.</typeparam>
	/// <returns>Returns the array of children that match the type listed.</returns>
	public static Array<T> GetChildrenRecursively<[MustBeVariant] T>(this Node node) where T : Node
	{
		Array<T> result = new Array<T>();
		Queue<Node> queue = new Queue<Node>();
		
		queue.Enqueue(node);
		
		while(queue.Count > 0)
		{
			Node temp = queue.Dequeue();
			
			if(temp is T || temp.GetType().IsSubclassOf(typeof(T)))
			{
				result.Add(temp as T);
			}
			
			foreach(Node child in temp.GetChildren())
			{
				queue.Enqueue(child);
			}
		}
		
		return result;
	}
	
	/// <summary>Finds the first child of the given type.</summary>
	/// <param name="node">The node to find the child of.</param>
	/// <typeparam name="T">The type to search for.</typeparam>
	/// <returns>Returns the first child of the given type, or null if nothing was found.</returns>
	public static T FindChild<T>(this Node node) where T : Node
	{
		Queue<Node> queue = new Queue<Node>();
		
		queue.Enqueue(node);
		
		while(queue.Count > 0)
		{
			Node temp = queue.Dequeue();
			
			if(temp == null) { break; }
			
			if(temp is T result)
			{
				return result;
			}
			
			foreach(Node child in temp.GetChildren())
			{
				queue.Enqueue(child);
			}
		}
		
		return null;
	}
	
	/// <summary>Sets the forward axis of the 3D node.</summary>
	/// <param name="node">The node to set the forward axis to.</param>
	/// <param name="direction">The direction to set the forward axis to.</param>
	public static void SetForwardAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			node.Basis = Basis.LookingAt(forward, Vector3.Up);
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, -0.5f * Mathf.Pi);
				node.Basis = basis;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, 0.5f * Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	/// <summary>Sets the right axis of the 3D node.</summary>
	/// <param name="node">The node to set the right axis to.</param>
	/// <param name="direction">The direction to set the right axis to.</param>
	public static void SetRightAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		float rightAngle = Mathf.Abs(forward.Dot(Vector3.Right));
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			Basis basis = Basis.LookingAt(forward, Vector3.Up);
			Vector3 right = forward.Cross(Vector3.Up).Normalized();
			Vector3 up = forward.Cross(right);
			
			basis = basis.Rotated(up, 0.5f * Mathf.Pi);
			basis = basis.Rotated(forward, -0.5f * Mathf.Pi);
			node.Basis = basis;
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Back, 0.5f * Mathf.Pi);
				node.Basis = basis;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Back, -0.5f * Mathf.Pi);
				basis = basis.Rotated(forward, Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	/// <summary>Sets the up axis of the 3D node.</summary>
	/// <param name="node">The node to set the up axis to.</param>
	/// <param name="direction">The direction to set the up axis to.</param>
	public static void SetUpAxis(this Node3D node, Vector3 direction)
	{
		Vector3 forward = -direction.Normalized();
		float angle = -forward.Dot(Vector3.Up);
		
		if(Mathf.Abs(angle) < 1.0f)
		{
			Basis basis = Basis.LookingAt(forward, Vector3.Up);
			Vector3 right = forward.Cross(Vector3.Up).Normalized();
			
			basis = basis.Rotated(right, 0.5f * Mathf.Pi);
			node.Basis = basis;
		}
		else
		{
			if(Mathf.Sign(angle) >= 0)
			{
				node.Basis = Basis.Identity;
			}
			else
			{
				Basis basis = Basis.Identity;
				
				basis = basis.Rotated(Vector3.Right, Mathf.Pi);
				node.Basis = basis;
			}
		}
	}
	
	/// <summary>Sets the canvas item to be active: visible and toggling it's process mode.</summary>
	/// <param name="item">The canvas item to set active to.</param>
	/// <param name="isActive">Set to true to make the canvas visible and it's scripts to process.</param>
	public static void SetActive(this CanvasItem item, bool isActive)
	{
		item.Visible = isActive;
		item.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	/// <summary>Sets the node to be active: visible and toggling it's process mode.</summary>
	/// <param name="node">The node to set active to.</param>
	/// <param name="isActive">Set to true to make the canvas visible and it's scripts to process.</param>
	public static void SetActive(this Node3D node, bool isActive)
	{
		node.Visible = isActive;
		node.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	/// <summary>Sets the node to be active: visible and toggling it's process mode.</summary>
	/// <param name="node">The node to set active to.</param>
	/// <param name="isActive">Set to true to make the canvas visible and it's scripts to process.</param>
	public static void SetActive(this Node2D node, bool isActive)
	{
		node.Visible = isActive;
		node.ProcessMode = isActive ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
	}
	
	#endregion // Public Methods
}
