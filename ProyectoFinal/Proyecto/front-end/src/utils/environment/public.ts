import { z } from 'zod';

const schema = z.object({
  NEXT_PUBLIC_API_BASE_URL: z.string().min(1).default('/api/proxy'),
  NEXT_PUBLIC_APP_URL: z.url().default('http://localhost:3000'),
});

const parsed = schema.safeParse({
  NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
});

if (!parsed.success) {
  const issues = parsed.error.issues.map(i => `  ${i.path.join('.')}: ${i.message}`).join('\n');
  console.error('❌ Invalid public environment variables:\n' + issues);
  throw new Error('Invalid public environment variables. Check the console for details.');
}

export const env = parsed.data;
