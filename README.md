# Desktop Pet (Godot)
Godot implementation of a customizable Desktop Pet. Compatible with Windows and Linux (X11)

<img alt="Desktop Preview" src="Preview\DesktopPreview.png" />

Creates a transparent window that allows for a buddy to stick around while other work (hopefully) gets done.

## Capabilities
- Easily Import custom pets with sprite sheets (See [Dynamic Sprite Loading](#dynamic-sprite-loading)).
- Customizable weighted behavior.
- Imported animation adjustment.
- Pets can be picked up and tossed across the screen (they don't mind).
- Resizable/Repositional window overlay (Windows OS only).

# Dynamic Sprite Loading
Allows for a pet to be easily imported by either dragging and dropping onto the program (or through the "Add Pet" dialog).

For importing a pet, it must be a folder that contains the following:
- AnimData.xml
- At least one sprite sheet (Each sheet should be named as "{AnimationName}-Anim.png", i.e. "Idle-Anim.png")

For further reference, see the provided "ExamplePet" folder.

### AnimData.xml
To ensure that each sprite sheet is imported properly, instructions must be provided in the form of AnimData.xml.

AnimData.xml structure example:
```xml
<Anims>
    <Anim>
        <Name>Walk</Name>
        <FrameWidth>48</FrameWidth>
        <FrameHeight>32</FrameHeight>
        <Durations>
            <Duration>10</Duration>
            <Duration>10</Duration>
            <Duration>10</Duration>
            <Duration>10</Duration>
        </Durations>
    </Anim>
    <Anim>
        <Name>Sleep</Name>
        <FrameWidth>32</FrameWidth>
        <FrameHeight>32</FrameHeight>
        <Durations>
            <Duration>30</Duration>
            <Duration>35</Duration>
        </Durations>
    </Anim>
</Anims>
```
- **Name:** Animation Name.
- **FrameWidth:** pixel width of each frame in this sprite sheet.
- **FrameHeight:** pixel height of each frame in this sprite sheet.
- **Duration:** How many frames this frame should hold before switching to the next (relative to fps. If unsure, use "1" for each entry). There must be at least as many duration entries as there are frames in the animation.

### Sprite Sheets
For each "Name" entry in AnimData.xml, there should be a corresponding sprite sheet. Each sheet should be named as "{AnimationName}-Anim.png", i.e. "Walk-Anim.png". This means that each animation should have a dedicated sprite sheet.

Sprite sheets should be layed out horizontally, with rows being dedicated to directional variations of each animation. Each sprite sheet can have 1, 2, or 8 rows (variations) of each animation. This is to allow for pets to face different directions when an animation is rolled, i.e. making it possible for a pet to sleep facing any cardinal/ordinal direction.

## Compatibility
This project was originally designed to be compatible with sprites from the [PMD Sprite Repository](https://sprites.pmdcollab.org/). As such, certain functions have been modified to accommodate these sprites.

> **Disclaimer!** This project is not associated with PMD Sprite Repository, Pokemon, or related content.

Accomodations include:
- Some animation speeds are automatically modified upon import. These can still be modified in the pet editor.
- Some animations are set to not loop upon finishing. These can still be modified in the pet editor.
- Animations named "Head" are blacklisted due to incompatibility with vertical sprite sheets and unlikelihood to be used.

Additionally, for the best experience it is recommended to provide an "Idle" and "Hop" animation. The "Idle" animation is favored when generating pet previews, and the "Hop" animation is favored when a pet first spawns. 