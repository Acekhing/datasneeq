'use client';

import { useState } from 'react';
import { ArrowRight, Link, ChevronDown, ChevronUp } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import BuildModeMappingEditor from './BuildModeMappingEditor';
import StringToBooleanEditor from './StringToBooleanEditor';
import type {
  TableSchema,
  ColumnMapping,
  LookupRule,
  TransformationType,
  StringToBooleanConfig,
} from '@/types';

interface MappingRowProps {
  excelColumn: string;
  selectedDbColumn: string;
  dbColumns: string[];
  usedDbColumns: Set<string>;
  confidence?: number;
  tableSchema: TableSchema;
  excelColumns: string[];
  connectionString: string;
  transformationType?: TransformationType;
  transformationConfig?: StringToBooleanConfig;
  lookupRule?: LookupRule;
  onChange: (dbCol: string) => void;
  onTransformationChange?: (type: TransformationType, config?: StringToBooleanConfig) => void;
  onFkModeChange?: (mode: 'lookup' | 'buildFromExcel' | 'useValueDirectly') => void;
  onBuildMappingsChange?: (mappings: ColumnMapping[]) => void;
  onBuildMatchColumnsChange?: (columns: string[]) => void;
}

const TRANSFORMATION_OPTIONS: { value: TransformationType; label: string }[] = [
  { value: 'none', label: 'None' },
  { value: 'stringToBoolean', label: 'String → Boolean' },
  { value: 'listPickFirst', label: 'Pick First Value' },
];

export default function MappingRow({
  excelColumn,
  selectedDbColumn,
  dbColumns,
  usedDbColumns,
  confidence,
  tableSchema,
  excelColumns,
  connectionString,
  transformationType = 'none',
  transformationConfig,
  lookupRule,
  onChange,
  onTransformationChange,
  onFkModeChange,
  onBuildMappingsChange,
  onBuildMatchColumnsChange,
}: MappingRowProps) {
  const [expanded, setExpanded] = useState(false);
  const isFk = tableSchema.foreignKeys.some((fk) => fk.columnName === selectedDbColumn);
  const colInfo = tableSchema.columns.find((c) => c.name === selectedDbColumn);
  const useBuildMode = lookupRule?.processingMode === 'buildFromExcel';
  const useValueDirectly = lookupRule?.processingMode === 'useValueDirectly';
  const needsOptions = selectedDbColumn && (isFk || onTransformationChange);

  return (
    <div className="rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors">
      <div className="flex items-center gap-3 p-3">
        <div className="flex-1 min-w-0">
          <span className="font-medium text-sm truncate block">{excelColumn}</span>
        </div>

        <ArrowRight className="w-4 h-4 text-muted-foreground shrink-0" />

        <div className="flex-1 min-w-0">
          <Select value={selectedDbColumn || 'unmapped'} onValueChange={(v) => onChange(!v || v === 'unmapped' ? '' : v)}>
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Select column..." />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="unmapped">
                <span className="text-muted-foreground">-- Skip --</span>
              </SelectItem>
              {dbColumns.map((col) => (
                <SelectItem
                  key={col}
                  value={col}
                  disabled={usedDbColumns.has(col) && col !== selectedDbColumn}
                >
                  {col}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex items-center gap-1 shrink-0 min-w-[100px] justify-end">
          {confidence !== undefined && confidence > 0 && (
            <Badge
              variant={confidence >= 0.9 ? 'default' : confidence >= 0.7 ? 'secondary' : 'outline'}
              className="text-xs"
            >
              {Math.round(confidence * 100)}%
            </Badge>
          )}
          {isFk && (
            <Badge variant="outline" className="text-xs gap-1">
              <Link className="w-3 h-3" />
              FK
            </Badge>
          )}
          {useValueDirectly && (
            <Badge variant="secondary" className="text-xs">
              Direct
            </Badge>
          )}
          {transformationType === 'listPickFirst' && (
            <Badge variant="secondary" className="text-xs">
              Pick first
            </Badge>
          )}
          {colInfo && !colInfo.isNullable && !colInfo.hasDefaultValue && (
            <Badge variant="destructive" className="text-xs">
              req
            </Badge>
          )}
          {needsOptions && (
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="h-7 w-7"
              onClick={() => setExpanded((e) => !e)}
            >
              {expanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
            </Button>
          )}
        </div>
      </div>

      {expanded && needsOptions && (
        <div className="px-3 pb-3 space-y-3">
          {isFk && (
            <div className="space-y-2">
              <label className="text-xs font-medium">FK Mode</label>
              <Select
                value={lookupRule?.processingMode || 'lookup'}
                onValueChange={(v) => onFkModeChange?.(v as 'lookup' | 'buildFromExcel' | 'useValueDirectly')}
              >
                <SelectTrigger className="h-8 w-48">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="lookup">Lookup Existing Record</SelectItem>
                  <SelectItem value="buildFromExcel">Build Related Table From Excel</SelectItem>
                  <SelectItem value="useValueDirectly">Use Value Directly</SelectItem>
                </SelectContent>
              </Select>
              {useBuildMode && lookupRule && onBuildMappingsChange && (
                <BuildModeMappingEditor
                  connectionString={connectionString}
                  lookupTable={lookupRule.lookupTable}
                  excelColumns={excelColumns}
                  mappings={lookupRule.foreignTableMappings || []}
                  buildMatchColumns={lookupRule.buildMatchColumns}
                  onChange={onBuildMappingsChange}
                  onBuildMatchColumnsChange={onBuildMatchColumnsChange}
                />
              )}
            </div>
          )}

          {!isFk && onTransformationChange && (
            <div className="space-y-2">
              <label className="text-xs font-medium">Transformation</label>
              <Select
                value={transformationType}
                onValueChange={(v) => onTransformationChange(v as TransformationType)}
              >
                <SelectTrigger className="h-8 w-48">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {TRANSFORMATION_OPTIONS.map((opt) => (
                    <SelectItem key={opt.value} value={opt.value}>
                      {opt.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {transformationType === 'stringToBoolean' && (
                <StringToBooleanEditor
                  config={
                    (transformationConfig as StringToBooleanConfig) || {
                      mappings: [],
                      defaultValue: false,
                      useDefaultWhenNoMatch: true,
                    }
                  }
                  onChange={(cfg) => onTransformationChange('stringToBoolean', cfg)}
                />
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
