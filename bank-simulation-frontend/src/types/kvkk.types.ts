import { ConsentType, KvkkRequestType, RequestStatus } from './enums';

export interface KvkkConsent {
  consentId: number;
  userId: number;
  consentType: ConsentType;
  consentGiven: boolean;
  consentText: string;
  consentVersion: string;
  grantedAt: string;
  revokedAt?: string;
  ipAddress?: string;
}

export interface KvkkConsentRequest {
  userId: number;
  consentType: ConsentType;
  consentGiven: boolean;
  consentText?: string;
  consentVersion?: string;
  ipAddress?: string;
}

export interface KvkkDataRequest {
  requestId: number;
  userId: number;
  requestType: KvkkRequestType;
  requestDate: string;
  status: RequestStatus;
  completedAt?: string;
  completedBy?: number;
  responseData?: string;
  responseFilePath?: string;
}

export interface KvkkDataRequestCreate {
  userId: number;
  requestType: KvkkRequestType;
}
