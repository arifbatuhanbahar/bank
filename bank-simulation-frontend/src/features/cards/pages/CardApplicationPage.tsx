import { useState } from 'react';
import { Box, Paper, Typography, TextField, Button, FormControl, InputLabel, Select, MenuItem, Alert } from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';
import { Controller, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { cardService } from '../../../api';
import { CardApplicationFormData, cardApplicationSchema } from '../../../utils/validators';
import { CardPrestigeLevel, EmploymentStatus } from '../../../types';

const CardApplicationPage = () => {
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  const { register, control, handleSubmit, formState: { errors }, reset } = useForm<CardApplicationFormData>({
    resolver: yupResolver(cardApplicationSchema),
    defaultValues: {
      cardTypeRequested: 'Standard',
      monthlyIncome: 10000,
      employmentStatus: 'Employed',
      employerName: '',
    },
  });

  const onSubmit = async (data: CardApplicationFormData) => {
    if (!user?.userId) {
      setErrorMessage('Kullanıcı bulunamadı.');
      return;
    }
    setIsSubmitting(true);
    setSuccessMessage(null);
    setErrorMessage(null);
    try {
      const payload = { ...data, userId: user.userId };
      const res = await cardService.applyForCard(payload);
      setSuccessMessage(res.message || 'Başvurunuz alındı.');
      reset({
        cardTypeRequested: 'Standard',
        monthlyIncome: 10000,
        employmentStatus: 'Employed',
        employerName: '',
      });
    } catch (err) {
      console.error(err);
      setErrorMessage('Başvuru sırasında hata oluştu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Kart Başvurusu
      </Typography>

      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <AssignmentTurnedInIcon color="primary" />
          <Typography variant="h6">Prestij Kart Başvurusu</Typography>
        </Box>

        {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}
        {errorMessage && <Alert severity="error" sx={{ mb: 2 }}>{errorMessage}</Alert>}

        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Controller
                control={control}
                name="cardTypeRequested"
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel id="card-type-label">Kart Türü</InputLabel>
                    <Select
                      {...field}
                      labelId="card-type-label"
                      label="Kart Türü"
                      value={field.value || 'Standard'}
                    >
                      {Object.values(CardPrestigeLevel).map(level => (
                        <MenuItem key={level} value={level}>
                          {level}
                        </MenuItem>
                      ))}
                    </Select>
                    {errors.cardTypeRequested && (
                      <Typography variant="caption" color="error">
                        {errors.cardTypeRequested.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Aylık Gelir (TL)"
                type="number"
                fullWidth
                {...register('monthlyIncome', { valueAsNumber: true })}
                error={!!errors.monthlyIncome}
                helperText={errors.monthlyIncome?.message}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <Controller
                control={control}
                name="employmentStatus"
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel id="employment-label">Çalışma Durumu</InputLabel>
                    <Select
                      {...field}
                      labelId="employment-label"
                      label="Çalışma Durumu"
                      value={field.value || 'Employed'}
                    >
                      {Object.values(EmploymentStatus).map(status => (
                        <MenuItem key={status} value={status}>
                          {status}
                        </MenuItem>
                      ))}
                    </Select>
                    {errors.employmentStatus && (
                      <Typography variant="caption" color="error">
                        {errors.employmentStatus.message}
                      </Typography>
                    )}
                  </FormControl>
                )}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="İşveren / Şirket"
                fullWidth
                {...register('employerName')}
                error={!!errors.employerName}
                helperText={errors.employerName?.message}
              />
            </Grid>
            <Grid item xs={12}>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? 'Gönderiliyor...' : 'Başvuru Yap'}
              </Button>
            </Grid>
          </Grid>
        </Box>
      </Paper>
    </Box>
  );
};

export default CardApplicationPage;
