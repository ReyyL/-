using Kindergarten.Helpers;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class MetricType: Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MetricGroup Group { get; set; }
        public MetricType()
        {

        }

        public MetricType(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";
            Description = $"{items[2]}";

            var groupId = int.Parse($"{items[3]}");
            Group = Database.MetricGroups.Where(x => x.Id == groupId).FirstOrDefault();
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}', [Description] = N'{Name}', MetricGroupId = {Group.Id}";

        public override string InsertValues => $"N'{Name}', N'{Description}', {Group.Id}";

        #endregion
    }
}
