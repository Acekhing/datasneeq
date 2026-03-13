'use client';

import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { useSaveTemplate } from '@/hooks/useTemplates';
import { getApiErrorMessage } from '@/lib/apiErrors';
import type { ColumnMapping, LookupRule, PrimaryKeyGenerationStrategy } from '@/types';

interface SaveTemplateDialogProps {
  tableName: string;
  mappings: ColumnMapping[];
  lookupRules: LookupRule[];
  primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy;
  duplicateKeyColumns?: string[];
  onClose: () => void;
}

export default function SaveTemplateDialog({
  tableName,
  mappings,
  lookupRules,
  primaryKeyGenerationStrategy = 'uuid',
  duplicateKeyColumns = [],
  onClose,
}: SaveTemplateDialogProps) {
  const [name, setName] = useState('');
  const save = useSaveTemplate();

  const handleSave = () => {
    save.mutate(
      {
        name,
        targetTable: tableName,
        mappings,
        lookupRules,
        primaryKeyGenerationStrategy,
        duplicateKeyColumns,
      },
      {
        onSuccess: () => onClose(),
      }
    );
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Save Mapping Template</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="templateName">Template Name</Label>
            <Input
              id="templateName"
              placeholder="e.g., Student Upload Template"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </div>
          <p className="text-sm text-muted-foreground">
            This will save {mappings.length} column mappings for table &quot;{tableName}&quot;
          </p>
          {save.isError && (
            <div className="bg-destructive/10 text-destructive p-3 rounded-lg text-sm">
              {getApiErrorMessage(save.error)}
            </div>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={!name.trim() || save.isPending}>
            {save.isPending ? 'Saving...' : 'Save Template'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
