using System.Data;

namespace Kindergarten.Models
{
    public class GroupType : Model
    {
        public string Name { get; set; }

        public GroupType()
        {

        }

        public GroupType(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}'";

        public override string InsertValues => $"N'{Name}'";

        #endregion
    }
}
