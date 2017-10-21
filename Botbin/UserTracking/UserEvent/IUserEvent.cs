using System;
using Botbin.UserTracking.UserEvent.Enums;

namespace Botbin.UserTracking.UserEvent {
    public interface IUserEvent {
        ulong Id { get; }
        DateTime Time { get; }
        UserAction Action { get; }
        string Username { get; }
    }
}