using Godot;
using System;

public partial class ReopenMenu : Timer
{
	private bool rmbClickedOnce = false;
	private SceneManager sceneManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sceneManager = GetNode<SceneManager>("../../../SceneManager");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("rClick"))
		{
			if(rmbClickedOnce)
			{
				sceneManager.LoadMenuScene();
			}
			else
			{
				rmbClickedOnce = true;
				Start();
			}
		}
	}

	private void DoubleClickTimedOut()
	{
		rmbClickedOnce = false;
	}
}
