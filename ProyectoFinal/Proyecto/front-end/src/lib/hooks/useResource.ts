'use client';
import { useState, useEffect, useCallback } from 'react';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { apiClient } from '@/lib/api/client';

interface UseResourceOptions {
  path: string;
  pageSize?: number;
}

type PaginatedBody<T> = {
  data?: T[];
  items?: T[];
  pagination?: { records?: number };
  total?: number;
  totalCount?: number;
};

type UntypedClient = {
  GET: (path: string, init: object) => Promise<{ data: unknown; response: Response }>;
  POST: (path: string, init: object) => Promise<{ data: unknown; response: Response }>;
  PATCH: (path: string, init: object) => Promise<{ data: unknown; response: Response }>;
};

export function useResource<T>({ path, pageSize = 10 }: UseResourceOptions) {
  const [data, setData] = useState<T[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const client = apiClient as unknown as UntypedClient;

  const fetch_ = useCallback(async (p: number) => {
    setLoading(true);
    setError(null);
    try {
      const { data: body, response } = await client.GET(path, {
        params: { query: { Page: p, PageSize: pageSize } },
      });
      if (response.status === 403) {
        setError("You don't have permission to view this resource.");
        return;
      }
      const b = body as PaginatedBody<T> | undefined;
      setData(b?.data ?? b?.items ?? []);
      setTotal(b?.pagination?.records ?? b?.total ?? b?.totalCount ?? 0);
    } catch {
      setError('Failed to load data. Backend may be unavailable.');
    } finally {
      setLoading(false);
    }
  }, [path, pageSize]);

  useEffect(() => { fetch_(page); }, [page, fetch_]);

  async function create(dto: unknown) {
    const { response } = await client.POST(path, { body: dto });
    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      throw parseProblemDetails(body);
    }
    await fetch_(page);
    toast.success('Created successfully');
  }

  async function update(id: number | string, dto: unknown) {
    const { response } = await client.PATCH(`${path}/{id}`, {
      params: { path: { id } },
      body: dto,
    });
    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      throw parseProblemDetails(body);
    }
    await fetch_(page);
    toast.success('Updated successfully');
  }

  return { data, total, page, setPage, loading, error, refetch: () => fetch_(page), create, update };
}
