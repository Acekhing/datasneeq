'use client';

import { useEffect } from 'react';
import { Eye, ArrowLeft, AlertTriangle, CheckCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { usePreview } from '@/hooks/usePreview';
import { getApiErrorMessage } from '@/lib/apiErrors';
import ValidationErrors from './ValidationErrors';
import LookupResolutions from './LookupResolutions';
import type { ColumnMapping, LookupRule, UploadPreview, PrimaryKeyGenerationStrategy } from '@/types';

interface PreviewTableProps {
  fileId: string;
  connectionString: string;
  tableName: string;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
  duplicateKeyColumns?: string[];
  onComplete: (preview: UploadPreview) => void;
  onBack: () => void;
}

export default function PreviewTable({
  fileId,
  connectionString,
  tableName,
  mappings,
  lookupRules,
  primaryKeyGenerationStrategy = 'uuid',
  duplicateKeyColumns = [],
  onComplete,
  onBack,
}: PreviewTableProps) {
  const preview = usePreview();

  useEffect(() => {
    preview.mutate({
      fileId,
      connectionString,
      tableName,
      mappings,
      lookupRules,
      primaryKeyGenerationStrategy,
      duplicateKeyColumns,
    });
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fileId, connectionString, tableName]);

  const data = preview.data;
  const dbColumns = mappings.map((m) => m.databaseColumn);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Eye className="w-5 h-5" />
          Data Preview
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {preview.isPending && (
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
            <span className="ml-3">Processing data...</span>
          </div>
        )}

        {preview.isError && (
          <div className="bg-destructive/10 text-destructive p-4 rounded-lg">
            {getApiErrorMessage(preview.error)}
          </div>
        )}

        {data && (
          <>
            <div className="flex items-center gap-4 flex-wrap">
              <Badge variant="outline" className="gap-1">
                Total: {data.totalRows} rows
              </Badge>
              <Badge variant="default" className="gap-1">
                <CheckCircle className="w-3 h-3" />
                Valid: {data.validRows}
              </Badge>
              {data.errorRows > 0 && (
                <Badge variant="destructive" className="gap-1">
                  <AlertTriangle className="w-3 h-3" />
                  Errors: {data.errorRows}
                </Badge>
              )}
              {data.lookupResolutions.length > 0 && (
                <Badge variant="secondary">
                  {data.lookupResolutions.length} FK lookups resolved
                </Badge>
              )}
            </div>

            <div className="border rounded-lg overflow-auto max-h-96">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12">#</TableHead>
                    {dbColumns.map((col) => (
                      <TableHead key={col} className="whitespace-nowrap">
                        {col}
                      </TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.rows.slice(0, 50).map((row, i) => (
                    <TableRow key={i}>
                      <TableCell className="text-muted-foreground">{i + 1}</TableCell>
                      {dbColumns.map((col) => (
                        <TableCell key={col} className="whitespace-nowrap max-w-[200px] truncate">
                          {row[col] != null ? String(row[col]) : (
                            <span className="text-muted-foreground italic">null</span>
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            {data.errors.length > 0 && <ValidationErrors errors={data.errors} />}
            {data.lookupResolutions.length > 0 && <LookupResolutions resolutions={data.lookupResolutions} />}

            <div className="flex justify-between">
              <Button variant="outline" onClick={onBack}>
                <ArrowLeft className="w-4 h-4 mr-2" />
                Adjust Mappings
              </Button>
              <Button onClick={() => onComplete(data)}>
                Continue to Upload
              </Button>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}
