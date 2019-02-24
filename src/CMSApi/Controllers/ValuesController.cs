using CMSApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Common;
namespace CMSApi.Controllers
{
    public class ValuesController : ApiController
    {
    
        // GET api/values
        public IEnumerable<string> Get()
        {
           
            return new string[] { "value1", "value2" };
        }
        [ApiAuthorize]
        // GET api/values/5
        public string Get(int id)
        {
            AuthInfo info = RequestContext.RouteData.Values["auth"] as AuthInfo;
            return info.UserName;
        }
        [OnlyAuthorize]
        // POST api/values
        public string Post([FromBody]string value)
        {
            return RedisHelper.StringGet("admin");
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
