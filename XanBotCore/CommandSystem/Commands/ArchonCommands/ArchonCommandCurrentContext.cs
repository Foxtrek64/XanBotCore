using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands.ArchonCommands
{
    public class ArchonCommandCurrentContext : ArchonCommand
    {
        public override string Name => "currentcontext";

        public override string Description => "Returns information on the BotContext representing this server.";

        public override string Syntax => Name;

        public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs)
        {
            if (context == null)
                throw new ArchonCommandException(this, "Cannot use currentcontext from the console, as it requires an instance of BotContext to be present.");
            //ResponseUtil.RespondTo(originalMessage, context.ToStringForDiscordMessage());
            //originalMessage?.RespondAsync(embed: context.ToEmbed());
            await ResponseUtil.RespondToAsync(originalMessage, context);
        }
    }
}
