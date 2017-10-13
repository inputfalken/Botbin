using System.Threading.Tasks;
using Discord;

namespace Botbin.GameTracking {
    internal interface IUserListener {
        Task Listen(IUser before, IUser after);
    }
}