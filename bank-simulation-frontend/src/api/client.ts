import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

// API Base URL - Environment variable'dan alınır
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5161/api';

// Axios instance
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor - Her istekte token ekle
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('authToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Request logging (development only)
    if (import.meta.env.DEV) {
      console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`);
    }

    return config;
  },
  (error: AxiosError) => {
    console.error('Request Error:', error);
    return Promise.reject(error);
  }
);

// Response Interceptor - Hata yönetimi
apiClient.interceptors.response.use(
  response => {
    // Response logging (development only)
    if (import.meta.env.DEV) {
      console.log(`[API Response] ${response.status} ${response.config.url}`);
    }
    return response;
  },
  (error: AxiosError) => {
    // Response logging
    if (import.meta.env.DEV) {
      console.error(`[API Error] ${error.response?.status} ${error.config?.url}`);
    }

    // 401 Unauthorized - Token expired veya geçersiz
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');

      // Login sayfasına yönlendir (eğer zaten login sayfasında değilse)
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }

    // 403 Forbidden - Yetki yok
    if (error.response?.status === 403) {
      console.error('Yetkiniz bulunmamaktadır.');
    }

    // 404 Not Found
    if (error.response?.status === 404) {
      console.error('Kaynak bulunamadı.');
    }

    // 500 Server Error
    if (error.response?.status === 500) {
      console.error('Sunucu hatası oluştu.');
    }

    // Network Error
    if (error.message === 'Network Error') {
      console.error('Ağ bağlantısı hatası. Lütfen internet bağlantınızı kontrol edin.');
    }

    return Promise.reject(error);
  }
);

export default apiClient;
