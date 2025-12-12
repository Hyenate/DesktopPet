using Godot;
using System;

public partial class Menu : Control
{
	private VBoxContainer menuContainer;
	private VBoxContainer petCollection;
	private PackedScene petSelection_res;
	private ConfigFile config;
	private static readonly float[] defaultCollisionRadii = [80.0f, 50.0f];
	private const string configPath = "user://pets.cfg";
	private const string configSection_Pet = "Pet Name";

	public override void _Ready()
	{
		// Hide incompatible modes
		if(OS.GetName() != "Windows")
		{
			GetNode<ColorRect>("VBoxContainer/Top Bar").Visible = false;
		}

		GetWindow().FilesDropped += AddPets;
		menuContainer = GetNode<VBoxContainer>("VBoxContainer");
		petCollection = menuContainer.GetNode<VBoxContainer>("Selection/ScrollContainer/VBoxContainer/Collection");
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
			petCollection.AddChild(petSelection);
			petSelection.LoadPetDetails(name);
		}
	}

	public void LoadSelectedPet(string name)
	{
		bool useOverlay = menuContainer.GetNode<CheckBox>("Top Bar/Windowed Mode/CheckBox").ButtonPressed;
		float[] collisionRadii = (float[])config.GetValue(configSection_Pet, name, defaultCollisionRadii);
		GetParent<SceneManager>().LoadPetScene(name, collisionRadii[0], collisionRadii[1], useOverlay);
	}

	private void OnLoadRandomPressed()
	{
		if(petCollection.GetChildCount() > 0)
		{
			Random rand = new Random();
			int choice = rand.Next(petCollection.GetChildCount());
			LoadSelectedPet(petCollection.GetChild(choice).Name);
		}

	}

	private void OnAddPetPressed()
	{
		menuContainer.GetNode<FileDialog>("Bottom Bar/VBoxContainer/RandomOrAdd/Add/FileDialog").Visible = true;
	}

	private void AddPets(string folder)
	{
		AddPets([folder]);
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

				Image preview = petSprites.SpriteFrames.GetFrameTexture("IdleSE", 0).GetImage();
				Rect2I croppedRect = preview.GetUsedRect();
				Image croppedPreview = Image.CreateEmpty(croppedRect.Size.X, croppedRect.Size.Y, false, preview.GetFormat());
				croppedPreview.BlitRect(preview, croppedRect, new Vector2I(0, 0));
				croppedPreview.SavePng("user://Pet" + newestIndex + "Icon.png");
				config.SetValue(configSection_Pet, "Pet" + newestIndex, defaultCollisionRadii);

				PetSelectionContainer petSelection = petSelection_res.Instantiate<PetSelectionContainer>();
				petCollection.AddChild(petSelection);
				petSelection.LoadPetDetails("Pet" + newestIndex);
			}
			else
			{
				GD.PrintErr("Error: \"" + folder + "\" is a file or does not exist.");
			}
		}
		config.Save(configPath);
	}

	public void LoadPetEditor(string petName)
	{
		menuContainer.Visible = false;
		float[] collisionRadii = (float[])config.GetValue(configSection_Pet, petName, defaultCollisionRadii);
		PackedScene petEditor_res = ResourceLoader.Load<PackedScene>("res://scenes/petEditor.tscn");
		PetEditor petEditor = petEditor_res.Instantiate<PetEditor>();
		AddChild(petEditor);
		petEditor.InitializePetEditor(petName, collisionRadii[0], collisionRadii[1]);
	}

	public void SavePetEdits(string petName, AnimatedSprite2D petSprites, float mouseHitboxRadius, float physicsHitboxRadius)
	{
		PackedScene packedScene = new PackedScene();
		packedScene.Pack(petSprites);
		ResourceSaver.Save(packedScene, "user://" + petName + ".res", ResourceSaver.SaverFlags.Compress);

		float[] hitboxRadii = [mouseHitboxRadius, physicsHitboxRadius];
		config.SetValue(configSection_Pet, petName, hitboxRadii);
		config.Save(configPath);
	}

	public void SubMenuExited()
	{
		menuContainer.Visible = true;
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

	public void ModifyPetNameInConfig(string newPetName, string oldPetName)
	{
		config.SetValue(configSection_Pet, newPetName, config.GetValue(configSection_Pet, oldPetName));
		config.EraseSectionKey(configSection_Pet, oldPetName);
		SaveCurrentPetOrder();
	}

	public void RemovePetFromConfig(string petName)
	{
		config.EraseSectionKey(configSection_Pet, petName);
		config.Save(configPath);
	}

	public void MovePetUp(PetSelectionContainer petSelection)
	{
		if(petSelection.GetIndex() > 0)
		{
			petCollection.MoveChild(petSelection, petSelection.GetIndex() - 1);
			SaveCurrentPetOrder();
		}
	}
	
	public void MovePetDown(PetSelectionContainer petSelection)
	{
		if(petSelection.GetIndex() < petCollection.GetChildCount() - 1)
		{
			petCollection.MoveChild(petSelection, petSelection.GetIndex() + 1);
			SaveCurrentPetOrder();
		}
	}

	public void SaveCurrentPetOrder()
	{
		ConfigFile newConfigOrder = new ConfigFile();
		foreach(Node petContainer in petCollection.GetChildren())
		{
			newConfigOrder.SetValue(configSection_Pet, petContainer.Name, config.GetValue(configSection_Pet, petContainer.Name));
		}
		config = newConfigOrder;
		config.Save(configPath);
	}
}
