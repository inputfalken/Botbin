namespace Botbin {
    internal class Settings {
        public char CommandPrefix { get; }

        public string BotToken { get; }

        public Settings(char commandPrefix, string botToken) {
            CommandPrefix = commandPrefix;
            BotToken = botToken;
        }
    }
}