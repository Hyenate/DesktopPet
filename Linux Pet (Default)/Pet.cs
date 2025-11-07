using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pet : CharacterBody2D
{
	private enum State
	{
		Idle,
		Walk,
		Sleep,
		Spin,
		Hop,
	}

	private enum Direction
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

	private Dictionary<State, int> Weights = new Dictionary<State, int>
	{
		{State.Idle, 20},
		{State.Walk, 40},
		{State.Sleep, 20},
		{State.Spin, 5 },
		{State.Hop, 15}
	};

	private int stateCount = Enum.GetNames(typeof(State)).Length;
	private int weightTotal = 0;
	private Direction dir = Direction.S;
	private const float Speed = 300.0f;
	private bool finishedSpawning = false;
	private AnimatedSprite2D anims;
	private Timer timer;
	private Random rand;

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
	}
	
	private void FinishSpawnAnim()
	{
		finishedSpawning = true;
		timer.Start();
		GetNode<Timer>("SpawnFloatTime").QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		if (!IsOnFloor() && finishedSpawning)
		{
			velocity += GetGravity() * (float)delta;
		}

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
		
		Velocity = velocity;
		MoveAndSlide();
	}

	public void RandomizeState()
	{
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
			anims.Play("Spin");
		}
		else if (state == State.Hop)
		{
			anims.Play("Hop" + dir.ToString());
		}
	}

	private State RollForRandomState()
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
		// If error, choose default
		return State.Idle;
	}

	public void OnAnimationLoopEnd()
	{
		//force Hop to end and reroll
		if(anims.Animation.ToString().Contains("Hop"))
		{
			RandomizeState();
		}    
	}
}
