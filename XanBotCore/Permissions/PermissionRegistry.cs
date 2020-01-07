using System;
using System.Collections.Generic;
using XanBotCore.DataPersistence;
using XanBotCore.Exceptions;
using XanBotCore.Logging;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.Permissions
{

    /// <summary>
    /// Represents permission levels.
    /// </summary>
    public class PermissionRegistry
    {

        private static readonly Dictionary<BotContext, Dictionary<ulong, byte>> PermissionsInContext = new Dictionary<BotContext, Dictionary<ulong, byte>>();

        /// <summary>
        /// The user ID of the bot's creator.
        /// </summary>
        public const ulong BOT_CREATOR_ID = 114163433980559366;

        /// <summary>
        /// The permission level that represents a user who is not a member of the current guild.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_NONMEMBER = 0;

        /// <summary>
        /// The permission level that represents someone who is blacklisted from using all commands.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_BLACKLISTED = 1;

        /// <summary>
        /// The default permission level of users.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_STANDARD_USER = 2;

        /// <summary>
        /// The permission level of standard users who are trusted with slightly more power than average users.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_TRUSTED_USER = 3;

        /// <summary>
        /// The permission level of operators, which represents users are allowed to control basic bot functions. In general setups, moderators will receive this level.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_OPERATOR = 63;

        /// <summary>
        /// The permission level of administrators, which represents users who are allowed to control advanced bot functions (like shutting down the bot). In general setups, administrators will receive this level.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_ADMINISTRATOR = 127;

        /// <summary>
        /// The permission level of the server owner. It is intended that all commands usable by <see cref="PERMISSION_LEVEL_BACKEND_CONSOLE"/> are also usable by this level.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_SERVER_OWNER = 254;

        /// <summary>
        /// The permission level used by the backend console.<para/>
        /// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
        /// </summary>
        public const byte PERMISSION_LEVEL_BACKEND_CONSOLE = 255;

        /// <summary>
        /// The default permission level of all member objects. Any members that have their permission level set to this value will not be stored in data persistence by this code.<para/>
        /// Throws an <see cref="InvalidOperationException"/> if this is set after the bot has been initialized, as references to commands may have the old value stored in them.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static byte DefaultPermissionLevel
        {
            get
            {
                return DefaultPermissionLevelInternal;
            }
            set
            {
                if (XanBotCoreSystem.Created)
                    throw new InvalidOperationException("Cannot set this value after bot initialization. Set this value before calling XanBotCoreSystem.InitializeBotAsync()");
                DefaultPermissionLevelInternal = value;
            }
        }
        private static byte DefaultPermissionLevelInternal = PERMISSION_LEVEL_STANDARD_USER;

        /// <summary>
        /// References the data store for user permissions and gets the user's associated permission level. Returns <see cref="DefaultPermissionLevel"/> if the value does not exist.
        /// </summary>
        /// <param name="userId">The ID of the user to get permissions of.</param>
        /// <param name="context">The bot context to grab the information from.</param>
        /// <exception cref="MalformedConfigDataException"/>
        /// <returns></returns>
        public static byte GetPermissionLevelOfUser(ulong userId, BotContext context)
        {
            XConfiguration cfg = XConfiguration.GetConfigurationUtility(context, "userPerms.permissions");
            string permLvl = cfg.GetConfigurationValue(userId.ToString(), DefaultPermissionLevel.ToString(), reloadConfigFile: true);
            if (byte.TryParse(permLvl, out byte perms))
            {
                // Catch case
                byte returnValue = perms;

                if (context.DefaultUserPermissions.TryGetValue(userId, out byte contextPermissionLevel))
                {
                    if (contextPermissionLevel == 255)
                        XanBotLogger.WriteDebugLine($"Note: The user [{userId}] has permission level 255 for BotContext [{context.Name}], which is intended for use in the backend console only. If any commands rely on 255 being console-only, this may cause problems!");

                    returnValue = contextPermissionLevel;
                }

                return returnValue;
            }
            throw new MalformedConfigDataException("The data stored for the permission level of user [" + userId + "] is malformed. Reason: Could not cast " + permLvl + " into a byte.");
        }

        /// <summary>
        /// Stores the set permission level of this user for the sake of data persistence. This is internal since it is called whenever the property <see cref="XanBotMember.PermissionLevel"/> is set, and should not be called by users.
        /// </summary>
        /// <param name="member">The member to set in the internal cache.</param>
        /// <param name="saveToFileNow">If true, the config file for this context's user permissions will be set.</param>
        internal static void UpdatePermissionLevelOfMember(XanBotMember member, bool saveToFileNow = false)
        {
            if (!PermissionsInContext.ContainsKey(member.Context))
                PermissionsInContext[member.Context] = new Dictionary<ulong, byte>();

            PermissionsInContext[member.Context][member.Id] = member.PermissionLevel;
            if (saveToFileNow)
            {
                SaveContextPermissionsToFile(member.Context);
            }
        }

        internal static void SaveAllUserPermissionsToFile()
        {
            foreach (BotContext context in BotContextRegistry.AllContexts)
            {
                SaveContextPermissionsToFile(context);
            }
        }

        public static void SaveContextPermissionsToFile(BotContext context)
        {
            if (!PermissionsInContext.ContainsKey(context))
                return;
            XConfiguration cfg = XConfiguration.GetConfigurationUtility(context, "userPerms.permissions");
            foreach (ulong id in PermissionsInContext[context].Keys)
            {
                cfg.SetConfigurationValue(id.ToString(), PermissionsInContext[context][id].ToString(), true);
            }
            cfg.SaveConfigurationFile();
        }
    }
}
