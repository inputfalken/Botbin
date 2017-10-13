using Botbin.GameTracking.UserEvent;

namespace Botbin.GameTracking {
    internal interface IGameTracker : IUserListener, IUserEventRetriever { }
}