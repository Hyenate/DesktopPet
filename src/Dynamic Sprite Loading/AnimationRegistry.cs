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
		public int HasDirections { get; set; }    // Functionally a bool, but having a directionless sprite set direction = 0 is useful in loading sprites
		public Vector2I FrameSize { get; set; }   // This will be loaded automatically

		public AnimationInfo(string sheetName, string internalName, int hasDirections, Vector2I frameSize)
		{
			SheetName = sheetName;
			InternalName = internalName;
			HasDirections = hasDirections;
			FrameSize = frameSize;
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
		AddAnimation("Attack-Anim", "Attack", 1, pullFrameSize("Attack"));
		AddAnimation("Charge-Anim", "Charge", 1, pullFrameSize("Charge"));
		AddAnimation("Hop-Anim",    "Hop",    1, pullFrameSize("Hop"));
		AddAnimation("Hurt-Anim",   "Hurt",   1, pullFrameSize("Hurt"));
		AddAnimation("Idle-Anim",   "Idle",   1, pullFrameSize("Idle"));
		AddAnimation("Rotate-Anim", "Spin",   1, pullFrameSize("Rotate"));
		AddAnimation("Sleep-Anim",  "Sleep",  0, pullFrameSize("Sleep"));
		AddAnimation("Swing-Anim",  "Swing",  1, pullFrameSize("Swing"));
		AddAnimation("Walk-Anim",   "Walk",   1, pullFrameSize("Walk"));
	}
	
	
	public Vector2I pullFrameSize(string animName)
	{
		XElement animElem = doc.Descendants("Anim").FirstOrDefault(a => (string)a.Element("Name") == animName);
		
		if (animElem == null)
		{
			GD.PrintErr($"Animation '{animName}' not found in {animDataPath}");
			return new Vector2I(0, 0);
		}
		return new Vector2I((int)animElem.Element("FrameWidth"), (int)animElem.Element("FrameHeight"));
	}

	public void AddAnimation(string sheetName, string internalName, int hasDirections, Vector2I frameSize)
	{
		Animations[internalName] = new AnimationInfo(sheetName, internalName, hasDirections, frameSize);
	}
}
