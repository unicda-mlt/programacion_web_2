import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { env } from '@/utils/environment/private';

export async function POST(request: NextRequest) {
  const body = await request.json();

  const backendUrl = env.API_BASE_URL;

  const res = await fetch(`${backendUrl}/api/auth/generate-token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: 'Login failed.' }));
    return NextResponse.json(error, { status: res.status });
  }

  const data = await res.json();
  const token: string = data.token ?? data.accessToken ?? data.data?.token ?? '';

  const response = NextResponse.json({ ok: true });
  response.cookies.set('auth-token', token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    path: '/',
    maxAge: 60 * 60 * 24, // 24 hours
  });

  return response;
}
