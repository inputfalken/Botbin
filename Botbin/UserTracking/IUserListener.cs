using System.Threading.Tasks;
using Discord;

namespace Botbin.UserTracking {
    internal interface IUserListener {
        Task ListenForGames(IUser before, IUser after);
        Task ListenForLoginsAndLogOuts(IUser before, IUser after);
        Task ListenForMessages(IMessage message);
    }
}