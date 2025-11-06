using Godot;
using System;
using Godot.Collections;

/// Class which extends AnimatedSprite2D, which automatically loads from sprite sheet data and AnimData.xml
public partial class PokeSprite : AnimatedSprite2D
{
	AnimationRegistry registry = new AnimationRegistry();
	[Export] public string SpriteFolder = "res://sprite/";
	public Array<string> animationDirections = new Array<string> {"", "S","SE","E","NE","N","NW","W","SW"};   // Blank direction (for anims with no direction) plus 8 compass directions

	public override void _Ready()
	{
		registry.Init();
		LoadAllAnimations();
	}

	/// Loads animations for each sprite sheet name.
	private void LoadAllAnimations()
	{
		 if (registry == null)
		{
			GD.PrintErr("No AnimationRegistry assigned!");
			return;
		}

		foreach (var entry in registry.Animations)
		{
			var info = entry.Value;
			string sheetPath = SpriteFolder + info.SheetName + ".png";
			BuildDirectionAnimationFromSpriteSheet(info.InternalName, sheetPath, info.FrameSize, info.HasDirections);
		}

		GD.Print("All animations loaded successfully!");
	}


	/// Loads a sprite sheet and slices it into animation frames.
	/// If the AnimationInfo.HasDirections = 0 (false), it will only load a single row, and append "" to the animation name
	/// If the AnimationInfo.HasDirections = 1 (true), it will load multiple rows, up to 8, each appending a direction from animationDirections[y + 1]
	private void BuildDirectionAnimationFromSpriteSheet(string animationName, string spriteSheetPath, Vector2I FrameSize, int hasDirections)
	{
		//GD.Print("Animation Name: " + animationName);
		Texture2D texture = GD.Load<Texture2D>(spriteSheetPath);
		if (texture == null)
		{
			GD.PrintErr($"[PokeSprite.cs: BuildDirectionAnimationFromSpriteSheet] Could not load sprite sheet: {spriteSheetPath}");
			return;
		}

		// Determine how many frames across and down
		int columns = texture.GetWidth() / FrameSize.X;
		int rows = texture.GetHeight() / FrameSize.Y;
		
		//GD.Print("Loaded " + columns + " columns and " + rows + " rows from sprite sheet: " + spriteSheetPath);
		
		for (int y = 0; y < rows; y++)
		{
			string finalAnimationName = animationName + animationDirections[y + hasDirections];			
			SpriteFrames.AddAnimation(finalAnimationName);
			SpriteFrames.SetAnimationLoop(finalAnimationName, true);
			//GD.Print("Final Animation Name: " + finalAnimationName);
			for (int x = 0; x < columns; x++)
			{
				// Create sub-texture for each frame
				var region = new Rect2I(x * FrameSize.X, y * FrameSize.Y, FrameSize.X, FrameSize.Y);
				var frameTexture = new AtlasTexture
				{
					Atlas = texture,
					Region = region
				};
				
				SpriteFrames.AddFrame(finalAnimationName, frameTexture);
			}
		}
	}
}
