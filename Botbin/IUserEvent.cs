using System;
using Discord;

namespace Botbin {
    internal interface IUserEvent : IUser {
        DateTime Time { get; }
        UserEventType Type { get; }
    }
}