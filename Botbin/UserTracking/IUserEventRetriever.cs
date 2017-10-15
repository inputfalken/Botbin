using System.Collections.Generic;
using Botbin.UserTracking.UserEvent;

namespace Botbin.UserTracking {
    public interface IUserEventRetriever {
        IEnumerable<IUserEvent> UserEventsById(ulong id);
        IEnumerable<IUserEvent> UserEvents();
    }
}