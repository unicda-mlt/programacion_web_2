'use client';
import { Menu } from 'lucide-react';

interface Props {
  onMenuClick: () => void;
  breadcrumbs?: { label: string; href?: string }[];
}

export function DashboardHeader({ onMenuClick, breadcrumbs = [] }: Props) {
  return (
    <header className="flex h-16 items-center justify-between border-b border-zinc-200 bg-white px-4">
      <div className="flex items-center gap-3">
        <button
          onClick={onMenuClick}
          className="rounded-md p-2 text-zinc-500 hover:bg-zinc-100 md:hidden"
          aria-label="Open navigation menu"
        >
          <Menu className="h-5 w-5" />
        </button>
        {breadcrumbs.length > 0 && (
          <nav aria-label="Breadcrumb" className="flex items-center gap-2 text-sm">
            {breadcrumbs.map((b, i) => (
              <span key={i} className="flex items-center gap-2">
                {i > 0 && <span className="text-zinc-300">/</span>}
                <span className={i === breadcrumbs.length - 1 ? 'text-zinc-900 font-medium' : 'text-zinc-500'}>
                  {b.label}
                </span>
              </span>
            ))}
          </nav>
        )}
      </div>
    </header>
  );
}
