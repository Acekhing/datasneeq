'use client';

import { useState } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { StringToBooleanConfig, StringBooleanMapping } from '@/types';

interface StringToBooleanEditorProps {
  config: StringToBooleanConfig;
  onChange: (config: StringToBooleanConfig) => void;
}

export default function StringToBooleanEditor({
  config,
  onChange,
}: StringToBooleanEditorProps) {
  const [mappings, setMappings] = useState<StringBooleanMapping[]>(
    config.mappings?.length ? config.mappings : [{ excelValue: '', booleanValue: true }]
  );
  const [defaultValue, setDefaultValue] = useState(config.defaultValue);
  const [useDefault, setUseDefault] = useState(config.useDefaultWhenNoMatch !== false);

  const updateMappings = (next: StringBooleanMapping[]) => {
    setMappings(next);
    onChange({
      ...config,
      mappings: next.filter((m) => m.excelValue.trim()),
      defaultValue,
      useDefaultWhenNoMatch: useDefault,
    });
  };

  const updateDefault = (val: boolean) => {
    setDefaultValue(val);
    onChange({ ...config, mappings, defaultValue: val, useDefaultWhenNoMatch: useDefault });
  };

  const updateUseDefault = (val: boolean) => {
    setUseDefault(val);
    onChange({ ...config, mappings, defaultValue, useDefaultWhenNoMatch: val });
  };

  const handleMappingChange = (idx: number, field: 'excelValue' | 'booleanValue', value: string | boolean) => {
    const next = [...mappings];
    next[idx] = { ...next[idx], [field]: value };
    updateMappings(next);
  };

  const addRow = () => {
    updateMappings([...mappings, { excelValue: '', booleanValue: true }]);
  };

  const removeRow = (idx: number) => {
    updateMappings(mappings.filter((_, i) => i !== idx));
  };

  return (
    <div className="mt-2 space-y-3 pl-4 border-l-2 border-muted">
      <div className="space-y-2">
        {mappings.map((m, idx) => (
          <div key={idx} className="flex items-center gap-2">
            <Input
              placeholder="Excel value (e.g. Active)"
              value={m.excelValue}
              onChange={(e) => handleMappingChange(idx, 'excelValue', e.target.value)}
              className="flex-1 h-8 text-sm"
            />
            <Select
              value={m.booleanValue ? 'true' : 'false'}
              onValueChange={(v) => handleMappingChange(idx, 'booleanValue', v === 'true')}
            >
              <SelectTrigger className="w-24 h-8">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="true">true</SelectItem>
                <SelectItem value="false">false</SelectItem>
              </SelectContent>
            </Select>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="h-8 w-8 shrink-0"
              onClick={() => removeRow(idx)}
            >
              <Trash2 className="w-4 h-4" />
            </Button>
          </div>
        ))}
        <Button type="button" variant="outline" size="sm" onClick={addRow} className="gap-1">
          <Plus className="w-3 h-3" />
          Add mapping
        </Button>
      </div>
      <div className="flex items-center gap-4">
        <Label className="text-xs flex items-center gap-2">
          <input
            type="checkbox"
            checked={useDefault}
            onChange={(e) => updateUseDefault(e.target.checked)}
          />
          Use default when no match
        </Label>
        {useDefault && (
          <Select value={defaultValue ? 'true' : 'false'} onValueChange={(v) => updateDefault(v === 'true')}>
            <SelectTrigger className="w-24 h-8">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="true">true</SelectItem>
              <SelectItem value="false">false</SelectItem>
            </SelectContent>
          </Select>
        )}
      </div>
    </div>
  );
}
