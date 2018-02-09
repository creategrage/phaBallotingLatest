namespace phaBalloting.Helpers
{
    public static class EnumManager
    {
        public enum Modules {
            Events=1,
            Projects=2,
            ProjectUnits=3,
            Balloting=4,
            ProjectType=5,
            ProjectConfiguration=6,
            Members=7, Ballotings=8
        }
        public enum Actions
        {
            ViewRecords = 1,
            EditRecords = 2,
            DeleteRecords = 3,
            AddRecords = 4,
            Any=5
        }
        public enum ballotinTypes
        {
            run, rerun, runwaiting
        }
    }
}