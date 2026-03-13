'use client';

import { useState } from 'react';
import { CheckCircle, XCircle, Upload, ArrowLeft, RotateCcw, Save } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useCommit } from '@/hooks/useCommit';
import { getApiErrorMessage } from '@/lib/apiErrors';
import SaveTemplateDialog from '../templates/SaveTemplateDialog';
import type {
  ColumnMapping,
  LookupRule,
  UploadPreview,
  UploadCommitResult,
  PrimaryKeyGenerationStrategy,
} from '@/types';

interface ConfirmUploadProps {
  fileId: string;
  connectionString: string;
  tableName: string;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
  duplicateKeyColumns?: string[];
  preview: UploadPreview;
  commitResult: UploadCommitResult | null;
  onCommitComplete: (result: UploadCommitResult) => void;
  onBack: () => void;
  onReset: () => void;
}

export default function ConfirmUpload({
  fileId,
  connectionString,
  tableName,
  mappings,
  lookupRules,
  primaryKeyGenerationStrategy = 'uuid',
  duplicateKeyColumns = [],
  preview,
  commitResult,
  onCommitComplete,
  onBack,
  onReset,
}: ConfirmUploadProps) {
  const commit = useCommit();
  const [showSaveTemplate, setShowSaveTemplate] = useState(false);

  const handleCommit = () => {
    commit.mutate(
      {
        fileId,
        connectionString,
        tableName,
        mappings,
        lookupRules,
        primaryKeyGenerationStrategy,
        duplicateKeyColumns,
      },
      { onSuccess: onCommitComplete }
    );
  };

  const result = commitResult;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Upload className="w-5 h-5" />
          {result ? 'Upload Complete' : 'Confirm Upload'}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {!result && (
          <>
            <div className="bg-muted/50 rounded-lg p-4 space-y-3">
              <h3 className="font-medium">Upload Summary</h3>
              <div className="grid grid-cols-2 gap-2 text-sm">
                <span className="text-muted-foreground">Target table:</span>
                <span className="font-medium">{tableName}</span>
                <span className="text-muted-foreground">Total rows:</span>
                <span>{preview.totalRows}</span>
                <span className="text-muted-foreground">Valid rows:</span>
                <span className="text-green-600">{preview.validRows}</span>
                <span className="text-muted-foreground">Error rows:</span>
                <span className={preview.errorRows > 0 ? 'text-destructive' : ''}>
                  {preview.errorRows}
                </span>
                <span className="text-muted-foreground">Columns mapped:</span>
                <span>{mappings.length}</span>
              </div>
            </div>

            {preview.errorRows > 0 && (
              <div className="bg-amber-50 dark:bg-amber-950/20 p-4 rounded-lg text-sm">
                <p className="font-medium text-amber-800 dark:text-amber-200">
                  {preview.errorRows} rows with errors will be skipped during upload.
                </p>
              </div>
            )}

            {commit.isError && (
              <div className="bg-destructive/10 text-destructive p-4 rounded-lg text-sm">
                {getApiErrorMessage(commit.error)}
              </div>
            )}

            <div className="flex justify-between items-center">
              <div className="flex gap-2">
                <Button variant="outline" onClick={onBack}>
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Back to Preview
                </Button>
                <Button variant="outline" onClick={() => setShowSaveTemplate(true)}>
                  <Save className="w-4 h-4 mr-2" />
                  Save Template
                </Button>
              </div>
              <Button
                onClick={handleCommit}
                disabled={commit.isPending || preview.validRows === 0}
              >
                {commit.isPending ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                    Uploading...
                  </>
                ) : (
                  <>
                    <Upload className="w-4 h-4 mr-2" />
                    Upload {preview.validRows} Rows
                  </>
                )}
              </Button>
            </div>
          </>
        )}

        {result && (
          <div className="text-center space-y-6 py-4">
            {result.success ? (
              <>
                <CheckCircle className="w-16 h-16 text-green-500 mx-auto" />
                <h3 className="text-xl font-semibold text-green-700 dark:text-green-400">
                  Upload Successful
                </h3>
              </>
            ) : (
              <>
                <XCircle className="w-16 h-16 text-destructive mx-auto" />
                <h3 className="text-xl font-semibold text-destructive">Upload Failed</h3>
                {result.errorMessage && (
                  <p className="text-sm text-destructive">{result.errorMessage}</p>
                )}
              </>
            )}

            <div className="flex justify-center gap-4 flex-wrap">
              <Badge variant="default">{result.insertedCount} inserted</Badge>
              {result.skippedCount > 0 && (
                <Badge variant="secondary">{result.skippedCount} skipped</Badge>
              )}
              {result.lookupRecordsCreated > 0 && (
                <Badge variant="outline">{result.lookupRecordsCreated} lookup records created</Badge>
              )}
            </div>

            {result.warnings.length > 0 && (
              <div className="text-left bg-muted/50 rounded-lg p-4 max-h-40 overflow-auto">
                <p className="text-sm font-medium mb-2">Warnings:</p>
                <ul className="text-xs space-y-1">
                  {result.warnings.map((w, i) => (
                    <li key={i} className="text-muted-foreground">{w}</li>
                  ))}
                </ul>
              </div>
            )}

            <Button onClick={onReset} className="mt-4">
              <RotateCcw className="w-4 h-4 mr-2" />
              Start New Upload
            </Button>
          </div>
        )}

        {showSaveTemplate && (
          <SaveTemplateDialog
            tableName={tableName}
            mappings={mappings}
            lookupRules={lookupRules}
            primaryKeyGenerationStrategy={primaryKeyGenerationStrategy}
            duplicateKeyColumns={duplicateKeyColumns}
            onClose={() => setShowSaveTemplate(false)}
          />
        )}
      </CardContent>
    </Card>
  );
}
