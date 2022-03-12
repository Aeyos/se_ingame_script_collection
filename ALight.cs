using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
// ---------------------------------- //
// BASIC CONFIGURATION   //
// ---------------------------------  //

// Speed of color change, default: 1.0f, Higher is faster, lower is slower
public float speed = 1.0f;
// How often should colors be updated, min: 1
public float updateTime = 3;
// Group name to look for
public string groupName = "RGB Lights";
// Group name for gradient
// You can name groups like:
// RGB Gradient Lights
// RGB Gradient Lights 0
// RGB Gradient Lights 1
// RGB Gradient Lights 2
// RGB Gradient Lights 3
// RGB Gradient Lights 4
// RGB Gradient Lights ABC
// RGB Gradient Lights DEF
public string groupNameGradient = "RGB Gradient Lights";

// ---------------------------------------- //
// ADVANCED CONFIGURATION  //
// ---------------------------------------  //

// Colors to switch between
// ADD double slash to deactivate
// REMOVE double slash to activate another profile

public Color[] colors = new Color[] {

// PROFILE 1 - RGB (Hue-chroma-luminance corrected)
new Color(255, 0, 0), new Color(255, 0, 58), new Color(255, 0, 103), new Color(235, 0, 155), new Color(179, 0, 208), new Color(0, 0, 255), new Color(0, 121, 255), new Color(0, 168, 255), new Color(0, 202, 249), new Color(0, 233, 146), new Color(0, 255, 0), new Color(145, 223, 0), new Color(196, 186, 0), new Color(229, 145, 0), new Color(249, 96, 0),
            
// PROFILE 2 - Strobe white
// Color.White, Color.Black,
            
// PROFILE 3 - Glow red
// Color.Red, Color.Black,
            
// PROFILE 4 - PoPo
// Color.Red, Color.Blue,

// PROFILE 5 - Simple RGB
// Color.Red, Color.Blue, Color.Green,

// PROFILE 6 - Programmers choice
// Color.Cyan, Color.Magenta

};





// -------------------------------------------------------- //
// DO NOT CHANGE CODE BELOW THIS LINE //
// -------------------------------------------------------- //
// UNLESS you know what you're doing         //
// -------------------------------------------------------- //
// otherwise, KEEP OUT!                                 //
// -------------------------------------------------------- //

public float progress = 0;
public int currentFrame = 0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string argument, UpdateType updateType)
{
    // Current frame is past limit
    if (currentFrame++ > updateTime)
    {
        // Reset it
        currentFrame = 0;
    }
    // Current frame is not 0
    if (currentFrame != 0)
    {
        // Skip it
        return;
    }
    // Get grid
    IMyGridTerminalSystem grid = this.GridTerminalSystem;
    // Get programmable block
    IMyProgrammableBlock me = this.Me;

    // Output info
    Echo($"RGB Lighting Script");
    Echo($"Update every: {updateTime} frames");
    Echo($"Light speed: {speed}");
    Echo($"Light group name: {groupName}");

    // Initialize light block list
    var lights = new List<IMyLightingBlock>();
    // Get blocks in group name
    var blockGroup = grid.GetBlockGroupWithName(groupName);
    // Check if there is group
    if (blockGroup == null)
    {
        Echo($"No blocks found in group {groupName}");
        return;
    }
    // Get blocks of type in list
    blockGroup.GetBlocksOfType<IMyLightingBlock>(lights);

    // Output more info
    Echo($"Blocks detected: {lights.Count}");

    // Check progress through rgb
    progress += (0.002f * speed * updateTime);
    progress %= colors.Length;
    // Calculate current colors using linear interpolation between two colors and the fraction of the progress while indexing colors with the integral part
    Color color = VRageMath.Color.Lerp(colors[MathHelper.Floor(progress)], colors[MathHelper.CeilToInt(progress) % colors.Length], progress - MathHelper.Floor(progress));
    Echo($"Current base color: RGB ({color.R}, {color.G}, {color.B})");
    // For all the lights
    foreach (var light in lights)
    {
        // Update name
        // light.CustomName = "My RGB!"; // not anymore you don't
        // Simply set the color
        light.Color = color;
    }

    // Allocate resourse
    var blockGroups = new List<IMyBlockGroup>();
    // Get blocks groups
    grid.GetBlockGroups(blockGroups);
    // Get block groups names for gradient lights (starts with name)
    blockGroups = blockGroups.FindAll(x => x.Name.StartsWith(groupNameGradient));
    // For each group
    Echo("Gradient Lights:");
    foreach (var blkGroup in blockGroups)
    {
        // Get lights from group
        blkGroup.GetBlocksOfType<IMyLightingBlock>(lights);
        // If there are lights
        Echo($"    {blkGroup.Name} ({lights.Count} blocks)");
        if (lights.Count > 0)
        {
            // Set gradient offset to zero
            float gradientOffset = 0f;
            // Calculate offset adder (depends on number of blocks and number of colors)
            float gradientOffsetAdder = (float)colors.Length / (float)lights.Count;
            // For every light
            foreach (var light in lights)
            {
                // Calculate progress with offset
                float gradientProgress = (progress + gradientOffset) % colors.Length;
                // Get color according to custom progress
                color = VRageMath.Color.Lerp(colors[MathHelper.Floor(gradientProgress)], colors[MathHelper.CeilToInt(gradientProgress) % colors.Length], (float)gradientProgress - (float)MathHelper.Floor(gradientProgress));
                // Set color
                light.Color = color;
                // Update offset for next light
                gradientOffset += gradientOffsetAdder;
            }
        }
    }
}
    }
}