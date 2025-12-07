import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  List,
  ListItem,
  ListItemText,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import LockResetIcon from '@mui/icons-material/LockReset';
import HistoryIcon from '@mui/icons-material/History';
import { userService } from '../../../api';
import { PasswordHistory } from '../../../types';
import { formatDateTime } from '../../../utils/formatters';
import { SUCCESS_MESSAGES } from '../../../utils/constants';

const ChangePasswordPage = () => {
  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;
  const [newPassword, setNewPassword] = useState('');
  const [history, setHistory] = useState<PasswordHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadHistory = async () => {
    if (!user?.userId) return;
    try {
      const data = await userService.getPasswordHistory(user.userId);
      setHistory(data);
    } catch {
      // tarihçe alınamazsa sessiz
    }
  };

  useEffect(() => {
    loadHistory();
  }, [user?.userId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user?.userId) {
      setError('Kullanıcı bulunamadı.');
      return;
    }
    setLoading(true);
    setMessage(null);
    setError(null);
    try {
      await userService.changePassword(user.userId, newPassword);
      setMessage(SUCCESS_MESSAGES.PASSWORD_CHANGED);
      setNewPassword('');
      await loadHistory();
    } catch (err) {
      setError('Şifre değiştirilemedi.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Şifre & Güvenlik
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <LockResetIcon color="primary" />
          <Typography variant="h6">Şifre Değiştir</Typography>
        </Box>

        {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            label="Yeni Şifre"
            type="password"
            fullWidth
            required
            sx={{ mb: 2 }}
            value={newPassword}
            onChange={e => setNewPassword(e.target.value)}
            helperText="Demo amaçlıdır; backend hash/salt üretir ve tarihçeye yazar."
          />
          <Button type="submit" variant="contained" disabled={loading}>
            {loading ? 'Güncelleniyor...' : 'Kaydet'}
          </Button>
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <HistoryIcon color="primary" />
          <Typography variant="h6">Şifre Tarihçesi</Typography>
        </Box>
        {history.length === 0 ? (
          <Typography color="text.secondary">Henüz kayıt yok.</Typography>
        ) : (
          <List>
            {history.map(item => (
              <ListItem key={item.historyId} divider>
                <ListItemText
                  primary={`Hash: ${item.passwordHash}`}
                  secondary={`Değiştiren: ${item.changedBy ?? 'kullanıcı'} · Tarih: ${formatDateTime(item.changedAt)}`}
                />
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default ChangePasswordPage;
