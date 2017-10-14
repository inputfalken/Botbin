using System;
using Botbin.GameTracking.UserEvent.Enums;
using Discord;

namespace Botbin.UserTracking.UserEvent {
    internal interface IUserEvent : IUser {
        DateTime Time { get; }
        UserAction Action { get; }
    }
}