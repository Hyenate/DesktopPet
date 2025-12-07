using Godot;
using System.Xml.Linq;
using System.Linq;
using CSharpDictionary = System.Collections.Generic.Dictionary<string, AnimationRegistry.AnimationInfo>;


/// Class for storing animation information from AnimData.xml
public partial class AnimationRegistry : Node
{
	// Define a struct for animation metadata
	public class AnimationInfo
	{
		public string SheetName { get; set; }     // This must match a png file in the res://sprite/ folder
		public string InternalName { get; set; }  // This name must match the name in the AnimData.xml
		public Vector2I FrameSize { get; set; }   // This will be loaded automatically
		public int[] FrameDurations { get; set; } // How long to hold each frame (1 = 1/60th of a second)

		public AnimationInfo(string sheetName, string internalName, Vector2I frameSize, int[] frameDurations)
		{
			SheetName = sheetName;
			InternalName = internalName;
			FrameSize = frameSize;
			FrameDurations = new int[frameDurations.Length];
			System.Array.Copy(frameDurations, FrameDurations, frameDurations.Length);
		}
	}

	// Dictionary maps internal names to info
	public CSharpDictionary Animations { get; private set; } = new();
	string animDataPath = "";
	XDocument doc;

	public void Init(string spriteFolder)
	{
		animDataPath = spriteFolder + "AnimData.xml";
		var file = FileAccess.Open(animDataPath, FileAccess.ModeFlags.Read);
		string xmlText = file.GetAsText();
		doc = XDocument.Parse(xmlText);
		
		if(doc == null)
		{
			 GD.PrintErr($"Unable to load animation data from {animDataPath}");
		}
		
		// Populate the Registry
		AddAnimation("Attack-Anim", "Attack", PullFrameSize("Attack"), PullFrameDurations("Attack"));
		AddAnimation("Charge-Anim", "Charge", PullFrameSize("Charge"), PullFrameDurations("Charge"));
		AddAnimation("Hop-Anim",    "Hop",    PullFrameSize("Hop"), PullFrameDurations("Hop"));
		AddAnimation("Hurt-Anim",   "Hurt",   PullFrameSize("Hurt"), PullFrameDurations("Hurt"));
		AddAnimation("Idle-Anim",   "Idle",   PullFrameSize("Idle"), PullFrameDurations("Idle"));
		AddAnimation("Rotate-Anim", "Spin",   PullFrameSize("Rotate"), PullFrameDurations("Rotate"));
		AddAnimation("Sleep-Anim",  "Sleep",  PullFrameSize("Sleep"), PullFrameDurations("Sleep"));
		AddAnimation("Swing-Anim",  "Swing",  PullFrameSize("Swing"), PullFrameDurations("Swing"));
		AddAnimation("Walk-Anim",   "Walk",   PullFrameSize("Walk"), PullFrameDurations("Walk"));
	}
	
	
	public Vector2I PullFrameSize(string animName)
	{
		XElement animElem = doc.Descendants("Anim").FirstOrDefault(a => (string)a.Element("Name") == animName);
		
		if (animElem == null)
		{
			GD.PrintErr($"Animation '{animName}' not found in {animDataPath}");
			return new Vector2I(0, 0);
		}
		return new Vector2I((int)animElem.Element("FrameWidth"), (int)animElem.Element("FrameHeight"));
	}

	public int[] PullFrameDurations(string animName)
	{
		XElement animElem = doc.Descendants("Anim").FirstOrDefault(a => (string)a.Element("Name") == animName);
		
		if (animElem == null)
		{
			GD.PrintErr($"Animation '{animName}' not found in {animDataPath}");
			return null;
		}

		return animElem.Descendants("Durations").Elements("Duration").Select(element => (int)element).ToArray();
	}

	public void AddAnimation(string sheetName, string internalName, Vector2I frameSize, int[] frameDurations)
	{
		Animations[internalName] = new AnimationInfo(sheetName, internalName, frameSize, frameDurations);
	}
}
