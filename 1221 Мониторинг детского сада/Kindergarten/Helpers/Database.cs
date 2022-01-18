using Kindergarten.Contexts;
using Kindergarten.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kindergarten.Helpers
{
    public static class Database
    {
        private static readonly string connectionString = @"Server=.\SQLEXPRESS;Database=KindergartenBD;Trusted_Connection=True;";
        private static SqlConnection connection;

        static Database()
        {

        }

        #region Свойства

        // свойство для получение туров
        public static List<Child> Children
        {
            get
            {
                return GetObjectByType<Child>("Child");
            }
        }

        public static List<Metric> Metrics
        {
            get
            {
                return GetObjectByType<Metric>("Metric");
            }
        }

        public static List<Record> Records
        {
            get
            {
                return GetObjectByType<Record>("Record");
            }
        }

        public static List<Group> Groups
        {
            get
            {
                return GetObjectByType<Group>("[Group]");
            }
        }

        public static List<User> Users
        {
            get
            {
                return GetObjectByType<User>("[User]");
            }
        }

        public static List<Role> Roles
        {
            get
            {
                return GetObjectByType<Role>("[Role]");
            }
        }

        public static List<GroupType> GroupTypes
        {
            get
            {
                return GetObjectByType<GroupType>("[GroupType]");
            }
        }

        public static List<MetricGroup> MetricGroups
        {
            get
            {
                return GetObjectByType<MetricGroup>("[MetricGroup]");
            }
        }

        public static List<MetricType> MetricTypes
        {
            get
            {
                return GetObjectByType<MetricType>("[MetricType]");
            }
        }

        #endregion

        #region Основные методы работы с БД

        // закрываем соединение
        //private static void Close() => connection.Close();

        // открываем соединение
        //private static void Open() => connection.Open();

        //функция отправки SQL запроса
        private static DataTable SendSQL(string query)
        {

            var connection = new SqlConnection(connectionString);

            connection.Open();

            // пустая таблица
            DataTable result = new DataTable();

            // пытаемся выполнить кол
            try
            {
                // создаем SQL команду по тексту
                SqlCommand command = new SqlCommand(query, connection);

                // Создаем считывающий элемент
                SqlDataAdapter adapter = new SqlDataAdapter(command);

                // заполняем таблицу
                adapter.Fill(result);
            }

            // если словили ошибку
            catch (Exception ex)
            {
                // закрываем соединение
                connection.Close();

                throw ex;
            }

            // закрываем соединение
            connection.Close();

            // возвращаем результат - таблицу
            return result;
        }

        //функция получения объекта из базы
        private static List<T> GetObjectByType<T>(string type) where T : Model, new()
        {
            // создаем пустой список моделей
            List<T> models = new List<T>();

            // делаем выборку из базы
            var table = SendSQL($"SELECT * FROM {type}");

            // проходимся по строкам результата
            foreach (DataRow row in table.Rows)
            {
                // пробуем выполнить код
                try
                {
                    // создаем модель по данным в столбце
                    T model = (T)Activator.CreateInstance(typeof(T), row);

                    // добавляем модель в список
                    models.Add(model);
                }
                // если словаили ошибку
                catch (Exception ex)
                {
                    // выводим сообщение
                    throw new Exception($"Не удалось преобразовать {type} по следующей причине: " + ex.Message);
                }
            }

            // возвращаем список
            return models;
        }

        //функция добавления объекта в базу
        internal static void Insert<T>(T obj, string type) where T : Model, new()
        {
            // отправляем запрос
            SendSQL($"INSERT INTO {type} VALUES ({obj.InsertValues})");
        }

        //функция удаления объекта из базы
        internal static void Delete<T>(T obj, string type) where T : Model, new()
        {
            // отправляем запрос
            SendSQL($"DELETE FROM {type} WHERE id = {obj.Id}");
        }

        //функция обновления объекта в базе
        internal static void Update<T>(T obj, string type) where T : Model, new()
        {
            // отправляем запрос
            SendSQL($"UPDATE {type} SET {obj.UpdateValues} WHERE id = {obj.Id}");
        }

        internal static void Drop<T>(T obj, string type) where T : Model, new()
        {
            // отправляем запрос
            SendSQL($"DELETE {type} WHERE id = {obj.Id}");
        }

        #endregion

        #region Методы для оптимизации
        public static float GetAverageMarkByGroupAndRole(Group group, Role role)
        {
            var result = SendSQL($"SELECT AVG(CurrentValue) FROM Record as r " +
                $"join Child as c on r.ChildId = c.Id " +
                $"join Metric as m on m.Id = r.MetricId " +
                $"where c.GroupId = {group.Id} and m.AcceptableRoleId = {role.Id} and r.[Date] = '{new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1):yyyy-MM-dd}'; ");
            var raw = $"{result.Rows[0].ItemArray[0]}";
            if (string.IsNullOrEmpty(raw))
                return 0;
            else
                return float.Parse(raw);
        }

        public static float GetAverageMarkByGroup(Group group)
        {
            var result = SendSQL($"SELECT AVG(CurrentValue) FROM Record as r join Child as c on r.ChildId = c.Id where c.GroupId = {group.Id} and r.[Date] = '{new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1):yyyy-MM-dd}';");
            var raw = $"{result.Rows[0].ItemArray[0]}";
            if (string.IsNullOrEmpty(raw))
                return 0;
            else
                return float.Parse(raw);
        }

        public static string GetAgeDifference(Group group)
        {
            var result = SendSQL($"SELECT MIN(c.Age), MAX(c.Age) FROM Child as c join[Group] as g on c.GroupId = g.Id where c.GroupId = {group.Id};");
            var rawMin = $"{result.Rows[0].ItemArray[0]}";
            var rawMax = $"{result.Rows[0].ItemArray[1]}";
            return $"{rawMin}-{rawMax} лет";
        }

        public static int GetChildrenCountByGroup(Group group)
        {
            var result = SendSQL($"SELECT COUNT(DISTINCT c.Id) FROM Child as c join[Group] as g on c.GroupId = g.Id where c.GroupId = {group.Id};");
            var raw = $"{result.Rows[0].ItemArray[0]}";
            if (string.IsNullOrEmpty(raw))
                return 0;
            else
                return int.Parse(raw);
        }

        public static Dictionary<string, float> GetRecordsByChildAndDate(Child child, DateTime date, int id, int groupTypeId)
        {
            var result = SendSQL($"SELECT CONVERT(varchar(1000), mt.Name), AVG(r.CurrentValue) FROM Record as r join Metric as m on r.MetricId = m.Id " +
                $"join MetricType as mt on m.TypeId = mt.Id where r.Date = '{date:yyyy-MM-dd}' and ChildId = {child.Id} and m.GroupTypeId = {groupTypeId} " +
                $"and mt.MetricGroupId = {id} group by ChildId, CONVERT(varchar(1000), mt.Name), r.Date order by ChildId");
            var list = new Dictionary<string, float>();
            foreach (DataRow row in result.Rows)
            {
                var name = row.ItemArray[0].ToString();
                var rawAvg = row.ItemArray[1].ToString();

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(rawAvg))
                {
                    var avg = Math.Round(float.Parse(rawAvg), 1);
                    list.Add(name, (float)avg);
                }

            }
            return list;
        }

        public static Dictionary<string, float> GetRecordsByChildAndDateAndType(Child child, DateTime date, int id, Group group)
        {
            var result = SendSQL($"SELECT CONVERT(varchar(1000), m.[Name]), AVG(r.CurrentValue) FROM Record as r join Metric as m on r.MetricId = m.Id " +
                $"join MetricType as mt on m.TypeId = mt.Id where r.Date = '{date:yyyy-MM-dd}' and ChildId = {child.Id} and " +
                $"m.TypeId = {id} and m.GroupTypeId = {group.Type.Id} group by ChildId, CONVERT(varchar(1000), m.[Name]), r.Date order by ChildId");
            var list = new Dictionary<string, float>();
            foreach (DataRow row in result.Rows)
            {
                var name = row.ItemArray[0].ToString();
                var rawAvg = row.ItemArray[1].ToString();

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(rawAvg))
                {
                    var avg = Math.Round(float.Parse(rawAvg), 1);
                    list.Add(name, (float)avg);
                }

            }
            return list;
        }

        public static Dictionary<string, float> GetAverage(DateTime date, int metricgroup, int role, int group = -1)
        {
            var groupQuery = $"= {group}";
            if (group == -1)
                groupQuery = $"!= {group}";

            var result = SendSQL("SELECT g.Name, AVG(r.CurrentValue) " +
                "from[Group] as g " +
                "join Child as c on c.GroupId = g.Id " +
                "join Record as r on r.ChildId = c.Id " +
                "join Metric as m on r.MetricId = m.Id " +
                "join MetricType as mt on m.TypeId = mt.Id " +
                "join MetricGroup as mg on mt.MetricGroupId = mg.Id " +
                $"where m.AcceptableRoleId = {role} and r.[Date] = '{date:yyyy-MM-dd}' and mg.Id = {metricgroup} and c.GroupId {groupQuery}" +
                "group by g.[Name] order by g.[Name]");
            var dictionary = new Dictionary<string, float>();
            try
            {
                foreach (DataRow row in result.Rows)
                {

                    var name = row.ItemArray[0].ToString();
                    var average = float.Parse($"{row.ItemArray[1]}");
                    dictionary.Add(name, average);
                }
            } catch { }


            return dictionary;
        }

        public static Dictionary<string, float> GetAverage(int year, int metricgroup, int role, int group = -1)
        {
            var groupQuery = $"= {group}";
            if (group == -1)
                groupQuery = $"!= {group}";

            var result = SendSQL("SELECT g.Name, AVG(r.CurrentValue) " +
                "from[Group] as g " +
                "join Child as c on c.GroupId = g.Id " +
                "join Record as r on r.ChildId = c.Id " +
                "join Metric as m on r.MetricId = m.Id " +
                "join MetricType as mt on m.TypeId = mt.Id " +
                "join MetricGroup as mg on mt.MetricGroupId = mg.Id " +
                $"where m.AcceptableRoleId = {role} and r.[Date] BETWEEN '{new DateTime(year, 1, 1):yyyy-MM-dd}' AND '{new DateTime(year, 12, 31):yyyy-MM-dd}' and mg.Id = {metricgroup} and c.GroupId {groupQuery}" +
                "group by g.[Name] order by g.[Name]");
            var dictionary = new Dictionary<string, float>();
            try
            {
                foreach (DataRow row in result.Rows)
                {

                    var name = row.ItemArray[0].ToString();
                    var average = float.Parse($"{row.ItemArray[1]}");
                    dictionary.Add(name, average);
                }
            } catch{ }

            return dictionary;
        }

        public static ReportContext GetReportBy(DateTime date, int groupId, int metricGroupId, bool exists)
        {
            var group = Groups.Where(x => x.Id == groupId).FirstOrDefault();
            DataTable result = null;
            if (exists)
            {
                result = SendSQL("SELECT t.FCs, AVG(t.CurrentValue), MetricName, CAST([Name] AS VARCHAR(1000)), CAST([Description] AS VARCHAR(1000))" +
                    " FROM(SELECT c.Id, c.FirstName + ' ' + c.SecondName as FCs, r.CurrentValue, r.MaxValue, CAST(m.[Name] AS VARCHAR(1000)) as MetricName, m.TypeId" +
                    " FROM Record as r" +
                    " join Metric as m on r.MetricId = m.Id" +
                    " join Child as c on c.Id = r.ChildId" +
                    $" WHERE [DATE] = '{date:yyyy-MM-dd}' and GroupTypeId = {group.Type.Id} and c.GroupId = {groupId}) as t" +
                    " join MetricType as mt on mt.Id = t.TypeId" +
                    $" where mt.MetricGroupId = {metricGroupId}" +
                    " group by t.Id, CAST([Description] AS VARCHAR(1000)), t.FCs,  CAST([Name] AS VARCHAR(1000)), MetricName order by t.Id");
            }
            else
            {
                result = SendSQL("SELECT t.FCs, AVG(t.CurrentValue), MetricName, CAST([Name] AS VARCHAR(1000)), CAST([Description] AS VARCHAR(1000))" +
                    " FROM(SELECT c.Id, c.FirstName + ' ' + c.SecondName as FCs, r.CurrentValue, r.MaxValue, CAST(m.[Name] AS VARCHAR(1000)) as MetricName, m.TypeId" +
                    " FROM Record as r" +
                    " join Metric as m on r.MetricId = m.Id" +
                    " join Child as c on c.Id = r.ChildId" +
                    " join [Group] as g on c.GroupId = g.Id" +
                    $" WHERE [DATE] = '{date:yyyy-MM-dd}' and GroupTypeId = {group.Type.Id} and g.[Exists] = 0) as t" +
                    " join MetricType as mt on mt.Id = t.TypeId" +
                    $" where mt.MetricGroupId = {metricGroupId}" +
                    " group by t.Id, CAST([Description] AS VARCHAR(1000)), t.FCs,  CAST([Name] AS VARCHAR(1000)), MetricName order by t.Id");
            }

            var metricGroup = MetricGroups.Where(x => x.Id == metricGroupId).FirstOrDefault();
            var context = new ReportContext(result, metricGroup.Name);
            return context;
        }

        public static List<MetricGroup> GetMetricGroupByUser(User user)
        {
            List<MetricGroup> metricGroups = new List<MetricGroup>();
            var result = SendSQL("Select DISTINCT CAST(g.[Name] as nvarchar(1000))" +
                " from MetricGroup as g" +
                " join MetricType as t on g.Id = t.MetricGroupId" +
                " join metric as m on m.TypeId = t.Id" +
                $" where m.AcceptableRoleId = {user.Role.Id}");
            foreach (DataRow row in result.Rows)
            {
                var item = MetricGroups.Where(x => x.Name == $"{row.ItemArray[0]}".Trim()).FirstOrDefault();
                metricGroups.Add(item);
            }
            return metricGroups;
        }

        public static void UpdateRecord(int id, float value)
        {
            SendSQL($"UPDATE RECORD SET CurrentValue = {value} WHERE id = {id};");
        }
        
        public static List<int> GetRecordsIdsByChild(int childId, int typeId, DateTime date, int groupType)
        {
            var query = SendSQL($"SELECT r.Id, r.[Date], r.CurrentValue, r.MaxValue, r.ChildId, r.MetricId " +
                $"FROM Record as r " +
                $"join Metric as m on r.MetricId = m.Id " +
                $"where ChildId = {childId} and m.TypeId = {typeId} and r.[Date] = '{date:yyyy-MM-dd}' and m.GroupTypeId = {groupType} order by r.ChildId");
            var result = new List<int>();
            foreach (DataRow row in query.Rows)
            {
                result.Add(int.Parse($"{row.ItemArray[0]}"));
            }
            return result;
        }
        
        public static List<string> GetCountMetricsByType(int typeId, int groupTypeId)
        {
            var result = new List<string>();
            var query = SendSQL($"SELECT m.[Name] FROM Metric as m join MetricType as mt on m.TypeId = mt.Id where mt.Id = {typeId} and m.GroupTypeId = {groupTypeId}");
            foreach (DataRow row in query.Rows)
            {
                result.Add($"{row.ItemArray[0]}");
            }
            return result;
        }

        public static bool CheckRecordExists(int id)
        {
            var query = SendSQL($"SELECT COUNT(*) FROM Record WHERE id = {id}");
            if (query.Rows[0].ItemArray[0].ToString() != "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Metric GetMetricByName(string name)
        {
            var query = SendSQL($"SELECT * FROM Metric WHERE [Name] like '%{name}%'");
            if (query.Rows.Count != 0)
            {
                Metric metric = new Metric(query.Rows[0]);
                return metric;
            }
            else
            {
                throw new Exception("Метрика не найдена!");
            }    
        }

        #endregion
    }
}
