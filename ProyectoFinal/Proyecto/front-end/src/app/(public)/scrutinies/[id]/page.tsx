import Link from 'next/link';
import { notFound } from 'next/navigation';
import { createServerClient } from '@/lib/api/client';
import { env } from '@/utils/environment/private';
import { ArrowLeft } from 'lucide-react';
import { VoteSection } from '@/components/public/VoteSection';
import { format, parseISO } from 'date-fns';

interface CandidacyData {
  slateId: string;
  candidacyTypeId: number;
  name: string | null;
  lastName: string | null;
  imageUrl?: string | null;
}

interface SlateData {
  id: string;
  scrutinyId: string;
  position: number;
  candidacies: CandidacyData[] | null;
}

interface ScrutinyDetail {
  id: string;
  title: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  imageUrl?: string | null;
  slates: SlateData[] | null;
}

interface CandidacyType {
  id: number;
  name: string | null;
  position: number;
}

const BACKEND = env.API_BASE_URL;

async function getScrutiny(id: string): Promise<ScrutinyDetail | null> {
  try {
    const { data } = await createServerClient({ baseUrl: BACKEND }).GET(
      '/api/public/scrutinies/{id}',
      { params: { path: { id } }, cache: 'no-store' }
    );
    return (data as { data?: ScrutinyDetail } | undefined)?.data ?? null;
  } catch {
    return null;
  }
}

async function getCandidacyTypes(): Promise<CandidacyType[]> {
  try {
    const { data } = await createServerClient({ baseUrl: BACKEND }).GET(
      '/api/public/candidacy-types',
      { params: { query: { Page: 1, PageSize: 100 } }, cache: 'no-store' }
    );
    return (data as { data?: CandidacyType[] } | undefined)?.data ?? [];
  } catch {
    return [];
  }
}

export default async function ScrutinyDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const [scrutiny, candidacyTypes] = await Promise.all([
    getScrutiny(id),
    getCandidacyTypes(),
  ]);

  if (!scrutiny) notFound();

  const sortedSlates = [...(scrutiny.slates ?? [])].sort(
    (a, b) => a.position - b.position
  );

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Back */}
      <Link
        href="/scrutinies"
        className="inline-flex items-center gap-1.5 text-sm text-[#78909c] hover:text-[#37474f] mb-6"
      >
        <ArrowLeft className="h-3.5 w-3.5" /> Back to Scrutinies
      </Link>

      {/* Header */}
      <div className="rounded-2xl border border-[#c5cae9] bg-[#e8eaf6] p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex-1 min-w-0">
            <span className="inline-block rounded-full bg-[#c8e6c9] px-3 py-1 text-xs font-semibold text-[#2e7d32] mb-2">
              Open for Voting
            </span>
            <h1 className="text-2xl font-bold text-[#37474f]">
              {scrutiny.title ?? '—'}
            </h1>
            {scrutiny.description && (
              <p className="mt-2 text-sm text-[#546e7a]">{scrutiny.description}</p>
            )}
            <p className="mt-2 text-xs text-[#90a4ae]">
              {format(parseISO(scrutiny.startDate), 'MMM d, yyyy h:mm a')} &rarr;{' '}
              {format(parseISO(scrutiny.endDate), 'MMM d, yyyy h:mm a')}
            </p>
          </div>
          {scrutiny.imageUrl && (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={`/api/proxy/api/media/${scrutiny.imageUrl}`}
              alt="Scrutiny cover"
              className="h-28 w-28 shrink-0 rounded-xl object-cover shadow-sm"
            />
          )}
        </div>
      </div>

      {/* Slates + voting */}
      <VoteSection
        scrutinyId={scrutiny.id}
        slates={sortedSlates}
        candidacyTypes={candidacyTypes}
      />
    </div>
  );
}
