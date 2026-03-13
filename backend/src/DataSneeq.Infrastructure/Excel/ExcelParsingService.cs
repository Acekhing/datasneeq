using System.Data;
using System.Text;
using ClosedXML.Excel;
using ExcelDataReader;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Models;

namespace DataSneeq.Infrastructure.Excel;

public class ExcelParsingService : IExcelParsingService
{
    private static readonly byte[] OLE2Signature = { 0xD0, 0xCF, 0x11, 0xE0 };

    static ExcelParsingService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task<ExcelFileData> ParseExcelFileAsync(Stream fileStream, string fileName)
    {
        var ms = new MemoryStream();
        fileStream.CopyTo(ms);
        ms.Position = 0;

        bool isOle2 = ms.Length >= 4 && DetectOle2(ms);
        ms.Position = 0;

        if (isOle2)
            return Task.FromResult(ParseWithExcelDataReader(ms, fileName));

        return Task.FromResult(ParseWithClosedXml(ms, fileName));
    }

    private static bool DetectOle2(Stream stream)
    {
        Span<byte> header = stackalloc byte[4];
        stream.ReadExactly(header);
        return header[0] == OLE2Signature[0]
            && header[1] == OLE2Signature[1]
            && header[2] == OLE2Signature[2]
            && header[3] == OLE2Signature[3];
    }

    private static ExcelFileData ParseWithClosedXml(Stream stream, string fileName)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        if (lastRow == 0 || lastCol == 0)
            throw new InvalidOperationException("The Excel file is empty or has no data.");

        var headers = new List<string>();
        for (int col = 1; col <= lastCol; col++)
        {
            var header = worksheet.Cell(1, col).GetString().Trim();
            headers.Add(string.IsNullOrEmpty(header) ? $"Column{col}" : header);
        }

        var rows = new List<Dictionary<string, string>>();
        for (int row = 2; row <= lastRow; row++)
        {
            var rowData = new Dictionary<string, string>();
            bool hasData = false;

            for (int col = 1; col <= lastCol; col++)
            {
                var cell = worksheet.Cell(row, col);
                var value = cell.IsEmpty() ? string.Empty : cell.GetString().Trim();
                rowData[headers[col - 1]] = value;
                if (!string.IsNullOrEmpty(value)) hasData = true;
            }

            if (hasData) rows.Add(rowData);
        }

        return new ExcelFileData
        {
            FileId = Guid.NewGuid().ToString(),
            Headers = headers,
            Rows = rows,
            FileName = fileName,
            UploadedAt = DateTime.UtcNow
        };
    }

    private static ExcelFileData ParseWithExcelDataReader(Stream stream, string fileName)
    {
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        var table = dataSet.Tables[0];
        if (table.Rows.Count == 0 && table.Columns.Count == 0)
            throw new InvalidOperationException("The Excel file is empty or has no data.");

        var headers = new List<string>();
        for (int col = 0; col < table.Columns.Count; col++)
        {
            var name = table.Columns[col].ColumnName.Trim();
            headers.Add(string.IsNullOrEmpty(name) ? $"Column{col + 1}" : name);
        }

        var rows = new List<Dictionary<string, string>>();
        foreach (DataRow dataRow in table.Rows)
        {
            var rowData = new Dictionary<string, string>();
            bool hasData = false;

            for (int col = 0; col < headers.Count; col++)
            {
                var value = dataRow[col]?.ToString()?.Trim() ?? string.Empty;
                rowData[headers[col]] = value;
                if (!string.IsNullOrEmpty(value)) hasData = true;
            }

            if (hasData) rows.Add(rowData);
        }

        return new ExcelFileData
        {
            FileId = Guid.NewGuid().ToString(),
            Headers = headers,
            Rows = rows,
            FileName = fileName,
            UploadedAt = DateTime.UtcNow
        };
    }
}
