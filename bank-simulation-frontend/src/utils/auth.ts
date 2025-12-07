import { ADMIN_EMAILS } from './constants';
import { User } from '../types';

export const isAdminUser = (user?: User | null): boolean => {
  if (!user?.email) return false;
  const email = user.email.toLowerCase();
  return ADMIN_EMAILS.map(e => e.toLowerCase()).includes(email);
};
