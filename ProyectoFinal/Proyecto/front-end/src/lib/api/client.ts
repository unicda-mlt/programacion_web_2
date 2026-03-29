import createClient from 'openapi-fetch';
import type { paths } from './schema.d';
import { env } from '@/utils/environment/public';

const BASE_URL = env.NEXT_PUBLIC_API_BASE_URL;

export const apiClient = createClient<paths>({ baseUrl: BASE_URL });

// Middleware: cookies are sent automatically by the browser for same-origin requests.
// For server-side usage, callers must set headers manually.
apiClient.use({
  async onRequest({ request }) {
    return request;
  },
  async onResponse({ response }) {
    return response;
  },
});

export function createServerClient({
  cookieHeader,
  authToken,
  baseUrl,
}: {
  cookieHeader?: string;
  authToken?: string;
  baseUrl?: string;
} = {}) {
  return createClient<paths>({
    baseUrl: baseUrl ?? env.NEXT_PUBLIC_API_BASE_URL,
    headers: {
      ...(cookieHeader ? { cookie: cookieHeader } : {}),
      ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
    },
  });
}
