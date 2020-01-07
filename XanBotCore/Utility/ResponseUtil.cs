using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XanBotCore.Logging;
using XanBotCore.Utility.DiscordObjects;

namespace XanBotCore.Utility
{

    /// <summary>
    /// Offers utilities to universally respond to <see cref="DiscordMessage"/>s (even if they are null, in which the response is placed in the console).
    /// </summary>
    public class ResponseUtil
    {

        /// <summary>
        /// \u00AD is a no-break soft hyphen that does not render in Discord's font. Other fonts, primarly fixed width fonts, may display this character as -<para/>
        /// This is used to soft-remove pings from a response, allowing text to seamlessly appear to have an unbroken @everyone or @here without actually pinging the role.
        /// </summary>
        private const char INVISIBLE = '\u00AD';

        /// <summary>
        /// Respond to the specified <see cref="DiscordMessage"/>. If <paramref name="message"/> is null, it will write the response to the console.<para/>
        /// This offers cross-functionality between commands executed in a chat channel vs. commands executed in the console (where a DiscordMessage object won't exist)<para/>
        /// Returns the created <see cref="DiscordMessage"/>, or null if <paramref name="message"/> is null.
        /// </summary>
        /// <param name="message">The DiscordMessage being responded to.</param>
        /// <param name="text">The text to respond with.</param>
        /// <param name="forceWriteToConsole">If true, this text will be written to the console even if <paramref name="message"/> is not null.</param>
        /// <param name="allowEveryoneAndHere">If true, any messages sent via this response can use @everyone and @here (This is false by default to prevent any commands that repeat user chats from being disruptive)</param>
        /// <returns>The response message, if applicable. Will be null if <paramref name="message"/> is null.</returns>
        public static async Task<DiscordMessage> RespondToAsync(DiscordMessage message, string text, bool forceWriteToConsole = false, bool allowEveryoneAndHere = false)
        {
            // Most of the code here is recycled from my personal bot, e.g. the forceAllowMassPing boolean.
            string msgText = text;
            if (!allowEveryoneAndHere)
                msgText = StripMassPings(msgText);
            DiscordMessage response = null;

            if (message != null)
            {
                if (XanBotLogger.MessageHasColors(text))
                {
                    msgText = XanBotLogger.StripColorFormattingCode(text);
                }

                response = await message.RespondAsync(msgText);
            }
            if (forceWriteToConsole || message == null)
            {
                text = text.Replace("```", ""); // There may be more stuff to do, but this just makes it better for display in the console.
                XanBotLogger.WriteLine(text, HasMassPings(msgText));
            }

            return response;
        }

        /// <summary>
        /// Respond to the specified <see cref="DiscordMessage"/> with the provided <see cref="DiscordEmbed"/>.
        /// </summary>
        /// <param name="message">The <see cref="DiscordMessage"/> to respond to. If this is null, nothing will happen. </param>
        /// <param name="response">The <see cref="DiscordEmbed"/> to respond with.</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> RespondToAsync(DiscordMessage message, DiscordEmbed response)
        {
            if (message != null)
                return await message.RespondAsync(embed: response);
            return null;
        }

        /// <summary>
        /// Respond to the specified <see cref="DiscordMessage"/> with the<see cref="DiscordEmbed"/> provided by the specified object that implements <see cref="IEmbeddable"/>
        /// </summary>
        /// <param name="message">The <see cref="DiscordMessage"/> to respond to. If this is null, nothing will happen. </param>
        /// <param name="embeddableObject">The <see cref="IEmbeddable"/> that provides a <see cref="DiscordEmbed"/> to respond with.</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> RespondToAsync(DiscordMessage message, IEmbeddable embeddableObject)
        {
            if (message != null)
                return await message.RespondAsync(embed: embeddableObject.ToEmbed());
            return null;
        }

        /// <summary>
        /// Posts a message in a specific DiscordChannel.
        /// </summary>
        /// <param name="channel">The DiscordChannel to send the response message to.</param>
        /// <param name="text">The text to respond with.</param>
        /// <param name="forceWriteToConsole">If true, this text will be written to the console.</param>
        /// <param name="tryFormattingForConsole">If true, this will strip certain formatting information off of messages that won't work in the console, like ``` for code blocks.</param>
        /// <param name="allowEveryoneAndHere">If true, any messages sent via this response can use @everyone and @here (This is false by default to prevent any commands that repeat user chats from being disruptive)</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> RespondInAsync(DiscordChannel channel, string text, bool forceWriteToConsole = false, bool tryFormattingForConsole = false, bool allowEveryoneAndHere = false)
        {
            if (!allowEveryoneAndHere)
                text = StripMassPings(text);

            if (XanBotLogger.MessageHasColors(text))
            {
                text = XanBotLogger.StripColorFormattingCode(text);
            }

            DiscordMessage response = await channel.SendMessageAsync(text);

            if (forceWriteToConsole)
            {
                if (tryFormattingForConsole)
                {
                    text = text.Replace("```", ""); //There may be more stuff to do
                }
                XanBotLogger.WriteLine(text, HasMassPings(text));
            }

            return response;
        }

        /// <summary>
        /// This method takes in a number of roles and a string message. The message is expected to contain a ping to the included roles by ID (Like so: &lt;@&amp;ID&gt;).<para/>
        /// This method will enable pinging for the specified roles, send the message that pings said roles, and then disable pinging for the specified roles. This does NOT respect previous settings on the role regarding whether it is pingable or not.<para/>
        /// Note: Attempt to use this method sparingly, as it makes two API calls per role which may cause throttling to occur if used in excess.
        /// </summary>
        /// <param name="targetChannel">The channel to send the message into.</param>
        /// <param name="message">A message to accompany the pings with. This can be null if you want to only ping the roles.</param>
        /// <param name="putWelcomingPrefix">If true, it attempts to add some feeling to the message (rather than a raw ping, it will say "Hey, role, role, and role!" followed by the message, if the message is not null)</param>
        /// <param name="roles">The role or roles that will be pinged.</param>
        /// <returns></returns>
        public static async Task SendPingsInMessageAsync(DiscordChannel targetChannel, string message, bool putWelcomingPrefix, params DiscordRole[] roles)
        {
            foreach (DiscordRole role in roles)
            {
                await role.ModifyAsync(mentionable: true, reason: "Call to SendPingsInMessage, role needs to be made temporarily pingable.");
            }
            string pingMsg = "";
            if (putWelcomingPrefix)
            {
                pingMsg = "Hey, ";
                foreach (DiscordRole role in roles)
                {
                    if (role == roles.Last())
                    {
                        if (roles.Length > 1)
                        {
                            pingMsg += "and <@&" + role.Id + ">!";
                        }
                        else
                        {
                            pingMsg += "<@&" + role.Id + ">!";
                        }
                    }
                    else
                    {
                        pingMsg += "<@&" + role.Id + ">, ";
                    }
                }
            }
            else
            {
                foreach (DiscordRole role in roles)
                {
                    pingMsg += "<@&" + role.Id + "> ";
                }
            }
            if (message != null && message != "")
            {
                await targetChannel.SendMessageAsync(pingMsg + message);
            }
            else
            {
                await targetChannel.SendMessageAsync(pingMsg);
            }
            foreach (DiscordRole role in roles)
            {
                await role.ModifyAsync(mentionable: false, reason: "Ping message sent. Role needs to be reset to non-pingable.");
            }
        }

        /// <summary>
        /// Removes all @everyone and @here pings by placing <see cref="INVISIBLE"/> between the @ and the text, causing the ping to not resolve.<para/>
        /// If the text does not have any @everyone or @here in it, the message will remain unchanged.
        /// </summary>
        /// <param name="message">The message that may contain @everyone or @here</param>
        /// <returns></returns>
        public static string StripMassPings(string message)
        {
            string noEveryone = Regex.Replace(message, "@everyone", "@" + INVISIBLE + "everyone", RegexOptions.IgnoreCase);
            string noHere = Regex.Replace(noEveryone, "@here", "@" + INVISIBLE + "here", RegexOptions.IgnoreCase);
            string noUser = Regex.Replace(noHere, @"(<@)\d+>", "`[REMOVED_PING]`", RegexOptions.IgnoreCase);
            return noUser;
        }

        /// <summary>
        /// Returns true if the message has @everyone or @here in it. Do NOT use this to determine if you should call <see cref="StripMassPings(string)"/>
        /// </summary>
        /// <param name="message">The message that may contain @everyone or @here</param>
        /// <returns></returns>
        private static bool HasMassPings(string message)
        {
            return message != StripMassPings(message);
        }

    }
}
