'use client';
import { useState, useEffect, useCallback } from 'react';
import { format, parseISO } from 'date-fns';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { Modal } from '@/components/shared/Modal';
import { ArrowLeft, Plus, Pencil, ChevronDown, ChevronRight, Upload, Trash2 } from 'lucide-react';

const PROXY = '/api/proxy';

// --- Types ---
interface ScrutinyDetail {
  id: string;
  statusId: number;
  status: { id: number; name: string | null };
  title: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  imageUrl?: string | null;
  signFileUrl?: string | null;
}

interface Slate {
  id: string;
  scrutinyId: string;
  position: number;
  countCandidacies: number;
}

interface Candidacy {
  id: string;
  scrutinyId: string;
  slateId: string;
  candidacyTypeId: number;
  name: string | null;
  lastName: string | null;
  imageUrl?: string | null;
  candidacyTypePosition: number;
}

interface CandidacyType {
  id: number;
  name: string | null;
  position: number;
}

// --- Status config ---
const STATUS: Record<number, { label: string; cls: string }> = {
  1: { label: 'Draft', cls: 'bg-zinc-100 text-zinc-600' },
  2: { label: 'Open', cls: 'bg-green-100 text-green-700' },
  3: { label: 'Closed', cls: 'bg-red-100 text-red-600' },
  4: { label: 'Signed', cls: 'bg-blue-100 text-blue-700' },
};

// --- API helpers ---
async function apiReq(path: string, init?: RequestInit) {
  const res = await fetch(PROXY + path, init);
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw parseProblemDetails(body);
  }
  return res.json();
}

async function jsonReq(path: string, method: string, body?: unknown) {
  return apiReq(path, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
}

function imgSrc(url: string | null | undefined) {
  if (!url) return null;
  return `${PROXY}/api/media/${url}`;
}

// --- Component ---
export default function ScrutinyDetailPage() {
  const params = useParams();
  const id = params.id as string;

  const [scrutiny, setScrutiny] = useState<ScrutinyDetail | null>(null);
  const [slates, setSlates] = useState<Slate[]>([]);
  const [candidacies, setCandidacies] = useState<Record<string, Candidacy[]>>({});
  const [candidacyTypes, setCandidacyTypes] = useState<CandidacyType[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  // Modals
  const [addSlateModal, setAddSlateModal] = useState(false);
  const [slatePosition, setSlatePosition] = useState('');
  type CandidacyModal = { slateId: string; editing: Candidacy | null };
  const [candidacyModal, setCandidacyModal] = useState<CandidacyModal | null>(null);
  const [candidacyForm, setCandidacyForm] = useState({ candidacyTypeId: '', name: '', lastName: '' });
  const [signModal, setSignModal] = useState(false);
  const [signFile, setSignFile] = useState<File | null>(null);
  const [coverModal, setCoverModal] = useState(false);
  const [coverFile, setCoverFile] = useState<File | null>(null);
  const [photoModal, setPhotoModal] = useState<{ slateId: string; candidacyId: string } | null>(null);
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [formError, setFormError] = useState('');

  // --- Data fetching ---
  const loadScrutiny = useCallback(async () => {
    const res = await apiReq(`/api/scrutinies/${id}`);
    setScrutiny(res.data);
  }, [id]);

  const loadCandidaciesForSlate = useCallback(async (slateId: string) => {
    const res = await apiReq(`/api/scrutinies/${id}/slates/${slateId}/candidacies?Page=1&PageSize=100`);
    setCandidacies(prev => ({ ...prev, [slateId]: res.data ?? [] }));
  }, [id]);

  const loadSlates = useCallback(async () => {
    const res = await apiReq(`/api/scrutinies/${id}/slates?Page=1&PageSize=100`);
    const list: Slate[] = res.data ?? [];
    setSlates(list);
    await Promise.all(list.map(s => loadCandidaciesForSlate(s.id)));
    setExpanded(prev => {
      const next = { ...prev };
      list.forEach(s => { if (!(s.id in next)) next[s.id] = true; });
      return next;
    });
  }, [id, loadCandidaciesForSlate]);

  const loadCandidacyTypes = useCallback(async () => {
    const res = await apiReq(`/api/candidacy-types?Page=1&PageSize=100`);
    setCandidacyTypes(res.data ?? []);
  }, []);

  useEffect(() => {
    setLoading(true);
    Promise.all([loadScrutiny(), loadSlates(), loadCandidacyTypes()])
      .catch(e => setPageError((e as { title?: string }).title ?? 'Failed to load.'))
      .finally(() => setLoading(false));
  }, [loadScrutiny, loadSlates, loadCandidacyTypes]);

  const isDraft = scrutiny?.statusId === 1;

  // --- Status transitions ---
  async function openScrutiny() {
    if (!confirm('Open this scrutiny for voting? At least 2 slates are required.')) return;
    setBusy(true);
    try {
      await apiReq(`/api/scrutinies/${id}/open`, { method: 'PATCH' });
      toast.success('Scrutiny opened.');
      await loadScrutiny();
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to open.');
    } finally { setBusy(false); }
  }

  async function closeScrutiny() {
    if (!confirm('Close this scrutiny? Voting will no longer be available.')) return;
    setBusy(true);
    try {
      await apiReq(`/api/scrutinies/${id}/close`, { method: 'POST' });
      toast.success('Scrutiny closed.');
      await loadScrutiny();
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to close.');
    } finally { setBusy(false); }
  }

  async function handleSign(e: React.FormEvent) {
    e.preventDefault();
    if (!signFile) return;
    setBusy(true);
    try {
      const fd = new FormData();
      fd.append('file', signFile);
      await apiReq(`/api/scrutinies/${id}/sign`, { method: 'POST', body: fd });
      toast.success('Scrutiny signed.');
      setSignModal(false);
      setSignFile(null);
      await loadScrutiny();
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to sign.');
    } finally { setBusy(false); }
  }

  // --- Cover image upload ---
  async function handleUploadCover(e: React.FormEvent) {
    e.preventDefault();
    if (!coverFile) return;
    setBusy(true);
    try {
      const fd = new FormData();
      fd.append('file', coverFile);
      await apiReq(`/api/scrutinies/${id}/image`, { method: 'POST', body: fd });
      toast.success('Cover image updated.');
      setCoverModal(false);
      setCoverFile(null);
      await loadScrutiny();
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to upload image.');
    } finally { setBusy(false); }
  }

  // --- Slate actions ---
  async function handleAddSlate(e: React.FormEvent) {
    e.preventDefault();
    setFormError('');
    setBusy(true);
    try {
      await jsonReq(`/api/scrutinies/${id}/slates`, 'POST', { position: Number(slatePosition) });
      toast.success('Slate added.');
      setAddSlateModal(false);
      setSlatePosition('');
      await loadSlates();
    } catch (e) {
      setFormError((e as { title?: string }).title ?? 'Failed to add slate.');
    } finally { setBusy(false); }
  }

  // --- Candidacy actions ---
  function openAddCandidacy(slateId: string) {
    setCandidacyForm({ candidacyTypeId: String(candidacyTypes[0]?.id ?? ''), name: '', lastName: '' });
    setFormError('');
    setCandidacyModal({ slateId, editing: null });
  }

  function openEditCandidacy(slateId: string, c: Candidacy) {
    setCandidacyForm({ candidacyTypeId: String(c.candidacyTypeId), name: c.name ?? '', lastName: c.lastName ?? '' });
    setFormError('');
    setCandidacyModal({ slateId, editing: c });
  }

  async function handleSaveCandidacy(e: React.FormEvent) {
    e.preventDefault();
    if (!candidacyModal) return;
    setFormError('');
    setBusy(true);
    const { slateId, editing } = candidacyModal;
    try {
      const dto = {
        candidacyTypeId: Number(candidacyForm.candidacyTypeId),
        name: candidacyForm.name,
        lastName: candidacyForm.lastName,
      };
      if (editing) {
        await jsonReq(`/api/scrutinies/${id}/slates/${slateId}/candidacies/${editing.id}`, 'PATCH', dto);
        toast.success('Candidacy updated.');
      } else {
        await jsonReq(`/api/scrutinies/${id}/slates/${slateId}/candidacies`, 'POST', dto);
        toast.success('Candidacy added.');
      }
      setCandidacyModal(null);
      await loadCandidaciesForSlate(slateId);
    } catch (e) {
      setFormError((e as { title?: string }).title ?? 'Failed to save candidacy.');
    } finally { setBusy(false); }
  }

  // --- Delete slate ---
  async function handleDeleteSlate(slateId: string) {
    if (!confirm('Delete this slate and all its candidacies? This cannot be undone.')) return;
    setBusy(true);
    try {
      await apiReq(`/api/scrutinies/${id}/slates/${slateId}`, { method: 'DELETE' });
      toast.success('Slate deleted.');
      await loadSlates();
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to delete slate.');
    } finally { setBusy(false); }
  }

  // --- Delete candidacy ---
  async function handleDeleteCandidacy(slateId: string, candidacyId: string) {
    if (!confirm('Delete this candidacy? This cannot be undone.')) return;
    setBusy(true);
    try {
      await apiReq(`/api/scrutinies/${id}/slates/${slateId}/candidacies/${candidacyId}`, { method: 'DELETE' });
      toast.success('Candidacy deleted.');
      await loadCandidaciesForSlate(slateId);
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to delete candidacy.');
    } finally { setBusy(false); }
  }

  // --- Photo upload ---
  async function handleUploadPhoto(e: React.FormEvent) {
    e.preventDefault();
    if (!photoFile || !photoModal) return;
    setBusy(true);
    try {
      const fd = new FormData();
      fd.append('file', photoFile);
      await apiReq(
        `/api/scrutinies/${id}/slates/${photoModal.slateId}/candidacies/${photoModal.candidacyId}/image`,
        { method: 'POST', body: fd }
      );
      toast.success('Photo uploaded.');
      setPhotoModal(null);
      setPhotoFile(null);
      await loadCandidaciesForSlate(photoModal.slateId);
    } catch (e) {
      toast.error((e as { title?: string }).title ?? 'Failed to upload photo.');
    } finally { setBusy(false); }
  }

  // --- Render ---
  if (loading) return <div className="p-8 text-sm text-zinc-400">Loading…</div>;
  if (pageError || !scrutiny) return <div className="p-8 text-sm text-red-600">{pageError ?? 'Scrutiny not found.'}</div>;

  const s = STATUS[scrutiny.statusId];

  return (
    <div className="space-y-6">
      {/* Back + heading */}
      <div>
        <Link href="/dashboard/scrutinies" className="inline-flex items-center gap-1.5 text-sm text-zinc-500 hover:text-zinc-800 mb-3">
          <ArrowLeft className="h-3.5 w-3.5" /> Back to Scrutinies
        </Link>
        <div className="flex items-start justify-between gap-4">
          <div className="flex items-start gap-4">
            {/* Cover image */}
            <div className="relative shrink-0">
              {imgSrc(scrutiny.imageUrl) ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={imgSrc(scrutiny.imageUrl)!}
                  alt="Cover"
                  className="h-20 w-20 rounded-xl object-cover border border-zinc-200"
                />
              ) : (
                <div className="flex h-20 w-20 items-center justify-center rounded-xl bg-zinc-100 border border-zinc-200">
                  <span className="text-2xl font-bold text-zinc-300">
                    {scrutiny.title?.[0]?.toUpperCase() ?? '?'}
                  </span>
                </div>
              )}
              {isDraft && (
                <button
                  onClick={() => { setCoverFile(null); setCoverModal(true); }}
                  className="absolute -bottom-2 -right-2 flex h-7 w-7 items-center justify-center rounded-full bg-white border border-zinc-300 shadow-sm hover:bg-zinc-50"
                  title="Upload cover image"
                >
                  <Upload className="h-3.5 w-3.5 text-zinc-500" />
                </button>
              )}
            </div>
            <div>
              <h1 className="text-2xl font-bold text-zinc-900">{scrutiny.title ?? '—'}</h1>
              {scrutiny.description && <p className="mt-1 text-sm text-zinc-500">{scrutiny.description}</p>}
              <p className="mt-1 text-xs text-zinc-400">
                {format(parseISO(scrutiny.startDate), 'MMM d, yyyy h:mm a')} &rarr; {format(parseISO(scrutiny.endDate), 'MMM d, yyyy h:mm a')}
              </p>
            </div>
          </div>
          <span className={`shrink-0 rounded-full px-3 py-1 text-sm font-medium ${s?.cls ?? 'bg-zinc-100 text-zinc-600'}`}>
            {s?.label ?? String(scrutiny.statusId)}
          </span>
        </div>
      </div>

      {/* Status actions bar */}
      <div className="flex flex-wrap items-center gap-2 rounded-lg border border-zinc-200 bg-zinc-50 px-4 py-3">
        <span className="mr-auto text-sm font-medium text-zinc-600">Status actions</span>
        {scrutiny.statusId === 1 && (
          <button
            onClick={openScrutiny}
            disabled={busy}
            className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
          >
            Open for Voting
          </button>
        )}
        {scrutiny.statusId === 2 && (
          <button
            onClick={closeScrutiny}
            disabled={busy}
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
          >
            Close Voting
          </button>
        )}
        {scrutiny.statusId === 3 && (
          <button
            onClick={() => { setSignFile(null); setSignModal(true); }}
            disabled={busy}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
          >
            Sign Scrutiny
          </button>
        )}
        {scrutiny.statusId === 4 && scrutiny.signFileUrl && (
          <a
            href={imgSrc(scrutiny.signFileUrl) ?? '#'}
            target="_blank"
            rel="noopener noreferrer"
            className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-100"
          >
            View Signature
          </a>
        )}
      </div>

      {/* Slates */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-semibold text-zinc-800">Slates</h2>
          {isDraft && (
            <button
              onClick={() => { setSlatePosition(''); setFormError(''); setAddSlateModal(true); }}
              className="flex items-center gap-2 rounded-lg bg-blue-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-blue-700"
            >
              <Plus className="h-4 w-4" /> Add Slate
            </button>
          )}
        </div>

        {slates.length === 0 ? (
          <div className="rounded-lg border border-dashed border-zinc-300 p-10 text-center text-sm text-zinc-500">
            No slates yet.{isDraft ? ' Add the first slate above.' : ''}
          </div>
        ) : (
          <div className="space-y-4">
            {slates.map(slate => {
              const isExpanded = expanded[slate.id] ?? true;
              const slateCandidacies = candidacies[slate.id] ?? [];
              return (
                <div key={slate.id} className="rounded-lg border border-zinc-200 bg-white">
                  <div className="flex w-full items-center rounded-t-lg">
                    <button
                      onClick={() => setExpanded(p => ({ ...p, [slate.id]: !isExpanded }))}
                      className="flex flex-1 items-center gap-3 px-4 py-3 text-left hover:bg-zinc-50 rounded-tl-lg"
                    >
                      {isExpanded
                        ? <ChevronDown className="h-4 w-4 text-zinc-400" />
                        : <ChevronRight className="h-4 w-4 text-zinc-400" />}
                      <span className="font-medium text-zinc-800">Slate #{slate.position}</span>
                      <span className="ml-auto text-xs text-zinc-400">{slateCandidacies.length} candidacies</span>
                    </button>
                    {isDraft && (
                      <button
                        onClick={() => handleDeleteSlate(slate.id)}
                        disabled={busy}
                        className="flex items-center gap-1 rounded-tr-lg px-3 py-3 text-xs text-red-500 hover:bg-red-50 hover:text-red-700 disabled:opacity-50"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    )}
                  </div>

                  {isExpanded && (
                    <div className="border-t border-zinc-100 px-4 pb-4 pt-3">
                      {isDraft && (
                        <div className="mb-3 flex justify-end">
                          <button
                            onClick={() => openAddCandidacy(slate.id)}
                            className="flex items-center gap-1.5 rounded-md bg-zinc-800 px-3 py-1.5 text-xs font-medium text-white hover:bg-zinc-700"
                          >
                            <Plus className="h-3 w-3" /> Add Candidacy
                          </button>
                        </div>
                      )}

                      {slateCandidacies.length === 0 ? (
                        <p className="py-4 text-center text-xs text-zinc-400">No candidacies in this slate.</p>
                      ) : (
                        <div className="overflow-x-auto rounded-lg border border-zinc-200">
                          <table className="min-w-full divide-y divide-zinc-200 bg-white text-sm">
                            <thead className="bg-zinc-50">
                              <tr>
                                <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">Photo</th>
                                <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">Name</th>
                                <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">Last Name</th>
                                <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">Candidacy Type</th>
                                {isDraft && <th className="px-3 py-2" />}
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-zinc-100">
                              {slateCandidacies.map(c => (
                                <tr key={c.id} className="hover:bg-zinc-50">
                                  <td className="px-3 py-2">
                                    {imgSrc(c.imageUrl) ? (
                                      // eslint-disable-next-line @next/next/no-img-element
                                      <img
                                        src={imgSrc(c.imageUrl)!}
                                        alt="photo"
                                        className="h-10 w-10 rounded-full object-cover"
                                      />
                                    ) : (
                                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-zinc-100">
                                        <span className="text-xs text-zinc-400">—</span>
                                      </div>
                                    )}
                                  </td>
                                  <td className="px-3 py-2 text-zinc-700">{c.name ?? '—'}</td>
                                  <td className="px-3 py-2 text-zinc-700">{c.lastName ?? '—'}</td>
                                  <td className="px-3 py-2 text-zinc-500">
                                    {candidacyTypes.find(t => t.id === c.candidacyTypeId)?.name ?? `Type #${c.candidacyTypeId}`}
                                  </td>
                                  {isDraft && (
                                    <td className="px-3 py-2">
                                      <div className="flex items-center gap-1">
                                        <button
                                          onClick={() => openEditCandidacy(slate.id, c)}
                                          className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
                                        >
                                          <Pencil className="h-3 w-3" /> Edit
                                        </button>
                                        <button
                                          onClick={() => { setPhotoFile(null); setPhotoModal({ slateId: slate.id, candidacyId: c.id }); }}
                                          className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
                                        >
                                          <Upload className="h-3 w-3" /> Photo
                                        </button>
                                        <button
                                          onClick={() => handleDeleteCandidacy(slate.id, c.id)}
                                          disabled={busy}
                                          className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-red-500 hover:bg-red-50 hover:text-red-700 disabled:opacity-50"
                                        >
                                          <Trash2 className="h-3 w-3" /> Delete
                                        </button>
                                      </div>
                                    </td>
                                  )}
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Modal: Add Slate */}
      {addSlateModal && (
        <Modal title="Add Slate" onClose={() => setAddSlateModal(false)}>
          <form onSubmit={handleAddSlate} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Position</label>
              <input
                type="number"
                min={1}
                required
                value={slatePosition}
                onChange={e => setSlatePosition(e.target.value)}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            {formError && <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => setAddSlateModal(false)} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={busy} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {busy ? 'Adding…' : 'Add'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Modal: Add/Edit Candidacy */}
      {candidacyModal && (
        <Modal
          title={candidacyModal.editing ? 'Edit Candidacy' : 'Add Candidacy'}
          onClose={() => setCandidacyModal(null)}
        >
          <form onSubmit={handleSaveCandidacy} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Candidacy Type</label>
              <select
                required
                value={candidacyForm.candidacyTypeId}
                onChange={e => setCandidacyForm(f => ({ ...f, candidacyTypeId: e.target.value }))}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                {[...candidacyTypes].sort((a, b) => a.position - b.position).map(t => (
                  <option key={t.id} value={t.id}>{t.name} (pos. {t.position})</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Name</label>
              <input
                type="text"
                required
                value={candidacyForm.name}
                onChange={e => setCandidacyForm(f => ({ ...f, name: e.target.value }))}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Last Name</label>
              <input
                type="text"
                required
                value={candidacyForm.lastName}
                onChange={e => setCandidacyForm(f => ({ ...f, lastName: e.target.value }))}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            {formError && <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">{formError}</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => setCandidacyModal(null)} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={busy} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {busy ? 'Saving…' : 'Save'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Modal: Sign */}
      {signModal && (
        <Modal title="Sign Scrutiny" onClose={() => setSignModal(false)}>
          <form onSubmit={handleSign} className="space-y-4">
            <p className="text-sm text-zinc-600">Upload the signature file (.jpg, .png, or .pdf — max 2 MB).</p>
            <input
              type="file"
              accept=".jpg,.jpeg,.png,.pdf"
              required
              onChange={e => setSignFile(e.target.files?.[0] ?? null)}
              className="block w-full text-sm text-zinc-600 file:mr-4 file:rounded-lg file:border-0 file:bg-blue-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-blue-700 hover:file:bg-blue-100"
            />
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => setSignModal(false)} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={busy || !signFile} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {busy ? 'Signing…' : 'Sign'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Modal: Upload cover image */}
      {coverModal && (
        <Modal title="Upload Cover Image" onClose={() => setCoverModal(false)}>
          <form onSubmit={handleUploadCover} className="space-y-4">
            <p className="text-sm text-zinc-600">Upload a cover image for this scrutiny (.jpg, .png — max 5 MB).</p>
            {scrutiny.imageUrl && (
              <div className="flex justify-center">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={imgSrc(scrutiny.imageUrl)!}
                  alt="Current cover"
                  className="h-32 w-32 rounded-xl object-cover border border-zinc-200"
                />
              </div>
            )}
            <input
              type="file"
              accept="image/*"
              required
              onChange={e => setCoverFile(e.target.files?.[0] ?? null)}
              className="block w-full text-sm text-zinc-600 file:mr-4 file:rounded-lg file:border-0 file:bg-blue-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-blue-700 hover:file:bg-blue-100"
            />
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => setCoverModal(false)} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={busy || !coverFile} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {busy ? 'Uploading…' : 'Upload'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Modal: Upload candidacy photo */}
      {photoModal && (
        <Modal title="Upload Photo" onClose={() => setPhotoModal(null)}>
          <form onSubmit={handleUploadPhoto} className="space-y-4">
            <input
              type="file"
              accept="image/*"
              required
              onChange={e => setPhotoFile(e.target.files?.[0] ?? null)}
              className="block w-full text-sm text-zinc-600 file:mr-4 file:rounded-lg file:border-0 file:bg-blue-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-blue-700 hover:file:bg-blue-100"
            />
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => setPhotoModal(null)} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={busy || !photoFile} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {busy ? 'Uploading…' : 'Upload'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
