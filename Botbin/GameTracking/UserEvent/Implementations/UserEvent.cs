using System;
using System.Threading.Tasks;
using Botbin.GameTracking.UserEvent.Enums;
using Discord;

namespace Botbin.GameTracking.UserEvent.Implementations {
    internal class UserEvent : IUserEvent {
        private readonly IUser _user;

        public UserEvent(IUser user, DateTime time, UserAction type) {
            _user = user;
            Time = time;
            Action = type;
            Id = user.Id;
            CreatedAt = user.CreatedAt;
            Mention = user.Mention;
            Game = user.Game;
            Status = user.Status;
            AvatarId = user.AvatarId;
            Discriminator = user.Discriminator;
            DiscriminatorValue = user.DiscriminatorValue;
            IsBot = user.IsBot;
            IsWebhook = user.IsWebhook;
            Username = user.Username;
        }

        public UserEvent(IUser user, UserAction type) : this(user, DateTime.Now, type) { }
        public DateTime Time { get; }
        public UserAction Action { get; }

        public ulong Id { get; }

        public DateTimeOffset CreatedAt { get; }

        public string Mention { get; }

        public Game? Game { get; }

        public UserStatus Status { get; }

        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128) =>
            _user.GetAvatarUrl(format, size);

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null) =>
            _user.GetOrCreateDMChannelAsync(options);

        public string AvatarId { get; }

        public string Discriminator { get; }

        public ushort DiscriminatorValue { get; }

        public bool IsBot { get; }

        public bool IsWebhook { get; }

        public string Username { get; }
    }
}