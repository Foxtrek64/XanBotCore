namespace XanBotCore.Logging
{

    /// <summary>
    /// Represents a class that has a special ToString() method designed for use in <see cref="XanBotLogger"/>'s formatting system.
    /// </summary>
    public interface IConsolePrintable
    {

        /// <summary>
        /// Translate this object into a string that uses <see cref="XanBotLogger"/>'s formatting system.
        /// </summary>
        /// <returns></returns>
        string ToConsoleString();

    }
}
