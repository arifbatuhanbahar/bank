import { ADMIN_EMAILS } from './constants';
import { User } from '../types';

export const isAdminUser = (user?: User | null): boolean => {
  if (!user) return false;
  const rawEmail = (user as any).email ?? (user as any).Email;
  if (!rawEmail) return false;
  const email = String(rawEmail).toLowerCase();
  return ADMIN_EMAILS.map(e => e.toLowerCase()).includes(email);
};
