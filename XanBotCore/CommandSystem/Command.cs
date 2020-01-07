using System;
using System.Threading.Tasks;
using XanBotCore.CommandSystem.Commands;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.CommandSystem
{

    /// <summary>
    /// Represents a runnable command for this bot.
    /// </summary>
    public abstract class Command : IComparable<Command>
    {
        /// <summary>
        /// The name of this command. This is also used to execute it. Should not include spaces or special characters. Should be all lowercase.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// An optional array of alternative command names that can be used to run the command.
        /// </summary>
        public virtual string[] AlternateNames { get; } = null;

        /// <summary>
        /// The description of this command returned by the stock help command. You should use {0} in place of the command prefix if any commands are referenced (e.g. "Say {0}help for help" instead of "Say >>help for help")
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The usage syntax of this command returned by the stock help command.
        /// </summary>
        public abstract string Syntax { get; }

        /// <summary>
        /// The permission level required to use this command.
        /// </summary>
        public abstract byte RequiredPermissionLevel { get; }

        /// <summary>
        /// Returns whether or not the specified member can use the command. The default behavior checks if <paramref name="member"/>.PermissionLevel is greater than or equal to this command's <see cref="RequiredPermissionLevel"/>.
        /// </summary>
        /// <param name="member">The member using this command.</param>
        /// <returns>true if the specified member can use the command, false if they can not.</returns>
        public virtual UsagePermissionPacket CanUseCommand(XanBotMember member)
        {
            return new UsagePermissionPacket(
                member.PermissionLevel >= RequiredPermissionLevel,
                $"You are not authorized to use {Name}. It is only available to Permission Level {RequiredPermissionLevel} (your Permission Level is {member.PermissionLevel})."
            );
        }

        /// <summary>
        /// Returns true if the specified member can use this command in the specified channel. If this is the case, <paramref name="optimalTargetChannel"/> is expected to be set to null.<para/>
        /// If the function returns false, it expects <paramref name="optimalTargetChannel"/> to be set to a non-null value, which represents the best channel to use the command in (so if there's multiple channels, it should be the best one suited for this command)
        /// </summary>
        /// <param name="member">The member using this command.</param>
        /// <param name="channel">The channel they are trying to use the command in.</param>
        /// <returns></returns>
        public virtual bool CanUseCommandInThisChannel(XanBotMember member, DiscordChannel channel, out DiscordChannel optimalTargetChannel)
        {
            optimalTargetChannel = null;
            return true;
        }

        /// <summary>
        /// Executes the command. This does not check if the user can run it. Check if the user is authorized before using this method.
        /// </summary>
        /// <param name="context">The <see cref="BotContext"/> that this command is running in.</param>
        /// <param name="executingMember">The member executing this command.</param>
        /// <param name="originalMessage">The DiscordMessage that was responsible for invoking this command.</param>
        /// <param name="args">The arguments of the command split via shell32.DLL's handling system.</param>
        /// <param name="allArgs">Every argument passed into this command as its raw string. This is used to preserve quotes and other characters stripped by shell32.</param>
        public abstract Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs);

        public int CompareTo(Command other)
        {
            if (other == null)
                return 1;
            // Start by sorting via permission level.
            // I'm returning math.sign so the value is always -1, 0, or 1 -- Certain stock CompareTo methods may return values at a wider range.
            int permComparison = Math.Sign(RequiredPermissionLevel.CompareTo(other.RequiredPermissionLevel));
            if (permComparison != 0)
                return permComparison;

            // If the permission level is the same, sort alphabetically.
            return Math.Sign(Name.CompareTo(other.Name));
        }
    }
}
