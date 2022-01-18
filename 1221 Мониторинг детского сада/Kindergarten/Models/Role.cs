using System.Data;

namespace Kindergarten.Models
{
    public class Role: Model
    {
        public string Name { get; set; }
        public string Description { get; set; } = "";

        public Role()
        {

        }

        public Role(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";
            Description = $"{items[2]}";
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}', [Description] = N'{Description}'";

        public override string InsertValues => $"N'{Name}', N'{Description}'";

        #endregion
    }
}
