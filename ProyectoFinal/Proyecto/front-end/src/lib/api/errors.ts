import type { ApiError } from '@/types/api';

export function parseProblemDetails(body: unknown): ApiError {
  if (typeof body !== 'object' || body === null) {
    return { title: 'An unexpected error occurred.' };
  }
  const obj = body as Record<string, unknown>;
  return {
    title: (obj.title as string) || (obj.message as string) || 'An error occurred.',
    detail: obj.detail as string | undefined,
    status: obj.status as number | undefined,
    errors: obj.errors as Record<string, string[]> | undefined,
  };
}
