using System.Collections.Generic;
using Botbin.UserTracking.UserEvent;

namespace Botbin.UserTracking {
    internal interface IUserEventRetriever {
        IEnumerable<IUserEvent> UserEventsById(ulong id);
        IEnumerable<IUserEvent> UserEvents();
    }
}