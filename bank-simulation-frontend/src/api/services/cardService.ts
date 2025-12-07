import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { 
  CreditCard, 
  CreateCardRequest, 
  CardTransactionRequest,
  CardApplicationRequest 
} from '../../types';

export const cardService = {
  /**
   * Kullanıcının kredi kartlarını getirir
   */
  getUserCards: async (userId: number): Promise<CreditCard[]> => {
    const response = await apiClient.get<CreditCard[]>(
      API_ENDPOINTS.PAYMENTS.GET_USER_CARDS(userId)
    );
    return response.data;
  },

  /**
   * Yeni kredi kartı oluşturur
   */
  createCard: async (cardData: CreateCardRequest): Promise<CreditCard> => {
    const response = await apiClient.post<CreditCard>(
      API_ENDPOINTS.PAYMENTS.CREATE_CARD,
      cardData
    );
    return response.data;
  },

  /**
   * Kart ile işlem yapar
   */
  makeTransaction: async (transactionData: CardTransactionRequest): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>(
      API_ENDPOINTS.PAYMENTS.CARD_TRANSACTION,
      transactionData
    );
    return response.data;
  },

  /**
   * Kredi kartı başvurusu yapar
   */
  applyForCard: async (applicationData: CardApplicationRequest): Promise<{ message: string; applicationId: number }> => {
    const response = await apiClient.post<{ message: string; applicationId: number }>(
      API_ENDPOINTS.APPLICATION.APPLY,
      applicationData
    );
    return response.data;
  },

  /**
   * Kart başvurusunu onaylar (Admin)
   */
  approveApplication: async (applicationId: number, approvedLimit: number): Promise<{ message: string; cardId: number }> => {
    const response = await apiClient.post<{ message: string; cardId: number }>(
      `${API_ENDPOINTS.APPLICATION.APPROVE(applicationId)}?approvedLimit=${approvedLimit}`
    );
    return response.data;
  },

  /**
   * Kart numarasını maskeler
   */
  maskCardNumber: (lastFour: string): string => {
    return `**** **** **** ${lastFour}`;
  },

  /**
   * Kart marka logosunu döndürür
   */
  getCardBrandLogo: (brand: string): string => {
    const logos: Record<string, string> = {
      'Visa': '/assets/cards/visa.svg',
      'MasterCard': '/assets/cards/mastercard.svg',
      'Troy': '/assets/cards/troy.svg',
      'AmericanExpress': '/assets/cards/amex.svg',
    };
    return logos[brand] || '/assets/cards/default.svg';
  },

  /**
   * Kart durumuna göre renk döndürür
   */
  getStatusColor: (status: string): 'success' | 'error' | 'warning' | 'default' => {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Blocked':
        return 'error';
      case 'Expired':
        return 'warning';
      default:
        return 'default';
    }
  },
};

export default cardService;
