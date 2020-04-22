using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext context;
        public ValuesController(DataContext _context)
        {
            context = _context;
        }

        // GET api/values
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetValues()
        {
            var values = context.Values.ToList();
            return Ok(values);
        }

        // GET api/values/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetValue(int id)
        {
            var value = context.Values.FirstOrDefault(x => x.Id == id);
            return Ok(value);
        }

        //// GET api/values
        //[AllowAnonymous]
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> GetValues()
        //{
        //    return new string[] {"val1", "val2  " };
        //}   

        //// GET api/values/5
        //[AllowAnonymous]
        //[HttpGet("{id}")]
        //public ActionResult<string> GetValue(int id)
        //{
        //    return "Value";
        //}

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
