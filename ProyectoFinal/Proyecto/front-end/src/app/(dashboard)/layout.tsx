import type { ReactNode } from 'react';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import { decodeJwt, isTokenExpired } from '@/lib/auth/session';
import { Role } from '@/types/auth';
import { DashboardClientLayout } from './DashboardClientLayout';

export default async function DashboardLayout({ children }: { children: ReactNode }) {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth-token')?.value;

  if (!token) redirect('/login');

  const session = decodeJwt(token);
  if (!session || isTokenExpired(session) || session.role !== Role.ADMIN) {
    redirect('/login');
  }

  return <DashboardClientLayout session={session}>{children}</DashboardClientLayout>;
}
