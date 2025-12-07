import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { Account, CreateAccountRequest } from '../../types';

export const accountService = {
  /**
   * Tüm hesapları getirir
   */
  getAllAccounts: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>(API_ENDPOINTS.ACCOUNTS.GET_ALL);
    return response.data;
  },

  /**
   * Belirli bir hesabı ID'ye göre getirir
   */
  getAccountById: async (id: number): Promise<Account> => {
    const response = await apiClient.get<Account>(API_ENDPOINTS.ACCOUNTS.GET_BY_ID(id));
    return response.data;
  },

  /**
   * Kullanıcının tüm hesaplarını getirir
   */
  getUserAccounts: async (userId: number): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>(API_ENDPOINTS.ACCOUNTS.GET_BY_USER(userId));
    return response.data;
  },

  /**
   * Yeni hesap oluşturur
   */
  createAccount: async (accountData: CreateAccountRequest): Promise<Account> => {
    const response = await apiClient.post<Account>(API_ENDPOINTS.ACCOUNTS.CREATE, accountData);
    return response.data;
  },

  /**
   * Hesap bakiyesini günceller
   */
  updateBalance: async (accountId: number, newBalance: number): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.ACCOUNTS.UPDATE_BALANCE(accountId), newBalance);
  },

  /**
   * Hesap özeti hesaplar
   */
  calculateAccountSummary: (accounts: Account[]) => {
    return {
      totalBalanceTRY: accounts
        .filter(a => a.currency === 'TRY')
        .reduce((sum, a) => sum + a.balance, 0),
      totalBalanceUSD: accounts
        .filter(a => a.currency === 'USD')
        .reduce((sum, a) => sum + a.balance, 0),
      totalBalanceEUR: accounts
        .filter(a => a.currency === 'EUR')
        .reduce((sum, a) => sum + a.balance, 0),
      totalAccounts: accounts.length,
      activeAccounts: accounts.filter(a => a.status === 'Active').length,
    };
  },
};

export default accountService;
