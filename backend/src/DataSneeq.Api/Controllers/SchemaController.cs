using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataSneeq.Api.Controllers;

[ApiController]
[Route("api/schema")]
public class SchemaController : ControllerBase
{
    private readonly IDatabaseProvider _dbProvider;

    public SchemaController(IDatabaseProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    [HttpPost("connect")]
    public async Task<ActionResult<object>> TestConnection([FromBody] ConnectionRequestDto request)
    {
        try
        {
            await _dbProvider.TestConnectionAsync(request.ConnectionString);
            var tables = await _dbProvider.GetTableNamesAsync(request.ConnectionString);
            return Ok(new { success = true, tables });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("tables")]
    public async Task<ActionResult<List<string>>> GetTables([FromQuery] string connectionString)
    {
        var tables = await _dbProvider.GetTableNamesAsync(connectionString);
        return Ok(tables);
    }

    [HttpGet("tables/{table}/columns")]
    public async Task<ActionResult<TableSchemaDto>> GetTableColumns(string table, [FromQuery] string connectionString)
    {
        var schema = await _dbProvider.GetTableSchemaAsync(connectionString, table);

        var dto = new TableSchemaDto
        {
            SchemaName = schema.SchemaName,
            TableName = schema.TableName,
            PrimaryKeys = schema.PrimaryKeys,
            Columns = schema.Columns.Select(c => new ColumnSchemaDto
            {
                Name = c.Name,
                DataType = c.DataType,
                IsNullable = c.IsNullable,
                IsPrimaryKey = c.IsPrimaryKey,
                HasDefaultValue = c.HasDefaultValue,
                MaxLength = c.MaxLength,
                IsForeignKey = c.IsForeignKey
            }).ToList(),
            ForeignKeys = schema.ForeignKeys.Select(fk => new ForeignKeyDto
            {
                ColumnName = fk.ColumnName,
                ReferencedTable = fk.ReferencedTable,
                ReferencedColumn = fk.ReferencedColumn,
                LookupDisplayColumn = fk.LookupDisplayColumn
            }).ToList()
        };

        return Ok(dto);
    }
}
