import { useEffect, useState } from 'react';
import { Box, Paper, Typography, Chip, Divider, Skeleton, List, ListItem, ListItemText } from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';
import { applicationService } from '../../../api';
import { CardApplication } from '../../../types';
import { APPLICATION_PRESTIGE_LABELS, APPLICATION_STATUS_LABELS } from '../../../utils/constants';
import { formatDateTime } from '../../../utils/formatters';

const UserApplicationsPage = () => {
  const [applications, setApplications] = useState<CardApplication[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const load = async () => {
      if (!user?.userId) {
        setError('Kullanıcı bulunamadı.');
        setLoading(false);
        return;
      }
      try {
        const data = await applicationService.getUserApplications(user.userId);
        setApplications(data);
      } catch (err) {
        console.error(err);
        setError('Başvurular alınamadı.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [user?.userId]);

  const statusColor = (status: string) => {
    switch (status) {
      case 'Approved':
        return 'success';
      case 'Rejected':
        return 'error';
      case 'Pending':
      default:
        return 'warning';
    }
  };

  return (
    <Box sx={{ pt: 1, pb: 2 }}>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Kart Başvurularım
      </Typography>

      <Paper sx={{ p: 2.5, borderRadius: 2 }} elevation={2}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AssignmentTurnedInIcon color="primary" />
          <Typography variant="h6" fontWeight={700}>
            Başvuru Listesi
          </Typography>
        </Box>
        <Divider sx={{ my: 2 }} />
        {loading ? (
          <Grid container spacing={2}>
            {[...Array(3)].map((_, idx) => (
              <Grid xs={12} key={idx}>
                <Skeleton variant="rectangular" height={64} sx={{ borderRadius: 1 }} />
              </Grid>
            ))}
          </Grid>
        ) : error ? (
          <Typography color="error">{error}</Typography>
        ) : applications.length === 0 ? (
          <Typography color="text.secondary">Başvurunuz bulunmuyor.</Typography>
        ) : (
          <List>
            {applications.map(app => (
              <ListItem key={app.applicationId} divider sx={{ py: 1.5 }}>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                      <Typography fontWeight={700}>
                        {APPLICATION_PRESTIGE_LABELS[app.cardTypeRequested] || app.cardTypeRequested}
                      </Typography>
                      <Chip
                        label={APPLICATION_STATUS_LABELS[app.status] || app.status}
                        size="small"
                        color={statusColor(app.status) as any}
                      />
                    </Box>
                  }
                  secondary={
                    <Typography variant="body2" color="text.secondary">
                      Aylık Gelir: {app.monthlyIncome.toLocaleString('tr-TR')} TL • {app.employmentStatus} •{' '}
                      Başvuru: {formatDateTime(app.applicationDate)}
                    </Typography>
                  }
                />
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default UserApplicationsPage;
