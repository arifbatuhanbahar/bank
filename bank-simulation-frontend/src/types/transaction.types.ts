import { TransactionType, TransactionStatus, Currency } from './enums';

export interface Transaction {
  transactionId: number;
  fromAccountId?: number;
  toAccountId?: number;
  amount: number;
  currency: Currency;
  transactionType: TransactionType;
  description?: string;
  referenceNumber: string;
  status: TransactionStatus;
  fraudScore: number;
  fraudFlags?: string;
  requiresReview: boolean;
  reviewedBy?: number;
  reviewedAt?: string;
  transactionDate: string;
  completedAt?: string;
  createdBy?: number;
  ipAddress?: string;
  userAgent?: string;
  isSuspicious: boolean;
  suspiciousReason?: string;
  reportedToMasak: boolean;
  masakReportDate?: string;
}

export interface TransactionFee {
  feeId: number;
  transactionId: number;
  feeType: string;
  feeAmount: number;
  appliedAt: string;
}

export interface ScheduledTransaction {
  scheduleId: number;
  userId: number;
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  frequency: string;
  startDate: string;
  endDate?: string;
  nextExecutionDate: string;
  lastExecutionDate?: string;
  status: string;
  createdAt: string;
}

export interface TransactionApproval {
  approvalId: number;
  transactionId: number;
  approverId: number;
  approvalLevel: number;
  status: string;
  comments?: string;
  approvedAt?: string;
}

// Request DTOs
export interface TransferRequest {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  description?: string;
}

export interface TransferResponse {
  message: string;
  transactionId: number;
  reference: string;
}

// Filter & Display Types
export interface TransactionFilter {
  accountId?: number;
  startDate?: string;
  endDate?: string;
  minAmount?: number;
  maxAmount?: number;
  transactionType?: TransactionType;
  status?: TransactionStatus;
}

export interface TransactionWithDetails extends Transaction {
  fromAccountNumber?: string;
  toAccountNumber?: string;
  senderName?: string;
  receiverName?: string;
}
