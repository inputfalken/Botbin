using System.Collections.Generic;
using Botbin.GameTracking.UserEvent;

namespace Botbin.GameTracking {
    internal interface IGameTracker {
        IEnumerable<IUserEvent> GetUserEventsById(ulong id);
        IEnumerable<IUserEvent> GetUserEvents();
    }
}