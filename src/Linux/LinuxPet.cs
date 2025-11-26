using Godot;

public partial class LinuxPet : Pet
{
	private Polygon2D polygon2D;
	public LinuxThrowableBehavior throwableBehavior;

	public override void InitializeOSSpecificBehavior()
	{
		GetWindow().MousePassthrough = false;
		throwableBehavior = GetNode<LinuxThrowableBehavior>("ThrowableBehavior");
		throwableBehavior.OnDragStarted += OnDragStarted;
		throwableBehavior.OnDragStopped += OnDragStopped;
		throwableBehavior.OnThrown += OnThrown;
		polygon2D = GetNode<Polygon2D>("ThrowableBehavior/Polygon2D");
	}

	public override void RunOSSpecificBehavior(double delta)
	{
		GetWindow().MousePassthroughPolygon = GetOffsetPolygon();
		// Only handle normal physics if ThrowableBehavior allows it
		if (throwableBehavior.ShouldParentHandlePhysics())
		{
			ApplyNormalPhysics(delta);
		}
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

}
