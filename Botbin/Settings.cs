using System.Collections.Generic;
using System.Linq;

namespace Botbin {
    internal class Settings {
        public char CommandPrefix { get; }

        public string BotToken { get; }
        private IReadOnlyList<ulong> AdminsIds { get; }

        public Settings(char commandPrefix, string botToken, ulong adminId, IEnumerable<ulong> adminsIds = null) {
            CommandPrefix = commandPrefix;
            BotToken = botToken;
            AdminsIds = adminsIds == null ? new List<ulong> {adminId} : new List<ulong>(adminsIds) {adminId};
        }

        public bool IsAdmin(ulong id) => AdminsIds.Contains(id);
    }
}