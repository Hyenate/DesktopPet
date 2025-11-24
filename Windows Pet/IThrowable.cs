using Godot;

public interface IThrowable
{
	// Properties
	bool IsBeingDragged { get; }
	bool IsBeingThrown { get; }
	float DragRadius { get; set; }
	
	// Methods
	void StartDragging(Vector2 dragOffset);
	void StopDragging();
	void Throw(Vector2 throwForce);
	
	// Events 
	event System.Action OnDragStarted;
	event System.Action OnDragStopped;
	event System.Action<Vector2> OnThrown;
}
