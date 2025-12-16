using Godot;
using System;
using System.Collections.Generic;

public partial class SaveMenu : Control
{
	private VBoxContainer menuContainer;
	private VBoxContainer petCollection;
	private PackedScene petSelection_res;
	private ConfigFile config;
	private static readonly float[] defaultCollisionRadii = [80.0f, 50.0f];
	private const string configPath = "user://pets.cfg";
	private const string configSection_Pet = "Pet Name";
	private const string configSection_Anim = "Extra Animation Settings";

	public class CollisionRadii(float[] radii)
	{
		public float Mouse { get; set; } = radii[0];
		public float Physics { get; set; } = radii[1];
	}

	// UsageCount tracks how many pets use this animation for accurate deletion purposes (+1 for default animations)
	public class AnimationSetting(int[] settings)
	{
		public int Weight { get; set; } = settings[0];
		public int UsageCount { get; set; } = settings[1];
	}

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
		}
		else
		{
			SetDefaultWeights();
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

	private void SetDefaultWeights()
	{
		config.SetValue(configSection_Anim, "Idle", (int[])[20, 1]);
		config.SetValue(configSection_Anim, "Walk", (int[])[40, 1]);
		config.SetValue(configSection_Anim, "Sleep", (int[])[20, 1]);
		config.SetValue(configSection_Anim, "Rotate", (int[])[5, 1]);
		config.SetValue(configSection_Anim, "Hop", (int[])[15, 1]);
	}

	private Dictionary<string, int> GetWeights()
	{
		Dictionary<string, int> weights = [];
		foreach(string animName in config.GetSectionKeys(configSection_Anim))
		{
			weights.Add(animName, new AnimationSetting((int[])config.GetValue(configSection_Anim, animName)).Weight);
		}
		return weights;
	}

	public void LoadSelectedPet(string name)
	{
		bool useOverlay = menuContainer.GetNode<CheckBox>("Top Bar/Windowed Mode/CheckBox").ButtonPressed;
		CollisionRadii radii = new((float[])config.GetValue(configSection_Pet, name, defaultCollisionRadii));
		GetParent<SceneManager>().LoadPetScene(name, radii.Mouse, radii.Physics, GetWeights(), useOverlay);
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

				Image preview = petSprites.SpriteFrames.GetFrameTexture("IdleSE", 0).GetImage();
				Rect2I croppedRect = preview.GetUsedRect();
				Image croppedPreview = Image.CreateEmpty(croppedRect.Size.X, croppedRect.Size.Y, false, preview.GetFormat());
				croppedPreview.BlitRect(preview, croppedRect, new Vector2I(0, 0));
				croppedPreview.SavePng("user://Pet" + newestIndex + "Icon.png");

				config.SetValue(configSection_Pet, "Pet" + newestIndex, defaultCollisionRadii);
				foreach(string animName in petSprites.registry.Animations.Keys)
				{
					AnimationSetting setting;
					if(config.HasSectionKey(configSection_Anim, animName))
					{
						setting = new ((int[])config.GetValue(configSection_Anim, animName));
						setting.UsageCount++;
					}
					else
					{
						// default (0 weight, 1 pet uses this animation) 
						setting = new ((int[])[0, 1]);
					}
					config.SetValue(configSection_Anim, animName, (int[])[setting.Weight, setting.UsageCount]);
				}

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
		CollisionRadii radii = new ((float[])config.GetValue(configSection_Pet, petName, defaultCollisionRadii));
		PackedScene petEditor_res = ResourceLoader.Load<PackedScene>("res://scenes/petEditor.tscn");
		PetEditor petEditor = petEditor_res.Instantiate<PetEditor>();
		AddChild(petEditor);
		petEditor.InitializePetEditor(petName, radii.Mouse, radii.Physics, GetWeights());
	}

	public void SavePetEdits(string petName, AnimatedSprite2D petSprites, CollisionRadii radii, Dictionary<string, int> newWeights)
	{
		PackedScene packedScene = new PackedScene();
		packedScene.Pack(petSprites);
		ResourceSaver.Save(packedScene, "user://" + petName + ".res", ResourceSaver.SaverFlags.Compress);

		float[] hitboxRadii = [radii.Mouse, radii.Physics];
		config.SetValue(configSection_Pet, petName, hitboxRadii);

		foreach(string animName in newWeights.Keys)
		{
			int usageCount = ((int[])config.GetValue(configSection_Anim, animName))[1];
			config.SetValue(configSection_Anim, animName, (int[])[newWeights[animName], usageCount]);
		}

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
		PackedScene resToDelete = ResourceLoader.Load<PackedScene>("user://" + petName + ".res", "" , ResourceLoader.CacheMode.Ignore);
		AnimatedSprite2D petToDelete = resToDelete.Instantiate<AnimatedSprite2D>();

		// Check for and delete settings for animations that are no longer used
		foreach(string animName in GetBaseAnimationNames(petToDelete.SpriteFrames.GetAnimationNames()))
		{
			if(config.HasSectionKey(configSection_Anim, animName))
			{
				AnimationSetting setting = new ((int[])config.GetValue(configSection_Anim, animName));
				setting.UsageCount--;
				if(setting.UsageCount == 0)
				{
					config.EraseSectionKey(configSection_Anim, animName);
				}
				else
				{
					config.SetValue(configSection_Anim, animName, (int[])[setting.Weight, setting.UsageCount]);
				}
			}
		}

		config.Save(configPath);
		DirAccess.RemoveAbsolute("user://" + petName + ".res");
		DirAccess.RemoveAbsolute("user://" + petName + "Icon.png");
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
