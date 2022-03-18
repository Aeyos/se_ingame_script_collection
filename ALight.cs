using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;


namespace ALight
{
    partial class Program : MyGridProgram
    {
        // ** START OF SE CODE ** //
        // Create config object
        AConfig config;
        // Create config options
        AConfig.AValue<string> c_GroupName = new AConfig.AValue<string>("Group Name", "RGB Lights");
        AConfig.AValue<float> c_Speed = new AConfig.AValue<float>("Speed", 1.0f);
        AConfig.AValue<Color[]> c_Colors = new AConfig.AValue<Color[]>("Colors", new Color[] { Color.Red, Color.Lime, Color.Blue });
        AConfig.AValue<int> c_UpdateEvery = new AConfig.AValue<int>("Update Every", 100);
        const String LIGHT_MODE_NORMAL = "Normal";
        const String LIGHT_MODE_GRADIENT = "Gradient";
        AConfig.AOptions c_LightMode = new AConfig.AOptions(
            "Light Mode",
            new List<AConfig.AOption> { new AConfig.AOption(LIGHT_MODE_NORMAL, true), new AConfig.AOption(LIGHT_MODE_GRADIENT, false) },
            singleSelect: true
        );
        const String FLOW_MODE_NORMAL = "First to last on group";
        const String FLOW_MODE_REVERSED = "Last to first on group";
        AConfig.AValue<float> c_GradientPatternRepetition = new AConfig.AValue<float>("Gradient repetition", 1f);
        AConfig.AOptions c_FlowMode = new AConfig.AOptions(
            "Sequence on group",
            new List<AConfig.AOption> { new AConfig.AOption(FLOW_MODE_NORMAL, true), new AConfig.AOption(FLOW_MODE_REVERSED, false) },
            singleSelect: true
        );

        // Create control variables
        public float progress = 0;
        public int currentFrame = 0;

        // SE "on compile" function
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            config = new AConfig(
                c_GroupName,
                c_Speed,
                c_Colors,
                c_UpdateEvery,
                c_FlowMode,
                c_LightMode,
                c_GradientPatternRepetition
            );
        }

        // SE "on run" function
        public void Main(string argument, UpdateType updateType)
        {
            // Current frame is past limit
            if (currentFrame++ > c_UpdateEvery.value)
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
            // Read config, parse, re-write to custom data
            config.Read(this.Me.CustomData);
            this.Me.CustomData = config.ToString();
            // Display config
            Echo(config.ToString());
            // Get grid
            IMyGridTerminalSystem grid = this.GridTerminalSystem;
            // Get programmable block
            IMyProgrammableBlock me = this.Me;
            // Init variables from config
            var groupName = c_GroupName.value;
            var speed = c_Speed.value;
            var updateTime = c_UpdateEvery.value;
            var colors = c_Colors.value;
            var gradientMode = c_LightMode.Selected(LIGHT_MODE_GRADIENT);
            var gradientMultiplier = c_GradientPatternRepetition.value;
            // Allocate resourse
            var blockGroups = new List<IMyBlockGroup>();
            // Get blocks groups
            grid.GetBlockGroups(blockGroups);
            // Get block groups names for gradient lights (starts with name)
            blockGroups = blockGroups.FindAll(x => x.Name.StartsWith(groupName));
            // Block groups is EMPTY
            if (blockGroups.Count == 0)
            {
                Echo($"No groups found with name \"{groupName}\"");
            }

            // Initialize light block list
            var lights = new List<IMyLightingBlock>();

            // Check if there is group
            Echo($"RGB Groups:");

            // Check progress through rgb
            progress += (0.002f * speed * updateTime);
            progress %= colors.Length;

            foreach (var blkGroup in blockGroups)
            {
                // Get lights from group
                blkGroup.GetBlocksOfType<IMyLightingBlock>(lights);
                // Output info
                Echo($"    {blkGroup.Name} ({lights.Count} blocks)");
                // Apply FLOW MODE configuration
                if (c_FlowMode.Selected(FLOW_MODE_REVERSED))
                {
                    lights.Reverse();
                }

                // If there are lights
                if (lights.Count > 0)
                {
                    // Set gradient offset to zero
                    float gradientOffset = 0f;
                    // Calculate offset adder (depends on number of blocks and number of colors)
                    float gradientOffsetAdder = (float)colors.Length / (float)lights.Count;
                    // Define color variable
                    Color color = Color.White;
                    // No gradient mode, assign same color for all
                    if (gradientMode == false)
                    {
                        // calculate it here once
                        color = VRageMath.Color.Lerp(colors[MathHelper.Floor(progress)], colors[MathHelper.CeilToInt(progress) % colors.Length], progress - MathHelper.Floor(progress));
                    }
                    // For every light
                    foreach (var light in lights)
                    {
                        // Is gradient mode
                        if (gradientMode)
                        {
                            // Calculate progress with offset
                            float gradientProgress = ((progress + gradientOffset) * gradientMultiplier) % colors.Length;
                            // Get color according to custom progress
                            color = VRageMath.Color.Lerp(colors[MathHelper.Floor(gradientProgress)], colors[MathHelper.CeilToInt(gradientProgress) % colors.Length], (float)gradientProgress - (float)MathHelper.Floor(gradientProgress));
                            // Set color
                            light.Color = color;
                            // Update offset for next light
                            gradientOffset += gradientOffsetAdder;
                        }
                        else
                        {
                            // Set color
                            light.Color = color;
                        }
                    }
                }
            }
        }
 
        // ** START OF AEYOS CONFIG HELPER ** //
        public class AConfig
        {
            public static Action<string> SEcho = (string _) => { };
            public abstract class AValue
            {
                public System.Type type;
                public string key;

                public abstract void UpdateValue(string v);
            }
            public class AValue<T> : AValue
            {
                public T value;

                public AValue(string key, T value)
                {
                    this.key = key;
                    this.value = value;
                    this.type = value.GetType();
                }

                public AValue(AValue<T> originalValue, string newValue)
                {
                    this.key = originalValue.key;
                    this.type = originalValue.type;
                    var parsedValue = (T)(object)AValue<T>.ParseValue(newValue, this.type, originalValue);
                    this.value = parsedValue != null ? parsedValue : originalValue.value;
                }

                private static string ValueToString(object v, Type type)
                {
                    if (type.IsEquivalentTo(typeof(Color[]))) return string.Join(",", (v as Color[]).Select(x => ValueToString(x, typeof(Color))));
                    if (type.IsEquivalentTo(typeof(int[]))) return string.Join(", ", v as int[]);
                    if (type.IsEquivalentTo(typeof(Color)))
                    {
                        Color color = (Color)v;
                        return $"<{color.R}, {color.G}, {color.B}>";
                    }
                    return v.ToString();
                }

                internal static object ParseValue(string value, Type type, AValue<T> originalValue)
                {
                    if (value == null) return null;
                    try
                    {
                        if (type.IsEquivalentTo(typeof(int))) return int.Parse(value);
                        else if (type.IsEquivalentTo(typeof(float))) return float.Parse(value);
                        else if (type.IsEquivalentTo(typeof(bool))) return bool.Parse(value);
                        else if (type.IsEquivalentTo(typeof(Color)))
                        {
                            int[] split = value.Substring(1, value.Length - 2).Split(',').Select(x => int.Parse(x)).ToArray();
                            return new Color(split[0], split[1], split[2]);
                        }
                        else if (type.IsEquivalentTo(typeof(Color[])))
                        {
                            if (value.Length < 2) return null;
                            int[] split = value.Substring(1, value.Length - 2).Replace("<", "").Replace(">", "").Split(',').Select(x => { int y; int.TryParse(x, out y); return y; }).ToArray();
                            if (split == null) return null;
                            Color[] colors = new Color[split.Length / 3];

                            for (var i = 0; i < colors.Length; i += 1)
                            {
                                colors[i] = new Color(split[i * 3], split[i * 3 + 1], split[i * 3 + 2]);
                            }
                            return colors;
                        }
                        else if (type.IsEquivalentTo(typeof(int[]))) return value.Split(',').Select(x => int.Parse(x.Trim()));
                        else if (type.IsEquivalentTo(typeof(string))) return value;
                    }
                    catch { }
                    return null;
                }
                override public string ToString()
                {
                    return $"{key}: {AValue<T>.ValueToString(value, type)}";
                }

                override public void UpdateValue(string v)
                {
                    var parsedValue = AValue<T>.ParseValue(v, this.type, this);
                    if (parsedValue != null) this.value = (T)(object)parsedValue;
                }
            }
            public class AOption
            {
                public string name;
                public bool value;
                public AOption(string name, bool check) { this.name = name; this.value = check; }
                public override string ToString() { return (value.Equals(null)) ? "" : $"     [{(value == true ? "X" : " ")}] {name}"; }
                public static AOption Parse(string v)
                {
                    var split = v.Trim().Substring(2, v.Length - 2).Split(']').Select(x => x.Trim()).ToArray();
                    bool value;
                    if (split.Length == 2 && bool.TryParse(split[1], out value))
                    {
                        return new AOption(split[0], value);
                    }
                    return new AOption(split[0], false);
                }
            }
            public class AOptions : AValue
            {
                public Dictionary<string, AOption> options = new Dictionary<string, AOption>();
                public bool singleSelect;

                public AOptions(string key, List<AOption> options, bool singleSelect = false)
                {
                    this.key = key;
                    this.singleSelect = singleSelect;
                    foreach (var option in options)
                    {
                        this.options.Add(option.name, option);
                    }
                }

                public void Add(AOption element)
                {
                    this.options.Add(element.name, element);
                }

                public bool Selected(string optionName)
                {
                    if (options.ContainsKey(optionName))
                    {
                        return options[optionName].value;
                    }
                    return false;
                }

                public override string ToString()
                {
                    return $"{key}:\n{string.Join("\n", options.Values.Select(x => x.ToString()))}";
                }

                override public void UpdateValue(string s)
                {
                    // Options read from custom data
                    var markedOptionsFromData = new HashSet<string>();
                    // Lines of each option in data
                    var optionDataSplit = s.Split('\n')?.Select(x => x.Trim().Split(']')?.Select(y => y.Trim()).ToArray()).Where(x => x.Length > 0).ToArray();
                    // If no lines found, skip this
                    if (optionDataSplit.Length == 0) return;
                    string firstTrueOption = null;
                    // For every line of option
                    foreach (var option in optionDataSplit)
                    {
                        // If no value/name found, skip
                        if (option == null || option.Length < 2) continue;
                        // If option was maked
                        if (option[0].ToUpper().Contains("X") && options.ContainsKey(option[1]))
                        {
                            // Set first marked option
                            if (firstTrueOption == null) firstTrueOption = option[1];
                            // Add field name to hashSet
                            markedOptionsFromData.Add(option[1]);
                        }
                    }
                    // If no option marked as true, skip
                    if (markedOptionsFromData.Count == 0) return;
                    // There is no option marked as true and singleSelect, skip
                    if (firstTrueOption == null && singleSelect) return;
                    // For every option in definition
                    foreach (var option in options.Values)
                    {
                        // Single select
                        if (singleSelect)
                        {
                            // Mark option as true if name is the same as the first marked option
                            option.value = option.name == firstTrueOption;
                        }
                        // Multi-select
                        else
                        {
                            // Mark as true if inside optionsFromData, mark as false if oterwise
                            option.value = markedOptionsFromData.Contains(option.name);
                        }
                    }
                }
            }
            public AConfig(params AValue[] values)
            {
                foreach (var o in values)
                {
                    this.serializableValues.Add(o.key, o);
                };
            }
            public Dictionary<string, AValue> serializableValues = new Dictionary<string, AValue>();
            override public string ToString()
            {
                var sb = new StringBuilder();
                foreach (var v in serializableValues)
                {
                    sb.AppendLine("\u2022 " + v.Value.ToString());
                }
                return sb.ToString();
            }
            public void Read(string values)
            {
                if (values == null || values.Length == 0) return;
                var split = values.Split('\u2022')?.Select(x => x.Split(':').Select(y => y.Trim()).ToArray()).Where(x => x.Length >= 2).ToArray();
                if (split == null || split.Length == 0) return;
                foreach (var v in split)
                {
                    if (serializableValues.ContainsKey(v[0]))
                    {
                        serializableValues[v[0]].UpdateValue(v[1]);
                    }
                }
            }
        }
        // ** END OF AEYOS CONFIG HELPER ** //
        // ** END OF SE CODE ** //
    }
}