
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Kindergarten.Contexts
{
    public class MetricTypeTable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> TableTitles { get; set; }
        public Dictionary<string, List<float>> ChildAveragePairs { get; set; }

        public MetricTypeTable(string name, string description)
        {
            Name = name;
            Description = description;
            TableTitles = new List<string>();
            ChildAveragePairs = new Dictionary<string, List<float>>();
        }

        public void Add(DataRow row)
        {
            var items = row.ItemArray;
            string FCs = $"{items[0]}".Trim();

            float average = float.Parse($"{items[1]}");
            string title = $"{items[2]}".Trim();

            if (!TableTitles.Contains(title))
            {
                AddTitle(title);
            }

            if (ChildAveragePairs.ContainsKey(FCs))
            {
                ChildAveragePairs[FCs].Add(average);
            }
            else
            {
                ChildAveragePairs.Add(FCs, new List<float>() {average});
            }

        }

        public void AddTitle(string title) => TableTitles.Add(title);

        public void AddLastColumn()
        {
            TableTitles.Add("Итоговый балл");
            foreach (string fcs in ChildAveragePairs.Keys)
            {
                var average = ChildAveragePairs[fcs].Average();
                ChildAveragePairs[fcs].Add(average);
            }
        }

        public Dictionary<string, float> GetFinalAverage()
        {
            var result = new Dictionary<string, float>();
            foreach (string fcs in ChildAveragePairs.Keys)
            {
                var average = ChildAveragePairs[fcs].Last();
                result.Add(fcs, average);
            }
            return result;
        }

        public float GetAverageByLast()
        {
            float count = 0;
            float sum = 0;
            foreach (var child in ChildAveragePairs.Keys)
            {
                var list = ChildAveragePairs[child];
                sum += list.Last();
                count++;
            }

            if (count == 0)
                return 0;

            return sum / count;
        }
    }

    public class ReportContext
    {
        public List<MetricTypeTable> MetricTypes { get; private set; }
        public ReportContext()
        {

        }

        public ReportContext(DataTable table, string metricGroupName)
        {
            MetricTypes = new List<MetricTypeTable>();
            List<string> descriptions = new List<string>();
            MetricTypeTable final = new MetricTypeTable(metricGroupName, "");
            MetricTypes.Add(final);

            foreach (DataRow row in table.Rows)
            {
                var metrictype = $"{row.ItemArray[3]}".Trim();
                var metricdescription = $"{row.ItemArray[4]}".Trim();

                if (!final.TableTitles.Contains(metricdescription))
                    final.AddTitle(metricdescription);
                
                var query = MetricTypes.Where(x => x.Name == metrictype);
                if (query.Count() == 0)
                {
                    MetricTypeTable typeTable = new MetricTypeTable(metrictype, metricdescription);
                    typeTable.Add(row);
                    MetricTypes.Add(typeTable);
                }
                else
                {
                    query.FirstOrDefault().Add(row);
                }
            }

            var finalAverage = new Dictionary<string, List<float>>();

            foreach (var temp in MetricTypes.Where(x => x != final))
            {
                temp.AddLastColumn();
                var result = temp.GetFinalAverage();
                foreach (string fcs in result.Keys)
                {
                    if (finalAverage.ContainsKey(fcs))
                    {
                        finalAverage[fcs].Add(result[fcs]);
                    }
                    else
                    {
                        finalAverage.Add(fcs, new List<float>() { result[fcs] });
                    }
                }
            }

            final.ChildAveragePairs = finalAverage;
            final.AddLastColumn();
        }

        public float GetAverageBy(string name) => MetricTypes.Where(x => x.Name == name).FirstOrDefault().GetAverageByLast(); 
    }
}
