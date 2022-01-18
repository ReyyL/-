using Kindergarten.Contexts;
using Kindergarten.Helpers;
using Kindergarten.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kindergarten.Controllers
{
    public class AccountController : Controller
    {
        private User currentUser;

        public ActionResult Index()
        {
            try
            {
                currentUser = Models.User.Current(Request);
                if (currentUser == null)
                    throw new Exception();

                MentorContext context = new(currentUser, DateTime.Now.Month);

                List<DataPoint> dataPoints = new List<DataPoint>();

                ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

                return View(context);
            }
            catch
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult DiagrammedIndex(int group, int role, int metricgroup, int month, int year)
        {
            try
            {
                currentUser = Models.User.Current(Request);
                if (currentUser == null)
                    throw new Exception();

                var date = new DateTime(year, DateTime.Now.Month, 1);
                if (month != -1)
                    date = new DateTime(year, month, 1);

                MentorContext context = new(currentUser, month, false)
                {
                    DiagrammaDate = date,
                    SelectedGroupId = group,
                    SelectedMetricGroupId = metricgroup,
                    SelectedRoleId = role
                };

                List<DataPoint> dataPoints = new List<DataPoint>();

                var rawData = Database.GetAverage(date, metricgroup, role, group);
                if (group == -1 && month == -1)
                {
                    rawData = Database.GetAverage(year, metricgroup, role);
                }
                else if (group == -1)
                {
                    rawData = Database.GetAverage(date, metricgroup, role);
                }
                else if (month == -1)
                {
                    rawData = Database.GetAverage(year, metricgroup, role, group);
                }
                
                foreach (string name in rawData.Keys)
                {
                    DataPoint point = new DataPoint(name, rawData[name]);
                    dataPoints.Add(point);
                }

                ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

                return View("Index", context);
            }
            catch
            {
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult Exit()
        {
            Models.User.Clear(Response);
            return RedirectToAction("Login", "Home");
        }

        public ActionResult Group(int id, int year, int month, int metric)
        {
            try
            {
                currentUser = Models.User.Current(Request);
            }
            catch
            {
                return RedirectToAction("Login", "Home");
            }

            if (id == 0 && year == 0 && month == 0 && metric == 0)
            {
                currentUser = Models.User.Current(Request);
                if (currentUser == null)
                    throw new Exception();

                MentorContext context = new(currentUser, month);
                return View("GroupAdd", context);
            }

            var group = Database.Groups.Where(x => x.Id == id).FirstOrDefault();

            if (group == null)
            {
                Redirect("/Account/Index");
            }

            var now = DateTime.Now;
            try
            {
                GroupContext context = new GroupContext(currentUser, group, new DateTime(year, month, 1), metric);
                return View("Report", context);
            }
            catch
            {
                GroupContext context = new GroupContext(currentUser, group, new DateTime(now.Year, now.Month, 1), 1);
                return View("Report", context);
            }
        }

        public ActionResult GroupAdd(int mentor, int grouptype, string groupName, string iconPath)
        {
            if (groupName == "")
                return Redirect("/Account/Index");

            var names = Request.Form["childfirstname"];
            var secondnames = Request.Form["childsecondname"];
            var ages = Request.Form["age"];

            if (string.IsNullOrEmpty(iconPath))
                iconPath = "/img/pics/sun.svg";

            Group group = new Group()
            {
                Name = groupName,
                IconPath = iconPath,
                Type = Database.GroupTypes.Where(x => x.Id == grouptype).First(),
                Mentor = Database.Users.Where(x => x.Id == mentor).First(),
                Exists = true,
            };

            Database.Insert(group, "[Group]");

            try
            {
                for (int i = 0; i < names.Count; i++)
                {
                    Child child = new Child()
                    {
                        FirstName = $"{names[i]}",
                        SecondName = $"{secondnames[i]}",
                        Age = int.Parse($"{ages[i]}"),
                        Group = Database.Groups.Last()
                    };
                    Database.Insert(child, "[Child]");
                }

                if (names.Count == 0)
                    Database.Drop(group, "[GROUP]");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                Database.Drop(group, "[GROUP]");
            }


            return Redirect("/Account/Index");
        }

        public ActionResult ExportToExcel(int id, int year, int month, int metric, bool exists = true)
        {
            try
            {
                currentUser = Models.User.Current(Request);
                var group = Database.Groups.Where(x => x.Id == id).FirstOrDefault();

                var date = new DateTime(year, month, 1);
                GroupContext context = new GroupContext(currentUser, group, date, metric);
                if (group == null)
                {
                    return View("Report", context);
                }

                var filepath = ExcelWorker.GetReportBy(date, group.Id, metric, currentUser, exists);
                string file_type = "application/vnd.ms-excel";
                return PhysicalFile(filepath, file_type);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Home");
            }


        }

        public ActionResult GroupEdit(int id, int metric, int metrictype, int year, int month)
        {
            try
            {
                currentUser = Models.User.Current(Request);
            }
            catch
            {
                return RedirectToAction("Login", "Home");
            }

            var group = Database.Groups.Where(x => x.Id == id).FirstOrDefault();

            if (group == null)
            {
                Redirect("/Account/Index");
            }

            GroupContext context = new GroupContext(currentUser, group, new DateTime(year, month, 1), metric, metrictype);
            return View("ReportEdit", context);
        }

        public ActionResult GroupSave(int id, int metric, int metrictype, int year, int month)
        {
            try
            {
                currentUser = Models.User.Current(Request);
            }
            catch
            {
                return RedirectToAction("Login", "Home");
            }

            var group = Database.Groups.Where(x => x.Id == id).FirstOrDefault();
            var date = new DateTime(year, month, 1);

            var marks = Request.Form["mark"];
            var metricsNames = Request.Form["metricNames"];

            var children = group.Children;

            int childindex = 0;
            int metricscount = marks.Count / children.Count;
            foreach (Child child in children)
            {
                var recordsIds = Database.GetRecordsIdsByChild(child.Id, metrictype, date, group.Type.Id);
                for (int i = metricscount * childindex; i < metricscount * (childindex+1); i++)
                {
                    int j = i - metricscount * childindex;
                    if (j < recordsIds.Count)
                    {
                        Database.UpdateRecord(recordsIds[j], float.Parse(marks[i]));
                    }
                    else
                    {
                        var tempMetric = Database.GetMetricByName(metricsNames[j]);
                        Record record = new Record()
                        {
                            Child = child,
                            Date = date,
                            Mark = float.Parse($"{marks[i]}"),
                            Metric = tempMetric,
                            MaxMark = 3
                        };
                        Database.Insert(record, "[Record]");
                    }
                }
                childindex++;
            }

            GroupContext context = new GroupContext(currentUser, group, date, metric);

            return View("Report", context);
        }
    }
}
