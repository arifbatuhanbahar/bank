import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import {
  KycDocument,
  KycUploadRequest,
  KvkkConsent,
  KvkkConsentRequest,
  KvkkDataRequest,
  KvkkDataRequestCreate,
} from '../../types';

export const complianceService = {
  // KYC
  getKycDocuments: async (userId: number): Promise<KycDocument[]> => {
    const res = await apiClient.get<KycDocument[]>(API_ENDPOINTS.COMPLIANCE.GET_KYC_DOCUMENTS(userId));
    return res.data;
  },
  uploadKycDocument: async (payload: KycUploadRequest): Promise<{ message: string; documentId: number }> => {
    const res = await apiClient.post<{ message: string; documentId: number }>(
      API_ENDPOINTS.COMPLIANCE.UPLOAD_DOCUMENT,
      payload
    );
    return res.data;
  },
  verifyKycDocument: async (documentId: number, isApproved: boolean, rejectionReason?: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.COMPLIANCE.VERIFY_DOCUMENT, null, {
      params: { documentId, isApproved, rejectionReason },
    });
  },

  // KVKK Consents
  getConsents: async (userId: number): Promise<KvkkConsent[]> => {
    const res = await apiClient.get<KvkkConsent[]>(API_ENDPOINTS.COMPLIANCE.GET_CONSENTS(userId));
    return res.data;
  },
  upsertConsent: async (payload: KvkkConsentRequest): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.COMPLIANCE.UPSERT_CONSENT, payload);
  },

  // KVKK Data Requests
  getKvkkRequests: async (userId: number): Promise<KvkkDataRequest[]> => {
    const res = await apiClient.get<KvkkDataRequest[]>(API_ENDPOINTS.COMPLIANCE.GET_KVKK_REQUESTS(userId));
    return res.data;
  },
  createKvkkRequest: async (payload: KvkkDataRequestCreate): Promise<{ requestId: number; message: string }> => {
    const res = await apiClient.post<{ requestId: number; message: string }>(
      API_ENDPOINTS.COMPLIANCE.CREATE_KVKK_REQUEST,
      payload
    );
    return res.data;
  },
};

export default complianceService;
