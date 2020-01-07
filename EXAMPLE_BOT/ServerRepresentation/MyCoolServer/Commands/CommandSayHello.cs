using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.CommandSystem;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace EXAMPLE_BOT.ServerRepresentation.MyCoolServer.Commands
{
    class CommandSayHello : Command
    {
        public override string Name => "sayhello";

        public override string Description => "Makes the bot say hello.";

        // Syntax should point to Name if there's no args. If there are args, it needs to be Name + " <arg0> [optionalArg1]" or something like that. Syntax is wrapped in ``s, but it detects if you terminate this early (e.g. your syntax is Name + " <arg0>` this text will show up normally"
        public override string Syntax => Name;

        public override byte RequiredPermissionLevel => PermissionRegistry.DefaultPermissionLevel;

        public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs)
        {
            await ResponseUtil.RespondToAsync(originalMessage, "Hello, " + executingMember.Member.Mention);
        }
    }
}
