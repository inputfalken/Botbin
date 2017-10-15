using System;
using Botbin.UserTracking.UserEvent.Enums;
using Discord;

namespace Botbin.UserTracking.UserEvent {
    public interface IUserEvent : IUser {
        DateTime Time { get; }
        UserAction Action { get; }
        string Description { get; }
    }
}