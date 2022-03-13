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
            public class AOption
            {
                public string name;
                public bool value;
                public AOption(string name, bool check) { this.name = name; this.value = check; }
                public override string ToString() { return (value.Equals(null)) ? "" : $"     [{(value == true ? "X" : "  ")}] {name}"; }
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
            public class AOptions
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

                public void Update(string s)
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
                        if (option[0].ToUpper().Contains("X"))
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
                    foreach (var option in values) {
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
            public abstract class AValue
            {
                public System.Type type;
                public string key;

                public abstract void UpdateValue(string v);
            }
            public class AValue<T> : AValue
            {
                public T value;

                public AValue(string key, T value) {
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
                    try {
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
                        else if (type.IsEquivalentTo(typeof(AOptions)))
                        {
                            ((AOptions)(object)originalValue.value).Update(value);
                            return originalValue;
                        }
                    } catch { }
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
            public AConfig(params AValue[] values) {
                foreach (var o in values) {
                    this.serializableValues.Add(o.key, o);
                };
            }
            public Dictionary<string, AValue> serializableValues = new Dictionary<string, AValue>();
            override
            public string ToString()
            {
                var sb = new StringBuilder();
                foreach (var v in serializableValues)
                {
                    sb.AppendLine("• " + v.Value.ToString());
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
                    if (serializableValues.ContainsKey(v[0]))
                    {
                        serializableValues[v[0]].UpdateValue(v[1]);
                    }
                }
            }
        }
        // END OF: Aeyos custom data config helper //

        AConfig config;

        AConfig.AValue<int> c_UpdateEvery = new AConfig.AValue<int>("Update Every", 100);
        AConfig.AValue<float> c_Speed = new AConfig.AValue<float>("Speed", 0.0f);
        AConfig.AValue<float> c_Repetition = new AConfig.AValue<float>("Repetition", 0.0f);
        AConfig.AValue<Color[]> c_Colors = new AConfig.AValue<Color[]>("Colors", new Color[]{Color.Red,Color.Lime,Color.Blue});
        AConfig.AValue<float> c_GradientPatternRepetition = new AConfig.AValue<float>("Gradient pattern repetition", 1f);
        AConfig.AValue<AConfig.AOptions> c_LightMode = new AConfig.AValue<AConfig.AOptions>("Light Mode", new AConfig.AOptions(
            new List<AConfig.AOption> { new AConfig.AOption("Normal", true), new AConfig.AOption("Gradient", false) },
            singleSelect: true
        ));
        AConfig.AValue<string> c_GroupName = new AConfig.AValue<string>("Group Name", "RGB Lights");


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
            Echo(config.ToString());
            Echo($"Getting ({c_GroupName.key}): {c_GroupName.value}");
            Echo($"Getting ({c_UpdateEvery.key}): {c_UpdateEvery.value}");
            Echo($"Getting ({c_GradientPatternRepetition.key}): {c_GradientPatternRepetition.value}");
        }
    }
}