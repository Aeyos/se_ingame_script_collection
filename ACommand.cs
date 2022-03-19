using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace ACommand
{
    partial class Program : MyGridProgram
    {
        // ** START OF SE CODE ** //
        public static class ACommands
        {        
            private static Dictionary<String, ACommand> commands = new Dictionary<String, ACommand>();
            private class ACommand {
                public Action<string[]> action;
                public String command;
                public String description;
                override public String ToString()
                {
                    return $"> {command}{Environment.NewLine}    Usage: {description ?? "Not Defined"}";
                }
            }

            public static void AddCommand(String commandName, Action<string[]> action, String description = null)
            {
                commands[commandName] = new ACommand()
                {
                    action = action,
                    command = commandName,
                    description = description,
                };
            }

            internal static bool Parse(string argumentString)
            {
                var command = argumentString;
                var arguments = new List<string>();

                while (command.Length > 0)
                {
                    if (commands.ContainsKey(command))
                    {
                        commands[command].action.Invoke(arguments.ToArray());
                        return true;
                    }
                    var splitIndex = command.LastIndexOf(":");
                    if (splitIndex < 0) return false;
                    arguments.Insert(0, command.Substring(splitIndex + 1));
                    command = command.Substring(0, splitIndex);
                }
                return false;
            }

            new internal static string ToString()
            {
                return string.Join(Environment.NewLine,
                    "    /*~/                                                \\~*\\\n"
                    + "/*~/    Commands Defined By Script    \\~*\\\n"
                    + "   /*~/                                                  \\~*\\\n\n"
                    + String.Join(Environment.NewLine + Environment.NewLine, commands.Values.Select((x) => x.ToString())));
            }
        }

        // SE "on compile" function
        public Program()
        {
            ACommands.AddCommand("SET:COLOR", (string[] args) =>
            {
                Echo($"Setting color to: {args[0]}");
            }, $"SET:COLOR:<COLOR1>,<COLOR2>{Environment.NewLine}        where COLOR is 3 component number, ex: <255,0,0>");
            ACommands.AddCommand("SET", (string[] args) =>
            {
                Echo($"Getting prop: {String.Join(",", args)}");
            });
        }

        // SE "on run" function
        public void Main(string argument, UpdateType updateType)
        {
            Me.CustomData = ACommands.ToString();
            if (ACommands.Parse(argument) == false)
            {
                Echo($"Unknown command: {argument}");
            }
        }
        // ** END OF SE CODE ** //
    }
}
