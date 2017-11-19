using System;
using System.Collections.Generic;
using System.Linq;
using Botbin.Services.UserTracking.UserEvent.Enums;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static System.Enum;

namespace Botbin.Services.UserTracking.UserEvent.Implementations {
    internal class UserLog : IUserEvent {
        internal UserLog(IUser user, DateTime occurrence, UserAction type) {
            Occurrence = occurrence;
            Action = type;
            Id = user.Id;
            Username = user.Username;
            Status = user.Status;
        }

        [JsonConstructor]
        protected UserLog(string action, DateTime occurrence, ulong id, string status, string username) {
            if (TryParse<UserAction>(action, out var userAction)) Action = userAction;
            Occurrence = occurrence;
            Id = id;
            if (TryParse<UserStatus>(status, out var userStatus)) Status = userStatus;
            Username = username;
        }

        public UserLog(IUser user, UserAction type) : this(user, DateTime.Now, type) { }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserStatus Status { get; }

        [JsonProperty("occurrence ")]
        public DateTime Occurrence { get; }

        [JsonProperty("action")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserAction Action { get; }

        [JsonProperty("id")]
        public ulong Id { get; }

        [JsonProperty("username")]
        public string Username { get; }

        public static IEnumerable<IUser> MapToUser(IEnumerable<IUser>users, IEnumerable<IUserEvent> userEvents) =>
            userEvents.Join(users, ue => ue.Id, u => u.Id, (ue, u) => u);
    }

    internal sealed class UserMessage : UserLog {
        public UserMessage(IUser user, UserAction type, string message) : base(user, type) => Message = message;

        public UserMessage(IMessage message) :
            base(message.Author, message.CreatedAt.DateTime, UserAction.SendMessage) {
            Message = message.Content;
            Channel = message.Channel.Name;
        }

        [JsonConstructor]
        private UserMessage(string action, DateTime occurrence, ulong id,
            string status, string username, string message, string channel) : base(action, occurrence, id,
            status,
            username) {
            Message = message;
            Channel = channel;
        }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("channel")]
        public string Channel { get; }
    }

    internal sealed class UserGame : UserLog {
        public UserGame(IUser user, UserAction type, Game game) : base(user, type) => Game = game.Name;


        [JsonProperty("game")]
        public string Game { get; }
    }
}