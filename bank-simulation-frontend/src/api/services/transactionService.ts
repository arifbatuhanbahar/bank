import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { Transaction, TransferRequest, TransferResponse } from '../../types';

export const transactionService = {
  /**
   * Belirli bir hesabın işlemlerini getirir
   */
  getAccountTransactions: async (accountId: number): Promise<Transaction[]> => {
    const response = await apiClient.get<Transaction[]>(
      API_ENDPOINTS.TRANSACTIONS.GET_BY_ACCOUNT(accountId)
    );
    return response.data;
  },

  /**
   * Para transferi yapar
   */
  transfer: async (transferData: TransferRequest): Promise<TransferResponse> => {
    const response = await apiClient.post<TransferResponse>(
      API_ENDPOINTS.TRANSACTIONS.TRANSFER,
      transferData
    );
    return response.data;
  },

  /**
   * İşlem durumuna göre renk döndürür
   */
  getStatusColor: (status: string): 'success' | 'error' | 'warning' | 'info' | 'default' => {
    switch (status) {
      case 'Completed':
        return 'success';
      case 'Pending':
      case 'Processing':
        return 'warning';
      case 'Failed':
      case 'Cancelled':
        return 'error';
      case 'Reversed':
        return 'info';
      default:
        return 'default';
    }
  },

  /**
   * İşlem türüne göre ikon adı döndürür
   */
  getTransactionIcon: (type: string): string => {
    switch (type) {
      case 'Transfer':
        return 'swap_horiz';
      case 'Deposit':
        return 'add_circle';
      case 'Withdrawal':
        return 'remove_circle';
      case 'Payment':
        return 'payment';
      default:
        return 'receipt';
    }
  },
};

export default transactionService;
