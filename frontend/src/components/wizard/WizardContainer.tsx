'use client';

import { useState, useCallback } from 'react';
import type { WizardState, ColumnMapping, LookupRule, ExcelUploadResult, TableSchema, UploadPreview, UploadCommitResult } from '@/types';
import StepIndicator from './StepIndicator';
import FileUploader from '../upload/FileUploader';
import ConnectionForm from '../connection/ConnectionForm';
import ColumnMapper from '../mapping/ColumnMapper';
import PreviewTable from '../preview/PreviewTable';
import ConfirmUpload from '../preview/ConfirmUpload';

export default function WizardContainer() {
  const [state, setState] = useState<WizardState>({
    currentStep: 0,
    uploadResult: null,
    connectionString: '',
    selectedTable: '',
    tableSchema: null,
    mappings: [],
    lookupRules: [],
    primaryKeyGenerationStrategy: 'uuid',
    duplicateKeyColumns: [],
    preview: null,
    commitResult: null,
  });

  const goToStep = useCallback((step: number) => {
    setState((prev) => ({ ...prev, currentStep: step }));
  }, []);

  const handleUploadComplete = useCallback((result: ExcelUploadResult) => {
    setState((prev) => ({ ...prev, uploadResult: result, currentStep: 1 }));
  }, []);

  const handleConnectionComplete = useCallback(
    (connectionString: string, table: string, schema: TableSchema) => {
      setState((prev) => ({
        ...prev,
        connectionString,
        selectedTable: table,
        tableSchema: schema,
        currentStep: 2,
      }));
    },
    []
  );

  const handleMappingComplete = useCallback(
    (
      mappings: ColumnMapping[],
      lookupRules: LookupRule[],
      primaryKeyGenerationStrategy: 'databaseDefault' | 'uuid' = 'uuid',
      duplicateKeyColumns: string[] = []
    ) => {
      setState((prev) => ({
        ...prev,
        mappings,
        lookupRules,
        primaryKeyGenerationStrategy,
        duplicateKeyColumns,
        currentStep: 3,
      }));
    },
    []
  );

  const handlePreviewComplete = useCallback((preview: UploadPreview) => {
    setState((prev) => ({ ...prev, preview, currentStep: 4 }));
  }, []);

  const handleCommitComplete = useCallback((result: UploadCommitResult) => {
    setState((prev) => ({ ...prev, commitResult: result }));
  }, []);

  const handleReset = useCallback(() => {
    setState({
      currentStep: 0,
      uploadResult: null,
      connectionString: '',
      selectedTable: '',
      tableSchema: null,
      mappings: [],
      lookupRules: [],
      primaryKeyGenerationStrategy: 'uuid',
      duplicateKeyColumns: [],
      preview: null,
      commitResult: null,
    });
  }, []);

  return (
    <div className="max-w-6xl mx-auto">
      <StepIndicator currentStep={state.currentStep} />

      {state.currentStep === 0 && <FileUploader onComplete={handleUploadComplete} />}

      {state.currentStep === 1 && state.uploadResult && (
        <ConnectionForm
          fileId={state.uploadResult.fileId}
          excelColumns={state.uploadResult.columns}
          onComplete={handleConnectionComplete}
          onBack={() => goToStep(0)}
        />
      )}

      {state.currentStep === 2 && state.uploadResult && state.tableSchema && (
        <ColumnMapper
          fileId={state.uploadResult.fileId}
          connectionString={state.connectionString}
          tableName={state.selectedTable}
          excelColumns={state.uploadResult.columns}
          tableSchema={state.tableSchema}
          initialMappings={state.mappings}
          initialLookupRules={state.lookupRules}
          initialPrimaryKeyStrategy={state.primaryKeyGenerationStrategy}
          initialDuplicateKeyColumns={state.duplicateKeyColumns}
          onComplete={handleMappingComplete}
          onBack={() => goToStep(1)}
        />
      )}

      {state.currentStep === 3 && state.uploadResult && (
        <PreviewTable
          fileId={state.uploadResult.fileId}
          connectionString={state.connectionString}
          tableName={state.selectedTable}
          mappings={state.mappings}
          lookupRules={state.lookupRules}
          primaryKeyGenerationStrategy={state.primaryKeyGenerationStrategy}
          duplicateKeyColumns={state.duplicateKeyColumns}
          onComplete={handlePreviewComplete}
          onBack={() => goToStep(2)}
        />
      )}

      {state.currentStep === 4 && state.preview && state.uploadResult && (
        <ConfirmUpload
          fileId={state.uploadResult.fileId}
          connectionString={state.connectionString}
          tableName={state.selectedTable}
          mappings={state.mappings}
          lookupRules={state.lookupRules}
          primaryKeyGenerationStrategy={state.primaryKeyGenerationStrategy}
          duplicateKeyColumns={state.duplicateKeyColumns}
          preview={state.preview}
          commitResult={state.commitResult}
          onCommitComplete={handleCommitComplete}
          onBack={() => goToStep(3)}
          onReset={handleReset}
        />
      )}
    </div>
  );
}
