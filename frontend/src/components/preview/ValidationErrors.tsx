'use client';

import { AlertTriangle } from 'lucide-react';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import type { ValidationError } from '@/types';

interface ValidationErrorsProps {
  errors: ValidationError[];
}

export default function ValidationErrors({ errors }: ValidationErrorsProps) {
  const grouped = errors.reduce(
    (acc, err) => {
      const key = err.errorType;
      if (!acc[key]) acc[key] = [];
      acc[key].push(err);
      return acc;
    },
    {} as Record<string, ValidationError[]>
  );

  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold flex items-center gap-2 text-destructive">
        <AlertTriangle className="w-4 h-4" />
        Validation Errors ({errors.length})
      </h3>
      {Object.entries(grouped).map(([type, errs]) => (
        <Alert key={type} variant="destructive">
          <AlertTriangle className="h-4 w-4" />
          <AlertTitle>{type} ({errs.length})</AlertTitle>
          <AlertDescription>
            <ul className="mt-1 space-y-1 text-xs max-h-32 overflow-auto">
              {errs.slice(0, 20).map((err, i) => (
                <li key={i}>
                  Row {err.rowNumber}: {err.message}
                  {err.value && <span className="text-muted-foreground ml-1">(value: &quot;{err.value}&quot;)</span>}
                </li>
              ))}
              {errs.length > 20 && (
                <li className="text-muted-foreground">...and {errs.length - 20} more</li>
              )}
            </ul>
          </AlertDescription>
        </Alert>
      ))}
    </div>
  );
}
