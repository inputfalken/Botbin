using System;
using System.Collections.Generic;
using System.Linq;
using Botbin.UserTracking.UserEvent.Enums;
using Discord;

namespace Botbin.UserTracking.UserEvent.Implementations {
    internal class UserLog : IUserEvent {
        public UserLog(IUser user, DateTime time, UserAction type) {
            Time = time;
            Action = type;
            Id = user.Id;
            Username = user.Username;
        }

        public UserLog(IUser user, UserAction type) : this(user, DateTime.Now, type) { }

        public DateTime Time { get; }

        public UserAction Action { get; }

        public ulong Id { get; }

        public string Username { get; }

        public static IEnumerable<IUser> MapToUser(IEnumerable<IUser>users, IEnumerable<IUserEvent> userEvents) =>
            userEvents.Join(users, ue => ue.Id, u => u.Id, (ue, u) => u);
    }
    internal sealed class UserMessage : UserLog {
        public UserMessage(IUser user, UserAction type, string message) : base(user, type) => Message = message;
        public string Message { get; }
    }

    internal sealed class UserGame : UserLog {
        public UserGame(IUser user, UserAction type, Game game) : base(user, type) => Game = game;
        public Game Game { get; }
    }
}