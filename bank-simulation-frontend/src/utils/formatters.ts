import { format, formatDistance, parseISO } from 'date-fns';
import { tr } from 'date-fns/locale';

// ==================== PARA FORMATLAMA ====================

/**
 * Para birimini formatlar
 * @example formatCurrency(1234.56, 'TRY') => '1.234,56 ₺'
 */
export const formatCurrency = (amount: number, currency: string = 'TRY'): string => {
  const currencySymbols: Record<string, string> = {
    TRY: '₺',
    USD: '$',
    EUR: '€',
  };

  const formatted = new Intl.NumberFormat('tr-TR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);

  const symbol = currencySymbols[currency] || currency;
  
  // Para birimi sembolünün konumu
  if (currency === 'TRY') {
    return `${formatted} ${symbol}`;
  }
  return `${symbol}${formatted}`;
};

/**
 * Sayıyı binlik ayırıcı ile formatlar
 * @example formatNumber(1234567) => '1.234.567'
 */
export const formatNumber = (num: number): string => {
  return new Intl.NumberFormat('tr-TR').format(num);
};

/**
 * Yüzde formatlar
 * @example formatPercentage(0.1234) => '%12,34'
 */
export const formatPercentage = (value: number, decimals: number = 2): string => {
  return `%${(value * 100).toFixed(decimals).replace('.', ',')}`;
};

// ==================== TARİH FORMATLAMA ====================

/**
 * Tarihi Türkçe formatlar
 * @example formatDate('2024-01-15T10:30:00') => '15 Ocak 2024'
 */
export const formatDate = (dateString: string | Date, formatStr: string = 'd MMMM yyyy'): string => {
  try {
    const date = typeof dateString === 'string' ? parseISO(dateString) : dateString;
    return format(date, formatStr, { locale: tr });
  } catch {
    return '-';
  }
};

/**
 * Tarih ve saati formatlar
 * @example formatDateTime('2024-01-15T10:30:00') => '15.01.2024 10:30'
 */
export const formatDateTime = (dateString: string | Date): string => {
  try {
    const date = typeof dateString === 'string' ? parseISO(dateString) : dateString;
    return format(date, 'dd.MM.yyyy HH:mm', { locale: tr });
  } catch {
    return '-';
  }
};

/**
 * Göreceli zaman formatlar
 * @example formatRelativeTime('2024-01-14T10:00:00') => '1 gün önce'
 */
export const formatRelativeTime = (dateString: string | Date): string => {
  try {
    const date = typeof dateString === 'string' ? parseISO(dateString) : dateString;
    return formatDistance(date, new Date(), { addSuffix: true, locale: tr });
  } catch {
    return '-';
  }
};

/**
 * Kart son kullanma tarihini formatlar
 * @example formatCardExpiry(12, 2025) => '12/25'
 */
export const formatCardExpiry = (month: number, year: number): string => {
  return `${month.toString().padStart(2, '0')}/${year.toString().slice(-2)}`;
};

// ==================== METİN FORMATLAMA ====================

/**
 * IBAN formatlar
 * @example formatIban('TR1234567890123456789012') => 'TR12 3456 7890 1234 5678 9012'
 */
export const formatIban = (iban: string): string => {
  const cleaned = iban.replace(/\s/g, '');
  return cleaned.replace(/(.{4})/g, '$1 ').trim();
};

/**
 * Telefon numarasını formatlar
 * @example formatPhone('5551234567') => '(555) 123 45 67'
 */
export const formatPhone = (phone: string): string => {
  const cleaned = phone.replace(/\D/g, '');
  if (cleaned.length === 10) {
    return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)} ${cleaned.slice(6, 8)} ${cleaned.slice(8)}`;
  }
  return phone;
};

/**
 * TC Kimlik numarasını formatlar (maskeleme opsiyonlu)
 * @example formatTcKimlik('12345678901', true) => '123****8901'
 */
export const formatTcKimlik = (tcNo: string, mask: boolean = false): string => {
  if (mask && tcNo.length === 11) {
    return `${tcNo.slice(0, 3)}****${tcNo.slice(-4)}`;
  }
  return tcNo;
};

/**
 * Kart numarasını maskeler
 * @example maskCardNumber('1234') => '**** **** **** 1234'
 */
export const maskCardNumber = (lastFour: string): string => {
  return `**** **** **** ${lastFour}`;
};

/**
 * IBAN'ı maskeler
 * @example maskIban('TR123456789012345678901234') => 'TR12 **** **** **** **** 1234'
 */
export const maskIban = (iban: string): string => {
  if (iban.length < 8) return iban;
  const masked = iban.slice(0, 4) + '*'.repeat(iban.length - 8) + iban.slice(-4);
  return formatIban(masked);
};

// ==================== İSİM FORMATLAMA ====================

/**
 * Tam adı formatlar
 * @example formatFullName('JOHN', 'DOE') => 'John Doe'
 */
export const formatFullName = (firstName: string, lastName: string): string => {
  const capitalize = (str: string) => 
    str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
  return `${capitalize(firstName)} ${capitalize(lastName)}`;
};

/**
 * Baş harfleri alır
 * @example getInitials('John Doe') => 'JD'
 */
export const getInitials = (name: string): string => {
  return name
    .split(' ')
    .map(part => part.charAt(0).toUpperCase())
    .slice(0, 2)
    .join('');
};
