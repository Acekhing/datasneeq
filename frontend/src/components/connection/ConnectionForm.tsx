'use client';

import { useState } from 'react';
import { Database, ArrowLeft } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useTestConnection, useTableSchema } from '@/hooks/useSchema';
import { getApiErrorMessage } from '@/lib/apiErrors';
import TableSelector from './TableSelector';
import type { TableSchema } from '@/types';

interface ConnectionFormProps {
  fileId: string;
  excelColumns: string[];
  onComplete: (connectionString: string, table: string, schema: TableSchema) => void;
  onBack: () => void;
}

export default function ConnectionForm({ fileId, excelColumns, onComplete, onBack }: ConnectionFormProps) {
  const [connectionString, setConnectionString] = useState('');
  const [tables, setTables] = useState<string[]>([]);
  const [selectedTable, setSelectedTable] = useState('');
  const [connected, setConnected] = useState(false);

  const testConn = useTestConnection();
  const tableSchema = useTableSchema(connectionString, selectedTable);

  const handleConnect = () => {
    testConn.mutate(connectionString, {
      onSuccess: (result) => {
        if (result.success && result.tables) {
          setTables(result.tables);
          setConnected(true);
        }
      },
    });
  };

  const handleTableSelect = (table: string) => {
    setSelectedTable(table);
  };

  const handleContinue = () => {
    if (tableSchema.data) {
      onComplete(connectionString, selectedTable, tableSchema.data);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Database className="w-5 h-5" />
          Database Connection
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-2">
          <Label htmlFor="connectionString">PostgreSQL Connection String</Label>
          <div className="flex gap-2">
            <Input
              id="connectionString"
              placeholder="Host=localhost;Port=5432;Database=mydb;Username=user;Password=pass"
              value={connectionString}
              onChange={(e) => setConnectionString(e.target.value)}
              className="flex-1"
            />
            <Button onClick={handleConnect} disabled={!connectionString || testConn.isPending}>
              {testConn.isPending ? 'Connecting...' : 'Connect'}
            </Button>
          </div>
          {testConn.isError && (
            <p className="text-sm text-destructive">Connection failed: {getApiErrorMessage(testConn.error)}</p>
          )}
          {testConn.data && !testConn.data.success && (
            <p className="text-sm text-destructive">Connection failed: {testConn.data.error}</p>
          )}
          {connected && (
            <p className="text-sm text-green-600">Connected successfully. {tables.length} tables found.</p>
          )}
        </div>

        {connected && tables.length > 0 && (
          <TableSelector
            tables={tables}
            selectedTable={selectedTable}
            onSelect={handleTableSelect}
          />
        )}

        {tableSchema.data && (
          <div className="bg-muted/50 rounded-lg p-4 space-y-2">
            <p className="text-sm font-medium">Table: {tableSchema.data.tableName}</p>
            <p className="text-sm text-muted-foreground">{tableSchema.data.columns.length} columns, {tableSchema.data.foreignKeys.length} foreign keys</p>
          </div>
        )}

        {tableSchema.isError && (
          <p className="text-sm text-destructive">Could not load table schema: {getApiErrorMessage(tableSchema.error)}</p>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={onBack}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back
          </Button>
          <Button onClick={handleContinue} disabled={!tableSchema.data}>
            Continue to Column Mapping
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
