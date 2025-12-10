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
    // Eski oturumu temizle ki hatali giriste eski token kalmasin
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');

    try {
      const response = await userService.login(data.email, data.password);

      const normalizedUser = {
        ...response.user,
        email: (response.user as any).email ?? (response.user as any).Email,
        userId: response.user.userId ?? (response.user as any).UserId,
        firstName: (response.user as any).firstName ?? (response.user as any).FirstName,
        lastName: (response.user as any).lastName ?? (response.user as any).LastName,
      };

      // Token ve kullanici bilgisini kaydet
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('user', JSON.stringify(normalizedUser));

      // Dashboard'a yonlendir
      navigate('/dashboard');
    } catch (err) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      setError('E-posta veya sifre hatali. Lutfen tekrar deneyin.');
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
        {/* Logo ve Baslik */}
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
            Hesabiniza giris yapin
          </Typography>
        </Box>

        {/* Hata Mesaji */}
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
            label="Sifre"
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
              Sifremi Unuttum
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
              'Giris Yap'
            )}
          </Button>
        </Box>

        {/* Kayit Linki */}
        <Box sx={{ textAlign: 'center', mt: 3 }}>
          <Typography variant="body2" color="text.secondary">
            Hesabiniz yok mu?{' '}
            <Link
              component={RouterLink}
              to="/register"
              underline="hover"
              fontWeight={600}
            >
              Kayit Olun
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
};

export default LoginPage;
