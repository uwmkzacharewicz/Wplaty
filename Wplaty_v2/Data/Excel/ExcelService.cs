﻿using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wplaty_v2.Data.Excel
{
    public class ExcelService
    {
        private string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private Cell ConstructCell(string value, CellValues dataTypes) =>
            new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataTypes)
            };

        public string GenerateExcel(String fileName)
        {
            Environment.SetEnvironmentVariable("MONO_URI_DOTNETRELATIVEORABSOLUTE", "true");

            // Creating the SpreadsheetDocument in the indicated FilePath
            var filePath = Path.Combine(AppFolder, fileName);
            var document = SpreadsheetDocument.Create(Path.Combine(AppFolder, fileName), SpreadsheetDocumentType.Workbook);

            var wbPart = document.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            var part = wbPart.AddNewPart<WorksheetPart>();
            part.Worksheet = new Worksheet(new SheetData());

            //  Here are created the sheets, you can add all the child sheets that you need.
            var sheets = wbPart.Workbook.AppendChild
            (
                new Sheets(
                    new Sheet()
                    {
                        Id = wbPart.GetIdOfPart(part),
                        SheetId = 1,
                        Name = "Invoice"
                    }
                )
            );

            // Just save and close you Excel file
            wbPart.Workbook.Save();
            document.Close();
            // Dont't forget return the filePath
            return filePath;
        }

        public void InsertDataIntoSheet(string fileName, string sheetName, ExcelStructure data)
        {
            Environment.SetEnvironmentVariable("MONO_URI_DOTNETRELATIVEORABSOLUTE", "true");

            using (var document = SpreadsheetDocument.Open(fileName, true))
            {
                var wbPart = document.WorkbookPart;
                var sheets = wbPart.Workbook.GetFirstChild<Sheets>().
                    Elements<Sheet>().FirstOrDefault().
                    Name = sheetName;

                var part = wbPart.WorksheetParts.First();
                var sheetData = part.Worksheet.Elements<SheetData>().First();

                var row = sheetData.AppendChild(new Row());

                foreach (var header in data.Headers)
                {
                    row.Append(ConstructCell(header, CellValues.String));
                }

                foreach (var value in data.Values)
                {
                    var dataRow = sheetData.AppendChild(new Row());

                    for (int i = 0; i < value.Count; i++)
                    {
                        if (i == 3)
                            dataRow.Append(ConstructCell(value[i], CellValues.Number));
                        else
                            dataRow.Append(ConstructCell(value[i], CellValues.String));
                    }
                }
                wbPart.Workbook.Save();
            }
        }

    }
}


