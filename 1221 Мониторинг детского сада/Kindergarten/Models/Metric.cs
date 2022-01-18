using Kindergarten.Helpers;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class Metric: Model
    {
        public string Name { get; set; }

        public MetricType MetricType { get; set; }

        public GroupType GroupType { get; set; }

        public Role AcceptableRole { get; set; }

        public Metric()
        {

        }

        public Metric(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";

            int typeID = int.Parse($"{items[2]}");
            MetricType = Database.MetricTypes.Where(x => x.Id == typeID).FirstOrDefault();

            int groupId = int.Parse($"{items[3]}");
            GroupType = Database.GroupTypes.Where(x => x.Id == groupId).FirstOrDefault();

            var raw = $"{items[4]}";
            if (!string.IsNullOrEmpty(raw))
            {
                int roleId = int.Parse(raw);
                AcceptableRole = Database.Roles.Where(x => x.Id == roleId).FirstOrDefault();
            }
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}', [TypeId] = {MetricType.Id}, [GroupTypeId] = {GroupType.Id}," +
            $"[AcceptableRoleId] = {AcceptableRole.Id}";

        public override string InsertValues => $"N'{Name}', {MetricType.Id}, {GroupType.Id}, {AcceptableRole.Id}";

        #endregion
    }
}
