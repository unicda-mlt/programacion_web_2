'use client';
import { format, parseISO } from 'date-fns';
import { useVoteStatus, type ScrutinyVoteStatus } from '@/lib/hooks/useVoteStatus';
import { Activity, Wifi, WifiOff, RefreshCw, Users } from 'lucide-react';

function StatusBadge({ status }: { status: string }) {
  const styles: Record<string, string> = {
    connected: 'bg-green-100 text-green-700',
    connecting: 'bg-yellow-100 text-yellow-700',
    reconnecting: 'bg-yellow-100 text-yellow-700',
    disconnected: 'bg-red-100 text-red-600',
  };
  const icons: Record<string, React.ReactNode> = {
    connected: <Wifi className="h-3 w-3" />,
    connecting: <RefreshCw className="h-3 w-3 animate-spin" />,
    reconnecting: <RefreshCw className="h-3 w-3 animate-spin" />,
    disconnected: <WifiOff className="h-3 w-3" />,
  };
  const cls = styles[status] ?? styles.disconnected;
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${cls}`}>
      {icons[status]}
      {status.charAt(0).toUpperCase() + status.slice(1)}
    </span>
  );
}

function SlateBar({ voteCount, totalVotes, position }: { voteCount: number; totalVotes: number; position: number }) {
  const pct = totalVotes > 0 ? Math.round((voteCount / totalVotes) * 100) : 0;
  return (
    <div className="w-full bg-zinc-100 rounded-full h-2">
      <div
        className="h-2 rounded-full transition-all duration-700"
        style={{
          width: `${pct}%`,
          backgroundColor: `hsl(${(position * 47) % 360}, 65%, 50%)`,
        }}
      />
    </div>
  );
}

function formatTimeLeft(mins: number): string {
  if (mins < 60) return `${mins} min left`;
  const hours = Math.round(mins / 60);
  if (hours < 24) return `${hours} hr left`;
  const days = Math.round(mins / 1440);
  if (days < 7) return `${days} day${days !== 1 ? 's' : ''} left`;
  const weeks = Math.round(mins / 10080);
  if (weeks < 4) return `${weeks} week${weeks !== 1 ? 's' : ''} left`;
  const months = Math.round(mins / 43200);
  return `${months} month${months !== 1 ? 's' : ''} left`;
}

function ScrutinyCard({ scrutiny }: { scrutiny: ScrutinyVoteStatus }) {
  const end = parseISO(scrutiny.endDate);
  const now = new Date();
  const minsLeft = Math.max(0, Math.round((end.getTime() - now.getTime()) / 60000));

  return (
    <div className="rounded-xl border border-zinc-200 bg-white p-5 shadow-sm">
      {/* Header */}
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="font-semibold text-zinc-900 truncate">{scrutiny.title}</h3>
          {scrutiny.description && (
            <p className="mt-0.5 text-xs text-zinc-500 line-clamp-2">{scrutiny.description}</p>
          )}
        </div>
        <div className="shrink-0 text-right">
          <div className="flex items-center gap-1 text-sm font-semibold text-zinc-800">
            <Users className="h-4 w-4 text-zinc-400" />
            {scrutiny.totalVotes}
          </div>
          <p className="text-xs text-zinc-400">total votes</p>
        </div>
      </div>

      {/* Time info */}
      <div className="mt-3 flex gap-4 text-xs text-zinc-500">
        <span>Start: {format(parseISO(scrutiny.startDate), 'MMM d, HH:mm')}</span>
        <span>End: {format(parseISO(scrutiny.endDate), 'MMM d, HH:mm')}</span>
        {minsLeft > 0 && (
          <span className="text-orange-500 font-medium">{formatTimeLeft(minsLeft)}</span>
        )}
      </div>

      {/* Slates */}
      {scrutiny.slates.length === 0 ? (
        <p className="mt-4 text-sm text-zinc-400 italic">No slates yet.</p>
      ) : (
        <ul className="mt-4 space-y-3">
          {scrutiny.slates
            .slice()
            .sort((a, b) => b.voteCount - a.voteCount)
            .map((slate) => {
              const pct = scrutiny.totalVotes > 0
                ? Math.round((slate.voteCount / scrutiny.totalVotes) * 100)
                : 0;
              return (
                <li key={slate.id}>
                  <div className="flex items-center justify-between mb-1">
                    <div className="flex items-center gap-2 min-w-0">
                      {slate.firstCandidacy?.imageUrl && (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img
                          src={`/api/proxy/api/media/${slate.firstCandidacy.imageUrl}`}
                          alt={`${slate.firstCandidacy.name} ${slate.firstCandidacy.lastName}`}
                          className="h-6 w-6 rounded-full object-cover shrink-0"
                        />
                      )}
                      <span className="text-sm font-medium text-zinc-700 truncate">
                        {slate.firstCandidacy
                          ? `${slate.firstCandidacy.name} ${slate.firstCandidacy.lastName}`
                          : `Slate #${slate.position}`}
                      </span>
                    </div>
                    <div className="shrink-0 ml-3 text-right">
                      <span className="text-sm font-semibold text-zinc-800">{slate.voteCount}</span>
                      <span className="ml-1 text-xs text-zinc-400">{pct}%</span>
                    </div>
                  </div>
                  <SlateBar
                    voteCount={slate.voteCount}
                    totalVotes={scrutiny.totalVotes}
                    position={slate.position}
                  />
                </li>
              );
            })}
        </ul>
      )}
    </div>
  );
}

export default function VoteStatusPage() {
  const { data, status, error } = useVoteStatus();

  return (
    <div>
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-zinc-900 flex items-center gap-2">
            <Activity className="h-6 w-6 text-blue-600" />
            Live Vote Status
          </h1>
          <p className="mt-1 text-sm text-zinc-500">
            Real-time results for open scrutinies — updates every 5 seconds
          </p>
        </div>
        <StatusBadge status={status} />
      </div>

      {/* Error banner */}
      {error && (
        <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Content */}
      <div className="mt-6">
        {status === 'connecting' && data.scrutinies.length === 0 && (
          <div className="flex items-center justify-center h-48 text-zinc-400 text-sm gap-2">
            <RefreshCw className="h-4 w-4 animate-spin" />
            Connecting to live feed…
          </div>
        )}

        {status !== 'connecting' && data.scrutinies.length === 0 && !error && (
          <div className="flex flex-col items-center justify-center h-48 text-zinc-400">
            <Activity className="h-8 w-8 mb-2 opacity-30" />
            <p className="text-sm">No open scrutinies right now.</p>
          </div>
        )}

        {data.scrutinies.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {data.scrutinies.map((s) => (
              <ScrutinyCard key={s.id} scrutiny={s} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
