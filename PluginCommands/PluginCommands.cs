using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using System.Threading;
using TerrariaApi.Server;

namespace PluginCommands
{
    [ApiVersion(2, 1)]
    public class PluginCommands : TerrariaPlugin
    {

        public override string Name
        {
            get { return "Updated PluginCommands"; }
        }
        public override string Author
        {
            get { return "crackruckles"; }
        }
        public override string Description
        {
            get { return "Provides a list of all commands used by you plugins."; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public PluginCommands(Main game)
            : base(game)
        {
            Order = -1;
        }
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("PluginCommands.allow", pluginCommands, "plugincommands"));
            Commands.ChatCommands.Add(new Command("PluginCommands.allow", pluginCommands, "pc"));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        private void pluginCommands(CommandArgs args)
        {
            bool verbose = false;
            bool permissions = false;
            bool plugins = false;
            bool help = false;
            bool alias = false;
            bool sort = false;
            bool noActionRequired = false;

            string arg;
            if (args.Parameters.Count == 0)
            {
                args.Player.SendMessage("Syntax: /PluginCommands [-help] ", Color.LightSalmon);
                args.Player.SendMessage("Flags: ", Color.LightSalmon);
                args.Player.SendMessage("   -help   this information", Color.LightSalmon);
                args.Player.SendMessage("   -a      lists commands by description", Color.LightSalmon);
                args.Player.SendMessage("   -s      lists command by command name sorted", Color.LightSalmon);
                args.Player.SendMessage("   -p      show permissions for command used with -s", Color.LightSalmon);
                args.Player.SendMessage("   -v      shows TShock commands with *", Color.LightSalmon);
                return;
            }

            for (int i = 0; i < args.Parameters.Count; i++)
            {
                arg = args.Parameters[i];
                switch (arg)
                {
                    case "-plug":
                        plugins = true;
                        break;
                    case "-a":
                        alias = true;
                        break;
                    case "-p":
                        permissions = true;
                        break;
                    case "-v":
                        verbose = true;
                        break;
                    case "-s":
                        sort = true;
                        break;

                    case "-help":
                        args.Player.SendMessage("Syntax: /PluginCommands [-help] ", Color.Red);
                        args.Player.SendMessage("Flags: ", Color.LightSalmon);
                        args.Player.SendMessage("   -help   this information", Color.LightSalmon);
                        args.Player.SendMessage("   -a      lists commands by description", Color.LightSalmon);
                        args.Player.SendMessage("   -s      lists command by command name sorted", Color.LightSalmon);
                        args.Player.SendMessage("   -p      show permissions for command used with -s", Color.LightSalmon);
                        args.Player.SendMessage("   -v      shows TShock commands with *", Color.LightSalmon);
                        return;

                    default:
                        args.Player.SendErrorMessage("Unkonown command argument:" + arg);
                        noActionRequired = true;
                        return;
                }
            }

            if (plugins)
            {
                args.Player.SendMessage(String.Format("Current Plugins ({0})", ServerApi.Plugins.Count), Color.LightSalmon);
                for (int i = 0; i < ServerApi.Plugins.Count; i++)
                {
                    PluginContainer pc = ServerApi.Plugins.ElementAt(i);
                    args.Player.SendMessage(String.Format("{0} {1} {2} {3}", pc.Plugin.Name, pc.Plugin.Description, pc.Plugin.Author, pc.Plugin.Version), Color.LightSalmon);
                }
            }

            IEnumerable<Command> cmds = TShockAPI.Commands.ChatCommands.FindAll(c => c != null);
            if (alias)
            {
                foreach (Command cmd in cmds.OrderBy(h => h.HelpText))
                {
                    args.Player.SendMessage(String.Format(" {0}", cmd.HelpText), Color.LightSalmon);
                    string names = "";
                    cmd.Names.Sort();
                    foreach (string name in cmd.Names)
                    {
                        names += name + ",";
                        var element = new KeyValuePair<string, string>(name, cmd.HelpText);

                        args.Player.SendMessage(String.Format("   {0}", name), Color.LightSalmon);
                    }
                }

            }

            if (sort)
            {

                Dictionary<string, string> commandPermissions = new Dictionary<string, string>();
                List<KeyValuePair<string, string>> commandList = new List<KeyValuePair<string, string>>();

                string tshockCommand = "";
                string permission = "";
                foreach (Command cmd in cmds)
                {
                    string names = "";
                    foreach (string name in cmd.Names)
                    {
                        names += name + ",";

                        tshockCommand = " ";
                        if (verbose)
                        {
                            for (int i = 0; i < TShockAPI.Commands.TShockCommands.Count; i++)
                            {
                                foreach (string cname in TShockAPI.Commands.TShockCommands[i].Names)
                                    if (cname.Equals(name))
                                    {
                                        tshockCommand = "*";
                                        break;
                                    }
                            }
                        }
                        var element = new KeyValuePair<string, string>(tshockCommand + name, cmd.HelpText);
                        commandList.Add(element);

                        string comma = "";
                        permission = "";
                        foreach (string p in cmd.Permissions)
                        {
                            permission += comma + p;
                            comma = ",";
                        }
                        try
                        {
                            commandPermissions.Add(tshockCommand + name, permission);
                        }
                        catch { }
                    }
                }
                string lastCommand = "";
                foreach (KeyValuePair<string, string> item in commandList.OrderBy(key => key.Key))
                {
                    permission = commandPermissions[item.Key];
                    if (lastCommand.ToLower().Equals(item.Key.ToLower()))
                        args.Player.SendMessage(String.Format(" {0}:{1}", item.Key, item.Value), Color.Red);
                    else
                        args.Player.SendMessage(String.Format(" {0}:{1}", item.Key, item.Value), Color.LightSalmon);
                    lastCommand = item.Key;
                    if (permissions)
                        args.Player.SendMessage(String.Format("    {0}", permission), Color.LightSalmon);
                }

            }
        }
    }
}
