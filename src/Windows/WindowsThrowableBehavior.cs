using Godot;
using System;
using System.Runtime.InteropServices;

public partial class WindowsThrowableBehavior : ThrowableBehavior
{
	// Windows API for mouse input
	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);
	private const int VK_LBUTTON = 0x01;

	public override bool IsMousePressed() {
		short keyState = GetAsyncKeyState(VK_LBUTTON);
		return (keyState & 0x8000) != 0;
	}
}
