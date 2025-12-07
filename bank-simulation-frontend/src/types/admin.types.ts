import {
  SecurityEventType,
  Severity,
  RuleType,
  SettingType,
  TemplateType,
} from './enums';

export interface SecurityEvent {
  eventId: number;
  eventType: SecurityEventType;
  userId?: number;
  severity: Severity;
  description: string;
  ipAddress?: string;
  userAgent?: string;
  eventDate: string;
  resolved: boolean;
  resolvedAt?: string;
  resolvedBy?: number;
}

export interface SecurityEventRequest {
  eventType: SecurityEventType;
  userId?: number;
  severity: Severity;
  description: string;
}

export interface FraudRule {
  ruleId: number;
  ruleName: string;
  ruleType: RuleType;
  ruleDescription?: string;
  ruleConditions: string;
  riskScoreWeight: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface FraudRuleRequest {
  ruleName: string;
  ruleType: RuleType;
  description: string;
  conditions: string;
  riskWeight: number;
}

export interface FraudCheckRequest {
  transactionId: number;
  userId: number;
  amount: number;
}

export interface FraudCheckResult {
  message: string;
  score: number;
}

export interface SystemSetting {
  settingId: number;
  settingKey: string;
  settingValue: string;
  settingType: SettingType;
  description?: string;
  updatedAt: string;
  updatedBy?: number;
}

export interface TemplateRequest {
  templateName: string;
  templateType: TemplateType;
  subject?: string;
  body: string;
  variables?: string;
}

export interface TableCountsResponse {
  success: boolean;
  totalTables: number;
  totalRecords: number;
  tables: Record<string, number>;
}
