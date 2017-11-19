using System;
using System.Collections.Generic;
using System.Linq;
using Botbin.Services.UserTracking.UserEvent;
using Botbin.Services.UserTracking.UserEvent.Implementations;
using Nest;

namespace Botbin.Services.UserTracking.Implementations {
    public class Elastic : IUserEventRetriever {
        private readonly ElasticClient _elasticClient;
        private readonly DateMathExpression _dateMathExpression;
        private const string Type = "discord";
        private const string UserLogIndex = "discord-user-action-*";
        private const string UserMessageIndex = "discord-user-message-action-*";
        private const string UserGameIndex = "discord-game-action-*";

        public Elastic(Uri address, DateTime minTime) {
            _dateMathExpression = new DateMathExpression(minTime);
            _elasticClient = new ElasticClient(new ConnectionSettings(address));
        }

        public IEnumerable<IUserEvent> UserEventsById(ulong id) => UserEvents().Where(u => u.Id == id);

        public IEnumerable<IUserEvent> UserEvents() {
            try {
                var useractions = _elasticClient.Search<UserLog>(s =>
                    s.Index(UserLogIndex)
                        .Query(q => q.DateRange(d => d.GreaterThanOrEquals(_dateMathExpression)))
                        .Type(Type)
                        .MatchAll()
                );
                var userMessages = _elasticClient.Search<UserMessage>(s =>
                    s.Index(UserMessageIndex)
                        .Query(q => q.DateRange(d => d.GreaterThanOrEquals(_dateMathExpression)))
                        .Type(Type)
                        .MatchAll()
                );
                var userGames = _elasticClient.Search<UserGame>(s =>
                    s.Index(UserGameIndex)
                        .Query(q => q.DateRange(d => d.GreaterThanOrEquals(_dateMathExpression)))
                        .Type(Type)
                        .MatchAll()
                );

                return useractions.Hits
                    .Concat(userMessages.Hits)
                    .Concat(userGames.Hits)
                    .Select(hit => hit.Source);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}