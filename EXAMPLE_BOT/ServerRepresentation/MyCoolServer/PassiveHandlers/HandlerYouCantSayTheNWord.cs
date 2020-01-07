﻿using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.PassiveHandlers;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace EXAMPLE_BOT.ServerRepresentation.MyCoolServer.PassiveHandlers
{
    class HandlerYouCantSayTheNWord : PassiveHandler
    {
        public override string Name => "You Can't Say The N Word!!!";

        public override string Description => "Responds to people who threaten to say the N word.";

        public override async Task<bool> RunHandlerAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage)
        {
            if (originalMessage.Content.ToLower().Contains("i'm gonna say the n word"))
            {
                await ResponseUtil.RespondToAsync(originalMessage, "THAT'S RACIST YOU CAN'T SAY THE N WORD!!!");
                return true; // We handled this message and don't want other handlers to intercept it.
            }
            return false; // We did not handle this message so we can allow other handlers to intercept it.
        }
    }
}
