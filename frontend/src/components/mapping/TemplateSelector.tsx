'use client';

import { useState } from 'react';
import { BookTemplate } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useTemplates } from '@/hooks/useTemplates';
import type { ColumnMapping, LookupRule, PrimaryKeyGenerationStrategy } from '@/types';

interface TemplateSelectorProps {
  tableName: string;
  onApply: (
    mappings: ColumnMapping[],
    rules: LookupRule[],
    primaryKeyGenerationStrategy?: PrimaryKeyGenerationStrategy,
    duplicateKeyColumns?: string[]
  ) => void;
}

export default function TemplateSelector({ tableName, onApply }: TemplateSelectorProps) {
  const { data: templates } = useTemplates();
  const [selectedId, setSelectedId] = useState('');

  const filteredTemplates = templates?.filter((t) => t.targetTable === tableName) ?? [];

  if (filteredTemplates.length === 0) return null;

  const handleApply = () => {
    const template = filteredTemplates.find((t) => t.id === selectedId);
    if (template) {
      onApply(
        template.mappings,
        template.lookupRules,
        template.primaryKeyGenerationStrategy,
        template.duplicateKeyColumns
      );
    }
  };

  return (
    <div className="flex items-center gap-2">
      <BookTemplate className="w-4 h-4 text-muted-foreground" />
      <Select value={selectedId} onValueChange={(v) => setSelectedId(v ?? '')}>
        <SelectTrigger className="w-[200px]">
          <SelectValue placeholder="Load template..." />
        </SelectTrigger>
        <SelectContent>
          {filteredTemplates.map((t) => (
            <SelectItem key={t.id} value={t.id}>
              {t.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Button variant="outline" size="sm" onClick={handleApply} disabled={!selectedId}>
        Apply
      </Button>
    </div>
  );
}
