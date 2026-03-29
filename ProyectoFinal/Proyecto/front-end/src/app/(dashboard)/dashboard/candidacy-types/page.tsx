'use client';
import { useState } from 'react';
import { useResource } from '@/lib/hooks/useResource';
import { DataTable } from '@/components/dashboard/table/DataTable';
import { Modal } from '@/components/shared/Modal';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { Plus, Pencil } from 'lucide-react';

interface CandidacyType {
  id: number;
  name?: string;
  position?: number;
}

interface FormState {
  name: string;
  position: string;
}

const empty: FormState = { name: '', position: '' };

export default function CandidacyTypesPage() {
  const { data, total, page, setPage, loading, error, create, update, refetch } = useResource<CandidacyType>({ path: '/api/candidacy-types' });
  const [modal, setModal] = useState<'create' | 'edit' | null>(null);
  const [editing, setEditing] = useState<CandidacyType | null>(null);
  const [form, setForm] = useState<FormState>(empty);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  function openCreate() {
    setForm(empty);
    setEditing(null);
    setFormError('');
    setModal('create');
  }

  function openEdit(row: CandidacyType) {
    setForm({ name: row.name ?? '', position: String(row.position ?? '') });
    setEditing(row);
    setFormError('');
    setModal('edit');
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
      const dto = { name: form.name, position: Number(form.position) };
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

  const toolbar = (
    <button
      onClick={openCreate}
      className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
    >
      <Plus className="h-4 w-4" /> New Candidacy Type
    </button>
  );

  return (
    <div>
      <h1 className="text-2xl font-bold text-zinc-900">Candidacy Types</h1>
      <p className="mt-1 text-sm text-zinc-500">Manage candidacy type definitions</p>
      <div className="mt-6">
        <DataTable
          columns={[
            { key: 'id', label: 'ID' },
            { key: 'name', label: 'Name' },
            { key: 'position', label: 'Position' },
            {
              key: 'actions',
              label: '',
              render: (row) => (
                <button
                  onClick={() => openEdit(row)}
                  className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-zinc-500 hover:bg-zinc-100 hover:text-zinc-800"
                >
                  <Pencil className="h-3 w-3" /> Edit
                </button>
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
          emptyLabel="No candidacy types found. Create the first one."
        />
      </div>

      {(modal === 'create' || modal === 'edit') && (
        <Modal title={modal === 'create' ? 'New Candidacy Type' : 'Edit Candidacy Type'} onClose={close}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Name</label>
              <input
                type="text"
                value={form.name}
                onChange={e => field('name', e.target.value)}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Position</label>
              <input
                type="number"
                value={form.position}
                onChange={e => field('position', e.target.value)}
                required
                min={0}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            {formError && (
              <p className="rounded-lg bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-600">{formError}</p>
            )}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={close} className="rounded-lg border border-zinc-300 px-4 py-2 text-sm text-zinc-700 hover:bg-zinc-50">
                Cancel
              </button>
              <button type="submit" disabled={saving} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving…' : 'Save'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
