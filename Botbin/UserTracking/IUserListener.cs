using System.Threading.Tasks;
using Discord;

namespace Botbin.UserTracking {
    public interface IUserListener {
        Task ListenForGames(IUser before, IUser after);
        Task ListenForLoginsAndLogOuts(IUser before, IUser after);
        Task ListenForMessages(IMessage message);
    }
}