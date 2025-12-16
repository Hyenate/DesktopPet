using Godot;
using System;

public partial class AnimCard : Button
{
	public int DirectionCount {get; set;}
	
	public void SetAnimCard(string animName, PackedScene animSprites_res, int directionCount)
	{
		Name = animName;
		GetNode<Label>("VBoxContainer/AnimName").Text = animName;
		SubViewport animView = GetNode<SubViewport>("VBoxContainer/MarginContainer/AnimView/SubViewport");

		DirectionCount = directionCount;
		string previewDirection;
		if(directionCount == 8)
		{
			previewDirection = "SE";
		}
		else if(directionCount == 2)
		{
			previewDirection = "E";
		}
		else
		{
			previewDirection = "";
		}

		AnimatedSprite2D animSprites = animSprites_res.Instantiate<AnimatedSprite2D>();
		Vector2 animSize = animSprites.SpriteFrames.GetFrameTexture(animName + previewDirection, 0).GetSize() * animSprites.Scale;
		animView.Size = (Vector2I)animSize;

		// Comparison must be done with consideration to container proportions. Slightly larger proportion helps with edge cases.
		if(animSize.X * 0.9f > animSize.Y)
		{
			animView.GetParent<TextureRect>().ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
		}
		else
		{
			animView.GetParent<TextureRect>().ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		}
		animView.AddChild(animSprites);
		animSprites.Position = animSize / 2;
		animSprites.SpriteFrames.SetAnimationLoop(animName + previewDirection, true);
		animSprites.Play(animName + previewDirection);
	}

	private void OnCardPressed()
	{
		GetNode<PetEditor>("../../../../../../../").SetAnimEditor(Name, DirectionCount);
	}
}
