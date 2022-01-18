using Kindergarten.Helpers;
using System;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class Record: Model
    {
        public DateTime Date { get; set; }
        public float Mark { get; set; }
        public float MaxMark { get; set; }
        public Child Child { get; set; } //TODO: добавить получение из базы
        public Metric Metric { get; set; } //TODO: добавить получение из базы

        public Record()
        {

        }

        public Record(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Date = DateTime.Parse($"{items[1]}");
            Mark = float.Parse($"{items[2]}");
            MaxMark = float.Parse($"{items[3]}");

            int rawChildId = int.Parse($"{items[4]}");
            Child = Database.Children.Where(x => x.Id == rawChildId).FirstOrDefault();

            int rawMetricId = int.Parse($"{items[5]}");
            Metric = Database.Metrics.Where(x => x.Id == rawMetricId).FirstOrDefault();
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Date] = {SQLDate}, [CurrentValue] = {Mark}, [MaxValue] = {MaxMark}, " +
            $"[ChildId] ={Child.Id}, [MetricId] =N'{Metric.Id}'";

        public override string InsertValues => $"{SQLDate}, {Mark}, N'{MaxMark}', {Child.Id}, {Metric.Id}";

        #endregion

        #region Вспомогательные свойства

        private string SQLDate => $"N'{Date:yyyy-MM-dd HH:mm:ss.fff}'";

        #endregion
    }
}
