using Godot;
using System;
using System.Collections.Generic;

public partial class SaveMenu : Control
{
	private VBoxContainer menuContainer;
	private VBoxContainer petCollection;
	private PackedScene petSelection_res;
	private ConfigFile config;
	private static readonly float[] defaultCollisionRadii = [80.0f, 70.0f];
	private const string configPath = "user://pets.cfg";
	private const string configSection_Pet = "Pet Name";
	private const string configSection_Globals = "Global Settings";

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
			if(config.HasSection(configSection_Pet))
			{
				LoadPetCollection();
			}

			if(!config.HasSection(configSection_Globals))
			{
				SetDefaultGlobalSettings();
			}
		}
		else
		{
			SetDefaultGlobalSettings();
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

	private void SetDefaultGlobalSettings()
	{
		config.SetValue(configSection_Globals, "WalkSpeed", 1);
		config.SetValue(configSection_Globals, "MinRerollTime", 3);
		config.SetValue(configSection_Globals, "MaxRerollTime", 10);
	}

	public void LoadSelectedPet(string name)
	{
		bool useOverlay = menuContainer.GetNode<CheckBox>("Top Bar/Windowed Mode/CheckBox").ButtonPressed;
		GetParent<SceneManager>().LoadPetScene(name, GetPetSettings(name), useOverlay);
	}

	private Pet.PetSettings GetPetSettings(string petName)
	{
		Pet.PetSettings petSettings = new();
		float[] radii = (float[])config.GetValue(configSection_Pet, petName, defaultCollisionRadii);
		petSettings.DragRadius = radii[0];
		petSettings.PhysicsRadius = radii[1];
		petSettings.WalkSpeed = (float)config.GetValue(configSection_Globals, "WalkSpeed");
		petSettings.MinRerollTime = (float)config.GetValue(configSection_Globals, "MinRerollTime");
		petSettings.MaxRerollTime = (float)config.GetValue(configSection_Globals, "MaxRerollTime");

		return petSettings;
	}

	private void OnLoadRandomPressed()
	{
		if(petCollection.GetChildCount() > 0)
		{
			Random rand = new();
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
				AnimationImportBuilder petSprites = petSprites_res.Instantiate<AnimationImportBuilder>();
				petSprites.LoadSpriteFiles(folder + "/");
				int newestIndex = 1;
				while(FileAccess.FileExists("user://Pet" + newestIndex + ".res"))
				{
					newestIndex++;
				}
				PackedScene packedScene = new PackedScene();
				packedScene.Pack(petSprites);
				ResourceSaver.Save(packedScene, "user://Pet" + newestIndex + ".res", ResourceSaver.SaverFlags.Compress);

				Image preview = GetPreviewImage(petSprites.SpriteFrames);
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

	private static Image GetPreviewImage(SpriteFrames anims)
	{
		if(anims.HasAnimation("IdleSE"))
		{
			return anims.GetFrameTexture("IdleSE", 0).GetImage();
		}
		else if(anims.HasAnimation("IdleE"))
		{
			return anims.GetFrameTexture("IdleE", 0).GetImage();
		}
		else if(anims.HasAnimation("Idle"))
		{
			return anims.GetFrameTexture("Idle", 0).GetImage();
		}
		else
		{
			// Default image if Idle does not exist
			return anims.GetFrameTexture(anims.GetAnimationNames()[0], 0).GetImage();
		}
	}

	public void LoadPetEditor(string petName)
	{
		menuContainer.Visible = false;
		PackedScene petEditor_res = ResourceLoader.Load<PackedScene>("res://scenes/petEditor.tscn");
		PetEditor petEditor = petEditor_res.Instantiate<PetEditor>();
		AddChild(petEditor);
		petEditor.InitializePetEditor(petName, GetPetSettings(petName));
	}

	public void SavePetEdits(string petName, AnimatedSprite2D petSprites, Pet.PetSettings petSettings)
	{
		PackedScene packedScene = new PackedScene();
		packedScene.Pack(petSprites);
		ResourceSaver.Save(packedScene, "user://" + petName + ".res", ResourceSaver.SaverFlags.Compress);

		float[] hitboxRadii = [petSettings.DragRadius, petSettings.PhysicsRadius];
		config.SetValue(configSection_Pet, petName, hitboxRadii);

		config.SetValue(configSection_Globals, "WalkSpeed", petSettings.WalkSpeed);
		config.SetValue(configSection_Globals, "MinRerollTime", petSettings.MinRerollTime);
		config.SetValue(configSection_Globals, "MaxRerollTime", petSettings.MaxRerollTime);

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

	public void ModifyPetName(string oldPetName, string newPetName)
	{
		DirAccess.RenameAbsolute("user://" + oldPetName + ".res", "user://" + newPetName + ".res");
		DirAccess.RenameAbsolute("user://" + oldPetName + "Icon.png", "user://" + newPetName + "Icon.png");
		config.SetValue(configSection_Pet, newPetName, config.GetValue(configSection_Pet, oldPetName));
		config.EraseSectionKey(configSection_Pet, oldPetName);
		SaveCurrentPetOrder();
	}

	public void DeletePet(string petName)
	{	
		DirAccess.RemoveAbsolute("user://" + petName + ".res");
		DirAccess.RemoveAbsolute("user://" + petName + "Icon.png");
		config.EraseSectionKey(configSection_Pet, petName);
		config.Save(configPath);
	}

	// Return array of animation names, dropping directional signatures
	private static string[] GetBaseAnimationNames(string[] allAnimations)
	{
		List<string> baseNames = [];
		// Subtract 1 to ignore empty default animation
		for(int i = 0; i < allAnimations.Length - 1; i++)
		{			
			if(allAnimations[i].EndsWith('E'))
			{
				// Oct-Directional animation detected (Next direction in alphabetical order of Oct)
				if(allAnimations[i + 1].EndsWith('N'))
				{
					baseNames.Add(allAnimations[i].Remove(allAnimations[i].Length - 1));
					i += 7;
				}
				// Bi-Directional animation detected
				else
				{
					baseNames.Add(allAnimations[i].Remove(allAnimations[i].Length - 1));
					i++;
				}
			}
			// Mono-Directional animation
			else
			{
				baseNames.Add(allAnimations[i]);
			}
		}
		return baseNames.ToArray();
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
		ConfigFile newConfigOrder = new();
		foreach(string section in config.GetSections())
		{
			// Modify "Pet Name" section, duplicate the rest
			if(section == configSection_Pet)
			{
				foreach(Node petContainer in petCollection.GetChildren())
				{
					newConfigOrder.SetValue(configSection_Pet, petContainer.Name, config.GetValue(configSection_Pet, petContainer.Name));
				}
			}
			else
			{
				foreach(string key in config.GetSectionKeys(section))
				{
					newConfigOrder.SetValue(section, key, config.GetValue(section, key));
				}
			}
			
		}

		config = newConfigOrder;
		config.Save(configPath);
	}
}
