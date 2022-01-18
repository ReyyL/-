using Kindergarten.Helpers;
using Kindergarten.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kindergarten.Contexts
{
    public class GroupContext
    {
        public User User { get; set; }
        public Group Group { get; set; }
        public Dictionary<Child, Dictionary<string, float>> Averages { get; set; }
        public DateTime Date { get; set; }
        public int MetricGroup { get; set; }
        public int MetricGroupType { get; set; }
        public GroupContext()
        {

        }
        public GroupContext(User user, Group group, DateTime date, int metricGroupId, int metricGroupType = -1)
        {
            User = user;
            Group = group;
            Date = date;
            Averages = new Dictionary<Child, Dictionary<string, float>>();
            MetricGroup = metricGroupId;
            MetricGroupType = metricGroupType;

            var metrics = Database.GetCountMetricsByType(metricGroupType, group.Type.Id);

            foreach (var child in Group.Children)
            {
                var records = Database.GetRecordsByChildAndDate(child, date, metricGroupId, Group.Type.Id);
                if (metricGroupType != -1)
                    records = Database.GetRecordsByChildAndDateAndType(child, date, metricGroupType, group);

                if (records.Count > 0)
                {
                    Averages.Add(child, records);
                }
                else if (metricGroupType != -1)
                {
                    var newItem = new Dictionary<string, float>();
                    for (int i = 0; i < metrics.Count; i++)
                    {
                        newItem.Add(metrics[i], 0f);
                    }
                    Averages.Add(child, newItem);
                }
                    
            }

            //Random rnd = new Random();
            //var metrics = Database.Metrics.Where(x => x.GroupType != null).Where(x => x.GroupType.Id == Group.Type.Id);
            ////metrics = metrics.Union(Database.Metrics.Where(x => x.GroupType == null));

            //foreach (var child in Group.Children)
            //{
            //    foreach (var metric in metrics)
            //    {
            //        Record rec = new Record()
            //        {
            //            Child = child,
            //            Date = new DateTime(2021, 09, 1),
            //            Metric = metric,
            //            MaxMark = 3,
            //            Mark = rnd.Next(0, 4)
            //        };
            //        Database.Insert(rec, "[Record]");
            //        Record rec2 = new Record()
            //        {
            //            Child = child,
            //            Date = new DateTime(2021, 10, 1),
            //            Metric = metric,
            //            MaxMark = 3,
            //            Mark = rnd.Next(0, 4)
            //        };
            //        Database.Insert(rec2, "[Record]");
            //    }
            //}
        }
    }
}
