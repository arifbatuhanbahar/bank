// User Management
export const UserStatus = {
  Active: 'Active',
  Suspended: 'Suspended',
  Locked: 'Locked',
  Closed: 'Closed',
} as const;
export type UserStatus = typeof UserStatus[keyof typeof UserStatus];

export const KycStatus = {
  Pending: 'Pending',
  Verified: 'Verified',
  Rejected: 'Rejected',
} as const;
export type KycStatus = typeof KycStatus[keyof typeof KycStatus];

export const RiskLevel = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
} as const;
export type RiskLevel = typeof RiskLevel[keyof typeof RiskLevel];

export const RoleName = {
  Customer: 'Customer',
  Employee: 'Employee',
  Admin: 'Admin',
  Auditor: 'Auditor',
} as const;
export type RoleName = typeof RoleName[keyof typeof RoleName];

// Account
export const AccountType = {
  Checking: 'Checking',
  Savings: 'Savings',
  Investment: 'Investment',
} as const;
export type AccountType = typeof AccountType[keyof typeof AccountType];

export const Currency = {
  TRY: 'TRY',
  USD: 'USD',
  EUR: 'EUR',
} as const;
export type Currency = typeof Currency[keyof typeof Currency];

export const AccountStatus = {
  Active: 'Active',
  Frozen: 'Frozen',
  Closed: 'Closed',
} as const;
export type AccountStatus = typeof AccountStatus[keyof typeof AccountStatus];

// Transaction
export const TransactionType = {
  Transfer: 'Transfer',
  Deposit: 'Deposit',
  Withdrawal: 'Withdrawal',
  Payment: 'Payment',
} as const;
export type TransactionType = typeof TransactionType[keyof typeof TransactionType];

export const TransactionStatus = {
  Pending: 'Pending',
  Processing: 'Processing',
  Completed: 'Completed',
  Failed: 'Failed',
  Reversed: 'Reversed',
  Cancelled: 'Cancelled',
} as const;
export type TransactionStatus = typeof TransactionStatus[keyof typeof TransactionStatus];

// Card
export const CardBrand = {
  Visa: 'Visa',
  MasterCard: 'MasterCard',
  Troy: 'Troy',
  AmericanExpress: 'AmericanExpress',
} as const;
export type CardBrand = typeof CardBrand[keyof typeof CardBrand];

export const CardType = {
  Physical: 'Physical',
  Virtual: 'Virtual',
} as const;
export type CardType = typeof CardType[keyof typeof CardType];

export const CardStatus = {
  Active: 'Active',
  Blocked: 'Blocked',
  Expired: 'Expired',
  Cancelled: 'Cancelled',
} as const;
export type CardStatus = typeof CardStatus[keyof typeof CardStatus];

export const CardTransactionStatus = {
  Pending: 'Pending',
  Approved: 'Approved',
  Declined: 'Declined',
  Refunded: 'Refunded',
} as const;
export type CardTransactionStatus = typeof CardTransactionStatus[keyof typeof CardTransactionStatus];

// Application
export const ApplicationStatus = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
} as const;
export type ApplicationStatus = typeof ApplicationStatus[keyof typeof ApplicationStatus];

export const CardPrestigeLevel = {
  Standard: 'Standard',
  Gold: 'Gold',
  Platinum: 'Platinum',
  Corporate: 'Corporate',
  Black: 'Black',
} as const;
export type CardPrestigeLevel = typeof CardPrestigeLevel[keyof typeof CardPrestigeLevel];

export const EmploymentStatus = {
  Employed: 'Employed',
  SelfEmployed: 'SelfEmployed',
  Unemployed: 'Unemployed',
  Retired: 'Retired',
  Student: 'Student',
} as const;
export type EmploymentStatus = typeof EmploymentStatus[keyof typeof EmploymentStatus];

// Compliance
export const ConsentType = {
  DataProcessing: 'DataProcessing',
  Marketing: 'Marketing',
  ThirdPartyTransfer: 'ThirdPartyTransfer',
} as const;
export type ConsentType = typeof ConsentType[keyof typeof ConsentType];

export const KvkkRequestType = {
  Access: 'Access',
  Rectification: 'Rectification',
  Erasure: 'Erasure',
  Portability: 'Portability',
} as const;
export type KvkkRequestType = typeof KvkkRequestType[keyof typeof KvkkRequestType];

export const RequestStatus = {
  Pending: 'Pending',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Rejected: 'Rejected',
} as const;
export type RequestStatus = typeof RequestStatus[keyof typeof RequestStatus];

export const DocumentType = {
  IdentityCard: 'IdentityCard',
  Passport: 'Passport',
  DriverLicense: 'DriverLicense',
  UtilityBill: 'UtilityBill',
  TaxPlate: 'TaxPlate',
} as const;
export type DocumentType = typeof DocumentType[keyof typeof DocumentType];

export const VerificationStatus = {
  Pending: 'Pending',
  Verified: 'Verified',
  Failed: 'Failed',
  Rejected: 'Rejected',
} as const;
export type VerificationStatus = typeof VerificationStatus[keyof typeof VerificationStatus];

// Fraud & Security
export const AlertSeverity = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
} as const;
export type AlertSeverity = typeof AlertSeverity[keyof typeof AlertSeverity];

export const AlertStatus = {
  Open: 'Open',
  InReview: 'InReview',
  Resolved: 'Resolved',
  FalsePositive: 'FalsePositive',
} as const;
export type AlertStatus = typeof AlertStatus[keyof typeof AlertStatus];

export const SecurityEventType = {
  LoginFailed: 'LoginFailed',
  AccountLocked: 'AccountLocked',
  PasswordChanged: 'PasswordChanged',
  SuspiciousActivity: 'SuspiciousActivity',
  UnusualIpAddress: 'UnusualIpAddress',
} as const;
export type SecurityEventType = typeof SecurityEventType[keyof typeof SecurityEventType];

export const Severity = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
} as const;
export type Severity = typeof Severity[keyof typeof Severity];
