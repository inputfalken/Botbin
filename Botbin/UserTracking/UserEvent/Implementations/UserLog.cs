using System;
using System.Collections.Generic;
using System.Linq;
using Botbin.UserTracking.UserEvent.Enums;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Botbin.UserTracking.UserEvent.Implementations {
    internal class UserLog : IUserEvent {
        public UserLog(IUser user, DateTime time, UserAction type) {
            Time = time;
            Action = type;
            Id = user.Id;
            Username = user.Username;
            Status = user.Status;
        }

        public UserLog(IUser user, UserAction type) : this(user, DateTime.Now, type) { }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserStatus Status { get; }

        [JsonProperty("time")]
        public DateTime Time { get; }

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