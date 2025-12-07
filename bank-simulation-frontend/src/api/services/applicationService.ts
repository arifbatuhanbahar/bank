import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { CardApplication } from '../../types';

const LOCAL_APPLICATIONS_KEY = 'cardApplications';

const readLocalApplications = (): Record<number, CardApplication[]> => {
  try {
    const raw = localStorage.getItem(LOCAL_APPLICATIONS_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
};

const writeLocalApplications = (data: Record<number, CardApplication[]>) => {
  localStorage.setItem(LOCAL_APPLICATIONS_KEY, JSON.stringify(data));
};

export const saveLocalApplication = (application: CardApplication) => {
  const store = readLocalApplications();
  const list = store[application.userId] || [];
  store[application.userId] = [application, ...list];
  writeLocalApplications(store);
};

export const applicationService = {
  // Kullanıcının kart başvurularını getirir; 404 dönerse boş liste veya local cache döner
  getUserApplications: async (userId: number): Promise<CardApplication[]> => {
    try {
      const res = await apiClient.get<CardApplication[]>(API_ENDPOINTS.APPLICATION.GET_BY_USER(userId));
      const serverData = res.data || [];
      if (serverData.length > 0) {
        return serverData;
      }
      const local = readLocalApplications();
      return local[userId] || [];
    } catch (err: any) {
      if (err?.response?.status === 404) {
        const local = readLocalApplications();
        return local[userId] || [];
      }
      throw err;
    }
  },
};

export default applicationService;
