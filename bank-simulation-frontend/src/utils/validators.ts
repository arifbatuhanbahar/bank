import * as yup from 'yup';

// ==================== CUSTOM VALIDATION FUNCTIONS ====================

/**
 * TC Kimlik numarası doğrulama
 */
export const isValidTcKimlik = (tcNo: string): boolean => {
  if (!/^\d{11}$/.test(tcNo)) return false;
  if (tcNo[0] === '0') return false;

  const digits = tcNo.split('').map(Number);
  
  // 10. hane kontrolü
  const oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
  const evenSum = digits[1] + digits[3] + digits[5] + digits[7];
  const digit10 = ((oddSum * 7) - evenSum) % 10;
  if (digit10 !== digits[9]) return false;

  // 11. hane kontrolü
  const first10Sum = digits.slice(0, 10).reduce((a, b) => a + b, 0);
  if (first10Sum % 10 !== digits[10]) return false;

  return true;
};

/**
 * IBAN doğrulama (Türkiye formatı)
 */
export const isValidIban = (iban: string): boolean => {
  const cleaned = iban.replace(/\s/g, '').toUpperCase();
  
  // Türkiye IBAN formatı: TR + 24 rakam
  if (!/^TR\d{24}$/.test(cleaned)) return false;

  // IBAN mod 97 kontrolü
  const rearranged = cleaned.slice(4) + cleaned.slice(0, 4);
  const numericIban = rearranged.replace(/[A-Z]/g, (char) => 
    (char.charCodeAt(0) - 55).toString()
  );
  
  let remainder = '';
  for (const char of numericIban) {
    remainder = (parseInt(remainder + char, 10) % 97).toString();
  }
  
  return parseInt(remainder, 10) === 1;
};

/**
 * Email doğrulama
 */
export const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

/**
 * Telefon numarası doğrulama (Türkiye)
 */
export const isValidPhone = (phone: string): boolean => {
  const cleaned = phone.replace(/\D/g, '');
  return /^5\d{9}$/.test(cleaned);
};

/**
 * Şifre güçlülük kontrolü
 */
export const getPasswordStrength = (password: string): {
  score: number;
  label: string;
  color: string;
} => {
  let score = 0;

  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;

  const levels = [
    { score: 0, label: 'Çok Zayıf', color: '#f44336' },
    { score: 1, label: 'Zayıf', color: '#ff9800' },
    { score: 2, label: 'Zayıf', color: '#ff9800' },
    { score: 3, label: 'Orta', color: '#ffeb3b' },
    { score: 4, label: 'Güçlü', color: '#8bc34a' },
    { score: 5, label: 'Çok Güçlü', color: '#4caf50' },
    { score: 6, label: 'Mükemmel', color: '#2e7d32' },
  ];

  return levels[score] || levels[0];
};

// ==================== YUP VALIDATION SCHEMAS ====================

/**
 * Login formu şeması
 */
export const loginSchema = yup.object({
  email: yup
    .string()
    .email('Geçerli bir e-posta adresi giriniz')
    .required('E-posta adresi zorunludur'),
  password: yup
    .string()
    .min(6, 'Şifre en az 6 karakter olmalıdır')
    .required('Şifre zorunludur'),
});

/**
 * Kayıt formu şeması
 */
export const registerSchema = yup.object({
  tcKimlikNo: yup
    .string()
    .length(11, 'TC Kimlik numarası 11 haneli olmalıdır')
    .matches(/^\d+$/, 'TC Kimlik numarası sadece rakamlardan oluşmalıdır')
    .test('valid-tc', 'Geçersiz TC Kimlik numarası', (value) => 
      value ? isValidTcKimlik(value) : false
    )
    .required('TC Kimlik numarası zorunludur'),
  firstName: yup
    .string()
    .min(2, 'İsim en az 2 karakter olmalıdır')
    .max(100, 'İsim en fazla 100 karakter olabilir')
    .required('İsim zorunludur'),
  lastName: yup
    .string()
    .min(2, 'Soyisim en az 2 karakter olmalıdır')
    .max(100, 'Soyisim en fazla 100 karakter olabilir')
    .required('Soyisim zorunludur'),
  email: yup
    .string()
    .email('Geçerli bir e-posta adresi giriniz')
    .required('E-posta adresi zorunludur'),
  phone: yup
    .string()
    .test('valid-phone', 'Geçerli bir telefon numarası giriniz', (value) =>
      value ? isValidPhone(value) : true
    ),
  password: yup
    .string()
    .min(8, 'Şifre en az 8 karakter olmalıdır')
    .matches(/[A-Z]/, 'Şifre en az bir büyük harf içermelidir')
    .matches(/[a-z]/, 'Şifre en az bir küçük harf içermelidir')
    .matches(/[0-9]/, 'Şifre en az bir rakam içermelidir')
    .required('Şifre zorunludur'),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Şifreler eşleşmiyor')
    .required('Şifre tekrarı zorunludur'),
  dateOfBirth: yup
    .date()
    .max(new Date(Date.now() - 18 * 365 * 24 * 60 * 60 * 1000), '18 yaşından büyük olmalısınız')
    .required('Doğum tarihi zorunludur'),
});

/**
 * Para transferi formu şeması
 */
export const transferSchema = yup.object({
  fromAccountId: yup
    .number()
    .positive('Kaynak hesap seçiniz')
    .required('Kaynak hesap zorunludur'),
  toAccountId: yup
    .number()
    .positive('Hedef hesap seçiniz')
    .notOneOf([yup.ref('fromAccountId')], 'Kaynak ve hedef hesap aynı olamaz')
    .required('Hedef hesap zorunludur'),
  amount: yup
    .number()
    .positive('Tutar pozitif olmalıdır')
    .min(1, 'Minimum transfer tutarı 1 TL')
    .max(100000, 'Maksimum transfer tutarı 100.000 TL')
    .required('Tutar zorunludur'),
  description: yup
    .string()
    .max(255, 'Açıklama en fazla 255 karakter olabilir')
    .required('Açıklama zorunludur'),
});

/**
 * Kredi kartı başvuru formu şeması
 */
export const cardApplicationSchema = yup.object({
  cardTypeRequested: yup
    .string()
    .oneOf(['Standard', 'Gold', 'Platinum', 'Corporate', 'Black'], 'Geçerli bir kart türü seçiniz')
    .required('Kart türü zorunludur'),
  monthlyIncome: yup
    .number()
    .positive('Aylık gelir pozitif olmalıdır')
    .min(5000, 'Minimum aylık gelir 5.000 TL olmalıdır')
    .required('Aylık gelir zorunludur'),
  employmentStatus: yup
    .string()
    .oneOf(['Employed', 'SelfEmployed', 'Unemployed', 'Retired', 'Student'], 'Geçerli bir çalışma durumu seçiniz')
    .required('Çalışma durumu zorunludur'),
  employerName: yup
    .string()
    .min(2, 'İşveren adı en az 2 karakter olmalıdır')
    .max(100, 'İşveren adı en fazla 100 karakter olabilir')
    .required('İşveren adı zorunludur'),
});

/**
 * IBAN doğrulama şeması
 */
export const ibanSchema = yup.object({
  iban: yup
    .string()
    .transform((value) => value?.replace(/\s/g, '').toUpperCase())
    .test('valid-iban', 'Geçersiz IBAN', (value) => value ? isValidIban(value) : false)
    .required('IBAN zorunludur'),
});

/**
 * KYC belge yükleme şeması
 */
export const kycUploadSchema = yup.object({
  documentType: yup.string().required('Belge türü zorunludur'),
  documentNumber: yup.string().min(4, 'Belge numarası çok kısa').required('Belge numarası zorunludur'),
});

/**
 * KVKK talep şeması
 */
export const kvkkRequestSchema = yup.object({
  requestType: yup
    .string()
    .oneOf(['Access', 'Rectification', 'Erasure', 'Portability'], 'Geçerli bir talep türü seçiniz')
    .required('Talep türü zorunludur'),
});

// Type exports for form data
export type LoginFormData = yup.InferType<typeof loginSchema>;
export type RegisterFormData = yup.InferType<typeof registerSchema>;
export type TransferFormData = yup.InferType<typeof transferSchema>;
export type CardApplicationFormData = yup.InferType<typeof cardApplicationSchema>;
export type KycUploadFormData = yup.InferType<typeof kycUploadSchema>;
export type KvkkRequestFormData = yup.InferType<typeof kvkkRequestSchema>;
