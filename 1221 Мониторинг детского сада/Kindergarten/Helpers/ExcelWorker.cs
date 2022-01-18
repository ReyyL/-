using ClosedXML.Excel;
using Kindergarten.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kindergarten.Helpers
{
    public static class ExcelWorker
    {
        public static string GetReportBy(DateTime date, int groupId, int metricGroupId, User user, bool exists)
        {
            var context = Database.GetReportBy(date, groupId, metricGroupId, exists);

            using (var workbook = new XLWorkbook())
            {
                foreach (var type in context.MetricTypes)
                {
                    if (type.Name.Length >= 31)
                    {
                        string newName = "";
                        for (int i = 0; i < 31; i++)
                        {
                            newName += type.Name[i];
                        }
                        type.Name = newName;
                    }
                    var worksheet = workbook.Worksheets.Add(type.Name);
                    worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell("A1").Value = $"Отчет был сформирован {DateTime.Now} пользователем {user.SecondName} {user.FirstName} ({user.Role.Name.Trim()})";
                    worksheet.Cell("A4").Value = "% п/п";
                    worksheet.Cell("B4").Value = "Фамилия, имя ребенка";
                    int index = 'B';
                    foreach (var title in type.TableTitles)
                    {
                        var temppath = $"{(char)(index + 1)}4";
                        if (temppath.Contains('['))
                            temppath = temppath.Replace('[', 'A');
                        else
                            index++;
                        worksheet.Cell(temppath).Value = title;
                    }

                    var indexRow = 5;
                    var num = 1;
                    var lastColumnIndex = "C";
                    foreach (var child in type.ChildAveragePairs.Keys)
                    {
                        worksheet.Cell($"A{indexRow}").Value = $"{num}";
                        worksheet.Cell($"B{indexRow}").Value = $"{child}";
                        index = 'B';
                        foreach (float value in type.ChildAveragePairs[child])
                        {
                            var temppath = $"{(char)(index + 1)}{indexRow}";
                            if (temppath.Contains('['))
                                temppath = temppath.Replace("[", "AA");
                            else
                                index++;
                            worksheet.Cell(temppath).SetValue($"{value}".Replace('.', ','));
                            worksheet.Cell(temppath).DataType = XLDataType.Text;
                            lastColumnIndex = $"{(char)index}".Replace("[", "AA");
                        }
                        
                        indexRow++;
                        num++;
                    }

                    worksheet.Cell($"{(char)(lastColumnIndex[0] - 1)}{indexRow + 2}").Value = "Ср. балл";
                    float average = 0f;
                    if (lastColumnIndex != "C" && indexRow != 5)
                    {
                        average = context.GetAverageBy(type.Name);
                    }
                    worksheet.Cell($"{lastColumnIndex}{indexRow + 2}").Value = $"{average}";
                }
                string path = Directory.GetCurrentDirectory() + $"\\wwwroot\\excelExport_{DateTime.Now:yyyy-MM-dd}.xlsx";

                if (File.Exists(path))
                    File.Delete(path);

                workbook.SaveAs(path);
                return path;
            }
        }

    }
}
