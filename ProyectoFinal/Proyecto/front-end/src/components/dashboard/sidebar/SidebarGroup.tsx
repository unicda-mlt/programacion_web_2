import type { ReactNode } from 'react';

export function SidebarGroup({ label, children, collapsed }: { label: string; children: ReactNode; collapsed: boolean }) {
  return (
    <div>
      {!collapsed && (
        <p className="mb-1 px-3 text-xs font-semibold uppercase tracking-wider text-zinc-400">{label}</p>
      )}
      <div className="space-y-0.5">{children}</div>
    </div>
  );
}
