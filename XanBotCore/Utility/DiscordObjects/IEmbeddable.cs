namespace XanBotCore.Utility.DiscordObjects
{

    /// <summary>
    /// Represents an object that can be turned into a <see cref="DiscordEmbed"/>.
    /// </summary>
    public interface IEmbeddable
    {

        /// <summary>
        /// Represent this object as a <see cref="DiscordEmbed"/>
        /// </summary>
        /// <returns></returns>
        DiscordEmbed ToEmbed();

    }
}
