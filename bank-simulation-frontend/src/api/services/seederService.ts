import apiClient from '../client';
import { API_ENDPOINTS } from '../endpoints';
import { TableCountsResponse } from '../../types';

const seederService = {
  getTableCounts: async (): Promise<TableCountsResponse> => {
    const res = await apiClient.get<TableCountsResponse>(API_ENDPOINTS.SEEDER.GET_TABLE_COUNTS);
    return res.data;
  },
};

export default seederService;
