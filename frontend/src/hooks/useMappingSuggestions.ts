import { useMutation } from '@tanstack/react-query';
import { suggestMappings } from '@/lib/api';

export function useMappingSuggestions() {
  return useMutation({
    mutationFn: ({ fileId, connectionString, tableName }: { fileId: string; connectionString: string; tableName: string }) =>
      suggestMappings(fileId, connectionString, tableName),
  });
}
