'use client';
import { useState } from 'react';
import { useResource } from '@/lib/hooks/useResource';
import { DataTable } from '@/components/dashboard/table/DataTable';
import { Modal } from '@/components/shared/Modal';
import { toast } from 'sonner';
import { parseProblemDetails } from '@/lib/api/errors';
import { Plus, Pencil } from 'lucide-react';

// userRoleId: 1 = ADMIN, 2 = STUDENT
const ROLE_LABELS: Record<number, string> = { 1: 'ADMIN', 2: 'STUDENT' };

interface User {
  id: string;
  userName?: string;
  userRoleId?: number;
  active?: boolean;
}

interface FormState {
  userName: string;
  password: string;
  userRoleId: string;
  active: boolean;
}

const emptyForm: FormState = { userName: '', password: '', userRoleId: '1', active: true };

export default function UsersPage() {
  const { data, total, page, setPage, loading, error, create, update } = useResource<User>({ path: '/api/users' });
  const [modal, setModal] = useState<'create' | 'edit' | null>(null);
  const [editing, setEditing] = useState<User | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  function openCreate() {
    setForm(emptyForm);
    setEditing(null);
    setFormError('');
    setModal('create');
  }

  function openEdit(row: User) {
    setForm({ userName: row.userName ?? '', password: '', userRoleId: String(row.userRoleId ?? 2), active: row.active ?? true });
    setEditing(row);
    setFormError('');
    setModal('edit');
  }

  function close() {
    setModal(null);
    setEditing(null);
  }

  function field(key: keyof FormState, value: string | boolean) {
    setForm(f => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setFormError('');
    try {
      if (modal === 'create') {
        await create({
          userName: form.userName,
          password: form.password,
          userRoleId: Number(form.userRoleId),
          active: form.active,
        });
      } else if (editing) {
        const dto: Record<string, unknown> = {
          userName: form.userName,
          userRoleId: Number(form.userRoleId),
          active: form.active,
        };
        if (form.password) dto.password = form.password;
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
      <Plus className="h-4 w-4" /> New User
    </button>
  );

  return (
    <div>
      <h1 className="text-2xl font-bold text-zinc-900">Users</h1>
      <p className="mt-1 text-sm text-zinc-500">Manage admin users. To create a student user, go to the Students page.</p>
      <div className="mt-6">
        <DataTable
          columns={[
            { key: 'userName', label: 'Username' },
            {
              key: 'userRoleId',
              label: 'Role',
              render: (row) => (
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${row.userRoleId === 1 ? 'bg-blue-100 text-blue-700' : 'bg-zinc-100 text-zinc-600'}`}>
                  {ROLE_LABELS[row.userRoleId ?? 0] ?? String(row.userRoleId ?? '—')}
                </span>
              ),
            },
            {
              key: 'active',
              label: 'Active',
              render: (row) => row.active ? (
                <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs text-green-700">Active</span>
              ) : (
                <span className="rounded-full bg-red-100 px-2 py-0.5 text-xs text-red-600">Inactive</span>
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
          emptyLabel="No users found. Create the first one."
        />
      </div>

      {(modal === 'create' || modal === 'edit') && (
        <Modal title={modal === 'create' ? 'New User' : 'Edit User'} onClose={close}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700">Username</label>
              <input
                type="text"
                value={form.userName}
                onChange={e => field('userName', e.target.value)}
                required
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">
                Password {modal === 'edit' && <span className="text-zinc-400 font-normal">(leave blank to keep current)</span>}
              </label>
              <input
                type="password"
                value={form.password}
                onChange={e => field('password', e.target.value)}
                required={modal === 'create'}
                autoComplete="new-password"
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-zinc-700">Role</label>
              <select
                value={form.userRoleId}
                onChange={e => field('userRoleId', e.target.value)}
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option value="1">ADMIN</option>
                {modal === 'edit' && <option value="2">STUDENT</option>}
              </select>
            </div>
            <div className="flex items-center gap-2">
              <input
                id="active"
                type="checkbox"
                checked={form.active}
                onChange={e => field('active', e.target.checked)}
                className="h-4 w-4 rounded border-zinc-300 text-blue-600"
              />
              <label htmlFor="active" className="text-sm text-zinc-700">Active</label>
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
