using Godot;
using System;
using Godot.Collections;

/// Class which extends AnimatedSprite2D, which automatically loads from sprite sheet data and AnimData.xml
public partial class PokeSprite : AnimatedSprite2D
{
	public static readonly Array<string> animationDirections = new Array<string> {"S","SE","E","NE","N","NW","W","SW"};   // 8 compass directions
	private AnimationRegistry registry = new AnimationRegistry();
	private const float AnimDefaultFPS = 30.0f;

	public void LoadSpriteFiles(string spriteFolder)
	{
		registry.Init(spriteFolder);
		LoadAllAnimations(spriteFolder);
	}

	/// Loads animations for each sprite sheet name.
	private void LoadAllAnimations(string spriteFolder)
	{
		 if (registry == null)
		{
			GD.PrintErr("No AnimationRegistry assigned!");
			return;
		}

		foreach (var entry in registry.Animations)
		{
			var info = entry.Value;
			string sheetPath = spriteFolder + info.SheetName + ".png";
			BuildAnimationFromSpriteSheet(info.InternalName, sheetPath, info.FrameSize, info.FrameDurations);
		}

		GD.Print("All animations loaded successfully!");
	}


	/// Loads a sprite sheet and slices it into animation frames.
	private void BuildAnimationFromSpriteSheet(string animationName, string spriteSheetPath, Vector2I FrameSize, int[] FrameDurations)
	{
		//GD.Print("Animation Name: " + animationName);
		Image image = new Image();
		image.Load(spriteSheetPath);

		ImageTexture texture = new ImageTexture();
		texture.SetImage(image);		
		if (texture == null)
		{
			GD.PrintErr($"[PokeSprite.cs: BuildAnimationFromSpriteSheet] Could not load sprite sheet: {spriteSheetPath}");
			return;
		}

		// Determine how many frames across and down
		int columns = texture.GetWidth() / FrameSize.X;
		int rows = texture.GetHeight() / FrameSize.Y;

		// Remove redundant "Rotate" frames if they exist
		if (animationName == "Spin")
		{
			columns = 8;
			rows = 1;
		}
		
		//GD.Print("Loaded " + columns + " columns and " + rows + " rows from sprite sheet: " + spriteSheetPath);
		
		for (int y = 0; y < rows; y++)
		{
			string finalAnimationName;
			if(rows == 1)
			{
				finalAnimationName = animationName;			
			}
			else
			{
				finalAnimationName = animationName + animationDirections[y];
			}
			SpriteFrames.AddAnimation(finalAnimationName);
			SpriteFrames.SetAnimationLoop(finalAnimationName, true);

			// Imported PMD anims are natively 60 fps, but can be distracting outside of their intended gameplay
			// Some timings have been adjusted to mitigate this issue
			SpriteFrames.SetAnimationSpeed(finalAnimationName, AnimDefaultFPS);
			if (animationName == "Hop" || animationName == "Spin")
			{
				SpriteFrames.SetAnimationSpeed(finalAnimationName, AnimDefaultFPS * 0.75f);
			}
			else if(animationName == "Sleep")
			{
				SpriteFrames.SetAnimationSpeed(finalAnimationName, AnimDefaultFPS * 2);
			}
			else
			{
				SpriteFrames.SetAnimationSpeed(finalAnimationName, AnimDefaultFPS);
			}

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
				
				SpriteFrames.AddFrame(finalAnimationName, frameTexture, FrameDurations[x]);
			}
		}

		// Walk animation row count compatibility
		if(animationName == "Walk")
		{
			if(rows == 8)
			{
				SpriteFrames.RemoveAnimation("WalkS");
				SpriteFrames.RemoveAnimation("WalkSE");
				SpriteFrames.RemoveAnimation("WalkNE");
				SpriteFrames.RemoveAnimation("WalkN");
				SpriteFrames.RemoveAnimation("WalkNW");
				SpriteFrames.RemoveAnimation("WalkSW");
			}
			else if(rows == 2)
			{
				SpriteFrames.RenameAnimation("WalkS", "WalkE");
				SpriteFrames.RenameAnimation("WalkSE", "WalkW");
			}
		}
	}
}
