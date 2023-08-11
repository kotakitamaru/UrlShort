using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using UrlShort.Models;

namespace UrlShort.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class ShortUrlController : ControllerBase
    {
        // GET: api/ShortUrl
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<UrlInfo>> Get()
        {
            using var context = new CosmosDBContext();
            return Ok(context.UrlsInfos.ToList());
        }

        // GET: api/ShortUrl/5
        [AllowAnonymous]
        [HttpGet("fullLink/{link}")]
        public ActionResult<UrlInfo> Get(string link)
        {
            using var context = new CosmosDBContext();
            var url = context.UrlsInfos.FirstOrDefault(x => x.ShortUrl == link);
            return url == null ? NotFound() : Ok(url);
        }
        
        // POST: api/ShortUrl
        [HttpPost]
        public async Task<ActionResult<UrlInfo>> Post([FromBody] UrlInfoDto url)
        {
            using (var context = new CosmosDBContext())
            {
                if (await context.UrlsInfos?.Where(x=>x.FullUrl == url.FullUrl).CountAsync() >= 1)
                {
                    return Conflict();
                }
                uint number = 1;
                if (await context.UrlsInfos?.CountAsync() != 0)
                    number = context.UrlsInfos.Max(x => x.Number) + 1;
                
                var newUrl = new UrlInfo
                {
                    Number = number,
                    FullUrl = url.FullUrl,
                    ShortUrl = Convert.ToBase64String(BitConverter.GetBytes(number)),
                    Author = url.Author
                };
                context.UrlsInfos?.Add(newUrl);
                await context.SaveChangesAsync();
                return Ok(newUrl);
            }
        }

        // DELETE: api/ShortUrl/5
        [HttpDelete("{shortUrl}")]
        public async Task<ActionResult> Delete(string shortUrl)
        {
            await using var context = new CosmosDBContext();
            context.UrlsInfos?.RemoveRange(context.UrlsInfos.Where(x=>x.ShortUrl == shortUrl));
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
