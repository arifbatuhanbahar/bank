import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import SecurityIcon from '@mui/icons-material/Security';
import ListAltIcon from '@mui/icons-material/ListAlt';
import { auditService } from '../../../api';
import { SecurityEvent, SecurityEventRequest, SecurityEventType, Severity } from '../../../types';
import { SEVERITY_LABELS } from '../../../utils/constants';
import { formatDateTime } from '../../../utils/formatters';

const AdminAuditPage = () => {
  const [events, setEvents] = useState<SecurityEvent[]>([]);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState<SecurityEventRequest>({
    eventType: 'LoginFailed',
    severity: 'Medium',
    description: '',
    userId: undefined,
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadEvents = async () => {
    setLoading(true);
    try {
      const data = await auditService.getSecurityEvents();
      setEvents(data);
    } catch (err) {
      setError('Güvenlik olayları alınamadı.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadEvents();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setError(null);
    try {
      await auditService.logSecurityEvent(form);
      setMessage('Güvenlik olayı kaydedildi.');
      await loadEvents();
      setForm(prev => ({ ...prev, description: '' }));
    } catch (err) {
      setError('Kayıt sırasında hata oluştu.');
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Audit & Güvenlik Olayları
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={5}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <SecurityIcon color="primary" />
              <Typography variant="h6">Güvenlik olayı kaydet</Typography>
            </Box>

            {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            <Box component="form" onSubmit={handleSubmit}>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="event-type-label">Olay Tipi</InputLabel>
                <Select
                  labelId="event-type-label"
                  label="Olay Tipi"
                  value={form.eventType}
                  onChange={e => setForm(prev => ({ ...prev, eventType: e.target.value as SecurityEventType }))}
                >
                  {Object.keys(SecurityEventType).map(key => (
                    <MenuItem key={key} value={SecurityEventType[key as keyof typeof SecurityEventType]}>
                      {key}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="severity-label">Önem</InputLabel>
                <Select
                  labelId="severity-label"
                  label="Önem"
                  value={form.severity}
                  onChange={e => setForm(prev => ({ ...prev, severity: e.target.value as Severity }))}
                >
                  {Object.keys(Severity).map(key => (
                    <MenuItem key={key} value={Severity[key as keyof typeof Severity]}>
                      {SEVERITY_LABELS[key] || key}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                label="Kullanıcı ID (opsiyonel)"
                type="number"
                fullWidth
                sx={{ mb: 2 }}
                value={form.userId ?? ''}
                onChange={e =>
                  setForm(prev => ({ ...prev, userId: e.target.value ? Number(e.target.value) : undefined }))
                }
              />

              <TextField
                label="Açıklama"
                fullWidth
                multiline
                minRows={3}
                sx={{ mb: 2 }}
                value={form.description}
                onChange={e => setForm(prev => ({ ...prev, description: e.target.value }))}
              />

              <Button type="submit" variant="contained" fullWidth disabled={loading}>
                Kaydet
              </Button>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={7}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <ListAltIcon color="primary" />
              <Typography variant="h6">Son güvenlik olayları</Typography>
            </Box>
            <Divider sx={{ mb: 2 }} />
            {loading ? (
              <Typography color="text.secondary">Yükleniyor...</Typography>
            ) : events.length === 0 ? (
              <Typography color="text.secondary">Kayıt yok.</Typography>
            ) : (
              events.map(evt => (
                <Box key={evt.eventId} sx={{ mb: 2, p: 1.5, borderRadius: 2, bgcolor: 'action.hover' }}>
                  <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
                    <Chip label={evt.eventType} size="small" />
                    <Chip
                      label={SEVERITY_LABELS[evt.severity] || evt.severity}
                      color="warning"
                      size="small"
                    />
                    {evt.resolved && <Chip label="Çözüldü" color="success" size="small" />}
                  </Box>
                  <Typography variant="body2" sx={{ mt: 0.5 }}>
                    {evt.description}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Tarih: {formatDateTime(evt.eventDate)} {evt.userId ? `· Kullanıcı: ${evt.userId}` : ''}
                  </Typography>
                </Box>
              ))
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminAuditPage;
