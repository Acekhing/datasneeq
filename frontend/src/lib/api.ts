import axios from 'axios';
import type {
  ExcelUploadResult,
  ConnectionResult,
  TableSchema,
  MappingSuggestion,
  UploadPreview,
  UploadCommitResult,
  MappingTemplate,
  MappingConfig,
  ColumnMapping,
  LookupRule,
  PrimaryKeyGenerationStrategy,
} from '@/types';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5062/api',
});

export async function uploadExcel(file: File): Promise<ExcelUploadResult> {
  const formData = new FormData();
  formData.append('file', file);
  const { data } = await api.post<ExcelUploadResult>('/upload/excel', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return data;
}

export async function testConnection(connectionString: string): Promise<ConnectionResult> {
  const { data } = await api.post<ConnectionResult>('/schema/connect', { connectionString });
  return data;
}

export async function getTableColumns(connectionString: string, table: string): Promise<TableSchema> {
  const { data } = await api.get<TableSchema>(`/schema/tables/${encodeURIComponent(table)}/columns`, {
    params: { connectionString },
  });
  return data;
}

export async function suggestMappings(
  fileId: string,
  connectionString: string,
  tableName: string
): Promise<MappingSuggestion[]> {
  const { data } = await api.post<MappingSuggestion[]>('/mapping/suggest', {
    fileId,
    connectionString,
    tableName,
  });
  return data;
}

export async function previewUpload(config: MappingConfig): Promise<UploadPreview> {
  const { data } = await api.post<UploadPreview>('/upload/preview', config);
  return data;
}

export async function commitUpload(config: MappingConfig): Promise<UploadCommitResult> {
  const { data } = await api.post<UploadCommitResult>('/upload/commit', config);
  return data;
}

export async function saveTemplate(
  name: string,
  targetTable: string,
  mappings: ColumnMapping[],
  lookupRules: LookupRule[],
  primaryKeyGenerationStrategy: PrimaryKeyGenerationStrategy = 'uuid',
  duplicateKeyColumns: string[] = []
): Promise<MappingTemplate> {
  const { data } = await api.post<MappingTemplate>('/mapping-templates', {
    name,
    targetTable,
    mappings,
    lookupRules,
    primaryKeyGenerationStrategy,
    duplicateKeyColumns,
  });
  return data;
}

export async function getTemplates(): Promise<MappingTemplate[]> {
  const { data } = await api.get<MappingTemplate[]>('/mapping-templates');
  return data;
}

export async function deleteTemplate(id: string): Promise<void> {
  await api.delete(`/mapping-templates/${id}`);
}
