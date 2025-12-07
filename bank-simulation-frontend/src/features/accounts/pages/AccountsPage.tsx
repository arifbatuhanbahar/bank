import { useEffect, useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Skeleton,
} from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import PauseCircleIcon from '@mui/icons-material/PauseCircle';
import { accountService } from '../../../api';
import { Account } from '../../../types';
import { formatCurrency, formatDate } from '../../../utils/formatters';

const AccountsPage = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const loadAccounts = async () => {
      if (!user?.userId) {
        setError('Kullanıcı bilgisi bulunamadı.');
        setLoading(false);
        return;
      }

      try {
        const data = await accountService.getUserAccounts(user.userId);
        setAccounts(data);
      } catch (err) {
        console.error(err);
        setError('Hesaplar yüklenirken bir hata oluştu.');
      } finally {
        setLoading(false);
      }
    };

    loadAccounts();
  }, [user?.userId]);

  const summary = accountService.calculateAccountSummary(accounts);

  const renderStatus = (status: string) => {
    if (status === 'Active') return <Chip label="Aktif" color="success" size="small" />;
    if (status === 'Closed') return <Chip label="Kapalı" color="default" size="small" />;
    return <Chip label={status} color="warning" size="small" />;
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Hesaplarım
      </Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        {[
          { label: 'Toplam Bakiye (TRY)', value: summary.totalBalanceTRY, currency: 'TRY' },
          { label: 'Toplam Bakiye (USD)', value: summary.totalBalanceUSD, currency: 'USD' },
          { label: 'Toplam Bakiye (EUR)', value: summary.totalBalanceEUR, currency: 'EUR' },
          { label: 'Aktif Hesap', value: summary.activeAccounts },
        ].map(item => (
          <Grid xs={12} sm={6} md={3} key={item.label}>
            <Paper sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
              <AccountBalanceWalletIcon color="primary" />
              <Box>
                <Typography variant="caption" color="text.secondary">
                  {item.label}
                </Typography>
                <Typography variant="h6">
                  {'currency' in item ? formatCurrency(item.value, item.currency) : item.value}
                </Typography>
              </Box>
            </Paper>
          </Grid>
        ))}
      </Grid>

      <Paper>
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
          <AccountBalanceWalletIcon color="primary" />
          <Typography variant="h6">Hesap Listesi</Typography>
        </Box>
        <Divider />
        {loading ? (
          <Box sx={{ p: 2 }}>
            {[...Array(3)].map((_, idx) => (
              <Skeleton key={idx} variant="rectangular" height={64} sx={{ mb: 1 }} />
            ))}
          </Box>
        ) : error ? (
          <Box sx={{ p: 2 }}>
            <Typography color="error">{error}</Typography>
          </Box>
        ) : (
          <List>
            {accounts.map(account => (
              <ListItem key={account.accountId} divider>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography fontWeight={700}>{account.accountNumber}</Typography>
                      {renderStatus(account.status)}
                    </Box>
                  }
                  secondary={
                    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                      <Typography variant="body2" color="text.secondary">
                        {account.accountType} • {account.currency}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Açılış: {formatDate(account.openedDate)}
                      </Typography>
                    </Box>
                  }
                />
                <ListItemSecondaryAction>
                  <Typography variant="subtitle1" fontWeight={700}>
                    {formatCurrency(account.balance, account.currency)}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                    {account.status === 'Active' ? (
                      <CheckCircleIcon color="success" fontSize="small" />
                    ) : (
                      <PauseCircleIcon color="warning" fontSize="small" />
                    )}
                    <Typography variant="caption" color="text.secondary">
                      Kullanılabilir: {formatCurrency(account.availableBalance, account.currency)}
                    </Typography>
                  </Box>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default AccountsPage;
