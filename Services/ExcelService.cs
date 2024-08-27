using OfficeOpenXml;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services
{
    public class ExcelService : BaseService<Excel>
    {
        public ExcelService(DataContext context) : base(context)
        {
        }

        public List<List<(string Value, string Name)>> ReadExcelFile(IFormFile file, List<(int Column, string Name)> fields)
        {
            var rows = new List<List<(string Value, string Name)>>();

            using (var stream = file.OpenReadStream())
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var rowValues = new List<(string Value, string Name)>();

                        foreach (var field in fields)
                        {
                            var fieldValue = worksheet.Cells[row, field.Column].Text;

                            rowValues.Add((fieldValue, field.Name));
                        }

                        rows.Add(rowValues);
                    }
                }
            }

            return rows;
        }
    }
}
