import { NextResponse } from 'next/server';
import { decodeJwt, isTokenExpired } from '@/lib/auth/session';
import { cookies } from 'next/headers';

export async function GET() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth-token')?.value;
  if (!token) {
    return NextResponse.json({ session: null }, { status: 401 });
  }
  const session = decodeJwt(token);
  if (!session || isTokenExpired(session)) {
    return NextResponse.json({ session: null }, { status: 401 });
  }
  return NextResponse.json({ session });
}
