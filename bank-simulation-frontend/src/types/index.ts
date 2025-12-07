// Enums
export * from './enums';

// Types
export * from './user.types';
export * from './account.types';
export * from './transaction.types';
export * from './card.types';
export * from './kyc.types';
export * from './kvkk.types';
export * from './admin.types';

// Common Types
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface SelectOption {
  value: string | number;
  label: string;
}

export interface TableColumn<T> {
  key: keyof T | string;
  header: string;
  width?: string;
  sortable?: boolean;
  render?: (item: T) => React.ReactNode;
}
