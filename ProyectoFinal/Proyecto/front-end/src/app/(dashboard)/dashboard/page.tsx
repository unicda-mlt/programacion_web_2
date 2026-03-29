import { cookies } from 'next/headers';
import { env } from '@/utils/environment/private';
import { createServerClient } from '@/lib/api/client';

async function fetchCount(
  client: ReturnType<typeof createServerClient>,
  path: '/api/scrutinies' | '/api/users' | '/api/students',
) {
  try {
    const { data, response } = await client.GET(path, {
      params: { query: { Page: 1, PageSize: 1 } },
    });
    if (!response.ok) return '?';
    return (data as { total?: number; data?: unknown[] } | undefined)?.total
      ?? (data as { data?: unknown[] } | undefined)?.data?.length
      ?? '?';
  } catch {
    return '?';
  }
}

export default async function DashboardPage() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth-token')?.value ?? '';
  const client = createServerClient({ authToken: token, baseUrl: env.API_BASE_URL });

  const [scrutinies, users, students] = await Promise.all([
    fetchCount(client, '/api/scrutinies'),
    fetchCount(client, '/api/users'),
    fetchCount(client, '/api/students'),
  ]);

  const stats = [
    { label: 'Scrutinies', value: scrutinies, color: 'bg-blue-50 text-blue-700' },
    { label: 'Users', value: users, color: 'bg-purple-50 text-purple-700' },
    { label: 'Students', value: students, color: 'bg-green-50 text-green-700' },
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold text-zinc-900">Dashboard</h1>
      <p className="mt-1 text-sm text-zinc-500">Overview of the election system</p>

      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {stats.map(stat => (
          <div key={stat.label} className={`rounded-xl p-6 ${stat.color} border border-current/10`}>
            <p className="text-sm font-medium opacity-70">{stat.label}</p>
            <p className="mt-2 text-3xl font-bold">{stat.value}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
