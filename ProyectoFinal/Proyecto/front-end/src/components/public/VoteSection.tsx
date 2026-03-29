'use client';
import { useState, useEffect } from 'react';
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
  position: number;
  candidacies: CandidacyData[] | null;
}

interface CandidacyType {
  id: number;
  name: string | null;
}

interface Props {
  scrutinyId: string;
  slates: SlateData[];
  candidacyTypes: CandidacyType[];
}

const PROXY = '/api/proxy';

export function VoteSection({ scrutinyId, slates, candidacyTypes }: Props) {
  const [voted, setVoted] = useState(false);
  const [votedSlateId, setVotedSlateId] = useState<string | null>(null);
  const [voteDate, setVoteDate] = useState<string | null>(null);
  const [statusLoading, setStatusLoading] = useState(true);
  const [confirming, setConfirming] = useState<{ slateId: string; action: 'vote' | 'revoke' } | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    async function fetchVoteStatus() {
      try {
        const res = await fetch(`${PROXY}/api/public/scrutinies/${scrutinyId}/vote-status`);
        if (!res.ok) return; // not authenticated or error — leave default state
        const body = await res.json().catch(() => null);
        const data = body?.data;
        if (data?.hasVoted && data?.slateId) {
          setVoted(true);
          setVotedSlateId(data.slateId);
          setVoteDate(data.voteDate ?? null);
        }
      } catch {
        // silently ignore — unauthenticated users won't have a status
      } finally {
        setStatusLoading(false);
      }
    }
    fetchVoteStatus();
  }, [scrutinyId]);

  const typeMap = new Map(candidacyTypes.map(t => [t.id, t.name]));
  const sortedSlates = [...slates].sort((a, b) => a.position - b.position);

  async function handleVote(slateId: string) {
    setSubmitting(true);
    setError('');
    try {
      const res = await fetch(`${PROXY}/api/public/scrutinies/${scrutinyId}/vote`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ slateId }),
      });
      const body = await res.json().catch(() => ({}));
      if (body?.badMessage === 'Ya ha ejercido su voto') {
        setVoted(true);
        setVotedSlateId(slateId);
        setConfirming(null);
        return;
      }
      if (!res.ok || body?.badMessage) {
        setError(body?.badMessage ?? body?.title ?? 'Failed to submit vote.');
        setConfirming(null);
        return;
      }
      setVoted(true);
      setVotedSlateId(slateId);
      setVoteDate(new Date().toISOString());
      setConfirming(null);
    } catch {
      setError('Network error. Your vote was NOT submitted.');
      setConfirming(null);
    } finally {
      setSubmitting(false);
    }
  }

  async function handleRevoke() {
    setSubmitting(true);
    setError('');
    try {
      const res = await fetch(`${PROXY}/api/public/scrutinies/${scrutinyId}/vote`, {
        method: 'DELETE',
      });
      const body = await res.json().catch(() => ({}));
      if (!res.ok || body?.badMessage) {
        setError(body?.badMessage ?? body?.title ?? 'Failed to revoke vote.');
        setConfirming(null);
        return;
      }
      setVoted(false);
      setVotedSlateId(null);
      setVoteDate(null);
      setConfirming(null);
    } catch {
      setError('Network error. Your vote was NOT revoked.');
      setConfirming(null);
    } finally {
      setSubmitting(false);
    }
  }

  const confirmingSlate = sortedSlates.find(s => s.id === confirming?.slateId);

  if (statusLoading) {
    return (
      <div className="mt-8">
        <h2 className="text-lg font-semibold text-[#37474f] mb-4">Slates ({sortedSlates.length})</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          {sortedSlates.map(slate => (
            <div key={slate.id} className="rounded-xl border-2 border-[#e0e0e0] bg-white p-5 animate-pulse">
              <div className="mb-4 h-4 w-24 rounded bg-[#e8eaf6]" />
              <div className="space-y-2">
                <div className="h-8 rounded bg-[#f5f5f7]" />
                <div className="h-8 rounded bg-[#f5f5f7]" />
              </div>
              <div className="mt-4 pt-3 border-t border-[#e8eaf6]">
                <div className="h-9 rounded-lg bg-[#e8eaf6]" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="mt-8">
      <h2 className="text-lg font-semibold text-[#37474f] mb-4">
        Slates ({sortedSlates.length})
      </h2>

      {error && (
        <div className="mb-4 rounded-lg bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-600">
          {error}
        </div>
      )}

      {sortedSlates.length === 0 ? (
        <p className="text-sm text-[#78909c]">No slates available.</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {sortedSlates.map(slate => {
            const candidacies = [...(slate.candidacies ?? [])].sort(
              (a, b) => a.candidacyTypeId - b.candidacyTypeId
            );
            const isVoted = voted && votedSlateId === slate.id;

            return (
              <div
                key={slate.id}
                className={`flex flex-col rounded-xl border-2 p-5 shadow-sm transition-all ${
                  isVoted
                    ? 'border-[#5c6bc0] bg-[#e8eaf6]'
                    : 'border-[#e0e0e0] bg-white'
                }`}
              >
                <div className="mb-4 flex items-center justify-between">
                  <span className={`text-sm font-semibold ${isVoted ? 'text-[#5c6bc0]' : 'text-[#37474f]'}`}>
                    Slate #{slate.position}
                  </span>
                  <span className="text-xs text-[#90a4ae]">
                    {candidacies.length} candidate{candidacies.length !== 1 ? 's' : ''}
                  </span>
                </div>

                {candidacies.length === 0 ? (
                  <p className="text-xs text-[#90a4ae]">No candidates.</p>
                ) : (
                  <ul className="flex-1 space-y-3">
                    {candidacies.map((c, i) => (
                      <li key={i} className="flex items-center gap-3">
                        {c.imageUrl ? (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img
                            src={`/api/proxy/api/media/${c.imageUrl}`}
                            alt={`${c.name} ${c.lastName}`}
                            className="h-10 w-10 shrink-0 rounded-full object-cover"
                          />
                        ) : (
                          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[#c5cae9]">
                            <span className="text-xs font-semibold text-[#5c6bc0]">
                              {(c.name?.[0] ?? '') + (c.lastName?.[0] ?? '')}
                            </span>
                          </div>
                        )}
                        <div className="min-w-0">
                          <p className="truncate text-sm font-medium text-[#37474f]">
                            {c.name} {c.lastName}
                          </p>
                          <p className="text-xs text-[#90a4ae]">
                            {typeMap.get(c.candidacyTypeId) ?? `Type #${c.candidacyTypeId}`}
                          </p>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}

                <div className="mt-4 pt-3 border-t border-[#e8eaf6]">
                  {isVoted ? (
                    <button
                      onClick={() => setConfirming({ slateId: slate.id, action: 'revoke' })}
                      disabled={submitting}
                      className="w-full rounded-lg border border-red-300 py-2 text-sm font-medium text-red-600 hover:bg-red-50 transition-colors disabled:opacity-50"
                    >
                      <span className="block">Revoke Vote</span>
                      {voteDate && (
                        <span className="block text-xs font-normal text-red-400">
                          Voted {format(parseISO(voteDate), 'MMM d, yyyy h:mm a')}
                        </span>
                      )}
                    </button>
                  ) : (
                    <button
                      onClick={() => setConfirming({ slateId: slate.id, action: 'vote' })}
                      disabled={submitting || voted}
                      className="w-full rounded-lg bg-[#5c6bc0] py-2 text-sm font-medium text-white hover:bg-[#3949ab] transition-colors disabled:opacity-50"
                    >
                      Vote Now
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Confirmation dialog */}
      {confirming && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
          <div className="w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl">
            {confirming.action === 'revoke' ? (
              <>
                <h3 className="text-lg font-semibold text-[#37474f]">Revoke your vote?</h3>
                <p className="mt-2 text-sm text-[#546e7a]">
                  Your vote for <strong>Slate #{confirmingSlate?.position}</strong> will be removed.
                </p>
                <p className="mt-1 text-xs text-[#90a4ae]">You can vote again afterwards.</p>
              </>
            ) : (
              <>
                <h3 className="text-lg font-semibold text-[#37474f]">Confirm your vote</h3>
                <p className="mt-2 text-sm text-[#546e7a]">
                  You are voting for <strong>Slate #{confirmingSlate?.position}</strong>.
                </p>
                <p className="mt-1 text-xs text-[#90a4ae]">You can revoke your vote while the election is open.</p>
              </>
            )}
            <div className="mt-6 flex gap-3">
              <button
                onClick={() => confirming.action === 'revoke' ? handleRevoke() : handleVote(confirming.slateId)}
                disabled={submitting}
                className={`flex-1 rounded-lg py-2.5 text-sm font-medium text-white transition-colors disabled:opacity-50 ${
                  confirming.action === 'revoke' ? 'bg-red-500 hover:bg-red-600' : 'bg-[#5c6bc0] hover:bg-[#3949ab]'
                }`}
              >
                {submitting ? 'Processing…' : confirming.action === 'revoke' ? 'Confirm Revoke' : 'Confirm Vote'}
              </button>
              <button
                onClick={() => setConfirming(null)}
                disabled={submitting}
                className="flex-1 rounded-lg border border-[#e0e0e0] py-2.5 text-sm font-medium text-[#546e7a] hover:bg-[#f5f5f7] transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
