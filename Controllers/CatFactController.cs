using CatFact.Applications.Interfaces;
using CatFact.Infrastructures.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CatFact.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CatFactController : ControllerBase
    {
        private readonly ILogger<CatFactController> _logger;
        private readonly ICatFactService _catFactService;
        private readonly IMemoizationService _memoize;
        public CatFactController(ILogger<CatFactController> logger, ICatFactService catFactService, IMemoizationService memoize)
        {
            _logger = logger;
            _catFactService = catFactService;
            _memoize = memoize;
        }

        [HttpGet(Name = "GetCatFact")]
        public async Task<IActionResult> GetCatFact()
        {
            var rnd = new Random();
            var option = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            };

            var randomCatFact =  await _memoize.MemoizeAsync(option, ()=> _catFactService.GetRandomFactAsync(rnd.Next(10,100)));

            return Ok(randomCatFact);
        }
    }
}