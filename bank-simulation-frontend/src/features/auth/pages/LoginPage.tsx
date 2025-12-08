import { useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Link,
  Alert,
  InputAdornment,
  IconButton,
  CircularProgress,
} from '@mui/material';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import { loginSchema, LoginFormData } from '../../../utils/validators';
import { userService } from '../../../api';

const LoginPage = () => {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: yupResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    setError(null);
    // Eski oturumu temizle ki yanlış girişte eski token kalmasın
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');

    try {
      const response = await userService.login(data.email, data.password);
      
      // Token ve kullanıcı bilgisini kaydet
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      // Dashboard'a yönlendir
      navigate('/dashboard');
    } catch (err) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      setError('E-posta veya şifre hatalı. Lütfen tekrar deneyin.');
    } finally {
      setIsLoading(false);
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
      <Paper
        elevation={0}
        sx={{
          p: 5,
          width: '100%',
          maxWidth: 440,
          borderRadius: 3,
          boxShadow: '0px 4px 20px rgba(0, 0, 0, 0.08)',
        }}
      >
        {/* Logo ve Başlık */}
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Box
            sx={{
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: 64,
              height: 64,
              borderRadius: 3,
              backgroundColor: 'primary.main',
              mb: 2,
            }}
          >
            <AccountBalanceIcon sx={{ fontSize: 36, color: 'white' }} />
          </Box>
          <Typography variant="h5" fontWeight={700} gutterBottom>
            Bank Simulation
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Hesabınıza giriş yapın
          </Typography>
        </Box>

        {/* Hata Mesajı */}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {/* Form */}
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <TextField
            {...register('email')}
            label="E-posta Adresi"
            type="email"
            fullWidth
            margin="normal"
            error={!!errors.email}
            helperText={errors.email?.message}
            autoComplete="email"
            autoFocus
          />

          <TextField
            {...register('password')}
            label="Şifre"
            type={showPassword ? 'text' : 'password'}
            fullWidth
            margin="normal"
            error={!!errors.password}
            helperText={errors.password?.message}
            autoComplete="current-password"
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={() => setShowPassword(!showPassword)}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />

          <Box sx={{ textAlign: 'right', mt: 1, mb: 3 }}>
            <Link
              component={RouterLink}
              to="/forgot-password"
              variant="body2"
              underline="hover"
            >
              Şifremi Unuttum
            </Link>
          </Box>

          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={isLoading}
            sx={{ py: 1.5 }}
          >
            {isLoading ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              'Giriş Yap'
            )}
          </Button>
        </Box>

        {/* Kayıt Linki */}
        <Box sx={{ textAlign: 'center', mt: 3 }}>
          <Typography variant="body2" color="text.secondary">
            Hesabınız yok mu?{' '}
            <Link
              component={RouterLink}
              to="/register"
              underline="hover"
              fontWeight={600}
            >
              Kayıt Olun
            </Link>
          </Typography>
        </Box>

        {/* Demo Bilgisi */}
        <Alert severity="info" sx={{ mt: 3 }}>
          <Typography variant="caption">
            <strong>Demo Giriş:</strong> Veritabanındaki herhangi bir kullanıcının
            e-posta adresi ile giriş yapabilirsiniz. (Şifre kontrol edilmiyor)
          </Typography>
        </Alert>
      </Paper>
    </Box>
  );
};

export default LoginPage;
