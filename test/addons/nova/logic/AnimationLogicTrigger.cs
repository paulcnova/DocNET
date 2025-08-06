
namespace Nova.Logic;

using Godot;
using Godot.Collections;

[GlobalClass] public partial class AnimationLogicTrigger : LogicTrigger
{
	#region Properties
	
	[Export] public LogicType Logic { get; set; }
	[Export] public string AnimationName { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[ExportGroup("Parameters")]
	[Export] public float Blend { get; set; } = -1.0f;
	[Export] public float Speed { get; set; } = 1.0f;
	[Export] public float SectionStartTime { get; set; } = 0.0f;
	[Export] public float SectionEndTime { get; set; } = 1.0f;
	[Export] public string SectionStartMarker { get; set; }
	[Export] public string SectionEndMarker { get; set; }
	[ExportGroup("Query Methods")]
	[Export] public string AnimationPlayerGroup { get; set; }
	[Export] public string AnimationPlayerNodePath { get; set; }
	[Export] public Array<AnimationPlayer> AnimationPlayers { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void TriggerLogic()
	{
		Array<AnimationPlayer> players = this.GetAnimationPlayers();
		
		foreach(AnimationPlayer player in players)
		{
			switch(this.Logic)
			{
				case LogicType.Play: player.Play(this.AnimationName, this.Blend, this.Speed); break;
				case LogicType.Reverse: player.PlayBackwards(this.AnimationName, this.Blend); break;
				case LogicType.Pause: player.Pause(); break;
				case LogicType.Stop: player.Stop(); break;
				case LogicType.Queue: player.Queue(this.AnimationName); break;
				case LogicType.PlaySection: player.PlaySection(this.AnimationName, this.SectionStartTime, this.SectionEndTime, this.Blend, this.Speed); break;
				case LogicType.PlaySectionWithMarkers: player.PlaySectionWithMarkers(this.AnimationName, this.SectionStartMarker, this.SectionEndMarker, this.Blend, this.Speed); break;
				case LogicType.ReverseSection: player.PlaySectionBackwards(this.AnimationName, this.SectionStartTime, this.SectionEndTime, this.Blend); break;
				case LogicType.ReverseSectionWithMarkers: player.PlaySectionWithMarkersBackwards(this.AnimationName, this.SectionStartMarker, this.SectionEndMarker, this.Blend); break;
			}
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private Array<AnimationPlayer> GetAnimationPlayers()
	{
		if(this.AnimationPlayer != null) { return new Array<AnimationPlayer>() { this.AnimationPlayer }; }
		
		if(!string.IsNullOrEmpty(this.AnimationPlayerGroup))
		{
			Array<Node> nodes = this.GetTree().GetNodesInGroup(this.AnimationPlayerGroup);
			Array<AnimationPlayer> players = new Array<AnimationPlayer>();
			
			foreach(Node node in nodes)
			{
				if(node is AnimationPlayer player)
				{
					players.Add(player);
				}
			}
			
			return players;
		}
		
		if(!string.IsNullOrEmpty(this.AnimationPlayerNodePath))
		{
			AnimationPlayer player = this.GetNodeOrNull<AnimationPlayer>(this.AnimationPlayerNodePath);
			
			if(player != null) { return new Array<AnimationPlayer>() { player }; }
		}
		
		return this.AnimationPlayers;
	}
	
	#endregion // Private Methods
	
	#region Types
	
	public enum LogicType
	{
		Play,
		PlaySection,
		PlaySectionWithMarkers,
		Pause,
		Reverse,
		ReverseSection,
		ReverseSectionWithMarkers,
		Stop,
		Queue,
	}
	
	#endregion // Types
}
