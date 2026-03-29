'use client';
import { useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const expired = searchParams.get('reason') === 'session_expired';
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userName, password }),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        setError(body.title ?? body.detail ?? 'Login failed. Please check your credentials.');
        return;
      }
      // Get session to redirect based on role
      const meRes = await fetch('/api/auth/me');
      if (meRes.ok) {
        const { session } = await meRes.json();
        if (session?.role === 'ADMIN') {
          router.replace('/dashboard');
        } else {
          router.replace('/scrutinies');
        }
      } else {
        router.replace('/');
      }
    } catch {
      setError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-full items-center justify-center py-12 px-4">
      <div className="w-full max-w-sm">
        {expired && (
          <div className="mb-4 rounded-lg bg-[#fff8e1] border border-[#ffe082] p-3 text-sm text-[#f57f17]">
            Your session has expired. Please log in again.
          </div>
        )}
        <div className="rounded-2xl border border-[#e0e0e0] bg-white p-8 shadow-sm">
          <h1 className="text-2xl font-bold text-[#37474f]">Sign in</h1>
          <p className="mt-1 text-sm text-[#78909c]">Enter your university credentials</p>

          <form onSubmit={handleSubmit} className="mt-6 space-y-4">
            <div>
              <label htmlFor="userName" className="block text-sm font-medium text-[#546e7a]">Username</label>
              <input
                id="userName"
                type="text"
                value={userName}
                onChange={e => setUserName(e.target.value)}
                required
                autoComplete="username"
                className="mt-1 block w-full rounded-lg border border-[#e0e0e0] bg-[#f5f5f7] px-3 py-2 text-sm text-[#37474f] placeholder-[#b0bec5] focus:border-[#5c6bc0] focus:bg-white focus:outline-none focus:ring-1 focus:ring-[#5c6bc0] transition-colors"
                placeholder="your.username"
              />
            </div>
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-[#546e7a]">Password</label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                required
                autoComplete="current-password"
                className="mt-1 block w-full rounded-lg border border-[#e0e0e0] bg-[#f5f5f7] px-3 py-2 text-sm text-[#37474f] placeholder-[#b0bec5] focus:border-[#5c6bc0] focus:bg-white focus:outline-none focus:ring-1 focus:ring-[#5c6bc0] transition-colors"
              />
            </div>
            {error && (
              <p className="rounded-lg bg-[#ffebee] border border-[#ef9a9a] px-3 py-2 text-sm text-[#c62828]">{error}</p>
            )}
            <button
              type="submit"
              disabled={loading}
              className="w-full rounded-lg bg-[#5c6bc0] py-2.5 text-sm font-medium text-white hover:bg-[#3949ab] disabled:opacity-50 transition-colors"
            >
              {loading ? 'Signing in\u2026' : 'Sign in'}
            </button>
          </form>
          <div className="mt-4 text-center">
            <Link href="/" className="text-sm text-[#78909c] hover:text-[#37474f]">&larr; Back to home</Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}
