using Godot;
using System;

public partial class AnimCard : Button
{
	public void SetAnimCard(string animName, PackedScene animSprites_res, string directionToPlay)
	{
		GetNode<Label>("VBoxContainer/AnimName").Text = animName;
		SubViewport animView = GetNode<SubViewport>("VBoxContainer/MarginContainer/AnimView/SubViewport");

		AnimatedSprite2D animSprites = animSprites_res.Instantiate<AnimatedSprite2D>();
		Vector2 animSize = animSprites.SpriteFrames.GetFrameTexture(animName + directionToPlay, 0).GetSize() * animSprites.Scale;
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
		animSprites.Play(animName + directionToPlay);
	}
}
