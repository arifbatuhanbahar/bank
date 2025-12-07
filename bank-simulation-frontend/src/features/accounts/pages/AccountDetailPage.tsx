import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Chip,
  Divider,
  Grid,
  Paper,
  Skeleton,
  Typography,
} from '@mui/material';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import HistoryIcon from '@mui/icons-material/History';
import { accountService, transactionService } from '../../../api';
import { Account, Transaction } from '../../../types';
import { formatCurrency, formatDate, formatDateTime } from '../../../utils/formatters';
import { TRANSACTION_STATUS_LABELS, TRANSACTION_TYPE_LABELS } from '../../../utils/constants';

const AccountDetailPage = () => {
  const { id } = useParams();
  const accountId = Number(id);
  const [account, setAccount] = useState<Account | null>(null);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!accountId) {
        setError('Geçersiz hesap.');
        setLoading(false);
        return;
      }
      try {
        const acc = await accountService.getAccountById(accountId);
        setAccount(acc);
        const tx = await transactionService.getAccountTransactions(accountId);
        setTransactions(tx);
      } catch (err) {
        setError('Hesap bilgisi alınamadı.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [accountId]);

  const statusChip = useMemo(() => {
    if (!account) return null;
    if (account.status === 'Active') return <Chip label="Aktif" color="success" size="small" />;
    if (account.status === 'Closed') return <Chip label="Kapalı" size="small" />;
    return <Chip label={account.status} color="warning" size="small" />;
  }, [account]);

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Hesap Detayı
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Skeleton variant="rectangular" height={160} />
      ) : account ? (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <AccountBalanceWalletIcon color="primary" />
            <Typography variant="h6" fontWeight={700}>
              {account.accountNumber}
            </Typography>
            {statusChip}
          </Box>
          <Divider sx={{ my: 1.5 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Hesap Türü</Typography>
              <Typography fontWeight={600}>{account.accountType}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Para Birimi</Typography>
              <Typography fontWeight={600}>{account.currency}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Bakiye</Typography>
              <Typography fontWeight={700}>{formatCurrency(account.balance, account.currency)}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Kullanılabilir</Typography>
              <Typography fontWeight={700}>{formatCurrency(account.availableBalance, account.currency)}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Açılış</Typography>
              <Typography>{formatDate(account.openedDate)}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Güncellendi</Typography>
              <Typography>{account.updatedAt ? formatDate(account.updatedAt) : '-'}</Typography>
            </Grid>
          </Grid>
        </Paper>
      ) : null}

      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <HistoryIcon color="primary" />
          <Typography variant="h6">İşlem Geçmişi</Typography>
        </Box>
        <Divider sx={{ mb: 1.5 }} />
        {loading ? (
          <Skeleton variant="rectangular" height={200} />
        ) : transactions.length === 0 ? (
          <Typography color="text.secondary">Bu hesap için işlem yok.</Typography>
        ) : (
          transactions.map(tx => (
            <Box key={tx.transactionId} sx={{ py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}>
              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
                <Typography fontWeight={700}>
                  {TRANSACTION_TYPE_LABELS[tx.transactionType] || tx.transactionType}
                </Typography>
                <Chip
                  label={TRANSACTION_STATUS_LABELS[tx.status] || tx.status}
                  size="small"
                  color={transactionService.getStatusColor(tx.status)}
                />
              </Box>
              <Typography variant="body2" color="text.secondary">
                {tx.description || 'Açıklama yok'} · {formatDateTime(tx.transactionDate)}
              </Typography>
              <Typography fontWeight={700}>
                {formatCurrency(tx.amount, tx.currency)}
              </Typography>
            </Box>
          ))
        )}
      </Paper>
    </Box>
  );
};

export default AccountDetailPage;
