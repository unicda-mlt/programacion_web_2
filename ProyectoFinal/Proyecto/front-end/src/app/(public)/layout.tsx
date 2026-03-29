import Link from 'next/link';
import { cookies } from 'next/headers';
import type { ReactNode } from 'react';
import { decodeJwt, isTokenExpired } from '@/lib/auth/session';
import { Role } from '@/types/auth';

const AUTH_COOKIE = 'auth-token';

async function getSession() {
  const cookieStore = await cookies();
  const token = cookieStore.get(AUTH_COOKIE)?.value;
  if (!token) return null;
  const session = decodeJwt(token);
  if (!session || isTokenExpired(session)) return null;
  return session;
}

export default async function PublicLayout({ children }: { children: ReactNode }) {
  const session = await getSession();

  return (
    <div className="min-h-screen flex flex-col bg-[#f5f5f7]">
      <header className="border-b border-[#e0e0e0] bg-white shadow-sm">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4">
          <Link href="/" className="text-xl font-bold text-[#5c6bc0]">
            LaVozEstudiantil
          </Link>
          <nav className="flex items-center gap-4">
            <Link href="/scrutinies" className="text-sm text-[#78909c] hover:text-[#37474f]">
              Scrutinies
            </Link>
            {session ? (
              <>
                {session.role === Role.ADMIN && (
                  <Link
                    href="/dashboard"
                    className="rounded-lg bg-[#5c6bc0] px-4 py-2 text-sm font-medium text-white hover:bg-[#3949ab] transition-colors"
                  >
                    Dashboard
                  </Link>
                )}
                <form action="/api/auth/logout" method="POST">
                  <button
                    type="submit"
                    className="rounded-lg border border-[#c5cae9] px-4 py-2 text-sm font-medium text-[#5c6bc0] hover:bg-[#e8eaf6] transition-colors"
                  >
                    Logout
                  </button>
                </form>
              </>
            ) : (
              <Link
                href="/login"
                className="rounded-lg bg-[#5c6bc0] px-4 py-2 text-sm font-medium text-white hover:bg-[#3949ab] transition-colors"
              >
                Login
              </Link>
            )}
          </nav>
        </div>
      </header>
      <main className="flex-1">{children}</main>
    </div>
  );
}
