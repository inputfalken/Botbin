using System;
using Botbin.GameTracking.UserEvent.Enums;
using Discord;

namespace Botbin.GameTracking.UserEvent {
    internal interface IUserEvent : IUser {
        DateTime Time { get; }
        UserAction Action { get; }
    }
}