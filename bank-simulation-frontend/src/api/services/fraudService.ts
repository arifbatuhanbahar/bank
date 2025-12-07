import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { FraudCheckRequest, FraudCheckResult, FraudRuleRequest } from '../../types';

const fraudService = {
  createRule: async (payload: FraudRuleRequest): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.FRAUD.CREATE_RULE, payload);
  },
  checkTransaction: async (payload: FraudCheckRequest): Promise<FraudCheckResult> => {
    const res = await apiClient.post<FraudCheckResult>(
      API_ENDPOINTS.FRAUD.CHECK_TRANSACTION,
      null,
      { params: payload }
    );
    return res.data;
  },
};

export default fraudService;
