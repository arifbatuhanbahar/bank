import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { User, CreateUserRequest, LoginResponse } from '../../types';

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
   * Kullanıcı girişi yapar (backend doğrulamalı)
   */
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, { email, password });
    return response.data;
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
