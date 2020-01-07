﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.UserObjects;

namespace XanBotCore.Utility
{

    /// <summary>
    /// Offers utilities intended for getting users from certain queries e.g. username, discriminator, or GUID.
    /// </summary>
    public class UserGetter
    {

        //private static bool HasAcquiredAllMembers { get; set; } = false;

        private static DateTime LastCheckTime = DateTime.MinValue;

        /// <summary>
        /// Whether or not the member cache needs to be repopulated.
        /// </summary>
        private static bool NeedsNewMemberCache => (DateTime.Now - LastCheckTime).TotalMinutes >= 5;

        private static IReadOnlyCollection<DiscordMember> Members { get; set; } = null;

        /// <summary>
        /// Attempts to get a member via three distinct methods: <para/>
        /// First, it will attempt to cast <paramref name="data"/> into a ulong and see if it is a user GUID (this includes processing pings, like &lt;@GUID_HERE&gt;)<para/>
        /// Second, it will attempt to see if <paramref name="data"/> is a discriminator (only if data is formatted as #XXXX where XXXX is four digits)<para/>
        /// Finally, and only if the second method had no results or wasn't used, it will attempt to find <paramref name="data"/> as a nickname or username.<para/>
        /// This will throw a NonSingularResultException if the query can return more than one user. 
        /// </summary>
        /// <param name="server">The Discord server to target.</param>
        /// <param name="data">The query to get a XanBotMember from. This can be a ulong as a string, a user ping (&lt;@ulong&gt;), a server nickname, a username (or optionally username#discriminator)</param>
        /// <exception cref="NonSingularResultException"/>
        /// <returns></returns>
        public static async Task<XanBotMember> GetMemberFromDataAsync(DiscordGuild server, string data)
        {
            // Wait! If it's a ping, it will start with <@ and end with >
            string newdata = data;
            if (data.StartsWith("<@") && data.EndsWith(">"))
            {
                newdata = data.Substring(2, data.Length - 3);
            }

            // I don't know if this method is used but it's one Discord supports so I have to support it too.
            if (data.StartsWith("<@!") && data.EndsWith(">"))
            {
                newdata = data.Substring(3, data.Length - 4);
            }

            if (ulong.TryParse(newdata, out ulong uuid))
            {
                try
                {
                    DiscordUser user = await XanBotCoreSystem.Client.GetUserAsync(uuid);

                    // Catch case: Someone's username is a bunch of numbers OR they link a user ID that isn't in the server.
                    if (user != null)
                    {
                        XanBotMember member = XanBotMember.GetMemberFromUser(server, user);
                        if (member != null)
                        {
                            return member;
                        }
                    }
                }
                catch (NotFoundException)
                {
                    // The person typed in a number, it doesn't correspond to a discord GUID, so we'll catch the NotFoundException and let the code continue.
                }
            }
            List<XanBotMember> potentialReturns = new List<XanBotMember>();
            string dataLower = data.ToLower();

            // Discriminator searching:
            if (dataLower.Length == 5 && dataLower.First() == '#' && ushort.TryParse(dataLower.Substring(1), out ushort _))
            {
                // This is a discriminator -- Length is 5, starts with #, and the last 4 chars are numbers.
                foreach (DiscordUser user in server.Members.Values)
                {
                    string ud = "#" + user.Discriminator;
                    if (dataLower == ud)
                    {
                        potentialReturns.Add(XanBotMember.GetMemberFromUser(server, user));
                    }
                }
            }

            if (potentialReturns.Count == 0)
            {
                // ONLY if discriminator searching found nothing will we search by display name or username.
                if (NeedsNewMemberCache)
                {
                    LastCheckTime = DateTime.Now;
                    Members = await server.GetAllMembersAsync();
                }
                foreach (DiscordMember member in Members)
                {
                    //foreach (DiscordMember member in server.Members.Values) { // DO NOT USE THIS. It's not fully populated in large servers.
                    string fullName = member.Username + "#" + member.Discriminator;
                    string nickName = member.Nickname ?? "";

                    fullName = fullName.ToLower();
                    nickName = nickName.ToLower();
                    if (nickName.Length >= dataLower.Length && dataLower == nickName.Substring(0, dataLower.Length))
                    {
                        potentialReturns.Add(XanBotMember.GetMemberFromUser(server, member));
                    }
                    else if (fullName.Length >= dataLower.Length && dataLower == fullName.Substring(0, dataLower.Length))
                    {
                        potentialReturns.Add(XanBotMember.GetMemberFromUser(server, member));
                    }

                    // Do NOT break if there are multiple. This is necessary for the values in a potential NonSingularResultException
                }
            }

            XanBotMember[] potentialReturnsArray = potentialReturns.ToArray();
            if (potentialReturnsArray.Length == 0)
            {
                return null;
            }
            else if (potentialReturnsArray.Length == 1)
            {
                return potentialReturnsArray[0];
            }
            else
            {
                throw new NonSingularResultException(string.Format("More than one member of the server was found with the search query `{0}`!", data), potentialReturnsArray);
            }
        }

        /// <summary>
        /// Strictly gets a user from their user ID. Unlike <see cref="GetMemberFromDataAsync(DiscordGuild, string)"/>, this code will return null if a name or discriminator is passed in.
        /// </summary>
        /// <param name="data">The query to get a XanBotMember from. This can either be a ulong as a string or a user ping (&lt;@ulong&gt;)</param>
        /// <returns></returns>
        public static async Task<XanBotMember> GetMemberFromDataIDStrictAsync(DiscordGuild server, string data)
        {
            string newdata = data;
            if (data.StartsWith("<@") && data.EndsWith(">"))
            {
                newdata = data.Substring(2, data.Length - 3);
            }
            if (ulong.TryParse(newdata, out ulong uuid))
            {
                DiscordUser user = await XanBotCoreSystem.Client.GetUserAsync(uuid);

                // Catch case: Someone's username is a bunch of numbers OR they link a user ID that isn't in the server.
                if (user != null)
                {
                    XanBotMember member = XanBotMember.GetMemberFromUser(server, user);
                    if (member != null)
                    {
                        return member;
                    }
                }
            }
            return null;
        }

    }
}
