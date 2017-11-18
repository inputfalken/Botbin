using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Enums;
using Botbin.UserTracking.UserEvent.Implementations;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin.UserTracking.Implementations {
    public class ConcurrentUserTracker : IUserListener, IUserEventRetriever {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;
        private readonly ILogger _logger;
        private readonly Settings _settings;

        public ConcurrentUserTracker(IServiceProvider provider) {
            _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();
            _settings = provider.GetService<Settings>();
            _logger = provider.GetService<ILogger>();
        }

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task ListenForGames(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            UserLog userLog;
            if (startGame) userLog = new UserGame(before, UserAction.StartGame, after.Game.Value);
            else if (quitGame) userLog = new UserGame(after, UserAction.QuitGame, before.Game.Value);
            else return Task.CompletedTask;

            Save(userLog);
            return Task.CompletedTask;
        }

        public Task ListenForLoginsAndLogOuts(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var logIn = before.Status == UserStatus.Offline && after.Status == UserStatus.Online;
            var logOff = before.Status == UserStatus.Online && after.Status == UserStatus.Offline;
            UserLog userLog;
            if (logIn) userLog = new UserLog(after, UserAction.LogIn);
            else if (logOff) userLog = new UserLog(after, UserAction.LogOff);
            else return Task.CompletedTask;

            Save(userLog);
            return Task.CompletedTask;
        }

        public Task ListenForMessages(IMessage message) {
            var author = message.Author;
            if (NotHuman(author) || Command(message.Content)) return Task.CompletedTask;
            var user = new UserMessage(message);
            Save(user);
            return Task.CompletedTask;
        }

        private bool Command(string message) => message.StartsWith(_settings.CommandPrefix.ToString());

        private static bool NotHuman(IUser user) => user.IsWebhook || user.IsBot;

        private void Save(IUserEvent user) {
            _logger.Log(user);
            _dictionary.AddOrUpdate(
                user.Id,
                _ => NewKey(user),
                (_, queue) => Update(user, queue)
            );
        }

        private static ConcurrentQueue<IUserEvent> NewKey(IUserEvent user) =>
            Update(user, new ConcurrentQueue<IUserEvent>());

        private static ConcurrentQueue<IUserEvent> Update(IUserEvent user,
            ConcurrentQueue<IUserEvent> events) {
            events.Enqueue(user);
            return events;
        }
    }
}