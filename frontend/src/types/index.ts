export interface ExcelUploadResult {
  fileId: string;
  columns: string[];
  rowCount: number;
  sampleRows: Record<string, string>[];
  fileName: string;
}

export interface ColumnSchema {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  hasDefaultValue: boolean;
  maxLength: number | null;
  isForeignKey: boolean;
}

export interface ForeignKey {
  columnName: string;
  referencedTable: string;
  referencedColumn: string;
  lookupDisplayColumn: string | null;
}

export interface TableSchema {
  schemaName: string;
  tableName: string;
  columns: ColumnSchema[];
  foreignKeys: ForeignKey[];
  primaryKeys: string[];
}

export interface MappingSuggestion {
  excelColumn: string;
  suggestedDbColumn: string | null;
  confidence: number;
}

export type TransformationType = 'none' | 'stringToBoolean' | 'listPickFirst';

export interface StringBooleanMapping {
  excelValue: string;
  booleanValue: boolean;
}

export interface StringToBooleanConfig {
  mappings: StringBooleanMapping[];
  defaultValue: boolean;
  useDefaultWhenNoMatch: boolean;
}

export interface ListPickFirstConfig {
  delimiters?: string[];
}

export interface ColumnMapping {
  excelColumn: string;
  databaseColumn: string;
  autoGenerate?: boolean;
  transformationType?: TransformationType;
  transformationConfig?: StringToBooleanConfig | ListPickFirstConfig;
}

export type ForeignKeyProcessingMode = 'lookup' | 'buildFromExcel' | 'useValueDirectly';

export interface LookupRule {
  foreignKeyColumn: string;
  lookupTable: string;
  lookupDisplayColumn: string;
  autoCreate: boolean;
  processingMode?: ForeignKeyProcessingMode;
  foreignTableMappings?: ColumnMapping[];
  /** When Build-from-Excel: db columns to check for existing record. Empty = always create. */
  buildMatchColumns?: string[];
}

export interface ValidationError {
  rowNumber: number;
  columnName: string;
  message: string;
  errorType: string;
  value: string | null;
}

export interface LookupResolution {
  columnName: string;
  originalValue: string;
  lookupTable: string;
  resolvedId: unknown;
  wasCreated: boolean;
  processingMode?: string;
  foreignRecordPreview?: Record<string, unknown>;
}

export interface UploadPreview {
  rows: Record<string, unknown>[];
  errors: ValidationError[];
  lookupResolutions: LookupResolution[];
  totalRows: number;
  validRows: number;
  errorRows: number;
}

export interface UploadCommitResult {
  success: boolean;
  insertedCount: number;
  skippedCount: number;
  lookupRecordsCreated: number;
  warnings: string[];
  errorMessage: string | null;
}

export interface MappingTemplate {
  id: string;
  name: string;
  targetTable: string;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
  duplicateKeyColumns?: string[];
  createdAt: string;
  updatedAt: string;
}

export interface ConnectionResult {
  success: boolean;
  tables?: string[];
  error?: string;
}

export type PrimaryKeyGenerationStrategy = 'databaseDefault' | 'uuid';

export interface MappingConfig {
  fileId: string;
  connectionString: string;
  tableName: string;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
  /** Database columns that form the uniqueness key for duplicate detection. Empty = no check. */
  duplicateKeyColumns?: string[];
}

export interface WizardState {
  currentStep: number;
  uploadResult: ExcelUploadResult | null;
  connectionString: string;
  selectedTable: string;
  tableSchema: TableSchema | null;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy: PrimaryKeyGenerationStrategy;
  duplicateKeyColumns: string[];
  preview: UploadPreview | null;
  commitResult: UploadCommitResult | null;
}
