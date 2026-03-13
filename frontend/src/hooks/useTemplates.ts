import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getTemplates, saveTemplate, deleteTemplate } from '@/lib/api';
import type {
  ColumnMapping,
  LookupRule,
  PrimaryKeyGenerationStrategy,
} from '@/types';

export function useTemplates() {
  return useQuery({
    queryKey: ['templates'],
    queryFn: getTemplates,
  });
}

export function useSaveTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      name,
      targetTable,
      mappings,
      lookupRules,
      primaryKeyGenerationStrategy = 'uuid',
      duplicateKeyColumns = [],
    }: {
      name: string;
      targetTable: string;
      mappings: ColumnMapping[];
      lookupRules: LookupRule[];
      primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
      duplicateKeyColumns?: string[];
    }) =>
      saveTemplate(
        name,
        targetTable,
        mappings,
        lookupRules,
        primaryKeyGenerationStrategy,
        duplicateKeyColumns
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
    },
  });
}

export function useDeleteTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
    },
  });
}
