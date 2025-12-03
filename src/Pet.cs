using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pet : CharacterBody2D
{
	public enum State
	{
		Idle,
		Walk,
		Sleep,
		Spin,
		Hop,
	}

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

	public Dictionary<State, int> Weights = new Dictionary<State, int>
	{
		{State.Idle, 20},
		{State.Walk, 40},
		{State.Sleep, 20},
		{State.Spin, 5 },
		{State.Hop, 15}
	};

	public bool UsingOverlay {get; set;}

	private int weightTotal = 0;
	private Direction dir = Direction.S;
	private const float Speed = 300.0f;
	private AnimatedSprite2D anims;
	private Timer timer;
	private Random rand;
	private bool initialized;
	private Polygon2D polygon2D;
	private ThrowableBehavior throwableBehavior;

	private const float Gravity = 980f;
	private const float TerminalVelocity = 2000f;

    public override void _Ready()
    {
        initialized = false;
    }

	public void InitializePet(AnimatedSprite2D petSprites)
    {
        foreach (var weight in Weights.Values.ToList())
		{
			weightTotal += weight;
		}
		petSprites.AnimationLooped += OnAnimationLoopEnd;
		AddChild(petSprites);
		anims = petSprites;
		anims.Play("HopS");
		timer = GetNode<Timer>("Timer");
		timer.Start();
		rand = new Random();
		Position = GetViewportRect().Size / 2;
		Velocity = new Vector2(0,-400);		// Initial Upwards Velocity
		polygon2D = GetNode<Polygon2D>("ThrowableBehavior/Polygon2D");
		GetWindow().MousePassthrough = false;

		throwableBehavior = GetNode<ThrowableBehavior>("ThrowableBehavior");
		throwableBehavior.OnDragStarted += OnDragStarted;
		throwableBehavior.OnDragStopped += OnDragStopped;
		throwableBehavior.OnThrown += OnThrown;

		initialized = true;
    }

	public override void _PhysicsProcess(double delta)
	{
		if(initialized)
        {
          	// Apply gravity regardless of throwable state
			Velocity = ApplyGravity(Velocity, delta);
			if(!UsingOverlay)
            {
            	GetWindow().MousePassthroughPolygon = GetOffsetPolygon();
            }
			// Only handle normal physics if ThrowableBehavior allows it
			if (throwableBehavior.ShouldParentHandlePhysics())
			{
				ApplyNormalPhysics(delta);
			}	
			MoveAndSlide();  
        }
	}

	private Vector2 ApplyGravity(Vector2 velocity, double delta)
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
		Vector2[] offsetPolygon = new Vector2[4];
		for(int i = 0; i < 4; i++)
		{
			offsetPolygon[i] = polygon2D.Polygon[i] + polygon2D.GlobalPosition;
		}
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

	public virtual bool KeepCurrentState()
    {
        return false;
    }
	
	public void RandomizeState()
	{
		if(KeepCurrentState())
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
			anims.Play("Spin" + dir.ToString());
		}
		else if (state == State.Hop)
		{
			anims.Play("Hop" + dir.ToString());
		}
	}

	public State RollForRandomState()
	{
		int num = rand.Next(weightTotal);
		foreach (var stateKey in Weights.Keys.ToList())
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
		return State.Idle;
	}

	public virtual void OnAnimationLoopEnd()
	{
		if(KeepCurrentState())
			return;
		
		//force Hop to end and reroll
		if(anims.Animation.ToString().Contains("Hop"))
		{
			RandomizeState();
		}    
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
