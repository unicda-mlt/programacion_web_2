export enum Role {
  ADMIN = 'ADMIN',
  STUDENT = 'STUDENT',
}

export interface Session {
  sub: string;
  email: string;
  role: Role;
  exp: number;
  iat: number;
}
