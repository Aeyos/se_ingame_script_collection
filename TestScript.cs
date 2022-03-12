using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript2
{
    partial class Program : MyGridProgram
    {
        // Aeyos custom data config helper //
        public class AConfig
        {
            public static Action<string> SEcho = (string _) => { };
            public struct AOption
            {
                public KeyValuePair<string, bool> value;
                public AOption(string s, bool v) { value = new KeyValuePair<string, bool>(s, v); }
                public override string ToString() { return (value.Equals(null)) ? "" : $"     [{(value.Value == true ? "X" : "  ")}] {value.Key}"; }
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
            public struct AOptions
            {
                public List<AOption> values;
                public bool singleSelect;

                public AOptions(List<AOption> list, bool singleSelect = false)
                {
                    this.singleSelect = singleSelect;
                    this.values = list;
                }

                public void Add(AOption element)
                {
                    this.values.Add(element);
                }

                public override string ToString()
                {
                    return "\n" + string.Join("\n", values.Select(x => x.ToString()));
                }

                public static AOptions Parse(string s, AOptions originalValue)
                {
                    var singleSelectValueFound = false;
                    // Create return object
                    var options = new AOptions(new List<AOption>());
                    // Get original options
                    var originalOptions = (AOptions)originalValue;
                    // Create map for values that will be read
                    var optionMap = new Dictionary<string, bool>();
                    // Split string into lines and lines into pairs of (X|"  ") and Name
                    var optionSplit = s.Split('\n')?.Select(x => x.Trim().Split(']')?.Select(y => y.Trim()).ToArray()).Where(x => x.Length > 0).ToArray();
                    // No options found, only name
                    if (optionSplit.Length == 0) return (AOptions)originalValue;
                    // For all the read values
                    foreach (var option in optionSplit)
                    {
                        // Skip if option is not well formatted
                        if (option == null || option.Length < 2) continue;
                        // Add NAME - BOOL to dictionary
                        var newValue = option[0].ToUpper().Contains("X");
                        optionMap.Add(option[1], newValue);
                        // Mark flag single select
                        if (newValue && originalValue.singleSelect)
                        {
                            singleSelectValueFound = true;
                        }
                    }
                    // Is single selection but no value was found, default to original
                    if (originalOptions.singleSelect && singleSelectValueFound == false)
                    {
                        return originalOptions;
                    }
                    // For all the original options
                    foreach (var option in originalOptions.values)
                    {
                        // Calculate new value, defaults to originalValue if not found
                        var newValue = optionMap.ContainsKey(option.value.Key) ? optionMap[option.value.Key] : option.value.Value;
                        // Add new option to return value
                        options.Add(new AOption(option.value.Key, newValue));
                    }
                    // Returns the object
                    return options;
                }
            }
            public struct AValue
            {
                public System.Type type;
                public string key;
                public object value;

                public AValue(string key, object value) { this.key = key; this.value = value; this.type = value.GetType(); }

                public AValue(AValue originalValue, string newValue)
                {
                    this.key = originalValue.key;
                    this.type = originalValue.type;
                    this.value = AValue.ParseValue(newValue, this.type, originalValue) ?? originalValue.value;
                }

                public override string ToString()
                {
                    return $"{key}: {AValue.ValueToString(value, type)}";
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

                private static object ParseValue(string value, Type type, AValue originalValue)
                {
                    if (value == null) return null;
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
                    else if (type.IsEquivalentTo(typeof(AOptions))) return AOptions.Parse(value, (AOptions)originalValue.value);
                    return "-";
                }
            }
            public AConfig(params AValue[] values) { this.serializableValues = values.ToList(); }
            public List<AValue> serializableValues;
            public T GetValue<T>(AValue valueDefinition)
            {
                var avalue = serializableValues.Find(x => x.key == valueDefinition.key);
                if (avalue.Equals(null)) return default(T);
                return (T)avalue.value;
            }
            new public string ToString()
            {
                var sb = new StringBuilder();
                foreach (var v in serializableValues)
                {
                    sb.AppendLine("• " + v.ToString());
                }
                return sb.ToString();
            }
            public void Read(string values)
            {
                if (values == null || values.Length == 0) return;
                var split = values.Split('•')?.Select(x => x.Split(':').Select(y => y.Trim()).ToArray()).Where(x => x.Length >= 2).ToArray();
                if (split == null || split.Length == 0) return;
                foreach (var v in split)
                {
                    int serialValueIndex = serializableValues.FindIndex((AValue x) => x.key == v[0]);
                    var cval = serializableValues[serialValueIndex];
                    if (cval.key != null)
                    {
                        serializableValues[serialValueIndex] = new AValue(cval, v[1]);
                    }
                }
            }
        }
        // END OF: Aeyos custom data config helper //

        AConfig config;

        AConfig.AValue c_UpdateEvery = new AConfig.AValue("Update Every", 100);
        AConfig.AValue c_Speed = new AConfig.AValue("Speed", 0.0f);
        AConfig.AValue c_Repetition = new AConfig.AValue("Repetition", 0.0f);
        AConfig.AValue c_Colors = new AConfig.AValue("Colors", new Color[]{Color.Red,Color.Lime,Color.Blue});
        AConfig.AValue c_LightMode = new AConfig.AValue("Light Mode", new AConfig.AOptions(
            new List<AConfig.AOption>
            {
                new AConfig.AOption(c_LightMode_Normal, true),
                new AConfig.AOption(c_LightMode_Gradient, false),
            },
            singleSelect: true
        ));
        AConfig.AValue c_GradientPatternRepetition = new AConfig.AValue("Gradient pattern repetition", 1f);
        AConfig.AValue c_GroupName = new AConfig.AValue("Group Name", "RGB Lights");
        const string c_LightMode_Normal = "Normal";
        const string c_LightMode_Gradient = "Gradient";

        public Program()
        {
            AConfig.SEcho = Echo;
            config = new AConfig(c_UpdateEvery, c_Speed, c_Repetition, c_Colors, c_LightMode, c_GradientPatternRepetition, c_GroupName);
            //Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument, UpdateType updateType)
        {
            config.Read(this.Me.CustomData);
            this.Me.CustomData = config.ToString();
            // Echo(config.ToString());
            Echo($"Getting ({c_GroupName}): {config.GetValue<string>(c_GroupName)}");
            Echo($"Getting ({c_UpdateEvery}): {config.GetValue<int>(c_UpdateEvery)}");
        }
    }
}