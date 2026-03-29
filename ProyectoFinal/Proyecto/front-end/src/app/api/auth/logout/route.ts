import { NextResponse } from 'next/server';
import { env } from '@/utils/environment/public';

export async function POST() {
  const response = NextResponse.redirect(new URL('/', env.NEXT_PUBLIC_APP_URL));
  response.cookies.delete('auth-token');
  return response;
}
