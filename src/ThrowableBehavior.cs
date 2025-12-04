using Godot;
using System;

public partial class ThrowableBehavior : Node, IThrowable
{
	// Interface implementation
	public bool IsBeingDragged { get; private set; }
	public bool IsBeingThrown { get; private set; }
	public float DragRadius { get; set; } = 80f;
	
	// Events
	public event Action OnDragStarted;
	public event Action OnDragStopped;
	public event Action<Vector2> OnThrown;

	// Throwing physics
	private Vector2 throwVelocity = Vector2.Zero;
	private Vector2 dragOffset = Vector2.Zero;
	private bool wasMousePressed = false;
	private Vector2 lastMousePosition;
	
	// Physics parameters (configurable)
	public float ThrowDeceleration { get; set; } = 0.999f;
	public float BounceDampening { get; set; } = 0.6f;
	public float ThrowStrengthMultiplier { get; set; } = 17.5f;
	public float MinThrowMovement { get; set; } = 10f;
	
	// References
	private CharacterBody2D parentBody;
	private int bounceCount = 0;
	private bool shouldParentHandlePhysics = true;

	public override void _Ready()
	{
		parentBody = GetParent<CharacterBody2D>();
		if (parentBody == null)
		{
			GD.PrintErr("ThrowableBehavior must be a child of a CharacterBody2D!");
			return;
		}
		
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Process(double delta)
	{
		HandleMouseInput();
		lastMousePosition = GetGlobalMousePosition();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsBeingDragged)
		{
			HandleDragging();
			shouldParentHandlePhysics = false;
		}
		else if (IsBeingThrown)
		{
			HandleThrowing(delta);
			shouldParentHandlePhysics = true;
		}
		else
		{
			shouldParentHandlePhysics = true;
		}
	}

	// Public method for parent to check if it should handle physics
	public bool ShouldParentHandlePhysics()
	{
		return shouldParentHandlePhysics;
	}

	public bool IsMousePressed() {
		return Input.IsActionPressed("lClick");
	}

	private void HandleMouseInput()
	{
		bool mousePressed = IsMousePressed();
		
		Vector2 mousePos = GetGlobalMousePosition();
		float distance = mousePos.DistanceTo(parentBody.GlobalPosition);
		
		// Check for mouse press
		if (mousePressed && !wasMousePressed)
		{
			if (distance <= DragRadius && !IsBeingDragged && !IsBeingThrown)
			{
				dragOffset = mousePos - parentBody.GlobalPosition;
				StartDragging(dragOffset);
			}
		}
		else if (!mousePressed && wasMousePressed)
		{
			if (IsBeingDragged)
			{
				Vector2 currentMousePos = GetGlobalMousePosition();
				Vector2 mouseMovement = currentMousePos - lastMousePosition;
				
				if (mouseMovement.Length() > MinThrowMovement)
				{
					Throw(mouseMovement * ThrowStrengthMultiplier);
				}
				else
				{
					StopDragging();
				}
			}
		}
		wasMousePressed = mousePressed;
	}

	private void HandleDragging()
	{
		Vector2 mousePos = GetGlobalMousePosition();
		parentBody.GlobalPosition = mousePos - dragOffset;
		parentBody.Velocity = Vector2.Zero;
		throwVelocity = Vector2.Zero;
		bounceCount = 0;
	}

	private void HandleThrowing(double delta)
	{
		// Apply deceleration to horizontal movement only
		throwVelocity.X *= ThrowDeceleration;
		throwVelocity.Y += 10;
		
		// Set velocity and handle movement
		parentBody.Velocity = throwVelocity;
		parentBody.MoveAndSlide();
		
		// Handle collisions after movement
		HandleCollisions();
		CheckIfStopped();
	}

	private void HandleCollisions()
	{
		// Handle wall collisions for bouncing
		if (parentBody.IsOnWall())
		{
			throwVelocity.X *= -BounceDampening;
			bounceCount++;
			//GD.Print($"Bounced off wall! Bounce count: {bounceCount}, New velocity: {throwVelocity}");
			
			// Stop if bouncing too much or velocity is very low
			if (bounceCount > 3 || Mathf.Abs(throwVelocity.X) < 20f)
			{
				throwVelocity.X = 0;
			}
		}
		
		// Handle floor collisions
		if (parentBody.IsOnFloor() && throwVelocity.Y > 0)
		{
			// Reduce vertical velocity but don't reverse it
			throwVelocity.Y *= BounceDampening;
			bounceCount++;
			//GD.Print($"Landed on floor! Bounce count: {bounceCount}, New velocity: {throwVelocity}");
			
			// Stop throwing if velocity is very low
			if (throwVelocity.Length() < 30f || bounceCount > 3)
			{
				throwVelocity = Vector2.Zero;
			}
		}
		
		// Handle ceiling collisions
		if (parentBody.IsOnCeiling() && throwVelocity.Y < 0)
		{
			throwVelocity.Y *= -BounceDampening;
			bounceCount++;
			//GD.Print($"Hit ceiling! Bounce count: {bounceCount}, New velocity: {throwVelocity}");
		}
	}

	private void CheckIfStopped()
	{
		if (IsBeingThrown && throwVelocity.Length() < 10f)
		{
			throwVelocity = Vector2.Zero;
			IsBeingThrown = false;
			shouldParentHandlePhysics = true;
			OnDragStopped?.Invoke();
			//GD.Print("Throwing stopped - returning control to parent");
		}
	}

	private Vector2 GetGlobalMousePosition()
	{
		var viewport = GetViewport();
		return viewport?.GetMousePosition() ?? Vector2.Zero;
	}

	// Interface implementation
	public void StartDragging(Vector2 offset)
	{
		if (IsBeingDragged) return;
		
		IsBeingDragged = true;
		dragOffset = offset;
		throwVelocity = Vector2.Zero;
		bounceCount = 0;
		shouldParentHandlePhysics = false;
		
		OnDragStarted?.Invoke();
		GD.Print("Started dragging");
	}

	public void StopDragging()
	{
		if (!IsBeingDragged) return;
		
		IsBeingDragged = false;
		shouldParentHandlePhysics = true;
		OnDragStopped?.Invoke();
		GD.Print("Stopped dragging");
	}

	public void Throw(Vector2 throwForce)
	{
		if (!IsBeingDragged) return;
		
		IsBeingDragged = false;
		IsBeingThrown = true;
		throwVelocity = throwForce;
		bounceCount = 0;
		//shouldParentHandlePhysics = false;
		
		OnThrown?.Invoke(throwForce);
		OnDragStopped?.Invoke();
		GD.Print($"Thrown with force: {throwForce}");
	}
}
