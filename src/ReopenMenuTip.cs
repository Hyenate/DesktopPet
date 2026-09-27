using Godot;
using System;

public partial class ReopenMenuTip : AnimationPlayer
{
	public void SetNotificationFinished()
	{
		GetNode<Pet>("../Pet").NotificationFinished = true;
	}
}
