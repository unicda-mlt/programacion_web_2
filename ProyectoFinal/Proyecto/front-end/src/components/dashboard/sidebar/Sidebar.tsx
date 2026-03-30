'use client';
import { useState, useEffect } from 'react';
import { usePathname } from 'next/navigation';
import { SidebarNavItem } from './SidebarNavItem';
import { SidebarGroup } from './SidebarGroup';
import {
  LayoutDashboard, Users, GraduationCap, Vote, ListChecks, ChevronLeft, ChevronRight, LogOut, Activity,
} from 'lucide-react';
import type { Session } from '@/types/auth';

interface Props {
  session: Session | null;
  mobileOpen?: boolean;
  onMobileClose?: () => void;
}

const navGroups = [
  {
    label: 'Overview',
    items: [
      { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    ],
  },
  {
    label: 'Elections',
    items: [
      { href: '/dashboard/scrutinies', label: 'Scrutinies', icon: Vote },
      { href: '/dashboard/vote-status', label: 'Live Vote Status', icon: Activity },
      { href: '/dashboard/candidacy-types', label: 'Candidacy Types', icon: ListChecks },
    ],
  },
  {
    label: 'People',
    items: [
      { href: '/dashboard/users', label: 'Users', icon: Users },
      { href: '/dashboard/students', label: 'Students', icon: GraduationCap },
    ],
  },
];

export function Sidebar({ session, mobileOpen, onMobileClose }: Props) {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    const stored = localStorage.getItem('sidebar-collapsed');
    if (stored !== null) setCollapsed(stored === 'true');
  }, []);

  useEffect(() => {
    onMobileClose?.();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pathname]);

  function toggleCollapsed() {
    const next = !collapsed;
    setCollapsed(next);
    localStorage.setItem('sidebar-collapsed', String(next));
  }

  async function handleLogout() {
    await fetch('/api/auth/logout', { method: 'POST' });
    window.location.href = '/';
  }

  const sidebarContent = (
    <div className={`flex h-full flex-col border-r border-zinc-200 bg-white transition-all duration-200 ${collapsed ? 'w-16' : 'w-64'}`}>
      {/* Header */}
      <div className="flex h-16 items-center justify-between border-b border-zinc-100 px-4">
        {!collapsed && (
          <span className="font-bold text-blue-700">LaVozEstudiantil</span>
        )}
        <button
          onClick={toggleCollapsed}
          className="ml-auto rounded-md p-1.5 text-zinc-400 hover:bg-zinc-100 hover:text-zinc-600"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
        </button>
      </div>

      {/* Nav */}
      <nav role="navigation" aria-label="Dashboard navigation" className="flex-1 overflow-y-auto p-3 space-y-4">
        {navGroups.map(group => (
          <SidebarGroup key={group.label} label={group.label} collapsed={collapsed}>
            {group.items.map(item => (
              <SidebarNavItem key={item.href} {...item} collapsed={collapsed} />
            ))}
          </SidebarGroup>
        ))}
      </nav>

      {/* Footer */}
      <div className="border-t border-zinc-100 p-3">
        {session && !collapsed && (
          <div className="mb-2 px-2">
            <p className="text-sm font-medium text-zinc-800 truncate">{session.email}</p>
            <span className="inline-block mt-0.5 rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-700">
              {session.role}
            </span>
          </div>
        )}
        <button
          onClick={handleLogout}
          className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-zinc-600 hover:bg-red-50 hover:text-red-600 transition-colors ${collapsed ? 'justify-center' : ''}`}
          title={collapsed ? 'Logout' : undefined}
        >
          <LogOut className="h-4 w-4 shrink-0" />
          {!collapsed && <span>Logout</span>}
        </button>
      </div>
    </div>
  );

  return (
    <>
      {/* Desktop sidebar */}
      <div className="hidden md:block h-screen sticky top-0">
        {sidebarContent}
      </div>

      {/* Mobile drawer */}
      {mobileOpen && (
        <div className="fixed inset-0 z-50 md:hidden">
          <div className="absolute inset-0 bg-black/50" onClick={onMobileClose} />
          <div className="absolute left-0 top-0 h-full">
            {sidebarContent}
          </div>
        </div>
      )}
    </>
  );
}
