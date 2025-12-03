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

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);
	private const int VK_LBUTTON = 0x01;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    public static extern IntPtr SetCursor(IntPtr hCursor);

	[DllImport("user32.dll")]
    //[return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

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
	private const int IDC_ARROW = 32512;
	private const int IDC_SIZENWSE = 32642;
	private const int IDC_SIZENESW = 32643;
	private const int IDC_SIZEWE = 32644;
	private const int IDC_SIZENS = 32645;
	private const int MouseSize = 20;		// Right/Bottom Window border are offset by 20 pixels. MouseSize?

	private enum WindowSide
    {
        Left,
		Top,
		Right,
		Bottom,
		None
    }

	private IntPtr windowHandle;
	public Pet pet;
	private WindowSide resizingWindowSide = WindowSide.None;
	private int topBarwindowHeight = 25;
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
			pet = GetParent<Pet>();
			
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

		bool isInTopBar = mousePos.Y <= topBarwindowHeight;
		bool isMouseNearBorder = IsMouseNearBorder(mousePos);
		bool isNearPet = IsMouseNearPet(mousePos);
		bool isPetActive = pet.IsBeingDragged() || pet.IsBeingThrown();

		// Make window click-through EXCEPT when:
		// - Mouse is in top bar (for window moving)
		// - Mouse is near pet AND pet is not active (for starting drag)
		// - Pet is being dragged or thrown (so we can continue interaction)
		bool shouldBeClickThrough = !(isInTopBar || isMouseNearBorder || (isNearPet && !isPetActive) || isPetActive);

		if (shouldBeClickThrough != isClickThrough)
		{
			//GD.Print($"Setting click-through: {shouldBeClickThrough}");
			SetWindowClickThrough(shouldBeClickThrough);
			isClickThrough = shouldBeClickThrough;
		}
	}

	private bool IsMouseNearBorder(Vector2 mousePos)	
	{
        if (GetWindowRect(windowHandle, out RECT rect))
        {
            int windowWidth = rect.Right - rect.Left;
            int windowHeight = rect.Bottom - rect.Top;

            // Upper/Lower Bounds
            if (mousePos.Y > -10 && mousePos.Y < windowHeight + 10)
            {
                if (Math.Abs(mousePos.X) < 10 || resizingWindowSide == WindowSide.Left)
                {
                    HandleWindowResize(WindowSide.Left, mousePos, rect);
                    return true;
                }
                else if (Math.Abs(windowWidth - mousePos.X - MouseSize) < 10 || resizingWindowSide == WindowSide.Right)
                {
                    HandleWindowResize(WindowSide.Right, mousePos, rect);
                    return true;
                }
            }
			// Left/Right Bounds
			if(mousePos.X > -10 && mousePos.X < windowWidth + 10)
            {
				if ((mousePos.Y > -topBarwindowHeight - 20 && mousePos.Y < -topBarwindowHeight - 10) || 
					resizingWindowSide == WindowSide.Top)
                {
                    HandleWindowResize(WindowSide.Top, mousePos, rect);
                    return true;
                }
				if (Math.Abs(windowHeight - mousePos.Y - topBarwindowHeight - MouseSize) < 10 || 
					resizingWindowSide == WindowSide.Bottom)
                {
                    HandleWindowResize(WindowSide.Bottom, mousePos, rect);
                    return true;
                }
            }
        }

        SetCursor(LoadCursor(IntPtr.Zero, IDC_ARROW));
		return false;
	}

	private void HandleWindowResize(WindowSide side, Vector2 mousePos, RECT rect)
    {
		if(side == WindowSide.Left || side == WindowSide.Right)
        {
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZEWE));
        }
		else if(side == WindowSide.Top || side == WindowSide.Bottom)
        {
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZENS));
        }

		if (IsMousePressed())
		{
			int windowWidth = rect.Right - rect.Left;
            int windowHeight = rect.Bottom - rect.Top;

			if(side == WindowSide.Left)
            {
				resizingWindowSide = WindowSide.Left;
                SetWindowPos(windowHandle, HWND_TOPMOST, rect.Left + (int)mousePos.X, rect.Top,
					windowWidth - (int)mousePos.X, windowHeight, SWP_NOACTIVATE);
            }
			else if (side == WindowSide.Right)
            {
				resizingWindowSide = WindowSide.Right;
                SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0,
                    (int)mousePos.X + MouseSize, windowHeight, SWP_NOMOVE | SWP_NOACTIVATE);
            }
			else if(side == WindowSide.Top)
            {
				resizingWindowSide = WindowSide.Top;
                SetWindowPos(windowHandle, HWND_TOPMOST, rect.Left, rect.Top + (int)mousePos.Y + topBarwindowHeight + 15,
                    windowWidth, windowHeight - (int)mousePos.Y - topBarwindowHeight - 15, SWP_NOACTIVATE);
            }
			else if(side == WindowSide.Bottom)
            {
				resizingWindowSide = WindowSide.Bottom;
                SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0,
                    windowWidth, (int)mousePos.Y + topBarwindowHeight + MouseSize , SWP_NOMOVE | SWP_NOACTIVATE);
            }
		}
		else
        {
            resizingWindowSide = WindowSide.None;
        }
    }

	private static bool IsMousePressed() {
		return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
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
