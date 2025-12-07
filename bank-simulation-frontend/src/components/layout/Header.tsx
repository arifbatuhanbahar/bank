import { useEffect, useState } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Avatar,
  Menu,
  MenuItem,
  ListItemIcon,
  Divider,
  Badge,
  Tooltip,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import NotificationsIcon from '@mui/icons-material/Notifications';
import LogoutIcon from '@mui/icons-material/Logout';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import { useNavigate } from 'react-router-dom';
import { getInitials, formatCurrency, formatDateTime } from '../../utils/formatters';
import { accountService, transactionService } from '../../api';
import { Transaction } from '../../types';
import { TRANSACTION_TYPE_LABELS } from '../../utils/constants';

interface HeaderProps {
  onMenuClick: () => void;
  user?: {
    firstName: string;
    lastName: string;
    email: string;
  } | null;
  onLogout: () => void;
}

const Header = ({ onMenuClick, user, onLogout }: HeaderProps) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [notificationAnchor, setNotificationAnchor] = useState<null | HTMLElement>(null);
  const [notifications, setNotifications] = useState<Transaction[]>([]);

  const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleNotificationOpen = (event: React.MouseEvent<HTMLElement>) => {
    setNotificationAnchor(event.currentTarget);
  };

  const handleNotificationClose = () => {
    setNotificationAnchor(null);
  };

  const handleLogoutClick = () => {
    handleMenuClose();
    onLogout();
  };

  const fullName = user ? `${user.firstName} ${user.lastName}` : 'Kullanıcı';
  const userJson = localStorage.getItem('user');
  const storedUser = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const loadNotifications = async () => {
      if (!storedUser?.userId) {
        setNotifications([]);
        return;
      }
      try {
        // Tüm hesaplardan işlemleri topla ve tarihe göre sırala
        const userAccounts = await accountService.getUserAccounts(storedUser.userId);
        const accountIds = userAccounts.map(acc => acc.accountId);
        if (accountIds.length === 0) {
          setNotifications([]);
          return;
        }
        const txLists = await Promise.all(
          accountIds.map(id => transactionService.getAccountTransactions(id).catch(() => []))
        );
        const dedup = new Map<number, Transaction>();
        txLists.flat().forEach(tx => {
          if (!dedup.has(tx.transactionId)) {
            dedup.set(tx.transactionId, tx);
          }
        });
        const sorted = Array.from(dedup.values()).sort(
          (a, b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime()
        );
        setNotifications(sorted.slice(0, 5));
      } catch (err) {
        console.error('Bildirimler alınamadı', err);
        setNotifications([]);
      }
    };
    loadNotifications();
  }, [storedUser?.userId]);

  const unreadCount = notifications.length;

  return (
    <AppBar position="fixed" color="default" elevation={0}>
      <Toolbar
        sx={{
          minHeight: 72,
          px: { xs: 2, md: 3 },
          borderBottom: '1px solid',
          borderColor: 'divider',
          backgroundColor: 'white',
        }}
      >
        <IconButton
          edge="start"
          color="inherit"
          aria-label="menu"
          onClick={onMenuClick}
          sx={{ mr: 2 }}
        >
          <MenuIcon />
        </IconButton>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AccountBalanceIcon color="primary" />
          <Typography variant="h6" fontWeight={700} color="primary">
            Bank Simulation
          </Typography>
        </Box>

        <Box sx={{ flexGrow: 1 }} />

        {/* Bildirimler */}
        <Tooltip title="Bildirimler">
          <IconButton
            color="inherit"
            onClick={handleNotificationOpen}
            sx={{ mr: 1 }}
          >
            <Badge badgeContent={unreadCount} color="error">
              <NotificationsIcon />
            </Badge>
          </IconButton>
        </Tooltip>

        <Menu
          anchorEl={notificationAnchor}
          open={Boolean(notificationAnchor)}
          onClose={handleNotificationClose}
          PaperProps={{
            sx: { width: 320, maxHeight: 400 },
          }}
        >
          <Box sx={{ p: 2 }}>
            <Typography variant="subtitle1" fontWeight={600}>
              Bildirimler
            </Typography>
          </Box>
          <Divider />
          {notifications.length === 0 ? (
            <MenuItem disabled>
              <Typography variant="body2" color="text.secondary">
                Bildirim yok
              </Typography>
            </MenuItem>
          ) : (
            notifications.map((n) => (
              <MenuItem key={n.transactionId} onClick={handleNotificationClose}>
                <Box>
                  <Typography variant="body2" fontWeight={500}>
                    {TRANSACTION_TYPE_LABELS[n.transactionType] || n.transactionType}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {formatCurrency(n.amount, n.currency)} • {n.description || 'İşlem'} • {formatDateTime(n.transactionDate)}
                  </Typography>
                </Box>
              </MenuItem>
            ))
          )}
        </Menu>

        {/* Profil Menüsü */}
        <Tooltip title="Hesabım">
          <IconButton onClick={handleProfileMenuOpen} sx={{ p: 0, ml: 1 }}>
            <Avatar
              sx={{
                bgcolor: 'primary.main',
                width: 36,
                height: 36,
                fontSize: '0.875rem',
              }}
            >
              {user ? getInitials(fullName) : null}
            </Avatar>
          </IconButton>
        </Tooltip>

        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
          PaperProps={{
            sx: { width: 220, mt: 1 },
          }}
          transformOrigin={{ horizontal: 'right', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        >
          <Box sx={{ px: 2, py: 1.5 }}>
            <Typography variant="subtitle2" fontWeight={600}>
              {fullName}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {user?.email}
            </Typography>
          </Box>
          <Divider />
          <MenuItem onClick={handleLogoutClick}>
            <ListItemIcon>
              <LogoutIcon fontSize="small" color="error" />
            </ListItemIcon>
            <Typography color="error">Çıkış Yap</Typography>
          </MenuItem>
        </Menu>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
