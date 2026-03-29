import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { decodeJwt } from '@/lib/auth/session';
import { Role } from '@/types/auth';

const AUTH_COOKIE = 'auth-token';

function getSession(request: NextRequest) {
  const token = request.cookies.get(AUTH_COOKIE)?.value;
  if (!token) return null;
  return decodeJwt(token);
}

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const session = getSession(request);

  // Dashboard routes — require ADMIN role
  if (pathname.startsWith('/dashboard')) {
    if (!session || session.role !== Role.ADMIN) {
      return NextResponse.redirect(new URL('/login', request.url));
    }
    return NextResponse.next();
  }

  // Login page — redirect if already authenticated
  if (pathname === '/login') {
    if (session) {
      if (session.role === Role.ADMIN) {
        return NextResponse.redirect(new URL('/dashboard', request.url));
      }
      if (session.role === Role.STUDENT) {
        return NextResponse.redirect(new URL('/scrutinies', request.url));
      }
    }
    return NextResponse.next();
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/login'],
};
