using System.Collections.Generic;
using Botbin.GameTracking.UserEvent;

namespace Botbin.GameTracking {
    internal interface IUserEventRetriever {
        IEnumerable<IUserEvent> UserEventsById(ulong id);
        IEnumerable<IUserEvent> UserEvents();
    }
}