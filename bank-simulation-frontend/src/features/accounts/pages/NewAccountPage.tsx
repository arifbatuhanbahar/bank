import { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Grid,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { accountService } from '../../../api';
import { AccountType, Currency } from '../../../types';

type NewAccountForm = {
  accountType: AccountType;
  currency: Currency;
  balance: number;
  dailyTransferLimit: number;
  dailyWithdrawalLimit: number;
  interestRate: number;
};

const generateIban = () => {
  const random = () => Math.floor(Math.random() * 10);
  return `TR${Math.floor(10 + Math.random() * 89)} ${Array.from({ length: 22 }, () => random()).join('')}`.trim();
};

const NewAccountPage = () => {
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  const { control, register, handleSubmit, formState: { errors }, reset } = useForm<NewAccountForm>({
    defaultValues: {
      accountType: 'Checking',
      currency: 'TRY',
      balance: 0,
      dailyTransferLimit: 50000,
      dailyWithdrawalLimit: 10000,
      interestRate: 0,
    },
  });

  const onSubmit = async (data: NewAccountForm) => {
    if (!user?.userId) {
      setError('Kullanıcı bulunamadı.');
      return;
    }
    setIsSubmitting(true);
    setError(null);
    setSuccess(null);
    try {
      await accountService.createAccount({
        userId: user.userId,
        accountNumber: generateIban(),
        accountType: data.accountType,
        currency: data.currency,
        balance: data.balance,
        dailyTransferLimit: data.dailyTransferLimit,
        dailyWithdrawalLimit: data.dailyWithdrawalLimit,
        interestRate: data.interestRate,
      });
      setSuccess('Hesap oluşturuldu.');
      reset();
    } catch (err) {
      console.error(err);
      setError('Hesap oluşturulurken hata oluştu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Yeni Hesap Aç
      </Typography>
      <Paper sx={{ p: 3 }}>
        {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel id="account-type-label">Hesap Türü</InputLabel>
                <Controller
                  name="accountType"
                  control={control}
                  render={({ field }) => (
                    <Select
                      {...field}
                      labelId="account-type-label"
                      label="Hesap Türü"
                    >
                      {Object.values(AccountType).map(type => (
                        <MenuItem key={type} value={type}>{type}</MenuItem>
                      ))}
                    </Select>
                  )}
                />
              </FormControl>
            </Grid>

            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel id="currency-label">Para Birimi</InputLabel>
                <Controller
                  name="currency"
                  control={control}
                  render={({ field }) => (
                    <Select
                      {...field}
                      labelId="currency-label"
                      label="Para Birimi"
                    >
                      {Object.values(Currency).map(cur => (
                        <MenuItem key={cur} value={cur}>{cur}</MenuItem>
                      ))}
                    </Select>
                  )}
                />
              </FormControl>
            </Grid>

            <Grid item xs={12} md={6}>
              <TextField
                label="Başlangıç Bakiye"
                type="number"
                fullWidth
                {...register('balance', { valueAsNumber: true })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Faiz Oranı (%)"
                type="number"
                fullWidth
                {...register('interestRate', { valueAsNumber: true })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Günlük Transfer Limiti"
                type="number"
                fullWidth
                {...register('dailyTransferLimit', { valueAsNumber: true })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Günlük Çekim Limiti"
                type="number"
                fullWidth
                {...register('dailyWithdrawalLimit', { valueAsNumber: true })}
              />
            </Grid>

            <Grid item xs={12}>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? 'Oluşturuluyor...' : 'Hesap Oluştur'}
              </Button>
            </Grid>
          </Grid>
        </Box>
      </Paper>
    </Box>
  );
};

export default NewAccountPage;
