using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pet : CharacterBody2D
{
	public enum Direction
	{
		S,
		SE,
		E,
		NE,
		N,
		NW,
		W,
		SW
	}

	public bool UsingOverlay {get; set;}

	private Dictionary<string, int> Weights;
	private int weightTotal = 0;
	private Direction dir = Direction.S;
	private const float Speed = 300.0f;
	private AnimatedSprite2D anims;
	private Timer timer;
	private Random rand;
	private bool initialized;
	private ThrowableBehavior throwableBehavior;

	private const float Gravity = 980f;
	private const float TerminalVelocity = 2000f;
	private static readonly Vector2 InitialVelocity = new(0, -400);

	public override void _Ready()
	{
		initialized = false;
	}

	public void InitializePet(AnimatedSprite2D petSprites, float dragRadius, float physicsRadius, Dictionary<string, int> weights)
	{
		AddChild(petSprites);
		anims = petSprites;
		anims.AnimationFinished += RandomizeState; 	// If animation doesn't loop, immediately reroll upon completion
		anims.Play("HopS");
		timer = GetNode<Timer>("Timer");
		timer.Start();

		((CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape).Radius = physicsRadius;
		rand = new Random();
		Position = GetViewportRect().Size / 2;
		Velocity = InitialVelocity;
		GetWindow().MousePassthrough = false;

		throwableBehavior = GetNode<ThrowableBehavior>("ThrowableBehavior");
		throwableBehavior.OnDragStarted += OnDragStarted;
		throwableBehavior.OnDragStopped += OnDragStopped;
		throwableBehavior.OnThrown += OnThrown;
		throwableBehavior.DragRadius = dragRadius;

		Weights = weights;
		foreach (var weight in Weights)
		{
			if(weight.Value == 0 || GetDirectionCount(weight.Key) == -1)
			{
				// Remove unused/nonexistent weights for this pet
				Weights.Remove(weight.Key);
			}
			else
			{
				weightTotal += weight.Value;
			}
		}

		initialized = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(initialized)
		{
			Velocity = ApplyGravity(Velocity, delta);

			if(!UsingOverlay)
			{
				GetWindow().MousePassthroughPolygon = GetOffsetPolygon();
			}

			if (throwableBehavior.ShouldParentHandlePhysics())
			{
				ApplyNormalPhysics(delta);
			}	
			MoveAndSlide();  
		}
	}

	private static Vector2 ApplyGravity(Vector2 velocity, double delta)
	{
		velocity.Y += Gravity * (float)delta;
		
		// Cap terminal velocity
		if (velocity.Y > TerminalVelocity)
			velocity.Y = TerminalVelocity;
			
		return velocity;
	}

	public void ApplyNormalPhysics(double delta)
	{
		Vector2 velocity = Velocity;

		// Only apply walking behavior if spawn finished and on floor
		if (IsOnFloor())
		{
			ApplyWalkingBehavior(ref velocity);
		}
		else
		{
			velocity.X = 0;
		}
		
		Velocity = velocity;
	}

	private Vector2[] GetOffsetPolygon()
	{
		Vector2 size = anims.SpriteFrames.GetFrameTexture(anims.Animation, anims.Frame).GetSize();
		Vector2[] offsetPolygon =
		[
			(new Vector2(-size.X / 2, size.Y /2) * anims.Scale) + GlobalPosition,
			(new Vector2(size.X / 2, size.Y /2) * anims.Scale) + GlobalPosition,
			(new Vector2(size.X / 2, -size.Y /2) * anims.Scale) + GlobalPosition,
			(new Vector2(-size.X / 2, -size.Y /2) * anims.Scale) + GlobalPosition,
		];
		
		return offsetPolygon;
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
	
	public void RandomizeState()
	{
		string state = RollForRandomState();
		timer.WaitTime = rand.Next(7) + 3;
		int dirCount = GetDirectionCount(state);

		if(dirCount == 1)
		{
			anims.Play(state);

			// Special behavior to maintain starting direction
			if(state == "Rotate")
			{
				anims.Frame = (int)dir;
			}
		}
		else if(dirCount == 2)
		{
			if (rand.Next(2) == 0)
			{
				dir = Direction.E;
				anims.Play(state + 'E');
			}
			else
			{
				dir = Direction.W;
				anims.Play(state + 'W');
			}
		}
		else if (dirCount == 8)
		{
			// Only change direction if anim is not a one off (i.e. loops)
			if(anims.SpriteFrames.GetAnimationLoop(state + 'S'))
			{
				dir = (Direction)rand.Next(8);
			}
			anims.Play(state + dir.ToString());
		}
	}

	private int GetDirectionCount(string state)
	{
		if(anims.SpriteFrames.HasAnimation(state))
		{
			return 1;
		}
		else if(!anims.SpriteFrames.HasAnimation(state + 'S') && anims.SpriteFrames.HasAnimation(state + 'E'))
		{
			return 2;
		}
		else if(anims.SpriteFrames.HasAnimation(state + 'S') && anims.SpriteFrames.HasAnimation(state + 'E'))
		{
			return 8;
		}
		else
		{	// Anim does not exist
			return -1;
		}
	}

	public string RollForRandomState()
	{
		int num = rand.Next(weightTotal);
		foreach (string stateKey in Weights.Keys.ToList())
		{
			if (num < Weights[stateKey])
			{
				return stateKey;
			}
			else
			{
				num -= Weights[stateKey];
			}
		}
		return "";
	}

	public void OnDragStarted()
	{
		// Stop any current animations and timer when dragging starts
		timer.Stop();
		anims.Stop();
	}

	public void OnDragStopped()
	{
		// Resume normal behavior when dragging stops
		timer.Start();
		RandomizeState();
	}

	public void OnThrown(Vector2 throwForce)
	{
		// Handle throw behavior - you might want to play a special animation
		string animationString = "Charge";
		if(Velocity.X > 0){
			animationString += "W";
		}
		else{
			animationString += "E";
		}
		anims.Play(animationString);
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
