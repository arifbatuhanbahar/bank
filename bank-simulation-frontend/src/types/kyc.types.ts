import { DocumentType, VerificationStatus } from './enums';

export interface KycDocument {
  documentId: number;
  userId: number;
  documentType: DocumentType;
  documentNumber: string;
  documentFilePath: string;
  documentHash?: string;
  uploadDate: string;
  verifiedAt?: string;
  verifiedBy?: number;
  verificationStatus: VerificationStatus;
  rejectionReason?: string;
}

export interface KycUploadRequest {
  userId: number;
  documentType: DocumentType;
  documentNumber: string;
}
