import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { SystemSetting } from '../../types';

const systemService = {
  getSettings: async (): Promise<SystemSetting[]> => {
    const res = await apiClient.get<SystemSetting[]>(API_ENDPOINTS.SYSTEM.GET_SETTINGS);
    return res.data;
  },
  updateSetting: async (key: string, value: string, description?: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.SYSTEM.UPDATE_SETTING, null, {
      params: { key, value, description },
    });
  },
};

export default systemService;
