using Godot;
using System;

public partial class PetEditor : MarginContainer
{
	Label nameTag;
	SubViewport mainAnimPreview;
	AnimatedSprite2D mainPetSprites;
	TextureRect mouseCollisionCircle;
	TextureRect physicsCollisionCircle;
	SpinBox mouseHitboxSetting;
	SpinBox physicsHitboxSetting;
	SpinBox scaleSetting;

	public override void _Ready()
	{
		Node petSettings = GetNode("ScrollContainer/VBoxContainer/PetSettings");

		nameTag = petSettings.GetNode<Label>("Name");
		mainAnimPreview = petSettings.GetNode<SubViewport>("Preview/PreviewMain/View/SubViewport");
		mouseCollisionCircle = petSettings.GetNode<TextureRect>("Preview/PreviewMain/MouseHitbox");
		physicsCollisionCircle = petSettings.GetNode<TextureRect>("Preview/PreviewMain/PhysicsHitbox");

		mouseHitboxSetting = petSettings.GetNode<SpinBox>("MouseSettings/InputRadius");
		physicsHitboxSetting = petSettings.GetNode<SpinBox>("PhysicsSettings/InputRadius");
		scaleSetting = petSettings.GetNode<SpinBox>("OtherSettings/InputScale");
	}

	public override void _Process(double delta)
	{
	}

	public void InitializePetEditor(string petName, float mouseHitboxRadius, float physicsHitboxRadius)
	{
		nameTag.Text = petName;
		mouseHitboxSetting.Value = mouseHitboxRadius;
		physicsHitboxSetting.Value = physicsHitboxRadius;

		// Spinboxes automatically clamps value for error handling.
		OnMouseHitboxRadiusChanged((float)mouseHitboxSetting.Value);
		OnPhysicsHitboxRadiusChanged((float)physicsHitboxSetting.Value);

		PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("user://" + petName + ".res");
		petSprites_res.SetLocalToScene(false);
		mainPetSprites = petSprites_res.Instantiate<AnimatedSprite2D>();

		mainPetSprites.Play("IdleSE");
		mainAnimPreview.AddChild(mainPetSprites);

		scaleSetting.Value = mainPetSprites.Scale.X;
		OnScaleChanged(mainPetSprites.Scale.X);

		Visible = true;
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

	private void OnSavePressed()
	{
		mainPetSprites.Position = Vector2.Zero;
		GetParent<Menu>().SavePetEdits(nameTag.Text, mainPetSprites, (float)mouseHitboxSetting.Value, (float)physicsHitboxSetting.Value);
		OnExitPressed();
	}

	private void OnExitPressed()
	{
		GetParent<Menu>().SubMenuExited();
		QueueFree();
	}
}
