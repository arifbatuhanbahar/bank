import { 
  CardType, 
  CardBrand, 
  CardStatus, 
  CardTransactionStatus,
  CardPrestigeLevel,
  EmploymentStatus,
  ApplicationStatus,
  Currency
} from './enums';

export interface CreditCard {
  cardId: number;
  userId: number;
  cardNumberEncrypted: string;
  cardLastFour: string;
  cardType: CardType;
  cardBrand: CardBrand;
  creditLimit: number;
  availableLimit: number;
  currentBalance: number;
  minimumPayment: number;
  paymentDueDate: string;
  interestRate: number;
  expiryMonth: number;
  expiryYear: number;
  cvvEncrypted: string;
  status: CardStatus;
  issuedAt: string;
  activatedAt?: string;
}

export interface CardTransaction {
  cardTransactionId: number;
  cardId: number;
  merchantName: string;
  merchantCategory: string;
  amount: number;
  currency: Currency;
  transactionDate: string;
  status: CardTransactionStatus;
  authorizationCode: string;
  ipAddress?: string;
  location?: string;
}

export interface CardLimit {
  limitId: number;
  cardId: number;
  limitType: string;
  limitAmount: number;
  usedAmount: number;
  resetDate?: string;
}

export interface CardApplication {
  applicationId: number;
  userId: number;
  cardTypeRequested: CardPrestigeLevel;
  monthlyIncome: number;
  employmentStatus: EmploymentStatus;
  employerName: string;
  applicationDate: string;
  status: ApplicationStatus;
  rejectionReason?: string;
  approvedBy?: number;
  approvedAt?: string;
  creditLimitApproved?: number;
}

// Request DTOs
export interface CreateCardRequest {
  userId: number;
  cardNumber: string;
  limit: number;
}

export interface CardTransactionRequest {
  cardId: number;
  amount: number;
  merchantName: string;
}

export interface CardApplicationRequest {
  userId: number;
  cardTypeRequested: CardPrestigeLevel;
  monthlyIncome: number;
  employmentStatus: EmploymentStatus;
  employerName: string;
}

// Display Types
export interface CreditCardDisplay extends CreditCard {
  maskedCardNumber: string;
  expiryDate: string;
  usedPercentage: number;
}

export interface CardSpendingCategory {
  category: string;
  amount: number;
  percentage: number;
  transactionCount: number;
}
