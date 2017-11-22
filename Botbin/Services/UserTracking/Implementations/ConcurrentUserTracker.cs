using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.Services.Interfaces;
using Botbin.Services.UserTracking.UserEvent;
using Botbin.Services.UserTracking.UserEvent.Implementations;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Optional;
using Optional.Linq;
using static Botbin.Services.UserTracking.UserEvent.Enums.UserAction;
using static Discord.UserStatus;

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
            var state = (before: before, after:after);
            var startGame = Option.Some((action: StartGame, state: state))
                .Where(t => !t.state.before.Game.HasValue)
                .Where(t => t.state.after.Game.HasValue)
                .Select(t => new UserGame(t.state.after, t.action, t.state.after.Game.Value));

            var quitGame = Option.Some((action: QuitGame, state: state))
                .Where(t => t.state.before.Game.HasValue)
                .Where(t => !t.state.after.Game.HasValue)
                .Select(t => new UserGame(t.state.after, t.action, t.state.before.Game.Value));

            NotHuman(before)
                .NoneWhen(b => b)
                .SelectMany(b => startGame.Else(quitGame))
                .MatchSome(Save);

            return Task.CompletedTask;
        }

        public Task ListenForMessages(IMessage message) {
            message.SomeWhen(m => !NotHuman(m.Author))
                .Where(m => !Command(m.Content))
                .Select(m => new UserMessage(m))
                .MatchSome(Save);
            return Task.CompletedTask;
        }

        public Task ListenForStatus(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            // There is probably much more states to take into account for proper tracking. :)
            // Tracking invis cant be done since arguments `before` and `after` understands it as going online/offline.
            if (before.Status != DoNotDisturb && after.Status == DoNotDisturb)
                Save(new UserLog(after, EnableDoNotDisturbMode));
            if (before.Status == DoNotDisturb && after.Status != DoNotDisturb)
                Save(new UserLog(after, DisableDoNotDisturbMode));
            if (before.Status != Idle && after.Status == Idle)
                Save(new UserLog(after, EnableIdle));
            if (before.Status == Idle && after.Status != Idle)
                Save(new UserLog(after, DisableIdle));
            // Logging on automatically makes you in online state.
            if (before.Status == Offline && after.Status == Online)
                Save(new UserLog(after, LogIn));
            // Just going offline from any state means you logged off.
            if (before.Status != Offline && after.Status == Offline)
                Save(new UserLog(after, LogOff));

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