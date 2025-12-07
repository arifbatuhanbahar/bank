import { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Paper,
  Typography,
  Card,
  CardContent,
  Button,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Skeleton,
} from '@mui/material';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import CreditCardIcon from '@mui/icons-material/CreditCard';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import SendIcon from '@mui/icons-material/Send';
import AddIcon from '@mui/icons-material/Add';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import { useNavigate } from 'react-router-dom';
import { accountService, transactionService } from '../../../api';
import { Account, Transaction } from '../../../types';
import { formatCurrency, formatDateTime, formatRelativeTime } from '../../../utils/formatters';
import { TRANSACTION_TYPE_LABELS, TRANSACTION_STATUS_LABELS } from '../../../utils/constants';

const DashboardPage = () => {
  const navigate = useNavigate();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [recentTransactions, setRecentTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Kullanıcı bilgisini localStorage'dan al
  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (user?.userId) {
          // Kullanıcının hesaplarını getir
          const userAccounts = await accountService.getUserAccounts(user.userId);
          setAccounts(userAccounts);

          // İlk hesabın işlemlerini getir (varsa)
          if (userAccounts.length > 0) {
            const transactions = await transactionService.getAccountTransactions(
              userAccounts[0].accountId
            );
            setRecentTransactions(transactions.slice(0, 5));
          }
        }
      } catch (error) {
        console.error('Dashboard data fetch error:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [user?.userId]);

  const accountSummary = accountService.calculateAccountSummary(accounts);

  const StatCard = ({
    title,
    value,
    icon,
    color,
    subtitle,
  }: {
    title: string;
    value: string;
    icon: React.ReactNode;
    color: string;
    subtitle?: string;
  }) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h5" fontWeight={700}>
              {isLoading ? <Skeleton width={120} /> : value}
            </Typography>
            {subtitle && (
              <Typography variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Box>
          <Box
            sx={{
              p: 1.5,
              borderRadius: 2,
              backgroundColor: `${color}15`,
              color: color,
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );

  const QuickAction = ({
    icon,
    title,
    onClick,
  }: {
    icon: React.ReactNode;
    title: string;
    onClick: () => void;
  }) => (
    <Button
      variant="outlined"
      onClick={onClick}
      sx={{
        flexDirection: 'column',
        py: 2,
        px: 3,
        borderRadius: 2,
        borderColor: 'divider',
        '&:hover': {
          borderColor: 'primary.main',
          backgroundColor: 'primary.50',
        },
      }}
    >
      {icon}
      <Typography variant="caption" sx={{ mt: 1 }}>
        {title}
      </Typography>
    </Button>
  );

  return (
    <Box className="fade-in">
      {/* Hoşgeldin Mesajı */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={700} gutterBottom>
          Hoş Geldiniz, {user?.firstName || 'Kullanıcı'} 👋
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Hesaplarınızın güncel durumunu görüntüleyin
        </Typography>
      </Box>

      {/* İstatistik Kartları */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Toplam Bakiye (TL)"
            value={formatCurrency(accountSummary.totalBalanceTRY, 'TRY')}
            icon={<AccountBalanceWalletIcon />}
            color="#1976D2"
            subtitle={`${accountSummary.activeAccounts} aktif hesap`}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Toplam Bakiye (USD)"
            value={formatCurrency(accountSummary.totalBalanceUSD, 'USD')}
            icon={<TrendingUpIcon />}
            color="#4CAF50"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Toplam Bakiye (EUR)"
            value={formatCurrency(accountSummary.totalBalanceEUR, 'EUR')}
            icon={<TrendingUpIcon />}
            color="#9C27B0"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <StatCard
            title="Toplam Hesap"
            value={accountSummary.totalAccounts.toString()}
            icon={<CreditCardIcon />}
            color="#FF9800"
          />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Hızlı İşlemler */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Hızlı İşlemler
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mt: 2 }}>
              <QuickAction
                icon={<SendIcon color="primary" />}
                title="Para Transfer"
                onClick={() => navigate('/transfer')}
              />
              <QuickAction
                icon={<AddIcon color="success" />}
                title="Hesap Aç"
                onClick={() => navigate('/accounts/new')}
              />
              <QuickAction
                icon={<CreditCardIcon color="secondary" />}
                title="Kart Başvuru"
                onClick={() => navigate('/cards/apply')}
              />
              <QuickAction
                icon={<SwapHorizIcon color="info" />}
                title="İşlem Geçmişi"
                onClick={() => navigate('/transactions')}
              />
            </Box>
          </Paper>
        </Grid>

        {/* Son İşlemler */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>
                Son İşlemler
              </Typography>
              <Button size="small" onClick={() => navigate('/transactions')}>
                Tümünü Gör
              </Button>
            </Box>

            {isLoading ? (
              <Box>
                {[1, 2, 3, 4, 5].map((i) => (
                  <Skeleton key={i} height={60} sx={{ mb: 1 }} />
                ))}
              </Box>
            ) : recentTransactions.length > 0 ? (
              <List disablePadding>
                {recentTransactions.map((tx, index) => (
                  <Box key={tx.transactionId}>
                    <ListItem
                      sx={{
                        px: 0,
                        '&:hover': { backgroundColor: 'action.hover', borderRadius: 1 },
                      }}
                    >
                      <ListItemIcon>
                        <Box
                          sx={{
                            p: 1,
                            borderRadius: 2,
                            backgroundColor:
                              tx.transactionType === 'Deposit' ||
                              tx.toAccountId === accounts[0]?.accountId
                                ? 'success.50'
                                : 'error.50',
                          }}
                        >
                          {tx.transactionType === 'Deposit' ||
                          tx.toAccountId === accounts[0]?.accountId ? (
                            <ArrowDownwardIcon color="success" fontSize="small" />
                          ) : (
                            <ArrowUpwardIcon color="error" fontSize="small" />
                          )}
                        </Box>
                      </ListItemIcon>
                      <ListItemText
                        primary={
                          <Typography variant="body2" fontWeight={500}>
                            {TRANSACTION_TYPE_LABELS[tx.transactionType] || tx.transactionType}
                          </Typography>
                        }
                        secondary={
                          <Typography variant="caption" color="text.secondary">
                            {tx.description || 'Açıklama yok'} • {formatRelativeTime(tx.transactionDate)}
                          </Typography>
                        }
                      />
                      <Box sx={{ textAlign: 'right' }}>
                        <Typography
                          variant="body2"
                          fontWeight={600}
                          color={
                            tx.transactionType === 'Deposit' ||
                            tx.toAccountId === accounts[0]?.accountId
                              ? 'success.main'
                              : 'text.primary'
                          }
                        >
                          {tx.transactionType === 'Deposit' ||
                          tx.toAccountId === accounts[0]?.accountId
                            ? '+'
                            : '-'}
                          {formatCurrency(tx.amount, tx.currency)}
                        </Typography>
                        <Chip
                          label={TRANSACTION_STATUS_LABELS[tx.status] || tx.status}
                          size="small"
                          color={transactionService.getStatusColor(tx.status)}
                          sx={{ mt: 0.5 }}
                        />
                      </Box>
                    </ListItem>
                    {index < recentTransactions.length - 1 && <Divider />}
                  </Box>
                ))}
              </List>
            ) : (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography color="text.secondary">
                  Henüz işlem bulunmuyor
                </Typography>
              </Box>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DashboardPage;
