using Kindergarten.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class Group: Model
    {
        public string Name { get; set; }
        public string IconPath { get; set; }
        public GroupType Type { get; set; }
        public User Mentor { get; set; }
        public bool Exists { get; set; }
        public Group()
        {

        }

        public Group(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Name = $"{items[1]}";
            IconPath = $"{items[2]}";

            var typeId = int.Parse($"{items[3]}");
            Type = Database.GroupTypes.Where(x => x.Id == typeId).FirstOrDefault();

            var mentorId = int.Parse($"{items[4]}");
            Mentor = Database.Users.Where(x => x.Id == mentorId).FirstOrDefault();

            Exists = bool.Parse($"{items[5]}");
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Name] = N'{Name}', [IconPath] = N'{IconPath}', [TypeId] = {Type.Id}, [MentorId] = {Mentor.Id}, [Exists] = {ExistsToInt}";

        public override string InsertValues => $"N'{Name}', N'{IconPath}', {Type.Id}, {Mentor.Id}, {ExistsToInt}";

        #endregion

        #region Вспомогательные свойства

        private int ExistsToInt
        {
            get
            {
                if (Exists)
                    return 1;
                else
                    return 0;
            }
        }

        public List<Child> Children
        {
            get 
            { 
                return Database.Children.Where(x => x.Group.Id == Id).ToList(); 
            }
        }

        public string AgeDifference => Database.GetAgeDifference(this);

        public float AverageMark => (float) Math.Round(Database.GetAverageMarkByGroup(this), 1);

        public float GetAverageMarkByRole(Role role) => (float)Math.Round(Database.GetAverageMarkByGroupAndRole(this, role), 1);

        public float ChildrenCount => Database.GetChildrenCountByGroup(this);

        public string GetColor(float average)
        {
            if (average < 1)
                return "#D32F2F";
            else if (average < 2)
                return "#FBC02D";
            else
                return "#8BC34A";
        }

        public string GetBackgroundColor(float average)
        {
            if (average < 1)
                return "#FFCBCB";
            else if (average < 2)
                return "#FFF0CB";
            else
                return "#E3FFC2";
        }

        public string Color => GetColor(AverageMark);
        public string BackgroundColor => GetBackgroundColor(AverageMark);

        #endregion
    }
}
