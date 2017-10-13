using System.Threading.Tasks;
using Discord;

namespace Botbin.GameTracking.UserEvent {
    internal interface IUserListener {
        Task Listen(IUser before, IUser after);
    }
}