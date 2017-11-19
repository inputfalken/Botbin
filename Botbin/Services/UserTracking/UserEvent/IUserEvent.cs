using System;
using Botbin.Services.UserTracking.UserEvent.Enums;

namespace Botbin.Services.UserTracking.UserEvent {
    public interface IUserEvent {
        ulong Id { get; }
        DateTime Occurrence { get; }
        UserAction Action { get; }
        string Username { get; }
    }
}