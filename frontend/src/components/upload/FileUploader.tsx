'use client';

import { useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, FileSpreadsheet } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useUploadExcel } from '@/hooks/useUploadExcel';
import { getApiErrorMessage } from '@/lib/apiErrors';
import type { ExcelUploadResult } from '@/types';
import ExcelPreview from './ExcelPreview';

interface FileUploaderProps {
  onComplete: (result: ExcelUploadResult) => void;
}

export default function FileUploader({ onComplete }: FileUploaderProps) {
  const upload = useUploadExcel();

  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      if (acceptedFiles.length > 0) {
        upload.mutate(acceptedFiles[0], {
          onSuccess: (data) => {
            // Don't auto-advance, let user review
          },
        });
      }
    },
    [upload]
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
    },
    maxFiles: 1,
    maxSize: 10 * 1024 * 1024,
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileSpreadsheet className="w-5 h-5" />
          Upload Excel File
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div
          {...getRootProps()}
          className={`border-2 border-dashed rounded-lg p-12 text-center cursor-pointer transition-colors ${
            isDragActive
              ? 'border-primary bg-primary/5'
              : 'border-muted-foreground/30 hover:border-primary/50'
          }`}
        >
          <input {...getInputProps()} />
          <Upload className="w-12 h-12 mx-auto text-muted-foreground mb-4" />
          {isDragActive ? (
            <p className="text-primary font-medium">Drop the file here...</p>
          ) : (
            <div>
              <p className="font-medium">Drag and drop an Excel file here, or click to select</p>
              <p className="text-sm text-muted-foreground mt-1">Supports .xlsx and .xls files up to 10MB</p>
            </div>
          )}
        </div>

        {upload.isPending && (
          <div className="flex items-center justify-center py-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
            <span className="ml-3">Parsing Excel file...</span>
          </div>
        )}

        {upload.isError && (
          <div className="bg-destructive/10 text-destructive p-4 rounded-lg">
            {getApiErrorMessage(upload.error)}
          </div>
        )}

        {upload.data && (
          <>
            <ExcelPreview result={upload.data} />
            <div className="flex justify-end">
              <Button onClick={() => onComplete(upload.data)}>
                Continue to Database Connection
              </Button>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}
