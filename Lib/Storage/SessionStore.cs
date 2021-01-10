using MealMatch.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealMatch.Lib.Storage
{
    public class SessionStore
    {
        private static string SessionPK(Guid sessionId) => $"Session_{sessionId}";
        private static string SessionRK(Guid sessionId) => $"{sessionId}";
        private static string ItemPK(Guid sessionId) => $"SessionItems_{sessionId}";
        private static string ItemRK(int itemId) => $"{itemId:D6}";
        private static string VoterPK(Guid sessionId) => $"Voters_{sessionId}";
        private static string VoterRK(Guid voterId) => $"{voterId}";
        private static string VotePK(Guid sessionId) => $"SessionVotes_{sessionId}";
        private static string VoteRK(Guid voterId, int itemId) => $"{voterId}_{itemId:D6}";
        private static string VoteLinkPK(Guid linkId) => $"VoteLinks_{linkId}";
        private static string VoteLinkRK(Guid linkId) => $"{linkId}";

        private readonly Store<Session> _sessions;
        private readonly Store<SessionItem> _items;
        private readonly Store<SessionVote> _votes;
        private readonly Store<Voter> _voters;
        private readonly Store<VoteLink> _links;

        public SessionStore(IConfiguration cfg)
        {
            _sessions = new Store<Session>(cfg);
            _items = new Store<SessionItem>(cfg);
            _votes = new Store<SessionVote>(cfg);
            _voters = new Store<Voter>(cfg);
            _links = new Store<VoteLink>(cfg);
        }

        public async Task<Session> CreateSessionAsync(CreateRequest req)
        {
            var sessionId = Guid.NewGuid();
            var linkId = Guid.NewGuid();
            var session = new Session
            {
                PartitionKey = SessionPK(sessionId),
                RowKey = SessionRK(sessionId),
                Id = sessionId,
                VoteLinkId = linkId,
                Name = req.Name,
                NumItems = req.Options.Count,
                NumVotesToWin = req.NumVotesToWin,
                NumWinsToEnd = req.NumWinsToEnd,
            };
            await _sessions.CreateAsync(session);

            var link = new VoteLink
            {
                PartitionKey = VoteLinkPK(linkId),
                RowKey = VoteLinkRK(linkId),
                Id = linkId,
                SessionId = sessionId,
            };
            await _links.CreateAsync(link);

            if (req.Shuffle)
            {
                var r = new Random();
                req.Options = req.Options.OrderBy(i => r.Next()).ToList();
            }

            await Task.WhenAll(GetCreateItemTasks(session, req.Options));
            return session;
        }

        public async Task<Session> GetSessionFromLinkAsync(Guid linkId)
        {
            var link = await _links.GetAsync(VoteLinkPK(linkId), VoteLinkRK(linkId));
            return await GetSessionAsync(link.SessionId);
        }

        public async Task<Session> GetSessionAsync(Guid id)
        {
            return await _sessions.GetAsync(SessionPK(id), SessionRK(id));
        }

        public async Task<SessionItem> GetItemAsync(Session session, int itemId)
        {
            return await _items.GetAsync(ItemPK(session.Id), ItemRK(itemId));
        }

        public async Task UpdateAsync(SessionItem item)
        {
            await _items.UpdateAsync(item);
        }

        public async Task<List<SessionItem>> GetItemsAsync(Session session)
        {
            return await _items.GetPartitionQuery(ItemPK(session.Id)).ToListAsync();
        }

        public async Task AddVoteAsync(Session session, SessionItem item, Voter voter, bool value)
        {
            var vote = new SessionVote
            {
                PartitionKey = VotePK(session.Id),
                RowKey = VoteRK(voter.Id, item.Id),
                ItemId = item.Id,
                UserId = voter.Id,
                SessionId = session.Id,
                Value = value,
            };
            await _votes.CreateAsync(vote);

            item.YayVotes += value ? 1 : 0;
            item.NayVotes += value ? 0 : 1;
            await _items.UpdateAsync(item);

            session.NumWins += (value && item.YayVotes == session.NumVotesToWin) ? 1 : 0;
            session.NumVoters += item.Id == 0 ? 1 : 0;
            await _sessions.UpdateAsync(session);
        }

        public async Task<List<SessionVote>> GetVotesAsync(Session session)
        {
            return await _votes.GetPartitionQuery(VotePK(session.Id)).ToListAsync();
        }

        public async Task<Voter> CreateVoterAsync(Session session, string name)
        {
            var id = Guid.NewGuid();
            var voter = new Voter
            {
                PartitionKey = VoterPK(session.Id),
                RowKey = VoterRK(id),
                Id = id,
                Name = name,
            };
            await _voters.CreateAsync(voter);
            return voter;
        }

        public async Task<Voter> GetVoterAsync(Session session, Guid id)
        {
            return await _voters.GetAsync(VoterPK(session.Id), VoterRK(id));
        }

        public async Task<List<Voter>> GetVotersAsync(Session session)
        {
            return await _voters.GetPartitionQuery(VoterPK(session.Id)).ToListAsync();
        }

        private IEnumerable<Task> GetCreateItemTasks(Session session, IEnumerable<OptionItem> items)
        {
            var pk = ItemPK(session.Id);
            int idx = 0;
            foreach (var item in items)
            {
                yield return CreateItemAsync(session, item, pk, idx++);
            }
        }

        private Task CreateItemAsync(Session session, OptionItem item, string pk, int idx)
        {
            var option = new SessionItem
            {
                PartitionKey = pk,
                RowKey = ItemRK(idx),
                Id = idx,
                SessionId = session.Id,
                Name = item.Name,
                Type = item.Type,
                YelpId = item.YelpId,
                Url = item.Url,
                ImageUrl = item.ImageUrl,
            };
            return _items.CreateAsync(option);
        }
    }
}
