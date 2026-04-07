'use client';
import { useState } from 'react';
import { format, parseISO } from 'date-fns';
import Link from 'next/link';
import { useResource } from '@/lib/hooks/useResource';
import { DataTable } from '@/components/dashboard/table/DataTable';
import { Modal } from '@/components/shared/Modal';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { Plus, Pencil, Settings, Trash2 } from 'lucide-react';

const STATUS_LABELS: Record<number, { label: string; className: string }> = {
  1: { label: 'Draft', className: 'bg-zinc-100 text-zinc-600' },
  2: { label: 'Open', className: 'bg-green-100 text-green-700' },
  3: { label: 'Closed', className: 'bg-red-100 text-red-600' },
  4: { label: 'Signed', className: 'bg-blue-100 text-blue-700' },
};

interface Scrutiny {
  id: string;
  title?: string;
  statusId?: number;
  startDate?: string;
  endDate?: string;
}

interface FormState {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
}

const emptyForm: FormState = { title: '', description: '', startDate: '', endDate: '' };

function toDatetimeLocal(iso?: string) {
  if (!iso) return '';
  return iso.slice(0, 16); // "YYYY-MM-DDTHH:mm"
}

export default function ScrutiniesPage() {
  const { data, total, page, setPage, loading, error, create, update, refetch } = useResource<Scrutiny>({ path: '/api/scrutinies' });
  const [modal, setModal] = useState<'create' | 'edit' | null>(null);
  const [editing, setEditing] = useState<Scrutiny | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [formError, setFormError] = useState('');

  function openCreate() {
    setForm(emptyForm);
    setEditing(null);
    setFormError('');
    setModal('create');
  }

  async function openEdit(row: Scrutiny) {
    setEditing(row);
    setFormError('');
    setForm({
      title: row.title ?? '',
      description: '',
      startDate: toDatetimeLocal(row.startDate),
      endDate: toDatetimeLocal(row.endDate),
    });
    setModal('edit');
    setLoadingDetail(true);
    try {
      const res = await fetch(`/api/proxy/api/scrutinies/${row.id}`);
      const body = await res.json().catch(() => null);
      const detail = body?.data;
      if (detail) {
        setForm({
          title: detail.title ?? row.title ?? '',
          description: detail.description ?? '',
          startDate: toDatetimeLocal(detail.startDate ?? row.startDate),
          endDate: toDatetimeLocal(detail.endDate ?? row.endDate),
        });
      }
    } catch {
      // modal is already open with partial data; description will remain empty
    } finally {
      setLoadingDetail(false);
    }
  }

  function close() {
    setModal(null);
    setEditing(null);
  }

  function field(key: keyof FormState, value: string) {
    setForm(f => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setFormError('');
    try {
      const dto: Record<string, unknown> = {
        title: form.title,
        startDate: form.startDate.slice(0, 16) + ':00',
        endDate: form.endDate.slice(0, 16) + ':00',
      };
      if (form.description) dto.description = form.description;
      if (modal === 'create') {
        await create(dto);
      } else if (editing) {
        await update(editing.id, dto);
      }
      close();
    } catch (err) {
      const pd = parseProblemDetails(err);
      setFormError(pd.title ?? 'Failed to save.');
      toast.error(pd.title ?? 'Failed to save.');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(row: Scrutiny) {
    if (!confirm(`Delete scrutiny "${row.title ?? row.id}"? This cannot be undone.`)) return;
    setSaving(true);
    try {
      const res = await fetch(`/api/proxy/api/scrutinies/${row.id}`, { method: 'DELETE' });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        const pd = parseProblemDetails(body);
        toast.error(pd.title ?? 'Failed to delete.');
        return;
      }
      toast.success('Scrutiny deleted.');
      refetch();
    } catch {
      toast.error('Failed to delete.');
    } finally {
      setSaving(false);
    }
  }

  const toolbar = (
    <button
      onClick={openCreate}
      className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
    >
      <Plus className="h-4 w-4" /> New Scrutiny
    </button>
  );

  return (
    <div>
      <h1 className="text-2xl font-bold text-zinc-900">Scrutinies</h1>
      <p className="mt-1 text-sm text-zinc-500">Manage elections and scrutinies</p>
      <div className="mt-6">
        <DataTable
          columns={[
            { key: 'title', label: 'Title' },
            {
              key: 'statusId',
              label: 'Status',
              render: (row) => {
                const s = STATUS_LABELS[row.statusId ?? 0];
                return s ? (
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${s.className}`}>{s.label}</span>
                ) : <span className="text-zinc-400">—</span>;
              },
            },
            {
              key: 'startDate',
              label: 'Start',
              render: (row) => row.startDate ? format(parseISO(row.startDate), 'MMM d, yyyy') : '—',
            },
            {
              key: 'endDate',
              label: 'End',
              render: (row) => row.endDate ? format(parseISO(row.endDate), 'MMM d, yyyy') : '—',
            },
            {
              key: 'actions',
              label: '',
              render: (row) => (
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => openEdit(row)}
                    className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
                  >
                    <Pencil className="h-3 w-3" /> Edit
                  </button>
                  <Link
                    href={`/dashboard/scrutinies/${row.id}`}
                    className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
                  >
                    <Settings className="h-3 w-3" /> Manage
                  </Link>
                  {row.statusId === 1 && (
                    <button
                      onClick={() => handleDelete(row)}
                      disabled={saving}
                      className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-red-500 hover:bg-red-50 hover:text-red-700 disabled:opacity-50"
                    >
                      <Trash2 className="h-3 w-3" /> Delete
                    </button>
                  )}
                </div>
              ),
            },
          ]}
          data={data}
          loading={loading}
          error={error}
          page={page}
          total={total}
          onPageChange={setPage}
          toolbar={toolbar}
          emptyLabel="No scrutinies found. Create the first one."
        />
      </div>

      {(modal === 'create' || modal === 'edit') && (
        <Modal title={modal === 'create' ? 'New Scrutiny' : 'Edit Scrutiny'} onClose={close}>
          <div className="relative">
            {loadingDetail && (
              <div className="absolute inset-0 z-10 flex items-center justify-center rounded-lg bg-white/70">
                <svg className="h-6 w-6 animate-spin text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
              </div>
            )}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Title</label>
              <input
                type="text"
                value={form.title}
                onChange={e => field('title', e.target.value)}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Description <span className="text-zinc-400 font-normal">(optional)</span></label>
              <textarea
                value={form.description}
                onChange={e => field('description', e.target.value)}
                rows={2}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Start Date</label>
              <input
                type="datetime-local"
                value={form.startDate}
                onChange={e => field('startDate', e.target.value)}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">End Date</label>
              <input
                type="datetime-local"
                value={form.endDate}
                onChange={e => field('endDate', e.target.value)}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            {formError && (
              <p className="rounded-lg bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-600">{formError}</p>
            )}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={close} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">Cancel</button>
              <button type="submit" disabled={saving} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving…' : 'Save'}
              </button>
            </div>
          </form>
          </div>
        </Modal>
      )}
    </div>
  );
}
