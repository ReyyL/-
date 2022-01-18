using Kindergarten.Helpers;
using Kindergarten.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kindergarten
{
    public class MentorContext
    {
        public List<Group> Groups { get; set; }
        public User Mentor { get; set; }
        public Dictionary<GroupType, List<Group>> GroupsByType { get; set; }
        public string PointsJson = "";

        #region Diagramma 
        public int DiagrammaMonth;
        public DateTime DiagrammaDate;
        public int SelectedGroupId;
        public int SelectedRoleId;
        public int SelectedMetricGroupId;
        #endregion
        public MentorContext(User mentor, int month, bool withoutDiagramma = true)
        {
            Mentor = mentor;
            DiagrammaMonth = month;

            if (withoutDiagramma)
            {
                GetDefaultData();
            }
            GetMentorData();
        }
        private void GetDefaultData()
        {
            DiagrammaDate = DateTime.Now;
            SelectedGroupId = Database.Groups.First().Id;
            SelectedMetricGroupId = Database.MetricGroups.First().Id;
            SelectedRoleId = Mentor.Role.Id;
            DiagrammaMonth = DiagrammaDate.Month;
        }

        private void GetMentorData()
        {
            GroupsByType = new Dictionary<GroupType, List<Group>>();

            if (Mentor.Role.Id == 4)
            {
                Groups = Database.Groups.Where(x => x.Mentor.Id == Mentor.Id).ToList();
            }
            else
            {
                Groups = Database.Groups;
            }

            var types = Database.GroupTypes.Where(x => Groups.Select(x => x.Type.Id).Contains(x.Id)).Distinct();

            foreach (var type in types)
            {
                GroupsByType.Add(type, Groups.Where(x => x.Type.Id == type.Id).ToList());
            }
        }

    }
}
