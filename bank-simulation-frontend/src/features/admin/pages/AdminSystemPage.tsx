import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Grid,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import TuneIcon from '@mui/icons-material/Tune';
import { systemService } from '../../../api';
import { SystemSetting } from '../../../types';
import { formatDateTime } from '../../../utils/formatters';

const AdminSystemPage = () => {
  const [settings, setSettings] = useState<SystemSetting[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<{ key: string; value: string; description: string }>({
    key: '',
    value: '',
    description: '',
  });

  const loadSettings = async () => {
    setLoading(true);
    try {
      const data = await systemService.getSettings();
      setSettings(data);
    } catch (err) {
      setError('Sistem ayarları alınamadı.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSettings();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setError(null);
    try {
      await systemService.updateSetting(form.key, form.value, form.description || undefined);
      setMessage('Ayar güncellendi.');
      await loadSettings();
      setForm({ key: '', value: '', description: '' });
    } catch (err) {
      setError('Ayar kaydedilemedi.');
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Sistem Ayarları
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={5}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <SettingsIcon color="primary" />
              <Typography variant="h6">Ayar oluştur / güncelle</Typography>
            </Box>

            {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            <Box component="form" onSubmit={handleSubmit}>
              <TextField
                label="Anahtar"
                fullWidth
                required
                sx={{ mb: 2 }}
                value={form.key}
                onChange={e => setForm(prev => ({ ...prev, key: e.target.value }))}
              />
              <TextField
                label="Değer"
                fullWidth
                required
                sx={{ mb: 2 }}
                value={form.value}
                onChange={e => setForm(prev => ({ ...prev, value: e.target.value }))}
              />
              <TextField
                label="Açıklama"
                fullWidth
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
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <TuneIcon color="primary" />
              <Typography variant="h6">Kayıtlı ayarlar</Typography>
            </Box>
            {loading ? (
              <Typography color="text.secondary">Yükleniyor...</Typography>
            ) : settings.length === 0 ? (
              <Typography color="text.secondary">Henüz ayar yok.</Typography>
            ) : (
              <Grid container spacing={2}>
                {settings.map(setting => (
                  <Grid item xs={12} key={setting.settingId}>
                    <Box sx={{ p: 2, borderRadius: 2, bgcolor: 'action.hover' }}>
                      <Typography fontWeight={600}>{setting.settingKey}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {setting.settingValue}
                      </Typography>
                      {setting.description && (
                        <Typography variant="body2" color="text.secondary">
                          {setting.description}
                        </Typography>
                      )}
                      <Typography variant="caption" color="text.secondary">
                        Güncellendi: {formatDateTime(setting.updatedAt)}
                      </Typography>
                    </Box>
                  </Grid>
                ))}
              </Grid>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminSystemPage;
