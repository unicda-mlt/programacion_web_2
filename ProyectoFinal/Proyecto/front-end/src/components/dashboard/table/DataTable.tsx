'use client';
import type { ReactNode } from 'react';
import { TableSkeleton } from '@/components/shared/Skeleton';

interface Column<T> {
  key: keyof T | string;
  label: string;
  render?: (row: T) => ReactNode;
}

interface Props<T> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  error?: string | null;
  page: number;
  pageSize?: number;
  total: number;
  onPageChange: (page: number) => void;
  toolbar?: ReactNode;
  emptyLabel?: string;
}

export function DataTable<T extends { id?: number | string }>({
  columns, data, loading, error, page, pageSize = 10, total, onPageChange, toolbar, emptyLabel = 'No records found.'
}: Props<T>) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div>
      {toolbar && <div className="mb-4">{toolbar}</div>}

      {error ? (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-600">
          {error}
        </div>
      ) : loading ? (
        <TableSkeleton rows={5} cols={columns.length} />
      ) : data.length === 0 ? (
        <div className="rounded-lg border border-dashed border-zinc-300 p-12 text-center">
          <p className="text-sm text-zinc-500">{emptyLabel}</p>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-zinc-200">
          <table className="min-w-full divide-y divide-zinc-200 bg-white text-sm">
            <thead className="bg-zinc-50">
              <tr>
                {columns.map(col => (
                  <th key={String(col.key)} className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    {col.label}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100">
              {data.map((row, i) => (
                <tr key={row.id ?? i} className="hover:bg-zinc-50">
                  {columns.map(col => (
                    <td key={String(col.key)} className="px-4 py-3 text-zinc-700">
                      {col.render ? col.render(row) : String((row as Record<string, unknown>)[String(col.key)] ?? '')}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between text-sm text-zinc-500">
          <span>Page {page} of {totalPages} ({total} total)</span>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="rounded-md border border-zinc-300 px-3 py-1.5 hover:bg-zinc-50 disabled:opacity-40"
            >
              Previous
            </button>
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="rounded-md border border-zinc-300 px-3 py-1.5 hover:bg-zinc-50 disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
