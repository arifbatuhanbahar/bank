import { AccountType, Currency, AccountStatus } from './enums';

export interface Account {
  accountId: number;
  userId: number;
  accountNumber: string; // IBAN
  accountType: AccountType;
  currency: Currency;
  balance: number;
  availableBalance: number;
  dailyTransferLimit: number;
  dailyWithdrawalLimit: number;
  interestRate: number;
  interestCalculationDate?: string;
  status: AccountStatus;
  openedDate: string;
  closedDate?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AccountBeneficiary {
  beneficiaryId: number;
  accountId: number;
  beneficiaryName: string;
  beneficiaryIban: string;
  beneficiaryBank?: string;
  nickname?: string;
  isVerified: boolean;
  addedAt: string;
}

export interface AccountLimit {
  limitId: number;
  accountId: number;
  limitType: string;
  limitAmount: number;
  usedAmount: number;
  resetDate: string;
  lastUpdated?: string;
}

// Request DTOs
export interface CreateAccountRequest {
  userId: number;
  accountNumber: string;
  accountType: AccountType;
  currency: Currency;
  balance: number;
  dailyTransferLimit: number;
  dailyWithdrawalLimit: number;
  interestRate: number;
}

export interface UpdateBalanceRequest {
  newBalance: number;
}

// Summary/Display Types
export interface AccountSummary {
  totalBalanceTRY: number;
  totalBalanceUSD: number;
  totalBalanceEUR: number;
  totalAccounts: number;
  activeAccounts: number;
}
