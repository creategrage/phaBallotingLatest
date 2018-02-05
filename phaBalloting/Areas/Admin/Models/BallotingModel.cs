using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Soothsilver.Random;
using phaBalloting.Data;


namespace phaBalloting.Areas.Admin.Models
{
    public class BallotingModel
    {
        private phaEntities db = new phaEntities();
        public bool AllocationBallots(List<Member> memberList, IEnumerable<PojectUnit> projectUnitList,double percentageToBeSet,int eventId)
        {
            int pid = 0;

            try
            {
                List<Balloting> ballot = new List<Balloting>();
                int j = projectUnitList.Count();

                //int i = 0;

                foreach (var pu in projectUnitList)
                {
                    int i = 0;
                    while (i < j)
                    {
                        Member m = R.GetRandom(memberList);
                        while (!this.MemeberExist(m))
                        {
                            Balloting b = new Balloting();
                            pid = int.Parse(pu.PojectId.ToString());
                            b.EventID = eventId;
                            b.MemberID = m.Id;
                            b.ProjectUnitID = pu.Id;
                            ballot.Add(b);
                            db.Ballotings.Add(b);
                            db.SaveChanges();
                            memberList.Remove(m);
                            i = j;
                        }

                        i++;
                    }

                }

                double percentage = percentageToBeSet * j;

                double percentageTobeWaited = Math.Ceiling(percentage);

                for (int i = 0; i < percentageTobeWaited; i++)
                {
                    Member member = R.GetRandom(memberList);
                    
                    if (!db.WaitingMembers.ToList().Any(x => x.MemberID == member.Id))
                    {
                        WaitingMember waitingMember = new WaitingMember();

                        waitingMember.EventID = eventId;
                        waitingMember.ProjectID = pid;
                        waitingMember.MemberID = member.Id;

                        db.WaitingMembers.Add(waitingMember);
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Exception:" + ex.ToString());
            }
            return false;
        }
        public bool MemeberExist(Member member)
        {
            try
            {
                return db.Ballotings.ToList().Any(x => x.MemberID == member.Id);
            }
            catch (Exception ex)
            {
                return false;
            }

            //return false;
        }
        public bool ProjectUnitExist(Balloting balloting)
        {
            try
            {
                return db.Ballotings.ToList().Any(x => x.ProjectUnitID == balloting.ProjectUnitID);
            }

            catch (Exception ex)
            {
                return false;
            }
        }

    }
}