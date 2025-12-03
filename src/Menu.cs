using Godot;
using System;

public partial class Menu : Control
{
	private VBoxContainer vBox;
	private PackedScene petSelection_res;
	private ConfigFile config;
	private const string configPath = "user://pets.cfg";
	private const string configSection_Pet = "Pet Name";

	public override void _Ready()
	{
		GetWindow().FilesDropped += AddPets;
		vBox = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer/Selection");
		petSelection_res = ResourceLoader.Load<PackedScene>("res://scenes/petSelectionContainer.tscn");
		config = new ConfigFile();
		Error err = config.Load(configPath);
		if (err == Error.Ok)
		{
			LoadPetCollection();
		}
	}

	private void LoadPetCollection()
	{
		foreach(string name in config.GetSectionKeys(configSection_Pet))
		{
			PetSelectionContainer petSelection = petSelection_res.Instantiate<PetSelectionContainer>();
			vBox.AddChild(petSelection);
			petSelection.LoadPetDetails(name);
		}
	}

	public void LoadSelectedPet(string name)
    {
		bool useOverlay = GetNode<CheckBox>("ScrollContainer/VBoxContainer/Windowed Mode/CheckBox").ButtonPressed;
        GetParent<SceneManager>().LoadPetScene(name, useOverlay);
    }

	private void AddPets(string[] folders)
	{
		foreach(string folder in folders)
		{
			PackedScene petSprites_res = ResourceLoader.Load<PackedScene>("res://scenes/spriteHandler.tscn");
			if(DirAccess.DirExistsAbsolute(folder))
			{
				PokeSprite petSprites = petSprites_res.Instantiate<PokeSprite>();
				petSprites.LoadSpriteFiles(folder + "/");
				int newestIndex = 1;
				while(FileAccess.FileExists("user://Pet" + newestIndex + ".res"))
				{
					newestIndex++;
				}
				PackedScene packedScene = new PackedScene();
				packedScene.Pack(petSprites);
				ResourceSaver.Save(packedScene, "user://Pet" + newestIndex + ".res", ResourceSaver.SaverFlags.Compress);
				petSprites.SpriteFrames.GetFrameTexture("WalkSE", 0).GetImage().SavePng("user://Pet" + newestIndex + "Icon.png");
				config.SetValue(configSection_Pet, "Pet" + newestIndex, 0);

				PetSelectionContainer petSelection = petSelection_res.Instantiate<PetSelectionContainer>();
				vBox.AddChild(petSelection);
				petSelection.LoadPetDetails("Pet" + newestIndex);
			}
			else
			{
				GD.PrintErr("Error: \"" + folder + "\" is a file or does not exist.");
			}
		}
		config.Save(configPath);
	}

	public bool DoesPetNameExists(string testName)
	{
		foreach(string name in config.GetSectionKeys(configSection_Pet))
		{
			if(testName.Equals(name))
			{
				return true;
			}
		}
		return false;
	}

	public void RemovePetFromConfig(string petName)
	{
		config.EraseSectionKey(configSection_Pet, petName);
		config.Save(configPath);
	}

	public void SaveCurrentPetOrder()
	{
		config.Clear();
		foreach(Node petContainer in vBox.GetChildren())
		{
			config.SetValue(configSection_Pet, petContainer.Name, 0);
		}
		config.Save(configPath);
	}
}
