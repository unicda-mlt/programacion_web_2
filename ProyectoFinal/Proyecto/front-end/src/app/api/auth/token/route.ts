import { NextResponse } from 'next/server';
import { cookies } from 'next/headers';
import { decodeJwt, isTokenExpired } from '@/lib/auth/session';

export async function GET() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth-token')?.value;
  if (!token) {
    return NextResponse.json({ token: null }, { status: 401 });
  }
  const session = decodeJwt(token);
  if (!session || isTokenExpired(session)) {
    return NextResponse.json({ token: null }, { status: 401 });
  }
  return NextResponse.json({ token });
}
