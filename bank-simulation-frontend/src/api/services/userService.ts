import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { User, CreateUserRequest } from '../../types';

export const userService = {
  /**
   * Tüm kullanıcıları getirir (silinmemiş olanlar)
   */
  getAllUsers: async (): Promise<User[]> => {
    const response = await apiClient.get<User[]>(API_ENDPOINTS.USERS.GET_ALL);
    return response.data;
  },

  /**
   * Belirli bir kullanıcıyı ID'ye göre getirir
   */
  getUserById: async (id: number): Promise<User> => {
    const response = await apiClient.get<User>(API_ENDPOINTS.USERS.GET_BY_ID(id));
    return response.data;
  },

  /**
   * Yeni kullanıcı oluşturur
   */
  createUser: async (userData: CreateUserRequest): Promise<User> => {
    const response = await apiClient.post<User>(API_ENDPOINTS.USERS.CREATE, userData);
    return response.data;
  },

  /**
   * Kullanıcıyı siler (Soft Delete)
   */
  deleteUser: async (id: number): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.USERS.DELETE(id));
  },

  /**
   * Kullanıcı günceller
   */
  updateUser: async (id: number, payload: Partial<User>): Promise<User> => {
    const response = await apiClient.put<User>(API_ENDPOINTS.USERS.UPDATE(id), payload);
    return response.data;
  },

  /**
   * Kullanıcı girişi yapar
   * NOT: Backend'de auth endpoint yoksa mock olarak kullanılır
   */
  login: async (email: string, _password: string): Promise<{ user: User; token: string }> => {
    // Geçici olarak kullanıcıları çekip email ile eşleştiriyoruz
    // Gerçek projede /auth/login endpoint'i kullanılmalı
    const users = await userService.getAllUsers();
    const user = users.find(u => u.email === email);
    
    if (!user) {
      throw new Error('Kullanıcı bulunamadı');
    }

    // Mock token oluştur
    const token = btoa(JSON.stringify({ userId: user.userId, email: user.email, exp: Date.now() + 3600000 }));
    
    return { user, token };
  },

  /**
   * Şifre değiştirir ve tarihçeye yazar
   */
  changePassword: async (userId: number, newPassword: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.USERS.CHANGE_PASSWORD(userId), { newPassword });
  },

  /**
   * Şifre tarihçesini getirir
   */
  getPasswordHistory: async (userId: number) => {
    const res = await apiClient.get(API_ENDPOINTS.USERS.PASSWORD_HISTORY(userId));
    return res.data;
  },
};

export default userService;
