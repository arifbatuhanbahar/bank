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
import CreditCardIcon from '@mui/icons-material/CreditCard';
import { cardService } from '../../../api';
import { CreditCard } from '../../../types';
import { formatCardExpiry, formatCurrency } from '../../../utils/formatters';
import { CARD_BRAND_LABELS, CARD_STATUS_LABELS, CARD_TYPE_LABELS } from '../../../utils/constants';

const CardDetailPage = () => {
  const { id } = useParams();
  const cardId = Number(id);
  const [card, setCard] = useState<CreditCard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const userJson = localStorage.getItem('user');
        const user = userJson ? JSON.parse(userJson) : null;
        if (!user?.userId) {
          setError('Kullanıcı bulunamadı.');
          return;
        }
        const cards = await cardService.getUserCards(user.userId);
        const found = cards.find(c => c.cardId === cardId);
        if (!found) {
          setError('Kart bulunamadı.');
        } else {
          setCard(found);
        }
      } catch (err) {
        setError('Kart bilgisi alınamadı.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [cardId]);

  const statusColor = useMemo(() => (card ? cardService.getStatusColor(card.status) : 'default'), [card]);

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Kart Detayı
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Skeleton variant="rectangular" height={180} />
      ) : card ? (
        <Paper sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <CreditCardIcon color="primary" />
            <Typography variant="h6" fontWeight={700}>
              {CARD_BRAND_LABELS[card.cardBrand] || card.cardBrand} · {CARD_TYPE_LABELS[card.cardType] || card.cardType}
            </Typography>
            <Chip label={CARD_STATUS_LABELS[card.status] || card.status} color={statusColor} size="small" />
          </Box>
          <Typography variant="h5" sx={{ letterSpacing: 2, mb: 1 }}>
            **** **** **** {card.cardLastFour}
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Son Kullanma</Typography>
              <Typography fontWeight={700}>{formatCardExpiry(card.expiryMonth, card.expiryYear)}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Limit</Typography>
              <Typography fontWeight={700}>{formatCurrency(card.creditLimit, 'TRY')}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Kullanılabilir</Typography>
              <Typography fontWeight={700}>{formatCurrency(card.availableLimit, 'TRY')}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Asgari Ödeme</Typography>
              <Typography fontWeight={700}>{formatCurrency(card.minimumPayment, 'TRY')}</Typography>
            </Grid>
          </Grid>
        </Paper>
      ) : null}
    </Box>
  );
};

export default CardDetailPage;
