'use client';

import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import type { ExcelUploadResult } from '@/types';

interface ExcelPreviewProps {
  result: ExcelUploadResult;
}

export default function ExcelPreview({ result }: ExcelPreviewProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4 flex-wrap">
        <Badge variant="secondary">{result.fileName}</Badge>
        <Badge variant="outline">{result.columns.length} columns</Badge>
        <Badge variant="outline">{result.rowCount} rows</Badge>
      </div>

      <div className="border rounded-lg overflow-auto max-h-64">
        <Table>
          <TableHeader>
            <TableRow>
              {result.columns.map((col) => (
                <TableHead key={col} className="whitespace-nowrap font-semibold">
                  {col}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {result.sampleRows.map((row, i) => (
              <TableRow key={i}>
                {result.columns.map((col) => (
                  <TableCell key={col} className="whitespace-nowrap">
                    {row[col] || <span className="text-muted-foreground italic">empty</span>}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <p className="text-xs text-muted-foreground">
        Showing first {result.sampleRows.length} of {result.rowCount} rows
      </p>
    </div>
  );
}
