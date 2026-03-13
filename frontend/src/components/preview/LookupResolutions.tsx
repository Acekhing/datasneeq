'use client';

import { Link } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import type { LookupResolution } from '@/types';

interface LookupResolutionsProps {
  resolutions: LookupResolution[];
}

export default function LookupResolutions({ resolutions }: LookupResolutionsProps) {
  const buildMode = resolutions.filter((r) => r.processingMode === 'BuildFromExcel');
  const lookupExisting = resolutions.filter((r) => !r.wasCreated && r.processingMode !== 'BuildFromExcel');
  const lookupCreated = resolutions.filter((r) => r.wasCreated && r.processingMode !== 'BuildFromExcel');

  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold flex items-center gap-2">
        <Link className="w-4 h-4" />
        Foreign Key Resolutions ({resolutions.length})
      </h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
        {lookupExisting.length > 0 && (
          <div className="bg-muted/50 rounded-lg p-3">
            <p className="text-xs font-medium mb-2">Lookup - Existing Records Used ({lookupExisting.length})</p>
            <div className="space-y-1 max-h-32 overflow-auto">
              {lookupExisting.slice(0, 10).map((r, i) => (
                <div key={i} className="flex items-center gap-2 text-xs">
                  <span className="text-muted-foreground">{r.columnName}:</span>
                  <span>&quot;{r.originalValue}&quot;</span>
                  <Badge variant="outline" className="text-[10px]">
                    ID: {String(r.resolvedId)}
                  </Badge>
                </div>
              ))}
            </div>
          </div>
        )}
        {lookupCreated.length > 0 && (
          <div className="bg-amber-50 dark:bg-amber-950/20 rounded-lg p-3">
            <p className="text-xs font-medium mb-2">Lookup - New Records Created ({lookupCreated.length})</p>
            <div className="space-y-1 max-h-32 overflow-auto">
              {lookupCreated.slice(0, 10).map((r, i) => (
                <div key={i} className="flex items-center gap-2 text-xs">
                  <span className="text-muted-foreground">{r.lookupTable}:</span>
                  <span>&quot;{r.originalValue}&quot;</span>
                  <Badge variant="secondary" className="text-[10px]">
                    New ID: {String(r.resolvedId)}
                  </Badge>
                </div>
              ))}
            </div>
          </div>
        )}
        {buildMode.length > 0 && (
          <div className="bg-blue-50 dark:bg-blue-950/20 rounded-lg p-3">
            <p className="text-xs font-medium mb-2">Built from Excel ({buildMode.length})</p>
            <div className="space-y-1 max-h-32 overflow-auto">
              {buildMode.slice(0, 10).map((r, i) => (
                <div key={i} className="flex flex-col gap-1 text-xs">
                  <div className="flex items-center gap-2">
                    <span className="text-muted-foreground">{r.columnName} → {r.lookupTable}:</span>
                    <Badge variant="secondary" className="text-[10px]">
                      {r.resolvedId === '(preview)' ? 'Preview' : `ID: ${String(r.resolvedId)}`}
                    </Badge>
                  </div>
                  {r.foreignRecordPreview && Object.keys(r.foreignRecordPreview).length > 0 && (
                    <div className="pl-2 text-muted-foreground">
                      {Object.entries(r.foreignRecordPreview).map(([k, v]) => (
                        <span key={k} className="mr-2">{k}: {String(v)}</span>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
