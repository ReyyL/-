using System.Data;

namespace Kindergarten.Models
{
    public class MetricGroup: Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MetricGroup()
        {

        }

        public MetricGroup(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";
            Description = $"{items[1]}";
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}', [Description] = N'{Name}'";

        public override string InsertValues => $"N'{Name}', N'{Description}'";

        #endregion
    }
}
