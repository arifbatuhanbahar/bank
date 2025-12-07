import { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Switch,
  FormControlLabel,
  Divider,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Chip,
  Skeleton,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import GavelIcon from '@mui/icons-material/Gavel';
import DescriptionIcon from '@mui/icons-material/Description';
import { complianceService } from '../../../api';
import {
  ConsentType,
  KvkkConsent,
  KvkkDataRequest,
  KvkkRequestType,
  RequestStatus,
} from '../../../types';
import {
  CONSENT_TYPE_LABELS,
  KVKK_REQUEST_TYPE_LABELS,
  REQUEST_STATUS_LABELS,
} from '../../../utils/constants';
import { kvkkRequestSchema, KvkkRequestFormData } from '../../../utils/validators';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { formatDateTime } from '../../../utils/formatters';

const consentOrder: ConsentType[] = [
  'DataProcessing',
  'Marketing',
  'ThirdPartyTransfer',
];

const KvkkPage = () => {
  const [consents, setConsents] = useState<KvkkConsent[]>([]);
  const [requests, setRequests] = useState<KvkkDataRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  const { control, handleSubmit, reset } = useForm<KvkkRequestFormData>({
    resolver: yupResolver(kvkkRequestSchema),
    defaultValues: { requestType: 'Access' },
  });

  const loadData = async () => {
    if (!user?.userId) {
      setError('Kullanıcı bulunamadı.');
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const [consentData, requestData] = await Promise.all([
        complianceService.getConsents(user.userId),
        complianceService.getKvkkRequests(user.userId),
      ]);
      setConsents(consentData);
      setRequests(requestData);
    } catch (err) {
      console.error(err);
      setError('KVKK verileri alınamadı.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [user?.userId]);

  const handleConsentToggle = async (type: ConsentType, value: boolean) => {
    if (!user?.userId) return;
    setError(null);
    try {
      await complianceService.upsertConsent({
        userId: user.userId,
        consentType: type,
        consentGiven: value,
        consentText: CONSENT_TYPE_LABELS[type] || 'KVKK Metni',
        consentVersion: 'v1.0',
        ipAddress: '127.0.0.1',
      });
      await loadData();
      setSuccess('Rıza güncellendi.');
    } catch (err) {
      console.error(err);
      setError('Rıza güncellenemedi.');
    }
  };

  const onSubmit = async (data: KvkkRequestFormData) => {
    if (!user?.userId) return;
    setError(null);
    setSuccess(null);
    try {
      await complianceService.createKvkkRequest({
        userId: user.userId,
        requestType: data.requestType as KvkkRequestType,
      });
      setSuccess('Talebiniz alındı.');
      reset({ requestType: 'Access' });
      await loadData();
    } catch (err) {
      console.error(err);
      setError('Talep gönderilemedi.');
    }
  };

  const requestStatusChip = (status: RequestStatus) => {
    const colorMap: Record<RequestStatus, 'default' | 'success' | 'warning' | 'error'> = {
      Pending: 'warning',
      InProgress: 'info',
      Completed: 'success',
      Rejected: 'error',
    } as any;
    return (
      <Chip
        label={REQUEST_STATUS_LABELS[status] || status}
        color={colorMap[status] || 'default'}
        size="small"
      />
    );
  };

  const consentValue = (type: ConsentType) =>
    consents.find(c => c.consentType === type)?.consentGiven ?? false;

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        KVKK & KYC Yönetimi
      </Typography>

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

      <Grid container spacing={3}>
        {/* Consents */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <GavelIcon color="primary" />
              <Typography variant="h6">KVKK Rızaları</Typography>
            </Box>
            <Divider sx={{ mb: 2 }} />
            {loading ? (
              <Box>
                {[1, 2, 3].map(i => (
                  <Skeleton key={i} height={48} sx={{ mb: 1 }} />
                ))}
              </Box>
            ) : (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {consentOrder.map(type => (
                  <FormControlLabel
                    key={type}
                    control={
                      <Switch
                        checked={consentValue(type)}
                        onChange={e => handleConsentToggle(type, e.target.checked)}
                      />
                    }
                    label={CONSENT_TYPE_LABELS[type] || type}
                  />
                ))}
              </Box>
            )}
          </Paper>
        </Grid>

        {/* KVKK Request */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <DescriptionIcon color="primary" />
              <Typography variant="h6">KVKK Talebi Oluştur</Typography>
            </Box>
            <Divider sx={{ mb: 2 }} />
            <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="kvkk-request-label">Talep Türü</InputLabel>
                <Controller
                  name="requestType"
                  control={control}
                  render={({ field }) => (
                    <Select
                      {...field}
                      labelId="kvkk-request-label"
                      label="Talep Türü"
                      value={field.value}
                    >
                      {Object.keys(KvkkRequestType).map(key => (
                        <MenuItem key={key} value={KvkkRequestType[key as keyof typeof KvkkRequestType]}>
                          {KVKK_REQUEST_TYPE_LABELS[key] || key}
                        </MenuItem>
                      ))}
                    </Select>
                  )}
                />
              </FormControl>
              <Button type="submit" variant="contained" fullWidth>
                Talep Gönder
              </Button>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Request list */}
      <Paper sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Talep Geçmişi
        </Typography>
        <Divider sx={{ mb: 2 }} />
        {loading ? (
          <Box>
            {[1, 2, 3].map(i => (
              <Skeleton key={i} height={52} sx={{ mb: 1 }} />
            ))}
          </Box>
        ) : requests.length === 0 ? (
          <Typography color="text.secondary">Talep bulunamadı.</Typography>
        ) : (
          <List disablePadding>
            {requests.map((req, idx) => (
              <Box key={req.requestId}>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
                        <Typography fontWeight={600}>
                          {KVKK_REQUEST_TYPE_LABELS[req.requestType] || req.requestType}
                        </Typography>
                        {requestStatusChip(req.status)}
                      </Box>
                    }
                    secondary={
                      <Typography variant="caption" color="text.secondary">
                        Oluşturma: {formatDateTime(req.requestDate)}
                        {req.completedAt ? ` • Tamamlandı: ${formatDateTime(req.completedAt)}` : ''}
                      </Typography>
                    }
                  />
                </ListItem>
                {idx < requests.length - 1 && <Divider />}
              </Box>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default KvkkPage;
