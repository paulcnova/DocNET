
namespace Nova;

using Godot;

using System.Collections;
using System.Collections.Generic;

/// <summary>A class that creates a spline out of an array of points.</summary>
public partial class Spline : GodotObject, IList<Vector3>
{
	#region Properties
	
	private float time;
	private float duration;
	
	/// <summary>Gets if the spline is readonly.</summary>
	public bool IsReadOnly => false;
	
	/// <summary>Gets the count of items within the spline.</summary>
	public int Count => this.Points.Count;
	
	/// <summary>Gets and sets the interpolation type of the spline.</summary>
	public Type InterpolationType { get; set; } = Type.Linear;
	
	/// <summary>Gets and sets the loop type of the spline.</summary>
	public Loop LoopType { get; set; } = Loop.None;
	
	/// <summary>Gets and sets the points in the spline.</summary>
	public List<Vector3> Points { get; set; } = new List<Vector3>();
	
	/// <summary>Gets if the spline is finished.</summary>
	public bool IsFinished => (int)this.LoopType / 2 == 0 && this.time >= this.duration;
	
	/// <summary>Gets and sets the duration (in seconds) of the spline.</summary>
	public float Duration
	{
		get => this.duration;
		set => this.duration = Mathf.Abs(value);
	}
	
	/// <summary>Gets and sets the time of the spline.</summary>
	/// <remarks>Time is between 0.0 and 1.0.</remarks>
	public float Time
	{
		get
		{
			float t = this.time / this.duration;
			
			if(this.IsBackwards)
			{
				t = 1.0f - t;
			}
			
			return t;
		}
		set
		{
			this.time = Mathf.Clamp(value, 0.0f, 1.0f) * this.duration;
		}
	}
	
	/// <summary>Gets and sets the items of the spline.</summary>
	/// <param name="index">The index of the item within the spline.</param>
	public Vector3 this[int index]
	{
		get
		{
			if(index < 0 || index >= this.Count)
			{
				GDX.PrintError("Index out of array");
			}
			return this.Points[index];
		}
		set
		{
			if(index < 0 || index >= this.Count)
			{
				GDX.PrintError("Index out of array");
			}
			this.Points[index] = value;
		}
	}
	
	/// <summary>Gets the current position of the spline.</summary>
	public Vector3 Value
	{
		get
		{
			if(this.Count == 0) { return Vector3.Zero; }
			if(this.Count == 1) { return this.Points[0]; }
			
			switch(this.InterpolationType)
			{
				default: case Type.Linear: return this.GetValueLinearly(this.Time);
				case Type.CatmullRom: return this.GetValueByCatmullRom(this.Time);
			}
		}
	}
	
	/// <summary>Gets if the spline is going backwards.</summary>
	private bool IsBackwards => (int)this.LoopType % 2 == 1;
	
	/// <summary>Gets if the spline is looping fully.</summary>
	private bool IsFullLooped => (int)this.LoopType / 2 == 1;
	
	/// <summary>A constructor that creates a spline with duration of 1 second and no points.</summary>
	public Spline() : this(1.0f, new List<Vector3>()) {}
	
	/// <summary>A constructor that creates a spline with a given duration and list of points.</summary>
	/// <param name="duration">The duration (in seconds) of the spline.</param>
	/// <param name="points">The list of points to add to the spline.</param>
	public Spline(float duration, IList<Vector3> points)
	{
		this.time = 0.0f;
		this.duration = duration;
		this.Points = new List<Vector3>(points);
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Creates a spline where the duration is the same between any point and the next.</summary>
	/// <param name="unitDuration">The duration (in seconds) between any point and the next.</param>
	/// <param name="points">The list of points.</param>
	/// <returns>Returns a new spline with unit duration between every point.</returns>
	public static Spline WithUnitDuration(float unitDuration, IList<Vector3> points)
	{
		if(points.Count <= 1) { return new Spline(0.0f, points); }
		if(points.Count == 2) { return new Spline(unitDuration, points); }
		
		Vector3 diff = points[1] - points[0];
		float sum = 1.0f;
		float max = diff.LengthSquared();
		float delta = 0.0f;
		
		for(int i = 1; i < points.Count - 1; ++i)
		{
			diff = points[i + 1] - points[i];
			delta = diff.LengthSquared();
			sum += delta / max;
		}
		
		return new Spline(sum * unitDuration, points);
	}
	
	/// <summary>Adds the point into the spline.</summary>
	/// <param name="point">The point to add.</param>
	public void Add(Vector3 point) => this.Points.Add(point);
	
	/// <summary>Removes the point from the spline.</summary>
	/// <param name="point">The point to remove.</param>
	/// <returns>Returns true if the point is successfully removed.</returns>
	public bool Remove(Vector3 point) => this.Points.Remove(point);
	
	/// <summary>Clears the spline from all points.</summary>
	public void Clear() => this.Points.Clear();
	
	/// <summary>Checks to see if the given point is contained within the spline.</summary>
	/// <param name="point">The point to check for.</param>
	/// <returns>Returns true if the point is found.</returns>
	public bool Contains(Vector3 point) => this.Points.Contains(point);
	
	/// <summary>Finds the index of the given point within the spline.</summary>
	/// <param name="point">The point to find the index for.</param>
	/// <returns>Returns the index of the point, returns -1 if nothing was found.</returns>
	public int IndexOf(Vector3 point) => this.Points.IndexOf(point);
	
	/// <summary>Inserts the point into the spline.</summary>
	/// <param name="index">The index to insert the point into.</param>
	/// <param name="point">The point to insert.</param>
	public void Insert(int index, Vector3 point) => this.Points.Insert(index, point);
	
	/// <summary>Removes the point from the spline at the given index.</summary>
	/// <param name="index">The index to remove the point from.</param>
	public void RemoveAt(int index) => this.Points.RemoveAt(index);
	
	/// <inheritdoc/>
	public void CopyTo(Vector3[] array, int arrayIndex) => this.Points.CopyTo(array, arrayIndex);
	
	/// <inheritdoc/>
	public IEnumerator<Vector3> GetEnumerator() => this.Points.GetEnumerator();
	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => this.Points.GetEnumerator();
	
	/// <summary>Finds the point at the given time.</summary>
	/// <param name="time">The time (between 0.0 and 1.0) to find a point within the spline.</param>
	/// <returns>Returns the point from the given time.</returns>
	public Vector3 GetPointAt(float time)
	{
		switch(this.InterpolationType)
		{
			default: case Type.Linear: return this.GetValueLinearly(time);
			case Type.CatmullRom: return this.GetValueByCatmullRom(time);
		}
	}
	
	/// <summary>Processes the spline, moving it forward in time.</summary>
	/// <param name="delta">The delta between frames to process the spline.</param>
	public void Process(float delta)
	{
		if((int)this.LoopType < 2)
		{
			this.time = Mathf.Clamp(this.time + delta, 0.0f, this.duration);
		}
		else
		{
			if(this.time + delta > this.duration)
			{
				if(this.LoopType == Loop.Yoyo)
				{
					this.LoopType = Loop.YoyoBackwards;
				}
				else if(this.LoopType == Loop.YoyoBackwards)
				{
					this.LoopType = Loop.Yoyo;
				}
			}
			this.time = (this.time + delta) % this.duration;
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the value linearly.</summary>
	/// <param name="time">The time in seconds</param>
	/// <returns>Returns the point in the spline linearly.</returns>
	private Vector3 GetValueLinearly(float time)
	{
		if(this.Count == 0) { return Vector3.Zero; }
		else if(this.Count == 1) { return this.Points[0]; }
		
		float t = time * (this.IsFullLooped
			? this.Count
			: this.Count - 1
		);
		int index = (int)t;
		
		t = Mathf.Clamp(t - index, 0.0f, 1.0f);
		
		if(this.IsFullLooped)
		{
			if(index == this.Count)
			{
				index = 0;
			}
		}
		else if(index >= this.Count - 1)
		{
			return this.Points[this.Count - 1];
		}
		
		return this.Points[index].Lerp(this.Points[index == this.Count - 1 ? 0 : index + 1], t);
	}
	
	/// <summary>Gets the value by the Catmull-Rom interpolation.</summary>
	/// <param name="time">The time in seconds.</param>
	/// <returns>Returns the point in the spline using the Catmull-Rom interpolation.</returns>
	private Vector3 GetValueByCatmullRom(float time)
	{
		if(this.Count == 0) { return Vector3.Zero; }
		else if(this.Count == 1) { return this.Points[0]; }
		
		float segments = this.IsFullLooped ? this.Count : this.Count - 1;
		int index = (int)(this.Time * segments);
		int p0 = this.GetLimits(index - 1);
		int p1 = this.GetLimits(index);
		int p2 = this.GetLimits(index + 1);
		int p3 = this.GetLimits(index + 2);
		float t = (time - (float)index / segments) * segments;
		float t2 = t * t;
		float t3 = t2 * t;
		Vector3 temp = this.Points[p0] * 0.5f * (-t3 + 2.0f * t2 - t);
		Vector3 temp2 = this.Points[p1] * 0.5f * (3.0f * t3 - 5.0f * t2 + 2.0f);
		
		temp += temp2;
		
		return 0.5f * (
			this.Points[p0] * (-t3 + 2.0f * t2 - t)
			+ this.Points[p1] * (3.0f * t3 - 5.0f * t2 + 2.0f)
			+ this.Points[p2] * (-3.0f * t3 + 4.0f * t2 + t)
			+ this.Points[p3] * (t3 - t2)
		);
	}
	
	/// <summary>Gets the limits of the list of points.</summary>
	/// <param name="index">The index to test the limits.</param>
	/// <returns>Returns the index clamped within the list of points.</returns>
	private int GetLimits(int index)
	{
		if(this.IsFullLooped) { return index % (this.Count - 1); }
		
		if(index < 0) { return 0; }
		else if(index >= this.Count) { return this.Count - 1; }
		
		return index;
	}
	
	#endregion // Private Methods
	
	#region Types
	
	/// <summary>The interpolation type for the spline.</summary>
	public enum Type
	{
		/// <summary>Interpolates the spline linearly.</summary>
		Linear,
		/// <summary>Interpolates the spline using the Catmull-Rom interpolation.</summary>
		CatmullRom,
	}
	
	/// <summary>The loop type for the spline.</summary>
	public enum Loop : int
	{
		/// <summary>There is no loop. Once the spline reaches the other side (1.0) it stops and <see cref="Spline.IsFinished"/> becomes true.</summary>
		None = 0,
		/// <summary>There is no loop. The spline moves backwards (from 1.0 to 0.0). Once the spline reaches the other side (0.0) it stops and <see cref="Spline.IsFinished"/> becomes true.</summary>
		NoneBackwards = 1,
		/// <summary>It loops fully around. Once the spline reaches the last point, it will interpolate between the last point and the first point then restarts from the beginning. <see cref="Spline.IsFinished"/> never becomes true.</summary>
		Full = 2,
		/// <summary>It loops fully around. The spline moves backwards (from 1.0 to 0.0). Once the spline reaches the first point, it will interpolate between the first point and the last point then restarts from the beginning. <see cref="Spline.IsFinished"/> never becomes true.</summary>
		FullBackwards = 3,
		/// <summary>It move forward and then backwards once it reaches the last point, it then moves forward again once it reaches the first point. <see cref="Spline.IsFinished"/> never becomes true.</summary>
		Yoyo = 4,
		/// <summary>It moves backwards and then forwards once it reaches the first point, it then moves backwards again once it reaches the last point. <see cref="Spline.IsFinished"/> never becomes true.</summary>
		YoyoBackwards = 5,
	}
	
	#endregion // Types
}
