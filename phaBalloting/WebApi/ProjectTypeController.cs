using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace phaBalloting.WebApi
{
    public class ProjectTypeController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        [Route("api/project-type/get-attributes/{id}")]
        public object[] GetAttributesByTypeId(int id)
        {
            Data.phaEntities db = new Data.phaEntities();
            var list = db.ProjectTypeConfigurations.Where(w => w.AttributesType.IsActive && !w.AttributesType.IsDeleted && w.PojectTypeId == id).Select(s => new {Id= s.AttributeTypeId, s.AttributesType.AttributeName }).ToArray();
            return list;
        }
    }
}