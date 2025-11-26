public partial class WindowsPet : Pet
{
	public WindowsThrowableBehavior throwableBehavior;

	public override void InitializeOSSpecificBehavior()
	{
		GetWindow().MousePassthrough = true;

		throwableBehavior = GetNode<WindowsThrowableBehavior>("ThrowableBehavior");
		throwableBehavior.OnDragStarted += OnDragStarted;
		throwableBehavior.OnDragStopped += OnDragStopped;
		throwableBehavior.OnThrown += OnThrown;
	}

	public override void RunOSSpecificBehavior(double delta)
	{
		// Only handle normal physics if ThrowableBehavior allows it
		if (throwableBehavior.ShouldParentHandlePhysics())
		{
			ApplyNormalPhysics(delta);
		}
	}

	public override bool KeepCurrentState()
	{
		return throwableBehavior.IsBeingThrown || throwableBehavior.IsBeingDragged;
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
