import Link from 'next/link';
import { createServerClient } from '@/lib/api/client';
import { env } from '@/utils/environment/private';
import { ArrowRight } from 'lucide-react';
import { format, parseISO } from 'date-fns';

interface Scrutiny {
  id: string;
  title: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  imageUrl?: string | null;
}

async function getRecentScrutinies(): Promise<Scrutiny[]> {
  try {
    // Filter scrutinies that haven't ended yet (endDate >= today midnight)
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const { data } = await createServerClient({ baseUrl: env.API_BASE_URL }).GET('/api/public/scrutinies', {
      params: { query: { Page: 1, PageSize: 3, EndDate: format(today, 'yyyy-MM-dd HH:mm:ss') } },
      cache: 'no-store',
    });
    return (data as { data?: Scrutiny[] } | undefined)?.data ?? [];
  } catch {
    return [];
  }
}

export default async function HomePage() {
  const scrutinies = await getRecentScrutinies();

  return (
    <div className="mx-auto max-w-5xl px-4 py-16">
      {/* Hero */}
      <div className="text-center">
        <h1 className="text-4xl font-bold tracking-tight text-[#37474f] sm:text-5xl">
          LaVoz<span className="text-[#5c6bc0]">Estudiantil</span>
        </h1>
        <p className="mt-4 text-lg text-[#78909c]">
          University Student Voting Platform
        </p>
      </div>

      {/* Scrutinies */}
      <div className="mt-14">
        {scrutinies.length === 0 ? (
          <div className="rounded-2xl border border-[#e0e0e0] bg-white p-10 text-center shadow-sm">
            <p className="text-[#78909c]">No active elections at this time.</p>
            <Link href="/scrutinies" className="mt-4 inline-block text-sm text-[#5c6bc0] hover:underline">
              Browse all scrutinies &rarr;
            </Link>
          </div>
        ) : (
          <>
            <h2 className="text-center text-sm font-semibold uppercase tracking-widest text-[#90a4ae] mb-6">
              Active Elections
            </h2>
            <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {scrutinies.map((s) => (
                <div
                  key={s.id}
                  className="group flex flex-col overflow-hidden rounded-2xl border border-[#c5cae9] bg-white shadow-sm"
                >
                  {/* Cover */}
                  <div className="relative h-40 w-full overflow-hidden bg-[#e8eaf6]">
                    {s.imageUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={`/api/proxy/api/media/${s.imageUrl}`}
                        alt={s.title ?? 'Scrutiny'}
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center">
                        <span className="text-5xl font-bold text-[#c5cae9] select-none">
                          {s.title?.[0]?.toUpperCase() ?? '?'}
                        </span>
                      </div>
                    )}
                    <span className="absolute top-3 right-3 rounded-full bg-[#c8e6c9] px-2.5 py-1 text-xs font-semibold text-[#2e7d32]">
                      Open
                    </span>
                  </div>

                  {/* Body */}
                  <div className="flex flex-1 flex-col p-4">
                    <h3 className="font-semibold text-[#37474f] line-clamp-2">{s.title ?? '—'}</h3>
                    {s.description && (
                      <p className="mt-1 text-xs text-[#78909c] line-clamp-2">{s.description}</p>
                    )}
                    <p className="mt-2 text-xs text-[#90a4ae]">
                      {format(parseISO(s.startDate), 'MMM d, yyyy')} &rarr; {format(parseISO(s.endDate), 'MMM d, yyyy')}
                    </p>
                    <div className="mt-auto pt-4">
                      <Link
                        href={`/scrutinies/${s.id}`}
                        className="flex items-center justify-center gap-2 rounded-lg bg-[#5c6bc0] px-4 py-2.5 text-sm font-medium text-white hover:bg-[#3949ab] transition-colors"
                      >
                        View &amp; Vote <ArrowRight className="h-4 w-4" />
                      </Link>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-8 text-center">
              <Link
                href="/scrutinies"
                className="inline-flex items-center gap-2 rounded-lg border border-[#c5cae9] bg-white px-6 py-2.5 text-sm font-medium text-[#5c6bc0] hover:bg-[#e8eaf6] transition-colors"
              >
                View all scrutinies <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
