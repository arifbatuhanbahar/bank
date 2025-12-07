import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Box,
  Divider,
  Collapse,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import SendIcon from '@mui/icons-material/Send';
import CreditCardIcon from '@mui/icons-material/CreditCard';
import AddCardIcon from '@mui/icons-material/AddCard';
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser';
import SecurityIcon from '@mui/icons-material/Security';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import PeopleIcon from '@mui/icons-material/People';
import WarningIcon from '@mui/icons-material/Warning';
import HistoryIcon from '@mui/icons-material/History';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';

interface SidebarProps {
  open: boolean;
  onClose: () => void;
  isAdmin?: boolean;
}

interface MenuItem {
  title: string;
  path?: string;
  icon: React.ReactNode;
  children?: MenuItem[];
}

const drawerWidth = 260;

const Sidebar = ({ open, onClose, isAdmin = false }: SidebarProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState<Record<string, boolean>>({});

  const handleToggleMenu = (title: string) => {
    setOpenMenus(prev => ({ ...prev, [title]: !prev[title] }));
  };

  const handleNavigate = (path: string) => {
    navigate(path);
    onClose();
  };

  const menuItems: MenuItem[] = [
    { title: 'Dashboard', path: '/dashboard', icon: <DashboardIcon /> },
    { title: 'Hesaplarım', path: '/accounts', icon: <AccountBalanceWalletIcon /> },
    {
      title: 'İşlemler',
      icon: <SwapHorizIcon />,
      children: [
        { title: 'İşlem Geçmişi', path: '/transactions', icon: <HistoryIcon /> },
        { title: 'Para Transferi', path: '/transfer', icon: <SendIcon /> },
      ],
    },
    {
      title: 'Kartlarım',
      icon: <CreditCardIcon />,
      children: [
        { title: 'Kart Listesi', path: '/cards', icon: <CreditCardIcon /> },
        { title: 'Kart Başvurusu', path: '/cards/apply', icon: <AddCardIcon /> },
      ],
    },
    {
      title: 'Uyumluluk',
      icon: <VerifiedUserIcon />,
      children: [
        { title: 'KYC Belgeleri', path: '/compliance/kyc', icon: <VerifiedUserIcon /> },
        { title: 'KVKK', path: '/compliance/kvkk', icon: <SecurityIcon /> },
      ],
    },
  ];

  const adminMenuItems: MenuItem[] = [
    {
      title: 'Admin Panel',
      icon: <AdminPanelSettingsIcon />,
      children: [
        { title: 'Kullanıcı Yönetimi', path: '/admin/users', icon: <PeopleIcon /> },
        { title: 'Fraud Alarmları', path: '/admin/fraud', icon: <WarningIcon /> },
        { title: 'Audit Logs', path: '/admin/audit', icon: <HistoryIcon /> },
      ],
    },
  ];

  const allMenuItems = isAdmin ? [...menuItems, ...adminMenuItems] : menuItems;

  const isActive = (path?: string) => {
    if (!path) return false;
    return location.pathname === path || location.pathname.startsWith(path + '/');
  };

  const renderMenuItem = (item: MenuItem, depth = 0) => {
    const hasChildren = item.children && item.children.length > 0;
    const isMenuOpen = openMenus[item.title];
    const active = isActive(item.path);

    return (
      <Box key={item.title}>
        <ListItem disablePadding sx={{ display: 'block' }}>
          <ListItemButton
            onClick={() => {
              if (hasChildren) {
                handleToggleMenu(item.title);
              } else if (item.path) {
                handleNavigate(item.path);
              }
            }}
            sx={{
              minHeight: 48,
              px: 2.5,
              pl: depth > 0 ? 4 : 2.5,
              borderRadius: 2,
              mx: 1,
              mb: 0.5,
              backgroundColor: active ? 'primary.main' : 'transparent',
              color: active ? 'white' : 'text.primary',
              '&:hover': {
                backgroundColor: active ? 'primary.dark' : 'action.hover',
              },
            }}
          >
            <ListItemIcon
              sx={{
                minWidth: 40,
                color: active ? 'white' : 'text.secondary',
              }}
            >
              {item.icon}
            </ListItemIcon>
            <ListItemText
              primary={item.title}
              primaryTypographyProps={{
                fontSize: '0.9rem',
                fontWeight: active ? 600 : 500,
              }}
            />
            {hasChildren && (isMenuOpen ? <ExpandLess /> : <ExpandMore />)}
          </ListItemButton>
        </ListItem>

        {hasChildren && (
          <Collapse in={isMenuOpen} timeout="auto" unmountOnExit>
            <List component="div" disablePadding>
              {item.children!.map(child => renderMenuItem(child, depth + 1))}
            </List>
          </Collapse>
        )}
      </Box>
    );
  };

  return (
    <Drawer
      variant="temporary"
      open={open}
      onClose={onClose}
      ModalProps={{ keepMounted: true }}
      sx={{
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
          backgroundColor: 'background.paper',
          borderRight: 'none',
        },
      }}
    >
      <Box sx={{ height: 64 }} />

      <Box sx={{ p: 2 }}>
        <Typography variant="overline" color="text.secondary" sx={{ px: 1 }}>
          Ana Menü
        </Typography>
      </Box>

      <List sx={{ px: 1 }}>
        {allMenuItems.slice(0, 4).map(item => renderMenuItem(item))}
      </List>

      <Divider sx={{ my: 1 }} />

      <Box sx={{ p: 2 }}>
        <Typography variant="overline" color="text.secondary" sx={{ px: 1 }}>
          Diğer
        </Typography>
      </Box>

      <List sx={{ px: 1 }}>
        {allMenuItems.slice(4).map(item => renderMenuItem(item))}
      </List>
    </Drawer>
  );
};

export default Sidebar;
