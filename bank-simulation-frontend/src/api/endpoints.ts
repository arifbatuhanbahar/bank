// API Endpoint Definitions
// Backend: http://localhost:5161/swagger

export const API_ENDPOINTS = {
  // ==================== USERS ====================
  USERS: {
    BASE: '/Users',
    GET_ALL: '/Users',
    GET_BY_ID: (id: number) => `/Users/${id}`,
    CREATE: '/Users',
    DELETE: (id: number) => `/Users/${id}`,
  },

  // ==================== ACCOUNTS ====================
  ACCOUNTS: {
    BASE: '/Accounts',
    GET_ALL: '/Accounts',
    GET_BY_ID: (id: number) => `/Accounts/${id}`,
    GET_BY_USER: (userId: number) => `/Accounts/user/${userId}`,
    CREATE: '/Accounts',
    UPDATE_BALANCE: (id: number) => `/Accounts/${id}/balance`,
  },

  // ==================== TRANSACTIONS ====================
  TRANSACTIONS: {
    BASE: '/Transactions',
    GET_BY_ACCOUNT: (accountId: number) => `/Transactions/account/${accountId}`,
    TRANSFER: '/Transactions/transfer',
  },

  // ==================== PAYMENTS & CARDS ====================
  PAYMENTS: {
    BASE: '/Payments',
    GET_USER_CARDS: (userId: number) => `/Payments/cards/user/${userId}`,
    CREATE_CARD: '/Payments/cards',
    CARD_TRANSACTION: '/Payments/transaction',
  },

  // ==================== APPLICATIONS ====================
  APPLICATION: {
    BASE: '/Application',
    APPLY: '/Application/apply',
    APPROVE: (applicationId: number) => `/Application/approve/${applicationId}`,
    GET_BY_USER: (userId: number) => `/Application/user/${userId}`,
  },

  // ==================== COMPLIANCE ====================
  COMPLIANCE: {
    BASE: '/Compliance',
    GET_KYC_DOCUMENTS: (userId: number) => `/Compliance/kyc-documents/${userId}`,
    UPLOAD_DOCUMENT: '/Compliance/upload-document',
    VERIFY_DOCUMENT: '/Compliance/verify-document',
    REPORT_SUSPICIOUS: '/Compliance/report-suspicious-activity',
    GET_CONSENTS: (userId: number) => `/Compliance/consents/${userId}`,
    UPSERT_CONSENT: '/Compliance/consents',
    GET_KVKK_REQUESTS: (userId: number) => `/Compliance/kvkk-requests/${userId}`,
    CREATE_KVKK_REQUEST: '/Compliance/kvkk-requests',
  },

  // ==================== FRAUD ====================
  FRAUD: {
    BASE: '/Fraud',
    CREATE_RULE: '/Fraud/rules',
    CHECK_TRANSACTION: '/Fraud/check-transaction',
  },

  // ==================== AUDIT ====================
  AUDIT: {
    BASE: '/Audit',
    LOG_SECURITY_EVENT: '/Audit/security-event',
    GET_SECURITY_EVENTS: '/Audit/security-events',
    LOG_DATA_ACCESS: '/Audit/access-log',
  },

  // ==================== SYSTEM ====================
  SYSTEM: {
    BASE: '/System',
    GET_SETTINGS: '/System/settings',
    UPDATE_SETTING: '/System/settings',
    CREATE_TEMPLATE: '/System/templates',
  },

  // ==================== SEEDER (Development) ====================
  SEEDER: {
    BASE: '/Seeder',
    SEED_ALL: '/Seeder/seed-all',
    GET_STATS: '/Seeder/stats',
    GET_TABLE_COUNTS: '/Seeder/table-counts',
    CLEAR_ALL: '/Seeder/clear-all',
  },
} as const;

// Helper type for endpoint paths
export type EndpointPath = string | ((...args: number[]) => string);
