import { useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Link,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import { userService } from '../../../api';

interface RegisterForm {
  firstName: string;
  lastName: string;
  email: string;
  tcKimlikNo: string;
}

const RegisterPage = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState<RegisterForm>({
    firstName: '',
    lastName: '',
    email: '',
    tcKimlikNo: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleChange = (key: keyof RegisterForm, value: string) => {
    setForm(prev => ({ ...prev, [key]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    setLoading(true);
    try {
      await userService.createUser({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        tcKimlikNo: form.tcKimlikNo,
        dateOfBirth: new Date(1990, 0, 1).toISOString(),
        passwordHash: 'demo-hash',
        passwordSalt: 'demo-salt',
      } as any);
      setSuccess('Kayıt oluşturuldu. Giriş yapabilirsiniz.');
      setTimeout(() => navigate('/login'), 800);
    } catch (err) {
      setError('Kayıt başarısız.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'background.default',
        p: 2,
      }}
    >
      <Paper sx={{ p: 4, width: '100%', maxWidth: 440 }}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Kayıt Ol
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Demo için temel bilgiler yeterlidir; parola hash/salt backend’de doğrulanmaz.
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            label="Ad"
            fullWidth
            required
            sx={{ mb: 2 }}
            value={form.firstName}
            onChange={e => handleChange('firstName', e.target.value)}
          />
          <TextField
            label="Soyad"
            fullWidth
            required
            sx={{ mb: 2 }}
            value={form.lastName}
            onChange={e => handleChange('lastName', e.target.value)}
          />
          <TextField
            label="E-posta"
            type="email"
            fullWidth
            required
            sx={{ mb: 2 }}
            value={form.email}
            onChange={e => handleChange('email', e.target.value)}
          />
          <TextField
            label="TC Kimlik No"
            fullWidth
            required
            sx={{ mb: 2 }}
            value={form.tcKimlikNo}
            onChange={e => handleChange('tcKimlikNo', e.target.value)}
          />
          <Button type="submit" variant="contained" fullWidth disabled={loading}>
            {loading ? 'Kaydediliyor...' : 'Kaydol'}
          </Button>
        </Box>

        <Box sx={{ textAlign: 'center', mt: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Zaten hesabınız var mı?{' '}
            <Link component={RouterLink} to="/login" underline="hover">
              Giriş yapın
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
};

export default RegisterPage;
