using System.Linq;
using System.Threading.Tasks;
using XanBotCore.DataPersistence;
using XanBotCore.Exceptions;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands
{
    public class CommandConfig : Command
    {
        public override string Name { get; } = "config";

        public override string Description { get; } = "Alters bot configuration data.";

        public override string Syntax
        {
            get
            {
                /*return	Name + " <get> <key>\n" +
						Name + " <set> <key> <value>\n" +
						Name + " <remove> <key>\n" +
						Name + " <list>";*/
                return Name + " :get|set|remove|list: [<key> [<value>]]";
            }
        }

        public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_OPERATOR;

        public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs)
        {
            if (args.Length == 0)
            {
                throw new CommandException(this, "Invalid argument count. Expected at least one argument.");
            }
            else if (args.Length >= 1)
            {
                XConfiguration targetConfig = XConfiguration.GetConfigurationUtility(context);

                // If there is 1 or more arguments...
                string subCommand = args[0].ToLower();

                // >> config list
                if (subCommand == "list")
                {
                    string message = "**Configuration Values:**\n```lua\n";
                    string[] keys = targetConfig.Keys.ToArray();
                    foreach (string key in keys)
                    {
                        message += "[\"" + key + "\"]=" + targetConfig.GetConfigurationValue(key) + "\n";
                    }
                    message += "```";
                    await ResponseUtil.RespondToAsync(originalMessage, message);

                    // >> config get
                }
                else if (subCommand == "get")
                {
                    if (args.Length != 2)
                    {
                        throw new CommandException(this, "Expected two arguments for operation \"get\" -- get <key>");
                    }
                    if (args[1].Contains(' '))
                    {
                        throw new CommandException(this, "Config keys cannot contain spaces.");
                    }
                    string value = targetConfig.GetConfigurationValue(args[1]);
                    if (value != null)
                    {
                        await ResponseUtil.RespondToAsync(originalMessage, "```\n[" + args[1] + "]=" + value + "\n```");
                    }
                    else
                    {
                        await ResponseUtil.RespondToAsync(originalMessage, "The specified key does not exist in the configuration.");
                    }

                    // >> config set
                }
                else if (subCommand == "set")
                {
                    if (args.Length != 3)
                    {
                        throw new CommandException(this, "Expected two arguments for operation \"set\" -- set <key> <value>");
                    }
                    if (args[1].Contains(' '))
                    {
                        throw new CommandException(this, "Config keys cannot contain spaces.");
                    }
                    targetConfig.SetConfigurationValue(args[1], args[2]);
                    await ResponseUtil.RespondToAsync(originalMessage, "Set [`" + args[1] + "`] to: `" + args[2] + "`");

                    // >> config remove
                }
                else if (subCommand == "remove")
                {
                    if (args.Length != 2)
                    {
                        throw new CommandException(this, "Expected two arguments for operation \"remove\" -- remove <key>");
                    }
                    if (args[1].Contains(' '))
                    {
                        throw new CommandException(this, "Config keys cannot contain spaces.");
                    }
                    bool wasRemoved = targetConfig.RemoveConfigurationValue(args[1]);
                    if (wasRemoved)
                    {
                        await ResponseUtil.RespondToAsync(originalMessage, $"Removed configuration entry `{args[1]}`");
                    }
                    else
                    {
                        await ResponseUtil.RespondToAsync(originalMessage, $"Could not remove configuration entry `{args[1]}` -- it doesn't exist in the first place.");
                    }

                    // something else
                }
                else
                {
                    throw new CommandException(this, string.Format("Invalid operation \"{0}\" (expected get, set, remove, or list)", subCommand));
                }
            }
        }
    }
}
