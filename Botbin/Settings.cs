using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Botbin {
    internal class Settings {
        public Settings(char commandPrefix, string botToken, ulong adminId, IEnumerable<ulong> adminsIds = null) {
            CommandPrefix = commandPrefix;
            BotToken = botToken;
            AdminsIds = adminsIds == null
                ? new List<ulong> {adminId}
                : new List<ulong>(adminsIds) {adminId};
        }

        public Settings(char commandPrefix, string botToken, IUser admin, IEnumerable<IUser> admins = null) : this(
            commandPrefix, botToken, admin.Id, admins?.Select(user => user.Id)) { }

        public char CommandPrefix { get; }

        public string BotToken { get; }
        private List<ulong> AdminsIds { get; }

        public bool IsAdmin(IUser user) => AdminsIds.Contains(user.Id);

        public void AddAdmin(IUser user) => AdminsIds.Add(user.Id);
    }
}