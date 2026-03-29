import type { Session } from '@/types/auth';
import { Role } from '@/types/auth';

function normalizeRole(raw: unknown): Role {
  // Backend returns numeric role IDs (1 = ADMIN, 2 = STUDENT)
  if (raw === 1 || raw === '1') return Role.ADMIN;
  if (raw === 2 || raw === '2') return Role.STUDENT;
  if (typeof raw === 'string') {
    if (raw.toUpperCase() === 'ADMIN') return Role.ADMIN;
    if (raw.toUpperCase() === 'STUDENT') return Role.STUDENT;
  }
  return raw as Role;
}

export function decodeJwt(token: string): Session | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
    const payload = JSON.parse(atob(padded));
    const rawRole = payload.role
      ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      ?? '';
    return {
      sub: payload.sub ?? payload.nameid ?? payload.unique_name ?? '',
      email: payload.email ?? payload.unique_name ?? payload.sub ?? '',
      role: normalizeRole(rawRole),
      exp: payload.exp ?? 0,
      iat: payload.iat ?? 0,
    };
  } catch {
    return null;
  }
}

export function isTokenExpired(session: Session): boolean {
  return Date.now() / 1000 > session.exp;
}

export function getRole(session: Session | null): Role | null {
  return session?.role ?? null;
}
