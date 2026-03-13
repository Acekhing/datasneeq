import { useMutation, useQuery } from '@tanstack/react-query';
import { testConnection, getTableColumns } from '@/lib/api';

export function useTestConnection() {
  return useMutation({
    mutationFn: (connectionString: string) => testConnection(connectionString),
  });
}

export function useTableSchema(connectionString: string, table: string) {
  return useQuery({
    queryKey: ['tableSchema', connectionString, table],
    queryFn: () => getTableColumns(connectionString, table),
    enabled: !!connectionString && !!table,
  });
}
