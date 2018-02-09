using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace phaBalloting.WebApi
{
    public class BallotingController : ApiController
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

        [Route("api/balloting/get-events")]
        public object[] GetAllEvents()
        {
            Data.phaEntities db = new Data.phaEntities();
            var list = db.Events.Select(s => new { s.Id, s.EventName }).ToArray();
            return list;
        }

        [Route("api/balloting/get-unballoted-projects")]
        public object[] GetUnballotedProjects()
        {
            Data.phaEntities db = new Data.phaEntities();
            var list = db.Projects.Where(w => w.PojectUnits.Where(u => u.Ballotings.Count > 0).Count() == 0).Select(s => new { s.Id, s.ProjectName }).ToArray();
            return list;
        }

        [Route("api/balloting/get-balloted-projects-for-events/{eventId}")]
        public object[] GetBallotedProjectsForEvents(int eventId)
        {
            Data.phaEntities db = new Data.phaEntities();
            var list = db.Ballotings.Where(w => w.EventID == eventId).Select(s => new { s.PojectUnit.Project.Id, s.PojectUnit.Project.ProjectName }).Distinct().ToArray();
            return list;
        }
        [Route("api/balloting/get-events-status-for-project/{eventId}/{projectId}")]
        public string GetProjectStatusForEvents(int eventId, int projectId)
        {
            Data.phaEntities db = new Data.phaEntities();
            var project = db.Projects.Where(w => w.Id == projectId).FirstOrDefault();
            var events = db.Events.Where(w => w.Id == eventId).FirstOrDefault();
            if (project == null)
            {
                return "no-project";
            }

            if (events == null)
            {
                return "no-event";
            }

            if (project.PojectUnits.Count == 0)
            {
                return "no-unit";
            }
            var data = db.Ballotings.Where(w => w.EventID == eventId && w.PojectUnit.PojectId == projectId );

            if (data.Count() == 0)
                return Helpers.EnumManager.ballotinTypes.run.ToString();
            else
            {
                var projectUnits = db.Projects.Where(w => w.Id == projectId).FirstOrDefault().PojectUnits.Count();
                if (data.Where(w => w.CancelledBallotings.Count == 0).Count() == projectUnits)
                {
                    return Helpers.EnumManager.ballotinTypes.rerun.ToString();
                }
                else
                {
                    return Helpers.EnumManager.ballotinTypes.runwaiting.ToString();
                }
            }
        }
      
    }
}