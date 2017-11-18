using System.Threading.Tasks;
using Discord;

namespace Botbin.UserTracking {
    public interface IUserListener {
        Task ListenForGames(IUser before, IUser after);
        Task ListenForMessages(IMessage message);
        Task ListenForStatus(IUser before, IUser after);
    }
}