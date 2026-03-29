'use client';
import { useState } from 'react';
import { useResource } from '@/lib/hooks/useResource';
import { DataTable } from '@/components/dashboard/table/DataTable';
import { Modal } from '@/components/shared/Modal';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { Plus, Pencil } from 'lucide-react';

interface Student {
  id: string;
  name?: string;
  lastName?: string;
  registrationNumber?: string;
  graduated?: boolean;
}

interface CreateForm {
  name: string;
  lastName: string;
}

interface EditForm {
  name: string;
  lastName: string;
  graduated: boolean;
}

export default function StudentsPage() {
  const { data, total, page, setPage, loading, error, create, update } = useResource<Student>({ path: '/api/students' });
  const [modal, setModal] = useState<'create' | 'edit' | null>(null);
  const [editing, setEditing] = useState<Student | null>(null);
  const [createForm, setCreateForm] = useState<CreateForm>({ name: '', lastName: '' });
  const [editForm, setEditForm] = useState<EditForm>({ name: '', lastName: '', graduated: false });
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  function openCreate() {
    setCreateForm({ name: '', lastName: '' });
    setFormError('');
    setModal('create');
  }

  function openEdit(row: Student) {
    setEditForm({ name: row.name ?? '', lastName: row.lastName ?? '', graduated: row.graduated ?? false });
    setEditing(row);
    setFormError('');
    setModal('edit');
  }

  function close() {
    setModal(null);
    setEditing(null);
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setFormError('');
    try {
      await create(createForm);
      close();
    } catch (err) {
      const pd = parseProblemDetails(err);
      setFormError(pd.title ?? 'Failed to save.');
      toast.error(pd.title ?? 'Failed to save.');
    } finally {
      setSaving(false);
    }
  }

  async function handleEdit(e: React.FormEvent) {
    e.preventDefault();
    if (!editing) return;
    setSaving(true);
    setFormError('');
    try {
      await update(editing.id, editForm);
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
      <Plus className="h-4 w-4" /> New Student
    </button>
  );

  return (
    <div>
      <h1 className="text-2xl font-bold text-zinc-900">Students</h1>
      <p className="mt-1 text-sm text-zinc-500">Manage registered students</p>
      <div className="mt-6">
        <DataTable
          columns={[
            { key: 'registrationNumber', label: 'Reg. Number' },
            { key: 'name', label: 'Name' },
            { key: 'lastName', label: 'Last Name' },
            {
              key: 'graduated',
              label: 'Graduated',
              render: (row) => row.graduated ? (
                <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs text-green-700">Yes</span>
              ) : (
                <span className="rounded-full bg-zinc-100 px-2 py-0.5 text-xs text-zinc-600">No</span>
              ),
            },
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
          emptyLabel="No students found. Add the first one."
        />
      </div>

      {modal === 'create' && (
        <Modal title="New Student" onClose={close}>
          <form onSubmit={handleCreate} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Name</label>
              <input
                type="text"
                value={createForm.name}
                onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Last Name</label>
              <input
                type="text"
                value={createForm.lastName}
                onChange={e => setCreateForm(f => ({ ...f, lastName: e.target.value }))}
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
        </Modal>
      )}

      {modal === 'edit' && (
        <Modal title="Edit Student" onClose={close}>
          <form onSubmit={handleEdit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Name</label>
              <input
                type="text"
                value={editForm.name}
                onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Last Name</label>
              <input
                type="text"
                value={editForm.lastName}
                onChange={e => setEditForm(f => ({ ...f, lastName: e.target.value }))}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="graduated"
                type="checkbox"
                checked={editForm.graduated}
                onChange={e => setEditForm(f => ({ ...f, graduated: e.target.checked }))}
                className="h-4 w-4 rounded border-zinc-300 text-blue-600"
              />
              <label htmlFor="graduated" className="text-sm text-zinc-700">Graduated</label>
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
        </Modal>
      )}
    </div>
  );
}
