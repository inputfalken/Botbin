using System.Threading.Tasks;
using Discord;

namespace Botbin.UserTracking {
    internal interface IUserListener {
        Task Listen(IUser before, IUser after);
    }
}