using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace XanBotCore.Utility
{

    /// <summary>
    /// A utility for making web requests.
    /// </summary>
    public class WebUtil
    {

        /// <summary>
        /// Get the raw contents of a webpage via an HTTP GET, returning a string of whatever the webpage returned (for instance, HTML data)
        /// </summary>
        /// <param name="url">The URL to go to.</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url)
        {
            WebResponse response = await WebRequest.Create(url).GetResponseAsync();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            dataStream.Close();
            reader.Close();
            response.Close();
            return responseFromServer;
        }

    }
}
