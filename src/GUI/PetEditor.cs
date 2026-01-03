using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PetEditor : MarginContainer
{
	private SpinBox minTimeSetting;
	private SpinBox maxTimeSetting;
	private SpinBox walkSpeedSetting;

	private Label nameTag;
	private SubViewport mainAnimPreview;
	private AnimatedSprite2D mainPetSprites;
	private TextureRect mouseCollisionCircle;
	private TextureRect physicsCollisionCircle;
	private SpinBox mouseHitboxSetting;
	private SpinBox physicsHitboxSetting;
	private SpinBox scaleSetting;

	private Label animEditorName;
	private SubViewport animEditorView;
	private SpinBox animSpeed;
	private SpinBox animWeight;
	private Label animWeightProportion;
	private CheckBox animLoop;
	private AnimatedSprite2D animEditorSprites;

	private Pet.Direction currDirection;
	private int editorDirectionCount;
	private Dictionary<string, int> WeightsForPet = [];
	private int weightSum = 0;

	public override void _Ready()
	{
		Node globalSettings = GetNode("ScrollContainer/VBoxContainer/GlobalSettings");
		minTimeSetting = globalSettings.GetNode<SpinBox>("Timing/MinInput");
		maxTimeSetting = globalSettings.GetNode<SpinBox>("Timing/MaxInput");
		walkSpeedSetting = globalSettings.GetNode<SpinBox>("WalkSpeed/Input");

		Node localSettings = GetNode("ScrollContainer/VBoxContainer/LocalSettings");
		nameTag = localSettings.GetNode<Label>("Title/Name");
		mainAnimPreview = localSettings.GetNode<SubViewport>("Preview/PreviewMain/View/SubViewport");
		mouseCollisionCircle = localSettings.GetNode<TextureRect>("Preview/PreviewMain/MouseHitbox");
		physicsCollisionCircle = localSettings.GetNode<TextureRect>("Preview/PreviewMain/PhysicsHitbox");
		mouseHitboxSetting = localSettings.GetNode<SpinBox>("MouseSettings/Input");
		physicsHitboxSetting = localSettings.GetNode<SpinBox>("PhysicsSettings/Input");
		scaleSetting = localSettings.GetNode<SpinBox>("OtherSettings/InputScale");

		Node animSettings = GetNode("ScrollContainer/VBoxContainer/AnimationEdit/AnimEdit");
		animEditorName = animSettings.GetNode<Label>("Title/Name");
		animEditorView = animSettings.GetNode<SubViewport>("Editor/AnimView/SubViewport");
		animSpeed = animSettings.GetNode<SpinBox>("Editor/Settings/AnimSpeed/Input");
		animWeight = animSettings.GetNode<SpinBox>("Editor/Settings/AnimWeight/Input");
		animWeightProportion = animSettings.GetNode<Label>("Editor/Settings/AnimWeight/Suffix");
		animLoop = animSettings.GetNode<CheckBox>("Editor/Settings/AnimLoop/Input");
	}

	public void InitializePetEditor(string petName, Pet.PetSettings configSettings)
	{
		PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("user://" + petName + ".res", "", ResourceLoader.CacheMode.Ignore);
		petSprites_res.SetLocalToScene(false);

		InitializeGlobalSettings(configSettings.MinRerollTime, configSettings.MaxRerollTime, configSettings.WalkSpeed);
		InitializeLocalSettings(petName, configSettings.DragRadius, configSettings.PhysicsRadius, petSprites_res);
		InitializeAnimSelection(petSprites_res);
		InitilizeAnimEditor(petSprites_res);

		Visible = true;
	}

	private void InitializeGlobalSettings(float minTime, float maxTime, float walkSpeed)
	{
		minTimeSetting.Value = minTime;
		maxTimeSetting.Value = maxTime;
		walkSpeedSetting.Value = walkSpeed;
	}

	private void InitializeLocalSettings(string petName, float mouseHitboxRadius, float physicsHitboxRadius, PackedScene petSprites_res)
	{
		nameTag.Text = petName;
		mouseHitboxSetting.Value = mouseHitboxRadius;
		physicsHitboxSetting.Value = physicsHitboxRadius;

		// Spinboxes automatically clamps value for error handling.
		OnMouseHitboxRadiusChanged((float)mouseHitboxSetting.Value);
		OnPhysicsHitboxRadiusChanged((float)physicsHitboxSetting.Value);

		mainPetSprites = petSprites_res.Instantiate<AnimatedSprite2D>();
		if(mainPetSprites.SpriteFrames.HasAnimation("IdleSE"))
		{
			mainPetSprites.Play("IdleSE");
		}
		else if(mainPetSprites.SpriteFrames.HasAnimation("IdleE"))
		{
			mainPetSprites.Play("IdleE");
		}
		else if(mainPetSprites.SpriteFrames.HasAnimation("Idle"))
		{
			mainPetSprites.Play("Idle");
		}
		else
		{
			mainPetSprites.Play(mainPetSprites.SpriteFrames.GetAnimationNames()[0]);
		}
		mainAnimPreview.AddChild(mainPetSprites);

		scaleSetting.Value = mainPetSprites.Scale.X;
		OnScaleChanged(mainPetSprites.Scale.X);
	}

	private void InitializeAnimSelection(PackedScene petSprites_res)
	{
		Node animationSelection = GetNode("ScrollContainer/VBoxContainer/AnimationSelect/ScrollWindow/ScrollContainer/HBoxContainer");
		PackedScene animCard_res = ResourceLoader.Load<PackedScene>("res://scenes/animCard.tscn");
		string[] animNames = mainPetSprites.SpriteFrames.GetAnimationNames();

		// Subtract 1 to ignore empty default animation
		for(int i = 0; i < animNames.Length - 1; i++)
		{
			AnimCard animCard = animCard_res.Instantiate<AnimCard>();
			string animBaseName;
			
			// Multi-Directional animation detected
			if(animNames[i].EndsWith('E'))
			{
				animBaseName = animNames[i].Remove(animNames[i].Length - 1);
				// Oct-Directional animation detected (Next direction in alphabetical order of Oct)
				if(animNames[i + 1].EndsWith('N'))
				{
					animCard.SetAnimCard(animBaseName, petSprites_res, 8);
					i += 7;
				}
				// Bi-Directional animation detected
				else
				{
					animCard.SetAnimCard(animBaseName, petSprites_res, 2);
					i++;
				}
			}
			// Mono-Directional animation
			else
			{
				animBaseName = animNames[i];
				animCard.SetAnimCard(animNames[i], petSprites_res, 1);
			}

			// Check for stored animation weight
			if(mainPetSprites.HasMeta(animBaseName))
			{
				WeightsForPet.Add(animBaseName, (int)mainPetSprites.GetMeta(animBaseName));
			}
			else
			{
				WeightsForPet.Add(animBaseName, 0);
			}

			weightSum += WeightsForPet[animBaseName];
			animationSelection.AddChild(animCard);
		}
	}

	private void InitilizeAnimEditor(PackedScene petSprites_res)
	{
		animEditorSprites = petSprites_res.Instantiate<AnimatedSprite2D>();
		animEditorSprites.SpriteFrames = mainPetSprites.SpriteFrames;
		animEditorSprites.AnimationLooped += OnEditorAnimEnd;
		animEditorSprites.AnimationFinished += OnEditorAnimEnd;
		animEditorView.AddChild(animEditorSprites);

		AnimCard firstAnim = (AnimCard)GetNode("ScrollContainer/VBoxContainer/AnimationSelect/ScrollWindow/ScrollContainer/HBoxContainer").GetChild(0);
		SetAnimEditor(firstAnim.Name, firstAnim.DirectionCount);
	}

	public void SetAnimEditor(string animName, int directionCount)
	{
		animEditorName.Text = animName;
		editorDirectionCount = directionCount;

		string previewDirection;
		if(directionCount == 8)
		{
			previewDirection = "SE";
			currDirection = Pet.Direction.SE;
		}
		else if(directionCount == 2)
		{
			previewDirection = "E";
		}
		else
		{
			previewDirection = "";
		}

		animSpeed.Value = animEditorSprites.SpriteFrames.GetAnimationSpeed(animName + previewDirection);
		animWeight.Value = WeightsForPet[animName];
		animWeightProportion.Text = $"/{weightSum} ({(animWeight.Value / weightSum * 100).ToString("F1")}%)";
		animLoop.ButtonPressed = animEditorSprites.SpriteFrames.GetAnimationLoop(animName + previewDirection);

		animEditorSprites.Play(animName + previewDirection);
		Vector2 spriteSize = animEditorSprites.SpriteFrames.GetFrameTexture(animName + previewDirection, 0).GetSize() * animEditorSprites.Scale;
		animEditorView.Size = (Vector2I)spriteSize;
		animEditorSprites.Position = spriteSize / 2;
	}

	private void OnEditorAnimEnd()
	{
		if(editorDirectionCount == 8)
		{
			// Set next direction in cycle
			if(currDirection == Pet.Direction.SW)
			{
				currDirection = Pet.Direction.S;
			}
			else
			{
				currDirection++;
			}
			animEditorSprites.Play(animEditorName.Text + currDirection.ToString());
		}
		else if(editorDirectionCount == 2)
		{
			animEditorSprites.Play(animEditorName.Text + "E");
		}
		else if(editorDirectionCount == 1)
		{
			animEditorSprites.Play(animEditorName.Text);
		}
	}

	private void OnMinTimeChanged(float newMinTime)
	{
		if(newMinTime > maxTimeSetting.Value)
		{
			maxTimeSetting.Value = newMinTime;
		}
	}

	private void OnMaxTimeChanged(float newMaxTime)
	{
		if(newMaxTime < minTimeSetting.Value)
		{
			minTimeSetting.Value = newMaxTime;
		}
	}

	private void OnMouseHitboxVisibilityToggled(bool visible)
	{
		mouseCollisionCircle.Visible = visible;
	}

	private void OnMouseHitboxRadiusChanged(float newRadius)
	{
		((GradientTexture2D)mouseCollisionCircle.Texture).SetHeight((int)newRadius * 2);
		((GradientTexture2D)mouseCollisionCircle.Texture).SetWidth((int)newRadius * 2);
	}

	private void OnPhysicsHitboxVisibilityToggled(bool visible)
	{
		physicsCollisionCircle.Visible = visible;
	}

	private void OnPhysicsHitboxRadiusChanged(float newRadius)
	{
		((GradientTexture2D)physicsCollisionCircle.Texture).SetHeight((int)newRadius * 2);
		((GradientTexture2D)physicsCollisionCircle.Texture).SetWidth((int)newRadius * 2);
	}

	private void OnScaleChanged(float newScale)
	{
		mainPetSprites.Scale = new Vector2(newScale, newScale);

		Vector2 spriteSize = mainPetSprites.SpriteFrames.GetFrameTexture("IdleSE", 0).GetSize() * mainPetSprites.Scale;
		mainAnimPreview.Size = (Vector2I)spriteSize;
		mainAnimPreview.GetNode<ColorRect>("../../../PreviewBG").CustomMinimumSize = spriteSize + new Vector2(10, 10);
		mainPetSprites.Position = spriteSize / 2;
	}

	private void OnAnimSpeedChanged(float newSpeed)
	{
		foreach(string animVariant in GetAnimNameVariants(animEditorName.Text, editorDirectionCount))
		{
			animEditorSprites.SpriteFrames.SetAnimationSpeed(animVariant, newSpeed);
		}
	}

	private void OnAnimWeightChanged(float newWeight)
	{
		int oldWeight = WeightsForPet[animEditorName.Text];
		WeightsForPet[animEditorName.Text] = (int)newWeight;
		weightSum = weightSum - oldWeight + (int)newWeight;
		animWeightProportion.Text = $"/{weightSum} ({(animWeight.Value / weightSum * 100).ToString("F1")}%)";
	}

	private void OnAnimLoopToggled(bool pressed)
	{
		foreach(string animVariant in GetAnimNameVariants(animEditorName.Text, editorDirectionCount))
		{
			animEditorSprites.SpriteFrames.SetAnimationLoop(animVariant, pressed);
		}
	}

	private static string[] GetAnimNameVariants(string baseName, int dirCount)
	{
		List<string> variants = [];
		if(dirCount == 8)
		{
			for(int i = 0; i < 8; i++)
			{
				variants.Add(baseName + ((Pet.Direction)i).ToString());
			}
		}
		else if(dirCount == 2)
		{
			variants.Add(baseName + 'E');
			variants.Add(baseName + 'W');
		}
		else
		{
			return [baseName];
		}
		return variants.ToArray();
	}

	private void OnSavePressed()
	{
		mainPetSprites.Position = Vector2.Zero;
		foreach(string animName in WeightsForPet.Keys)
		{
			mainPetSprites.SetMeta(animName, WeightsForPet[animName]);
		}

		Pet.PetSettings petSettings = new();
		petSettings.DragRadius = (float)mouseHitboxSetting.Value;
		petSettings.PhysicsRadius = (float)physicsHitboxSetting.Value;
		petSettings.WalkSpeed = (float)walkSpeedSetting.Value;
		petSettings.MinRerollTime = (float)minTimeSetting.Value;
		petSettings.MaxRerollTime = (float)maxTimeSetting.Value;

		GetParent<SaveMenu>().SavePetEdits(nameTag.Text, mainPetSprites, petSettings);
		OnExitPressed();
	}

	private void OnExitPressed()
	{
		GetParent<SaveMenu>().SubMenuExited();
		QueueFree();
	}
}
