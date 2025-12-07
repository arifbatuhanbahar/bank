import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { SecurityEvent, SecurityEventRequest } from '../../types';

const auditService = {
  getSecurityEvents: async (): Promise<SecurityEvent[]> => {
    const res = await apiClient.get<SecurityEvent[]>(API_ENDPOINTS.AUDIT.GET_SECURITY_EVENTS);
    return res.data;
  },
  logSecurityEvent: async (payload: SecurityEventRequest): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.AUDIT.LOG_SECURITY_EVENT, payload);
  },
};

export default auditService;
