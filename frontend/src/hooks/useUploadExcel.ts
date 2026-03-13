import { useMutation } from '@tanstack/react-query';
import { uploadExcel } from '@/lib/api';

export function useUploadExcel() {
  return useMutation({
    mutationFn: (file: File) => uploadExcel(file),
  });
}
