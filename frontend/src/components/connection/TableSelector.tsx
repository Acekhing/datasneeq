'use client';

import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface TableSelectorProps {
  tables: string[];
  selectedTable: string;
  onSelect: (table: string) => void;
}

export default function TableSelector({ tables, selectedTable, onSelect }: TableSelectorProps) {
  return (
    <div className="space-y-2">
      <Label>Select Target Table</Label>
      <Select value={selectedTable} onValueChange={(v) => { if (v) onSelect(v); }}>
        <SelectTrigger>
          <SelectValue placeholder="Choose a table..." />
        </SelectTrigger>
        <SelectContent>
          {tables.map((table) => (
            <SelectItem key={table} value={table}>
              {table}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
