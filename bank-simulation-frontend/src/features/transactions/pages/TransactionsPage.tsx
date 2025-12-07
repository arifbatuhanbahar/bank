import { useEffect, useMemo, useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  List,
  ListItem,
  ListItemText,
  Divider,
  Skeleton,
} from '@mui/material';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import { accountService, transactionService } from '../../../api';
import { Account, Transaction } from '../../../types';
import { TRANSACTION_STATUS_LABELS, TRANSACTION_TYPE_LABELS } from '../../../utils/constants';
import { formatCurrency, formatDateTime } from '../../../utils/formatters';

const TransactionsPage = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [selectedAccountId, setSelectedAccountId] = useState<number | ''>('');
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const loadAccounts = async () => {
      if (!user?.userId) {
        setError('Kullanıcı bilgisi bulunamadı.');
        return;
      }
      try {
        setLoading(true);
        const data = await accountService.getUserAccounts(user.userId);
        setAccounts(data);
        if (data.length > 0) setSelectedAccountId(data[0].accountId);
      } catch (err) {
        console.error(err);
        setError('Hesaplar alınırken hata oluştu.');
      } finally {
        setLoading(false);
      }
    };
    loadAccounts();
  }, [user?.userId]);

  useEffect(() => {
    const loadTransactions = async () => {
      if (!selectedAccountId) return;
      try {
        setLoading(true);
        const data = await transactionService.getAccountTransactions(selectedAccountId);
        setTransactions(data);
      } catch (err) {
        console.error(err);
        setError('İşlemler yüklenirken hata oluştu.');
      } finally {
        setLoading(false);
      }
    };
    loadTransactions();
  }, [selectedAccountId]);

  const selectedAccount = useMemo(
    () => accounts.find(a => a.accountId === selectedAccountId),
    [accounts, selectedAccountId]
  );

  const statusColor = (status: string) => transactionService.getStatusColor(status);

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        İşlem Geçmişi
      </Typography>

      <Paper sx={{ p: 2, mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <SwapHorizIcon color="primary" />
          <Typography variant="h6">Hesap Seçimi</Typography>
        </Box>
        <FormControl fullWidth>
          <InputLabel id="account-select-label">Hesap</InputLabel>
          <Select
            labelId="account-select-label"
            value={selectedAccountId}
            label="Hesap"
            onChange={e => setSelectedAccountId(Number(e.target.value))}
          >
            {accounts.map(acc => (
              <MenuItem key={acc.accountId} value={acc.accountId}>
                {acc.accountNumber} ({acc.currency})
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {selectedAccount && (
          <Box sx={{ mt: 2, display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            <Typography variant="body2" color="text.secondary">
              Bakiye: {formatCurrency(selectedAccount.balance, selectedAccount.currency)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Kullanılabilir: {formatCurrency(selectedAccount.availableBalance, selectedAccount.currency)}
            </Typography>
          </Box>
        )}
      </Paper>

      <Paper>
        <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="h6">İşlemler</Typography>
        </Box>
        <Divider />
        {loading ? (
          <Box sx={{ p: 2 }}>
            {[...Array(5)].map((_, idx) => (
              <Skeleton key={idx} variant="rectangular" height={60} sx={{ mb: 1 }} />
            ))}
          </Box>
        ) : error ? (
          <Box sx={{ p: 2 }}>
            <Typography color="error">{error}</Typography>
          </Box>
        ) : (
          <List>
            {transactions.map(tx => (
              <ListItem key={tx.transactionId} divider>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', flexWrap: 'wrap' }}>
                      <Typography fontWeight={700}>
                        {TRANSACTION_TYPE_LABELS[tx.transactionType] || tx.transactionType}
                      </Typography>
                      <Chip
                        label={TRANSACTION_STATUS_LABELS[tx.status] || tx.status}
                        color={statusColor(tx.status)}
                        size="small"
                      />
                    </Box>
                  }
                  secondary={
                    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                      <Typography variant="body2" color="text.secondary">
                        Ref: {tx.referenceNumber}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Tarih: {formatDateTime(tx.transactionDate)}
                      </Typography>
                      {tx.description && (
                        <Typography variant="body2" color="text.secondary">
                          {tx.description}
                        </Typography>
                      )}
                    </Box>
                  }
                />
                <Typography
                  variant="subtitle1"
                  fontWeight={700}
                  color={tx.transactionType === 'Deposit' ? 'success.main' : 'error.main'}
                >
                  {tx.transactionType === 'Deposit' ? '+' : '-'}
                  {formatCurrency(tx.amount, tx.currency)}
                </Typography>
              </ListItem>
            ))}
            {transactions.length === 0 && (
              <Box sx={{ p: 2 }}>
                <Typography color="text.secondary">Bu hesap için işlem bulunamadı.</Typography>
              </Box>
            )}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default TransactionsPage;
