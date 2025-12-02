using Godot;
using System;

public partial class SceneManager : Node
{
	public void LoadPet(string petName)
	{
		foreach(Node child in GetChildren())
		{
			child.QueueFree();
		}
		PackedScene initializeOS_res = ResourceLoader.Load<PackedScene>("res://scenes/InitializeOS.tscn");
		var initializeOS = initializeOS_res.Instantiate();
		AddChild(initializeOS);

		PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("user://" + petName + ".res");
		Pet pet = initializeOS.GetNode<Pet>("Pet");
		pet.InitializePet(petSprites_res.Instantiate<AnimatedSprite2D>());
	}
}
