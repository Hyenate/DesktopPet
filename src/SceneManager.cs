using Godot;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
{
	public void LoadPetScene(string petName, float dragRadius, float physicsRadius, Dictionary<string, int> weights, bool useOverlay)
	{
		foreach(Node child in GetChildren())
		{
			child.QueueFree();
		}
		PackedScene pet_res = ResourceLoader.Load<PackedScene>("res://scenes/pet.tscn");
		Pet pet = pet_res.Instantiate<Pet>();
		pet.Name = "Pet";
		AddChild(pet);
		pet.UsingOverlay = useOverlay;

		PackedScene initializeOS_res = ResourceLoader.Load<PackedScene>("res://scenes/InitializeOS.tscn");
		var initializeOS = initializeOS_res.Instantiate();
		AddChild(initializeOS);
		initializeOS.Call("load_OS_settings", useOverlay, pet);

		PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("user://" + petName + ".res", "", ResourceLoader.CacheMode.Ignore);
		pet.InitializePet(petSprites_res.Instantiate<AnimatedSprite2D>(), dragRadius, physicsRadius, weights);
	}
}
