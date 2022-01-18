using Kindergarten.Helpers;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class Child: Model
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Patronymic { get; set; } = "";
        public string HomeAddress { get; set; } = "";
        public int Age { get; set; } = -1;
        
        public Group Group { get; set; }

        public Child()
        {

        }

        public Child(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            FirstName = $"{items[1]}";
            SecondName = $"{items[2]}";
            Patronymic = $"{items[3]}";

            var rawAge = $"{items[4]}";
            if (!string.IsNullOrEmpty(rawAge))
            {
                Age = int.Parse(rawAge);
            }

            //Patronymic = $"{items[5]}";

            int rawGroupId = int.Parse($"{items[6]}");
            Group = Database.Groups.Where(x => x.Id == rawGroupId).FirstOrDefault();
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[FirstName] = N'{FirstName}', [SecondName] =N'{SecondName}', [Patronymic] =N'{Patronymic}', " +
            $"[Age] ={Age}, [HomeAddress] =N'{HomeAddress}'";

        public override string InsertValues => $"N'{FirstName}', N'{SecondName}', N'{Patronymic}', {Age}, N'{HomeAddress}', {Group.Id}";

        #endregion
    }
}
