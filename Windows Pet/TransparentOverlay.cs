using System;
using System.Runtime.InteropServices;
using Godot;

public partial class TransparentOverlay : Node
{
	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	private const int GWL_EXSTYLE = -20;
	private const int WS_EX_LAYERED = 0x80000;
	private const int WS_EX_TRANSPARENT = 0x20;
	private const uint LWA_ALPHA = 0x00000002;
	private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOACTIVATE = 0x0010;
	private const uint SWP_NOZORDER = 0x0004;
	private const uint SWP_FRAMECHANGED = 0x0020;

	private IntPtr windowHandle;
	public WindowsPet pet;
	private int topBarHeight = 25;
	private bool isClickThrough = false;
	private bool isWindows;

	public override void _Ready()
	{
		if (OS.GetName() != "Windows")
		{
			GD.Print("Skipping Windows-specific setup (not running on Windows).");
			isWindows = false;
			return;
		}
		else
		{
			isWindows = true;
		}

		// Get the window handle after the window is created
		Callable.From(() => InitializeWindow()).CallDeferred();
	}

	private void InitializeWindow()
	{
		try
		{
			windowHandle = GetActiveWindow();
			
			// Find the pet
			pet = GetNode<WindowsPet>("/root/Node2D/Pet"); // Adjust path as needed
			
			// Set up layered window with transparency
			SetupLayeredWindow();
			
			// Make sure window is topmost
			SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
			
			//GD.Print("Transparent overlay initialized successfully");
		}
		catch (Exception e)
		{
			GD.PrintErr("Initialization failed: " + e.Message);
		}
	}

	public override void _Process(double delta)
	{
		if (!isWindows)
		{
			GD.Print("Not on windows");
			return;
		}
		
		if (windowHandle == IntPtr.Zero) return;
		
		UpdateClickThrough();
	}

	private void SetupLayeredWindow()
	{
		try
		{
			// Get current style and add layered attribute
			int currentStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
			int newStyle = currentStyle | WS_EX_LAYERED;
			
			SetWindowLong(windowHandle, GWL_EXSTYLE, newStyle);
			SetLayeredWindowAttributes(windowHandle, 0, 255, LWA_ALPHA);
			
			//GD.Print($"Window style updated: 0x{newStyle:X8}");
		}
		catch (Exception e)
		{
			GD.PrintErr("Window setup failed: " + e.Message);
		}
	}

	private void UpdateClickThrough()
	{
		if (pet == null) return;

		var mousePos = GetGlobalMousePosition();
		
		// Check if mouse is in top bar area
		bool isInTopBar = mousePos.Y <= topBarHeight;
		
		// Check if mouse is near pet
		bool isNearPet = IsMouseNearPet(mousePos);
		
		// Check if pet is being dragged or thrown
		bool isPetActive = pet.IsBeingDragged() || pet.IsBeingThrown();
		
		// Make window click-through EXCEPT when:
		// - Mouse is in top bar (for window moving)
		// - Mouse is near pet AND pet is not active (for starting drag)
		// - Pet is being dragged or thrown (so we can continue interaction)
		bool shouldBeClickThrough = !(isInTopBar || (isNearPet && !isPetActive) || isPetActive);

		if (shouldBeClickThrough != isClickThrough)
		{
			//GD.Print($"Setting click-through: {shouldBeClickThrough}");
			SetWindowClickThrough(shouldBeClickThrough);
			isClickThrough = shouldBeClickThrough;
		}
	}

	private bool IsMouseNearPet(Vector2 mousePos)
	{
		if (pet == null) return false;
		float distance = mousePos.DistanceTo(pet.GlobalPosition);
		return distance <= 50f; // Increased radius for better interaction
	}

	private void SetWindowClickThrough(bool clickThrough)
	{
		try
		{
			int currentStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
			int newStyle;
			
			if (clickThrough)
			{
				newStyle = currentStyle | WS_EX_TRANSPARENT;
				//GD.Print("Enabling click-through (WS_EX_TRANSPARENT)");
			}
			else
			{
				newStyle = currentStyle & ~WS_EX_TRANSPARENT;
				//GD.Print("Disabling click-through");
			}
			
			SetWindowLong(windowHandle, GWL_EXSTYLE, newStyle);
			
			// Force window update
			SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
		}
		catch (Exception e)
		{
			GD.PrintErr("Failed to set click-through: " + e.Message);
		}
	}

	private Vector2 GetGlobalMousePosition()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			return viewport.GetMousePosition();
		}
		return Vector2.Zero;
	}

	public override void _ExitTree()
	{
		if (!isWindows || windowHandle == IntPtr.Zero) return;
		
		try
		{
			// Restore normal window behavior
			int currentStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
			int newStyle = currentStyle & ~WS_EX_TRANSPARENT & ~WS_EX_LAYERED;
			SetWindowLong(windowHandle, GWL_EXSTYLE, newStyle);
			
			//GD.Print("Window restored to normal state");
		}
		catch (Exception e)
		{
			GD.PrintErr("Error cleaning up: " + e.Message);
		}
	}
}
