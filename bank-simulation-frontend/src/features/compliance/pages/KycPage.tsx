import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button,
  Alert,
  Chip,
  Divider,
  Skeleton,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import CancelIcon from '@mui/icons-material/Cancel';
import { Controller, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { complianceService } from '../../../api';
import { DocumentType, VerificationStatus, KycDocument, KycUploadRequest } from '../../../types';
import { DOCUMENT_TYPE_LABELS } from '../../../utils/constants';
import { formatDateTime } from '../../../utils/formatters';
import { kycUploadSchema, KycUploadFormData } from '../../../utils/validators';
import { isAdminUser } from '../../../utils/auth';

const KycPage = () => {
  const [documents, setDocuments] = useState<KycDocument[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;
  const isAdmin = isAdminUser(user);

  const {
    control,
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<KycUploadFormData>({
    resolver: yupResolver(kycUploadSchema),
    defaultValues: {
      documentType: 'IdentityCard',
      documentNumber: '',
    },
  });

  const loadDocs = async () => {
    if (!user?.userId) {
      setError('Kullanıcı bilgisi bulunamadı.');
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const data = await complianceService.getKycDocuments(user.userId);
      setDocuments(data);
    } catch (err) {
      setError('Belgeler yüklenirken hata oluştu.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDocs();
  }, [user?.userId]);

  const onSubmit = async (data: KycUploadFormData) => {
    if (!user?.userId) {
      setError('Kullanıcı bilgisi bulunamadı.');
      return;
    }
    setError(null);
    setSuccess(null);
    try {
      const payload: KycUploadRequest = {
        userId: user.userId,
        documentType: data.documentType as DocumentType,
        documentNumber: data.documentNumber,
      };
      await complianceService.uploadKycDocument(payload);
      setSuccess('Belge yüklendi. Doğrulama bekleniyor.');
      reset({ documentType: data.documentType, documentNumber: '' });
      await loadDocs();
    } catch (err) {
      setError('Belge yüklenemedi.');
    }
  };

  const statusChip = (status: VerificationStatus) => {
    switch (status) {
      case 'Verified':
        return <Chip label="Doğrulandı" color="success" size="small" icon={<CheckCircleIcon />} />;
      case 'Pending':
        return <Chip label="Beklemede" color="warning" size="small" icon={<HourglassEmptyIcon />} />;
      case 'Rejected':
      case 'Failed':
        return <Chip label="Reddedildi" color="error" size="small" icon={<CancelIcon />} />;
      default:
        return <Chip label={status} size="small" />;
    }
  };

  const handleVerify = async (documentId: number, approve: boolean) => {
    if (!isAdmin) return;
    setError(null);
    try {
      await complianceService.verifyKycDocument(documentId, approve, approve ? undefined : 'Reddedildi');
      await loadDocs();
    } catch {
      setError('Belge onay işlemi başarısız.');
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        KYC Belgeleri
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={5}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <CloudUploadIcon color="primary" />
              <Typography variant="h6">Belge Yükle</Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}
            {success && (
              <Alert severity="success" sx={{ mb: 2 }}>
                {success}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="doc-type-label">Belge Türü</InputLabel>
                <Controller
                  name="documentType"
                  control={control}
                  render={({ field }) => (
                    <Select
                      {...field}
                      labelId="doc-type-label"
                      label="Belge Türü"
                      value={field.value}
                    >
                      {Object.keys(DocumentType).map(key => (
                        <MenuItem key={key} value={DocumentType[key as keyof typeof DocumentType]}>
                          {DOCUMENT_TYPE_LABELS[key] || key}
                        </MenuItem>
                      ))}
                    </Select>
                  )}
                />
                {errors.documentType && (
                  <Typography variant="caption" color="error">
                    {errors.documentType.message}
                  </Typography>
                )}
              </FormControl>

              <TextField
                label="Belge Numarası"
                fullWidth
                sx={{ mb: 2 }}
                {...register('documentNumber')}
                error={!!errors.documentNumber}
                helperText={errors.documentNumber?.message}
              />

              <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}>
                {isSubmitting ? 'Gönderiliyor...' : 'Yükle'}
              </Button>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={7}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Belgelerim
            </Typography>
            <Divider sx={{ mb: 2 }} />
            {loading ? (
              <Box>
                {[1, 2, 3].map(i => (
                  <Skeleton key={i} height={60} sx={{ mb: 1 }} />
                ))}
              </Box>
            ) : documents.length === 0 ? (
              <Typography color="text.secondary">Henüz yüklenmiş belge yok.</Typography>
            ) : (
              <List disablePadding>
                {documents.map(doc => (
                  <Box key={doc.documentId}>
                    <ListItem sx={{ px: 0 }} secondaryAction={
                      isAdmin && doc.verificationStatus === 'Pending' ? (
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Button size="small" color="success" onClick={() => handleVerify(doc.documentId, true)}>
                            Onayla
                          </Button>
                          <Button size="small" color="error" onClick={() => handleVerify(doc.documentId, false)}>
                            Reddet
                          </Button>
                        </Box>
                      ) : null
                    }>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                            <Typography fontWeight={600}>
                              {DOCUMENT_TYPE_LABELS[doc.documentType] || doc.documentType}
                            </Typography>
                            {statusChip(doc.verificationStatus)}
                          </Box>
                        }
                        secondary={
                          <Typography variant="caption" color="text.secondary">
                            No: {doc.documentNumber} · Yüklendi: {formatDateTime(doc.uploadDate)}
                          </Typography>
                        }
                      />
                    </ListItem>
                    <Divider />
                  </Box>
                ))}
              </List>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default KycPage;
