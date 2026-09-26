using Godot;
using System;

public partial class SceneManager : Node
{
	private Vector2I defaultWindowSize;

    public override void _Ready()
	{
		defaultWindowSize = GetWindow().Size;
	}
	
	public void LoadPetScene(string petName, Pet.PetSettings petSettings, bool useOverlay)
	{	
		foreach(Node child in GetChildren())
		{
			child.QueueFree();
		}
		PackedScene pet_res = ResourceLoader.Load<PackedScene>("res://scenes/pet.tscn");
		Pet pet = pet_res.Instantiate<Pet>();
		pet.Name = "Pet";
		CallDeferred("add_child", pet);

		pet.UsingOverlay = useOverlay;

		PackedScene initializeOS_res = ResourceLoader.Load<PackedScene>("res://scenes/InitializeOS.tscn");
		var initializeOS = initializeOS_res.Instantiate();
		CallDeferred("add_child", initializeOS);
		initializeOS.Call("load_OS_settings", useOverlay, pet);

		PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("user://" + petName + ".res", "", ResourceLoader.CacheMode.Ignore);
		Callable.From(() => pet.InitializePet(petSprites_res.Instantiate<AnimatedSprite2D>(), petSettings)).CallDeferred();
	}

	public void LoadMenuScene()
	{
		foreach(Node child in GetChildren())
		{
			child.QueueFree();
		}

		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.AlwaysOnTop, false);
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		DisplayServer.WindowSetSize(defaultWindowSize);

		PackedScene saveMenu_res = ResourceLoader.Load<PackedScene>("res://scenes/saveMenu.tscn");
		SaveMenu menu = saveMenu_res.Instantiate<SaveMenu>();
		menu.Name = "Menu";
		menu.Reopened = true;
		AddChild(menu);
		GetWindow().MousePassthroughPolygon = [];
	}
}
