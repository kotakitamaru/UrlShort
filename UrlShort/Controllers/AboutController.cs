using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UrlShort.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private const string _fileName = "About.txt";
        // GET: api/About
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            if (!System.IO.File.Exists(_fileName))
            {
                return Ok("");
            }
            StreamReader stream = new StreamReader(_fileName, true);
            var aboutText = await stream.ReadToEndAsync();
            stream.Close();
            return Ok(aboutText);
        }
        
        // POST: api/About
        public class AboutTextClass
        {
            public string AboutText { get; set; }
        }
        
        [Authorize(Policy = "Admin")]
        [HttpPost]
        public ActionResult Post(AboutTextClass value)
        {
            try
            {
                if (!System.IO.File.Exists(_fileName))
                {
                    System.IO.File.Create(_fileName);
                }

                StreamWriter stream = new StreamWriter(_fileName);
                stream.WriteLine(value.AboutText);
                stream.Close();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
