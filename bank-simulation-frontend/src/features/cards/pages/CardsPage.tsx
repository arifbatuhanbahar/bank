import { useEffect, useState } from 'react';
import { Box, Typography, Paper, Chip, Divider, Skeleton } from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import CreditCardIcon from '@mui/icons-material/CreditCard';
import { cardService } from '../../../api';
import { CreditCard } from '../../../types';
import { formatCardExpiry, formatCurrency } from '../../../utils/formatters';
import { CARD_BRAND_LABELS, CARD_STATUS_LABELS, CARD_TYPE_LABELS } from '../../../utils/constants';

const CardsPage = () => {
  const [cards, setCards] = useState<CreditCard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : null;

  useEffect(() => {
    const loadCards = async () => {
      if (!user?.userId) {
        setError('Kullanıcı bulunamadı.');
        setLoading(false);
        return;
      }
      try {
        const data = await cardService.getUserCards(user.userId);
        setCards(data);
      } catch (err) {
        console.error(err);
        setError('Kartlar yüklenirken hata oluştu.');
      } finally {
        setLoading(false);
      }
    };
    loadCards();
  }, [user?.userId]);

  const statusColor = (status: string) => cardService.getStatusColor(status);

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Kartlarım
      </Typography>

      {loading ? (
        <Grid container spacing={2}>
          {[...Array(3)].map((_, idx) => (
            <Grid item xs={12} md={4} key={idx}>
              <Skeleton variant="rectangular" height={170} />
            </Grid>
          ))}
        </Grid>
      ) : error ? (
        <Typography color="error">{error}</Typography>
      ) : (
        <Grid container spacing={2}>
          {cards.map(card => (
            <Grid item xs={12} md={4} key={card.cardId}>
              <Paper sx={{ p: 2, position: 'relative', height: '100%' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <CreditCardIcon color="primary" />
                  <Typography variant="subtitle1" fontWeight={700}>
                    {CARD_BRAND_LABELS[card.cardBrand] || card.cardBrand} •{' '}
                    {CARD_TYPE_LABELS[card.cardType] || card.cardType}
                  </Typography>
                </Box>
                <Typography variant="h6" sx={{ letterSpacing: 2, mb: 1 }}>
                  **** **** **** {card.cardLastFour}
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    Son Kullanma: {formatCardExpiry(card.expiryMonth, card.expiryYear)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Limit: {formatCurrency(card.creditLimit, 'TRY')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Kullanılabilir: {formatCurrency(card.availableLimit, 'TRY')}
                  </Typography>
                </Box>
                <Divider sx={{ my: 1 }} />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Chip
                    label={CARD_STATUS_LABELS[card.status] || card.status}
                    color={statusColor(card.status)}
                    size="small"
                  />
                  <Typography variant="body2" color="text.secondary">
                    Ekstre asgari: {formatCurrency(card.minimumPayment, 'TRY')}
                  </Typography>
                </Box>
              </Paper>
            </Grid>
          ))}
          {cards.length === 0 && (
            <Grid item xs={12}>
              <Typography color="text.secondary">Henüz tanımlı kartınız yok.</Typography>
            </Grid>
          )}
        </Grid>
      )}
    </Box>
  );
};

export default CardsPage;
