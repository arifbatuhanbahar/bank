import { useEffect, useState } from 'react';
import {
  Alert,
  Avatar,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  Paper,
  Stack,
  TextField,
  Skeleton,
  Typography,
} from '@mui/material';
import PeopleIcon from '@mui/icons-material/People';
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser';
import { userService } from '../../../api';
import { User } from '../../../types';
import { KYC_STATUS_LABELS, RISK_LEVEL_LABELS } from '../../../utils/constants';

const AdminUsersPage = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [editing, setEditing] = useState<User | null>(null);
  const [editPayload, setEditPayload] = useState<Partial<User>>({});

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await userService.getAllUsers();
        setUsers(data);
      } catch (err) {
        setError('Kullanıcı listesi alınamadı.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const openEdit = (user: User) => {
    setEditing(user);
    setEditPayload({
      firstName: user.firstName,
      lastName: user.lastName,
      status: user.status,
      kycStatus: user.kycStatus,
      riskLevel: user.riskLevel,
    });
  };

  const handleEditChange = (key: keyof User, value: any) => {
    setEditPayload(prev => ({ ...prev, [key]: value }));
  };

  const saveEdit = async () => {
    if (!editing) return;
    try {
      setSuccess(null);
      setError(null);
      const updated = await userService.updateUser(editing.userId, editPayload);
      setUsers(prev => prev.map(u => (u.userId === updated.userId ? { ...u, ...updated } : u)));
      setSuccess('Kullanıcı güncellendi.');
      setEditing(null);
    } catch (err) {
      setError('Kullanıcı güncellenemedi.');
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Kullanıcı Yönetimi
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <PeopleIcon color="primary" />
          <Typography variant="h6">Aktif kullanıcılar</Typography>
        </Box>
        <Divider sx={{ mb: 2 }} />
        {loading ? (
          <Skeleton variant="rectangular" height={220} />
        ) : users.length === 0 ? (
          <Typography color="text.secondary">Kayıt bulunamadı.</Typography>
        ) : (
          <Grid container spacing={2}>
            {users.map(user => (
              <Grid item xs={12} md={6} key={user.userId}>
                <Box sx={{ p: 2, borderRadius: 2, bgcolor: 'action.hover' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Avatar>{user.firstName?.[0] ?? 'U'}</Avatar>
                    <Box>
                      <Typography fontWeight={700}>{user.firstName} {user.lastName}</Typography>
                      <Typography variant="body2" color="text.secondary">{user.email}</Typography>
                    </Box>
                  </Box>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 1 }}>
                    <Chip label={user.status} size="small" color={user.status === 'Active' ? 'success' : 'default'} />
                    <Chip
                      label={KYC_STATUS_LABELS[user.kycStatus] || user.kycStatus}
                      size="small"
                      icon={<VerifiedUserIcon />}
                      color={user.kycStatus === 'Verified' ? 'success' : 'warning'}
                    />
                    <Chip label={RISK_LEVEL_LABELS[user.riskLevel] || user.riskLevel} size="small" color="info" />
                  </Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Ülke: {user.country} · Oluşturma: {new Date(user.createdAt).toLocaleDateString('tr-TR')}
                  </Typography>
                  <Button sx={{ mt: 1 }} size="small" variant="outlined" onClick={() => openEdit(user)}>
                    Düzenle
                  </Button>
                </Box>
              </Grid>
            ))}
          </Grid>
        )}
      </Paper>

      <Dialog open={!!editing} onClose={() => setEditing(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Kullanıcı Düzenle</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Ad"
              value={editPayload.firstName || ''}
              onChange={e => handleEditChange('firstName', e.target.value)}
            />
            <TextField
              label="Soyad"
              value={editPayload.lastName || ''}
              onChange={e => handleEditChange('lastName', e.target.value)}
            />
            <TextField
              label="Durum"
              value={editPayload.status || ''}
              onChange={e => handleEditChange('status', e.target.value)}
              helperText="Örn: Active, Suspended"
            />
            <TextField
              label="KYC"
              value={editPayload.kycStatus || ''}
              onChange={e => handleEditChange('kycStatus', e.target.value)}
              helperText="Örn: Pending, Verified, Rejected"
            />
            <TextField
              label="Risk"
              value={editPayload.riskLevel || ''}
              onChange={e => handleEditChange('riskLevel', e.target.value)}
              helperText="Örn: Low, Medium, High"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditing(null)}>İptal</Button>
          <Button variant="contained" onClick={saveEdit}>Kaydet</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AdminUsersPage;
