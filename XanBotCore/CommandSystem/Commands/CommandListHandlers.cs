using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.PassiveHandlers;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands
{
    public class CommandListHandlers : Command
    {

        public override string Name { get; } = "listhandlers";

        public override string Description { get; } = "Lists all passive handlers that are currently active. Passive handlers are like commands, but they run based on any applicable message (not just commands)";

        public override string Syntax => Name + " [handlerName]";

        public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_STANDARD_USER;

        public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs)
        {
            if (args.Length == 0)
            {
                string res = "```\n";
                foreach (PassiveHandler handler in context.Handlers)
                {
                    // Secret handlers can't be displayed in normal command usage. Only display handlers if they aren't secret, or if they are secret, only allow it in the mod channel.
                    res += handler.Name + "\n";
                }
                res += "```";
                await ResponseUtil.RespondToAsync(originalMessage, res);
            }
            else if (args.Length == 1)
            {
                foreach (PassiveHandler handler in context.Handlers)
                {
                    if (args[0].ToLower() == handler.Name.ToLower())
                    {
                        // Same thing as above: Prevent users from getting info on secret handlers unless the command is executed in the bot channel.
                        await ResponseUtil.RespondToAsync(originalMessage, "**" + handler.Name + ":** " + handler.Description);
                        return;
                    }
                }
                await ResponseUtil.RespondToAsync(originalMessage, "There is no Passive Handler with the name " + args[0] + "\n(If there's a space in the name, try adding quotation marks around the name!)");
            }
            else
            {
                throw new CommandException(this, "Invalid amount of command arguments. Try putting quotation marks around the name of the handler.");
            }
        }
    }
}
