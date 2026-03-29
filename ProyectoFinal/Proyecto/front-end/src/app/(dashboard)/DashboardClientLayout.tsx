'use client';
import { useState } from 'react';
import type { ReactNode } from 'react';
import { Sidebar } from '@/components/dashboard/sidebar/Sidebar';
import { DashboardHeader } from '@/components/dashboard/layout/DashboardHeader';
import type { Session } from '@/types/auth';
import { Toaster } from 'sonner';

export function DashboardClientLayout({ children, session }: { children: ReactNode; session: Session }) {
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <div className="flex h-screen overflow-hidden bg-zinc-50">
      <Sidebar session={session} mobileOpen={mobileOpen} onMobileClose={() => setMobileOpen(false)} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <DashboardHeader onMenuClick={() => setMobileOpen(true)} />
        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </div>
      <Toaster richColors />
    </div>
  );
}
