import { UserStatus, KycStatus, RiskLevel, RoleName } from './enums';

export interface User {
  userId: number;
  tcKimlikNo: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  emailVerified: boolean;
  phone?: string;
  phoneVerified: boolean;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  postalCode?: string;
  country: string;
  status: UserStatus;
  failedLoginAttempts: number;
  lockedUntil?: string;
  kycStatus: KycStatus;
  riskLevel: RiskLevel;
  isPep: boolean;
  createdAt: string;
  updatedAt?: string;
  lastLoginAt?: string;
  deletedAt?: string;
  gdprAnonymized: boolean;
}

export interface UserRole {
  roleId: number;
  userId: number;
  roleName: RoleName;
  assignedAt: string;
  assignedBy?: number;
  expiresAt?: string;
}

export interface UserSession {
  sessionId: number;
  userId: number;
  sessionToken: string;
  ipAddress?: string;
  userAgent?: string;
  deviceInfo?: string;
  createdAt: string;
  expiresAt: string;
  lastActivityAt?: string;
  isActive: boolean;
}

export interface LoginAttempt {
  attemptId: number;
  userId?: number;
  emailAttempted: string;
  ipAddress?: string;
  success: boolean;
  failureReason?: string;
  attemptedAt: string;
  userAgent?: string;
}

// Request/Response DTOs
export interface CreateUserRequest {
  tcKimlikNo: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  passwordHash: string;
  passwordSalt: string;
  phone?: string;
  addressLine1?: string;
  city?: string;
  postalCode?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  token: string;
  expiresAt: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}
