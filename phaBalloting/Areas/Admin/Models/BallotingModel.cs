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
        public bool AllocationBallots(List<Member> memberList, IEnumerable<PojectUnit> projectUnitList, double percentageToBeSet, int eventId)
        {

            int UnitCountToBallot = projectUnitList.Count();
            List<Balloting> ToSave = new List<Balloting>();
            foreach (var projectUnit in projectUnitList)
            {
                int i = 0;
                while (i < UnitCountToBallot)
                {
                    Member randomMember = R.GetRandom(memberList);
                    while (!this.MemeberExist(randomMember) && !ToSave.Any(x => x.MemberID == randomMember.Id))
                    {
                        Balloting b = new Balloting();
                        b.EventID = eventId;
                        b.MemberID = randomMember.Id;
                        b.ProjectUnitID = projectUnit.Id;
                        ToSave.Add(b);
                        memberList.Remove(randomMember);
                        i = UnitCountToBallot;
                    }

                    i++;
                }

            }
            try
            {
                db.Ballotings.AddRange(ToSave);
                db.SaveChanges();

                double percentage = percentageToBeSet * UnitCountToBallot;

                double percentageTobeWaited = Math.Ceiling(percentage);
                List<WaitingMember> ToSaveWaiting = new List<WaitingMember>();
                for (int i = 0; i < percentageTobeWaited; i++)
                {
                    Member member = R.GetRandom(memberList);

                    if (!db.WaitingMembers.ToList().Any(x => x.MemberID == member.Id))
                    {
                        WaitingMember waitingMember = new WaitingMember();

                        waitingMember.EventID = eventId;
                        waitingMember.ProjectID = projectUnitList.FirstOrDefault().PojectId;
                        waitingMember.MemberID = member.Id;
                        ToSaveWaiting.Add(waitingMember);
                    }
                }
                try
                {
                    db.WaitingMembers.AddRange(ToSaveWaiting);
                    db.SaveChanges();
                    return true;
                }
                catch
                {

                }
            }
            catch
            {
            }
            return false;

        }
        public bool MemeberExist(Member member)
        {
            try
            {
                return db.Ballotings.ToList().Any(x => x.MemberID == member.Id);
            }
            catch
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

            catch
            {
                return false;
            }
        }

    }
}