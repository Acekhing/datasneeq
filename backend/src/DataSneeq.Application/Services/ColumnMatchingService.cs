using FuzzySharp;
using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class ColumnMatchingService : IColumnMatchingService
{
    private static readonly Dictionary<string, string[]> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        { "dob", new[] { "dateofbirth", "birthdate", "birthday" } },
        { "fname", new[] { "firstname" } },
        { "lname", new[] { "lastname" } },
        { "addr", new[] { "address" } },
        { "tel", new[] { "telephone", "phone", "phonenumber" } },
        { "num", new[] { "number" } },
        { "qty", new[] { "quantity" } },
        { "amt", new[] { "amount" } },
        { "desc", new[] { "description" } },
        { "dept", new[] { "department" } },
        { "org", new[] { "organization", "organisation" } },
        { "id", new[] { "identifier" } },
        { "no", new[] { "number" } },
        { "doj", new[] { "dateofjoining", "joindate" } }
    };

    public List<MappingSuggestionDto> SuggestMappings(List<string> excelColumns, List<ColumnSchema> dbColumns)
    {
        var suggestions = new List<MappingSuggestionDto>();
        var usedDbColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var excelCol in excelColumns)
        {
            var best = FindBestMatch(excelCol, dbColumns, usedDbColumns);
            suggestions.Add(best);
            if (best.SuggestedDbColumn != null)
                usedDbColumns.Add(best.SuggestedDbColumn);
        }

        return suggestions;
    }

    private MappingSuggestionDto FindBestMatch(string excelCol, List<ColumnSchema> dbColumns, HashSet<string> used)
    {
        string normalizedExcel = Normalize(excelCol);
        MappingSuggestionDto? bestMatch = null;

        foreach (var dbCol in dbColumns)
        {
            if (used.Contains(dbCol.Name)) continue;

            double confidence = 0;
            string normalizedDb = Normalize(dbCol.Name);

            if (string.Equals(excelCol, dbCol.Name, StringComparison.OrdinalIgnoreCase))
            {
                confidence = 1.0;
            }
            else if (string.Equals(normalizedExcel, normalizedDb, StringComparison.OrdinalIgnoreCase))
            {
                confidence = 0.9;
            }
            else if (MatchesAbbreviation(normalizedExcel, normalizedDb))
            {
                confidence = 0.85;
            }
            else
            {
                var ratio = Fuzz.Ratio(normalizedExcel.ToLower(), normalizedDb.ToLower());
                if (ratio >= 70)
                    confidence = ratio / 100.0;
            }

            if (confidence > 0 && (bestMatch == null || confidence > bestMatch.Confidence))
            {
                bestMatch = new MappingSuggestionDto
                {
                    ExcelColumn = excelCol,
                    SuggestedDbColumn = dbCol.Name,
                    Confidence = Math.Round(confidence, 2)
                };
            }

            if (bestMatch?.Confidence >= 1.0) break;
        }

        return bestMatch ?? new MappingSuggestionDto
        {
            ExcelColumn = excelCol,
            SuggestedDbColumn = null,
            Confidence = 0
        };
    }

    private static string Normalize(string name)
    {
        return name.Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    private static bool MatchesAbbreviation(string a, string b)
    {
        return CheckAbbreviation(a, b) || CheckAbbreviation(b, a);
    }

    private static bool CheckAbbreviation(string abbr, string full)
    {
        if (Abbreviations.TryGetValue(abbr, out var expansions))
        {
            return expansions.Any(e => string.Equals(Normalize(e), Normalize(full), StringComparison.OrdinalIgnoreCase));
        }
        return false;
    }
}
