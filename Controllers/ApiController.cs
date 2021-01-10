using MealMatch.Lib;
using MealMatch.Lib.Storage;
using MealMatch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
    }
}
