import Link from 'next/link';
import { createServerClient } from '@/lib/api/client';
import { env } from '@/utils/environment/private';
import { format, parseISO } from 'date-fns';

interface Scrutiny {
  id: string;
  title: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  imageUrl?: string | null;
}

async function getScrutinies(): Promise<Scrutiny[]> {
  try {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const { data } = await createServerClient({ baseUrl: env.API_BASE_URL }).GET('/api/public/scrutinies', {
      params: { query: { Page: 1, PageSize: 20, EndDate: format(today, 'yyyy-MM-dd HH:mm:ss') } },
      cache: 'no-store',
    });
    return (data as { data?: Scrutiny[] } | undefined)?.data ?? [];
  } catch {
    return [];
  }
}

export default async function ScrutiniesPage() {
  const scrutinies = await getScrutinies();

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-bold text-[#37474f]">Active Elections</h1>
      <p className="mt-1 text-sm text-[#78909c]">Open scrutinies available for voting</p>

      {scrutinies.length === 0 ? (
        <p className="mt-6 text-[#78909c]">No active elections at this time.</p>
      ) : (
        <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {scrutinies.map((s) => (
            <Link
              key={s.id}
              href={`/scrutinies/${s.id}`}
              className="group flex flex-col overflow-hidden rounded-2xl border border-[#c5cae9] bg-white shadow-sm hover:shadow-md transition-shadow"
            >
              {/* Cover image */}
              <div className="relative h-44 w-full overflow-hidden bg-[#e8eaf6]">
                {s.imageUrl ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={`/api/proxy/api/media/${s.imageUrl}`}
                    alt={s.title ?? 'Scrutiny'}
                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center">
                    <span className="text-4xl font-bold text-[#c5cae9] select-none">
                      {s.title?.[0]?.toUpperCase() ?? '?'}
                    </span>
                  </div>
                )}
                <span className="absolute top-3 right-3 rounded-full bg-[#c8e6c9] px-2.5 py-1 text-xs font-semibold text-[#2e7d32]">
                  Open
                </span>
              </div>

              {/* Card body */}
              <div className="flex flex-1 flex-col p-4">
                <h2 className="font-semibold text-[#37474f] line-clamp-2 group-hover:text-[#5c6bc0] transition-colors">
                  {s.title ?? '—'}
                </h2>
                {s.description && (
                  <p className="mt-1 text-xs text-[#78909c] line-clamp-2">{s.description}</p>
                )}
                <p className="mt-2 text-xs text-[#90a4ae]">
                  {format(parseISO(s.startDate), 'MMM d, yyyy')} &rarr; {format(parseISO(s.endDate), 'MMM d, yyyy')}
                </p>
                <div className="mt-4 pt-3 border-t border-[#e8eaf6]">
                  <span className="text-sm font-medium text-[#5c6bc0] group-hover:underline">
                    View &amp; Vote →
                  </span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
