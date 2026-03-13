import { useMutation } from '@tanstack/react-query';
import { commitUpload } from '@/lib/api';
import type { MappingConfig } from '@/types';

export function useCommit() {
  return useMutation({
    mutationFn: (config: MappingConfig) => commitUpload(config),
  });
}
