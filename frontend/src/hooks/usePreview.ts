import { useMutation } from '@tanstack/react-query';
import { previewUpload } from '@/lib/api';
import type { MappingConfig } from '@/types';

export function usePreview() {
  return useMutation({
    mutationFn: (config: MappingConfig) => previewUpload(config),
  });
}
