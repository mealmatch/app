using MealMatch.Lib;
using MealMatch.Lib.Storage;
using MealMatch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MealMatch.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> _log;
        private readonly YelpClient _yelp;
        private readonly SessionStore _session;

        public ApiController(ILogger<ApiController> log, YelpClient yelp, SessionStore session)
        {
            _log = log;
            _yelp = yelp;
            _session = session;
        }

        [HttpPost]
        [Route("yelp")]
        public async Task<IActionResult> YelpAsync([FromBody] YelpRequest req)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception(ModelState.ValidationState.ToString());
            }

            _log.LogInformation($"Received yelp search: [{req.Location}] - [{req.Search}]");
            var result = await _yelp.GetRestaurantsAsync(req.Location, req.Search, req.Page);
            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRequest req)
        {
            var session = await _session.CreateSessionAsync(req);
            var url = Url.ActionLink("Results", "Home", new { SessionId = session.Id });
            return Ok(new { url });
        }

        [HttpPost]
        [Route("vote")]
        public async Task<IActionResult> VoteAsync([FromBody] VoteRequest req)
        {
            var session = await _session.GetSessionAsync(req.SessionId);
            var voter = await _session.GetVoterAsync(session, req.UserId);
            var item = await _session.GetItemAsync(session, req.ItemId);
            await _session.AddVoteAsync(session, item, voter, req.Vote);
            
            item.YayVotes += req.Vote ? 1 : 0;
            item.NayVotes += req.Vote ? 0 : 1;
            await _session.UpdateAsync(item);
            return Ok(new { success = true });
        }

        [HttpGet]
        [Route("summary/{sessionId}")]
        public async Task<IActionResult> SummaryAsync(Guid sessionId)
        {
            var session = await _session.GetSessionAsync(sessionId);
            var items = await _session.GetItemsAsync(session);
            var result = from i in items
                         orderby i.YayVotes descending
                         select new { i.Name, i.YayVotes, i.NayVotes };
            
            return Ok(result.ToList());
        }
    }
}
