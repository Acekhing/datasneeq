'use client';

import { useEffect, useState } from 'react';
import { ArrowRight } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { getTableColumns } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiErrors';
import type { TableSchema, ColumnMapping } from '@/types';

interface BuildModeMappingEditorProps {
  connectionString: string;
  lookupTable: string;
  excelColumns: string[];
  mappings: ColumnMapping[];
  buildMatchColumns?: string[];
  onChange: (mappings: ColumnMapping[]) => void;
  onBuildMatchColumnsChange?: (columns: string[]) => void;
}

export default function BuildModeMappingEditor({
  connectionString,
  lookupTable,
  excelColumns,
  mappings,
  buildMatchColumns = [],
  onChange,
  onBuildMatchColumnsChange,
}: BuildModeMappingEditorProps) {
  const [schema, setSchema] = useState<TableSchema | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setLoadError(null);
    getTableColumns(connectionString, lookupTable)
      .then((s) => {
        if (!cancelled) {
          setSchema(s);
          setLoadError(null);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setSchema(null);
          setLoadError(getApiErrorMessage(err));
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [connectionString, lookupTable]);

  const mappingByExcel = Object.fromEntries(
    mappings.map((m) => [m.excelColumn, m.databaseColumn])
  );
  const usedDbCols = new Set(Object.values(mappingByExcel).filter(Boolean));

  const handleChange = (excelCol: string, dbCol: string) => {
    const next = mappings.filter((m) => m.excelColumn !== excelCol);
    if (dbCol) {
      next.push({ excelColumn: excelCol, databaseColumn: dbCol });
    }
    onChange(next);
  };

  if (loading) {
    return (
      <div className="text-sm text-muted-foreground py-2">
        Loading foreign table schema...
      </div>
    );
  }

  if (!schema) {
    return (
      <div className="text-sm text-destructive py-2">
        {loadError ?? `Could not load table ${lookupTable}`}
      </div>
    );
  }

  const fkCols = schema.columns.map((c) => c.name);
  const mappedDbCols = [...new Set(mappings.filter((m) => m.databaseColumn).map((m) => m.databaseColumn!))];

  const handleMatchColumnToggle = (dbCol: string, checked: boolean) => {
    if (!onBuildMatchColumnsChange) return;
    const next = checked
      ? [...buildMatchColumns, dbCol]
      : buildMatchColumns.filter((c) => c !== dbCol);
    onBuildMatchColumnsChange(next);
  };

  return (
    <div className="mt-2 space-y-2 pl-4 border-l-2 border-muted">
      <p className="text-xs font-medium text-muted-foreground">
        Map Excel columns → {lookupTable}
      </p>
      <div className="space-y-1">
        {excelColumns.map((excelCol) => (
          <div
            key={excelCol}
            className="flex items-center gap-2 py-1 text-sm"
          >
            <span className="w-28 truncate">{excelCol}</span>
            <ArrowRight className="w-3 h-3 text-muted-foreground shrink-0" />
            <Select
              value={mappingByExcel[excelCol] || 'none'}
              onValueChange={(v) => handleChange(excelCol, v && v !== 'none' ? v : '')}
            >
              <SelectTrigger className="h-8 flex-1 max-w-[180px]">
                <SelectValue placeholder="Select..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">-- Skip --</SelectItem>
                {fkCols.map((col) => (
                  <SelectItem
                    key={col}
                    value={col}
                    disabled={usedDbCols.has(col) && mappingByExcel[excelCol] !== col}
                  >
                    {col}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        ))}
      </div>
      {mappedDbCols.length > 0 && onBuildMatchColumnsChange && (
        <div className="pt-2 mt-2 border-t border-muted space-y-1">
          <p className="text-xs font-medium text-muted-foreground">Match before create (deduplicate by)</p>
          <div className="flex flex-wrap gap-3">
            {mappedDbCols.map((dbCol) => (
              <label key={dbCol} className="flex items-center gap-2 text-sm cursor-pointer">
                <input
                  type="checkbox"
                  checked={buildMatchColumns.includes(dbCol)}
                  onChange={(e) => handleMatchColumnToggle(dbCol, e.target.checked)}
                  className="rounded border-input"
                />
                <span>{dbCol}</span>
              </label>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
