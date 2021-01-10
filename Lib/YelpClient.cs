using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MealMatch.Models;
using Microsoft.Extensions.Configuration;

namespace MealMatch.Lib
{
    public class YelpClient 
    {
        private const int PageSize = 20;

        private const string BaseUrl = "https://api.yelp.com/v3";
        private const string SearchPath = "businesses/search";
        private readonly IConfiguration _cfg;

        public YelpClient(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public async Task<List<OptionItem>> GetRestaurantsAsync(string zip, string search, int page = 0)
        {
            var tok = _cfg["yelp"];
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", new[] {$"Bearer {tok}"});

            var term = Uri.EscapeDataString(search);
            var url = $"{BaseUrl}/{SearchPath}?terms=restaurants&location={zip}&term={term}&limit={PageSize}&offset={page * PageSize}";
            using var resp = await http.GetAsync(url);
            
            var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            if (!doc.RootElement.TryGetProperty("businesses", out var businesses))
            {
                return new List<OptionItem>();
            }

            return businesses.EnumerateArray().Select(GetItem).ToList();
        }

        private OptionItem GetItem(JsonElement el)
        {
            return new OptionItem
            {
                Type = "Yelp",
                Name = el.GetProperty("name").GetString(),
                ImageUrl = el.GetProperty("image_url").GetString(),
                Url = el.GetProperty("url").GetString(),
            };
        }
    }
}
