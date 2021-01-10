using MealMatch.Lib.Storage;
using MealMatch.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MealMatch.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly SessionStore _session;

        public HomeController(SessionStore session)
        {
            _session = session;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index() => View();

        [HttpGet]
        [Route("results/{sessionId}")]
        public async Task<IActionResult> ResultsAsync(Guid sessionId)
        {
            try
            {
                var session = await _session.GetSessionAsync(sessionId);
                var items = await _session.GetItemsAsync(session);
                var votes = await _session.GetVotesAsync(session);
                var users = await _session.GetVotersAsync(session);
                return View((session, items, votes, users));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("vote/{linkId}")]
        public async Task<IActionResult> VoteStartAsync(Guid linkId)
        {
            try
            {
                var session = await _session.GetSessionFromLinkAsync(linkId);
                return session.NumWins < session.NumWinsToEnd
                    ? View(session)
                    : RedirectToAction("VoteEnd", new { linkId });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Route("vote/{linkId}")]
        public async Task<IActionResult> VoteStartAsync(Guid linkId, [FromForm] string name)
        {
            try
            {
                var session = await _session.GetSessionFromLinkAsync(linkId);
                var voter = await _session.CreateVoterAsync(session, name);
                return RedirectToAction("VoteGet", new { linkId, userId = voter.Id, itemId = 0 });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("vote/{linkId}/{userId}/{itemId}")]
        public async Task<IActionResult> VoteGetAsync(Guid linkId, Guid userId, int itemId)
        {
            try
            {
                var session = await _session.GetSessionFromLinkAsync(linkId);
                var user = await _session.GetVoterAsync(session, userId);
                var item = await _session.GetItemAsync(session, itemId);
                return View((session, user, item));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Route("vote/{linkId}/{userId}/{itemId}")]
        public async Task<IActionResult> VotePostAsync(Guid linkId, Guid userId, int itemId, [FromForm] bool vote)
        {
            try
            {
                var session = await _session.GetSessionFromLinkAsync(linkId);
                var user = await _session.GetVoterAsync(session, userId);
                var item = await _session.GetItemAsync(session, itemId);
                await _session.AddVoteAsync(session, item, user, vote);

                var continueVoting = ++itemId < session.NumItems && session.NumWins < session.NumWinsToEnd;
                return continueVoting
                    ? RedirectToAction("VoteGet", new { linkId, userId, itemId })
                    : RedirectToAction("VoteEnd", new { linkId });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("end/{linkId}")]
        public async Task<IActionResult> VoteEndAsync(Guid linkId)
        {
            try
            {
                var session = await _session.GetSessionFromLinkAsync(linkId);
                return View(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Route("about")]
        public IActionResult About() => View();

        [Route("privacy")]
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
