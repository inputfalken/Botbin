using System.Collections.Generic;
using Botbin.Services.UserTracking.UserEvent;

namespace Botbin.Services.UserTracking {
    public interface IUserEventRetriever {
        IEnumerable<IUserEvent> UserEventsById(ulong id);
        IEnumerable<IUserEvent> UserEvents();
    }
}