import { z } from 'zod';

const schema = z.object({
  // Direct backend URL used in server-side Route Handlers and RSC fetches.
  // Never exposed to the client.
  API_BASE_URL: z.url(),
});

const parsed = schema.safeParse({
  API_BASE_URL: process.env.API_BASE_URL,
});

if (!parsed.success) {
  const issues = parsed.error.issues.map(i => `  ${i.path.join('.')}: ${i.message}`).join('\n');
  console.error('❌ Invalid private environment variables:\n' + issues);
  throw new Error('Invalid private environment variables. Check the console for details.');
}

export const env = parsed.data;
