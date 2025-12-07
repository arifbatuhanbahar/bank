import { useEffect, useState } from 'react';
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
} from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import SendIcon from '@mui/icons-material/Send';
import { Controller, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { accountService, transactionService } from '../../../api';
import { Account, TransferRequest } from '../../../types';
import { transferSchema, TransferFormData } from '../../../utils/validators';
import { formatCurrency } from '../../../utils/formatters';

const TransferPage = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    control,
  } = useForm<TransferFormData>({
    resolver: yupResolver(transferSchema),
    defaultValues: {
      fromAccountId: undefined as unknown as number,
      toAccountId: undefined as unknown as number,
      amount: 0,
      description: '',
    },
  });

  useEffect(() => {
    const loadAccounts = async () => {
      if (!user?.userId) return;
      try {
        const data = await accountService.getUserAccounts(user.userId);
        setAccounts(data);
        if (data.length > 0) {
          const defaultFrom = data[0].accountId;
          const defaultTo = data[1]?.accountId ?? data[0].accountId;
          reset({
            fromAccountId: defaultFrom,
            toAccountId: defaultTo,
            amount: 0,
            description: '',
          });
        }
      } catch (err) {
        console.error(err);
        setErrorMessage('Hesaplar yüklenemedi.');
      }
    };
    loadAccounts();
  }, [user?.userId, reset]);

  const onSubmit = async (data: TransferFormData) => {
    setIsSubmitting(true);
    setSuccessMessage(null);
    setErrorMessage(null);
    try {
      const payload: TransferRequest = {
        fromAccountId: data.fromAccountId,
        toAccountId: data.toAccountId,
        amount: data.amount,
        description: data.description,
      };
      const response = await transactionService.transfer(payload);
      setSuccessMessage(response.message || 'Transfer başarılı');
      reset({ ...data, amount: 0, description: '' });
    } catch (err) {
      console.error(err);
      setErrorMessage('Transfer sırasında bir hata oluştu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const selectedFrom = accounts.find(a => a.accountId === watch('fromAccountId'));
  const selectedTo = accounts.find(a => a.accountId === watch('toAccountId'));

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Para Transferi
      </Typography>

      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <SendIcon color="primary" />
          <Typography variant="h6">Hesaplar arası transfer</Typography>
        </Box>

        {successMessage && (
          <Alert severity="success" sx={{ mb: 2 }}>
            {successMessage}
          </Alert>
        )}
        {errorMessage && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Controller
                control={control}
                name="fromAccountId"
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel id="from-account-label">Kaynak Hesap</InputLabel>
                    <Select
                      {...field}
                      labelId="from-account-label"
                      label="Kaynak Hesap"
                      value={field.value || ''}
                      onChange={e => field.onChange(Number(e.target.value))}
                      error={!!errors.fromAccountId}
                    >
                      {accounts.map(acc => (
                        <MenuItem key={acc.accountId} value={acc.accountId}>
                          {acc.accountNumber} ({acc.currency})
                        </MenuItem>
                      ))}
                    </Select>
                    {errors.fromAccountId && (
                      <Typography variant="caption" color="error">
                        {errors.fromAccountId.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
              {selectedFrom && (
                <Typography variant="caption" color="text.secondary">
                  Bakiye: {formatCurrency(selectedFrom.balance, selectedFrom.currency)}
                </Typography>
              )}
            </Grid>

            <Grid item xs={12} md={6}>
              <Controller
                control={control}
                name="toAccountId"
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel id="to-account-label">Hedef Hesap</InputLabel>
                    <Select
                      {...field}
                      labelId="to-account-label"
                      label="Hedef Hesap"
                      value={field.value || ''}
                      onChange={e => field.onChange(Number(e.target.value))}
                      error={!!errors.toAccountId}
                    >
                      {accounts.map(acc => (
                        <MenuItem key={acc.accountId} value={acc.accountId}>
                          {acc.accountNumber} ({acc.currency})
                        </MenuItem>
                      ))}
                    </Select>
                    {errors.toAccountId && (
                      <Typography variant="caption" color="error">
                        {errors.toAccountId.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
              {selectedTo && (
                <Typography variant="caption" color="text.secondary">
                  Bakiye: {formatCurrency(selectedTo.balance, selectedTo.currency)}
                </Typography>
              )}
            </Grid>

            <Grid item xs={12} md={6}>
              <TextField
                label="Tutar"
                type="number"
                fullWidth
                inputProps={{ min: 1 }}
                error={!!errors.amount}
                helperText={errors.amount?.message}
                {...register('amount', { valueAsNumber: true })}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Açıklama"
                fullWidth
                {...register('description')}
                error={!!errors.description}
                helperText={errors.description?.message}
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                type="submit"
                variant="contained"
                startIcon={<SendIcon />}
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Gönderiliyor...' : 'Transfer Yap'}
              </Button>
            </Grid>
          </Grid>
        </Box>
      </Paper>
    </Box>
  );
};

export default TransferPage;
