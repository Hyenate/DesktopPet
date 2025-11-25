using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WindowsPet : Pet
{
	private const float Gravity = 980f;
	private const float TerminalVelocity = 2000f;
	
	public string currentState = "";
	private ThrowableBehavior throwableBehavior;

	private int physicsFrame = 0;    // Debug

	public override void _Ready()
	{
		foreach (var weight in Weights.Values.ToList())
		{
			weightTotal += weight;
		}
		anims = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		anims.Play("HopS");
		timer = GetNode<Timer>("Timer");
		rand = new Random();
		
		// Get throwable behavior
		throwableBehavior = GetNode<ThrowableBehavior>("ThrowableBehavior");
		if (throwableBehavior == null)
		{
			GD.PrintErr("ThrowableBehavior node not found! Please add it as a child.");
		}
		else
		{
			// Subscribe to the throwable behavior events
			throwableBehavior.OnDragStarted += OnDragStarted;
			throwableBehavior.OnDragStopped += OnDragStopped;
			throwableBehavior.OnThrown += OnThrown;
		}
		
		//GD.Print("Pet initialized - waiting for spawn to finish");
	}

	public override void _PhysicsProcess(double delta)
	{
		physicsFrame++;
		
		// Debug: Print state every 60 frames
		if (physicsFrame % 60 == 0)
		{
			//GD.Print($"Physics: FinishedSpawning={finishedSpawning}, OnFloor={IsOnFloor()}, Velocity={Velocity}, ThrowableHandling={!throwableBehavior.ShouldParentHandlePhysics()}");
		}

		// Apply gravity regardless of throwable state
		Velocity = ApplyGravity(Velocity, delta);

		// Only handle normal physics if ThrowableBehavior allows it
		if (throwableBehavior.ShouldParentHandlePhysics())
		{
			ApplyNormalPhysics(delta);
		}
		
		MoveAndSlide();
	}

	private Vector2 ApplyGravity(Vector2 velocity, double delta)
	{
		velocity.Y += Gravity * (float)delta;
		
		// Cap terminal velocity
		if (velocity.Y > TerminalVelocity)
			velocity.Y = TerminalVelocity;
			
		return velocity;
	}

	private void ApplyNormalPhysics(double delta)
	{
		Vector2 velocity = Velocity;

		// Only apply walking behavior if spawn finished and on floor
		if (IsOnFloor())
		{
			ApplyWalkingBehavior(ref velocity);
		}
		else
		{
			// If still spawning or falling, don't walk
			velocity.X = 0;
		}
		
		Velocity = velocity;
	}


	private void ApplyWalkingBehavior(ref Vector2 velocity)
	{
		if (anims.Animation == "WalkE")
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 60, Speed);
			if (IsOnWall())
			{
				dir = Direction.W;
				anims.Animation = "WalkW";
				velocity.X = Mathf.MoveToward(Velocity.X, -60, Speed);
			}
		}
		else if (anims.Animation == "WalkW")
		{
			velocity.X = Mathf.MoveToward(Velocity.X, -60, Speed);
			if (IsOnWall())
			{
				dir = Direction.E;
				anims.Animation = "WalkE";
				velocity.X = Mathf.MoveToward(Velocity.X, 60, Speed);
			}
		}
		else
		{
			velocity.X = 0;
		}
	}

	public override void RandomizeState()
	{
		if (throwableBehavior.IsBeingThrown || throwableBehavior.IsBeingDragged) 
			return;

		State state = RollForRandomState();
		timer.WaitTime = rand.Next(7) + 3;

		if (state == State.Idle)
		{
			dir = (Direction)rand.Next(8);
			anims.Play("Idle" + dir.ToString());
		}
		else if (state == State.Walk)
		{
			if (rand.Next(2) == 0)
			{
				dir = Direction.E;
				anims.Play("WalkE");
			}
			else
			{
				dir = Direction.W;
				anims.Play("WalkW");
			}
		}
		else if (state == State.Sleep)
		{
			anims.Play("Sleep");
		}
		else if (state == State.Spin)
		{
			anims.Play("Idle");
		}
		else if (state == State.Hop)
		{
			anims.Play("Hop" + dir.ToString());
		}

		//GD.Print($"Randomized state: {state}");
	}

	public override void OnAnimationLoopEnd()
	{
		if (throwableBehavior.IsBeingDragged || throwableBehavior.IsBeingThrown) 
			return;

		if(anims.Animation.ToString().Contains("Hop"))
		{
			RandomizeState();
		}    
	}
	
	
	private void OnDragStarted()
	{
		// Stop any current animations and timer when dragging starts
		timer.Stop();
		anims.Stop();
		//GD.Print("Pet: Drag started");
	}

	private void OnDragStopped()
	{
		// Resume normal behavior when dragging stops
		timer.Start();
		RandomizeState();
		//GD.Print("Pet: Drag stopped");
	}

	private void OnThrown(Vector2 throwForce)
	{
		// Handle throw behavior - you might want to play a special animation
		//GD.Print($"Pet: Thrown with force {throwForce}");
		
		// You could add a special "thrown" animation here if desired
		// For example, play a spinning or flailing animation
		string animationString = "Charge";
		if(Velocity.X > 0){
			animationString += "W";
		}
		else{
			animationString += "E";
		}
		anims.Play(animationString); // Or create a "Thrown" animation
	}

	// Public methods for TransparentOverlay
	public bool IsBeingDragged()
	{
		return throwableBehavior?.IsBeingDragged ?? false;
	}

	public bool IsBeingThrown()
	{
		return throwableBehavior?.IsBeingThrown ?? false;
	}
}
