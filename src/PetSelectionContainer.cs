using Godot;
using GodotPlugins.Game;
using System;

public partial class PetSelectionContainer : HBoxContainer
{
	private Menu menuHandler;
	private TextureRect icon;
	private LineEdit nameEdit;
	private string newName;

	public override void _Ready()
	{
		menuHandler = GetNode<Menu>("../../../../../../../Menu");
		icon = GetNode<TextureRect>("LoadPet/HBoxContainer/MarginContainer/Icon");
		nameEdit = GetNode<LineEdit>("Name");
		Name = "Pet";
		newName = "";
	}

	public void LoadPetDetails(string petName)
	{
		Image image = new Image();
		image.Load("user://" + petName + "Icon.png");
		ImageTexture texture = new ImageTexture();
		texture.SetImage(image);
		icon.Texture = texture;

		Name = petName;
		nameEdit.Text = petName;
	}

	private void OnLoadButtonPressed()
	{
		menuHandler.LoadSelectedPet(Name);
	}

	private void OnTextSubmitted(string submittedName)
	{
		string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + ".");
		string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );
		string sanitizedName = System.Text.RegularExpressions.Regex.Replace(submittedName, invalidRegStr, "" );

		if(sanitizedName != Name)
		{
			if(menuHandler.DoesPetNameExists(sanitizedName))
			{
				AcceptDialog errorWindow = GetNode<AcceptDialog>("Name/Error");
				errorWindow.Visible = true;
				errorWindow.DialogText = "\"" + sanitizedName + "\" already exists!";
				nameEdit.Text = Name;
			}
			else
			{
				newName = sanitizedName;
				AcceptDialog acceptDialog = GetNode<AcceptDialog>("Name/AcceptDialog");
				acceptDialog.Visible = true;
				acceptDialog.DialogText = "Change \"" + Name + "\" to \"" + newName + "\"?";
			}
		}   
	}

	private void OnTextUnfocused()
	{
		OnTextSubmitted(nameEdit.Text);
	}

	private void OnTextConfirmed()
	{
		DirAccess.RenameAbsolute("user://" + Name + ".res", "user://" + newName + ".res");
		DirAccess.RenameAbsolute("user://" + Name + "Icon.png", "user://" + newName + "Icon.png");
		string oldName = Name;
		Name = newName;
		nameEdit.Text = newName;
		menuHandler.ModifyPetNameInConfig(newName, oldName);
	}

	private void OnTextCanceled()
	{
		nameEdit.Text = Name;
	}

	private void OnEditPressed()
	{
		menuHandler.LoadPetEditor(Name);
	}

	private void OnDeletePressed()
	{
		GetNode<AcceptDialog>("Delete/AcceptDialog").Visible = true;
	}

	private void OnDeleteConfirmed()
	{
		DirAccess.RemoveAbsolute("user://" + Name + ".res");
		DirAccess.RemoveAbsolute("user://" + Name + "Icon.png");
		menuHandler.RemovePetFromConfig(Name);
		QueueFree();
	}

	private void OnMoveUpPressed()
	{
		menuHandler.MovePetUp(this);
	}

	private void OnMoveDownPressed()
	{
		menuHandler.MovePetDown(this);
	}
}
