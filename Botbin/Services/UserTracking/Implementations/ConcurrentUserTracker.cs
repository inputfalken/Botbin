using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.Services.Interfaces;
using Botbin.Services.UserTracking.UserEvent;
using Botbin.Services.UserTracking.UserEvent.Enums;
using Botbin.Services.UserTracking.UserEvent.Implementations;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Botbin.Services.UserTracking.Implementations {
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
            !_dictionary.TryGetValue(id, out var result) ? Enumerable.Empty<IUserEvent>() : result;

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

        public Task ListenForMessages(IMessage message) {
            var author = message.Author;
            if (NotHuman(author) || Command(message.Content)) return Task.CompletedTask;
            var user = new UserMessage(message);
            Save(user);
            return Task.CompletedTask;
        }

        public Task ListenForStatus(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            // There is probably much more states to take into account for proper tracking. :)
            // Tracking invis cant be done since arguments `before` and `after` understands it as going online/offline.
            if (before.Status != UserStatus.AFK && after.Status == UserStatus.AFK)
                Save(new UserLog(after, UserAction.AwayFromKeyBoardEnabled));
            if (before.Status == UserStatus.AFK && after.Status != UserStatus.AFK)
                Save(new UserLog(after, UserAction.AwayFromKeyBoardDisabled));
            if (before.Status != UserStatus.DoNotDisturb && after.Status == UserStatus.DoNotDisturb)
                Save(new UserLog(after, UserAction.DoNotDisturbEnabled));
            if (before.Status == UserStatus.DoNotDisturb && after.Status != UserStatus.DoNotDisturb)
                Save(new UserLog(after, UserAction.DoNotDistubDisabled));
            if (before.Status != UserStatus.Idle && after.Status == UserStatus.Idle)
                Save(new UserLog(after, UserAction.IdleEnabled));
            if (before.Status == UserStatus.Idle && after.Status != UserStatus.Idle)
                Save(new UserLog(after, UserAction.IdleDisabled));
            // Logging on automatically makes you in online state.
            if (before.Status == UserStatus.Offline && after.Status == UserStatus.Online)
                Save(new UserLog(after, UserAction.LogIn));
            // Just going offline from any state means you logged off.
            if (before.Status != UserStatus.Offline && after.Status == UserStatus.Offline)
                Save(new UserLog(after, UserAction.LogOff));

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