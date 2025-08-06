
namespace Nova.UI;

using Godot;

/// <summary>A node that holds all the data used for <see cref="UIControl"/></summary>
/// <remarks>This is so that when views change, the data doesn't go with it. As well as being able to save or access any data it holds without figuring out which view it's tied to.</remarks>
public abstract partial class UIData : Node {}
