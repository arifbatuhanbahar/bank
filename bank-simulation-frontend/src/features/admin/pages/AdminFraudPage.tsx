import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import Grid from '@mui/material/GridLegacy';
import WarningIcon from '@mui/icons-material/Warning';
import RuleIcon from '@mui/icons-material/Rule';
import { fraudService } from '../../../api';
import { FraudCheckRequest, FraudRuleRequest, RuleType } from '../../../types';

const AdminFraudPage = () => {
  const [ruleForm, setRuleForm] = useState<FraudRuleRequest>({
    ruleName: '',
    ruleType: 'AmountAnomaly',
    description: '',
    conditions: '{"threshold":50000}',
    riskWeight: 40,
  });
  const [checkForm, setCheckForm] = useState<FraudCheckRequest>({
    transactionId: 0,
    userId: 0,
    amount: 0,
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [checkResult, setCheckResult] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleCreateRule = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setError(null);
    try {
      await fraudService.createRule(ruleForm);
      setMessage('Yeni fraud kuralı kaydedildi.');
    } catch (err) {
      setError('Kural oluşturulamadı.');
    }
  };

  const handleCheck = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setCheckResult(null);
    setLoading(true);
    try {
      const res = await fraudService.checkTransaction(checkForm);
      setCheckResult(`${res.message} · Risk Skoru: ${res.score}`);
    } catch (err) {
      setError('İşlem kontrolü başarısız.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        Fraud Yönetimi
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <RuleIcon color="primary" />
              <Typography variant="h6">Yeni kural tanımla</Typography>
            </Box>
            {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            <Box component="form" onSubmit={handleCreateRule}>
              <TextField
                label="Kural Adı"
                fullWidth
                required
                sx={{ mb: 2 }}
                value={ruleForm.ruleName}
                onChange={e => setRuleForm(prev => ({ ...prev, ruleName: e.target.value }))}
              />
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="rule-type-label">Kural Türü</InputLabel>
                <Select
                  labelId="rule-type-label"
                  label="Kural Türü"
                  value={ruleForm.ruleType}
                  onChange={e => setRuleForm(prev => ({ ...prev, ruleType: e.target.value as RuleType }))}
                >
                  {Object.keys(RuleType).map(key => (
                    <MenuItem key={key} value={RuleType[key as keyof typeof RuleType]}>
                      {key}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                label="Risk Ağırlığı (1-100)"
                type="number"
                fullWidth
                sx={{ mb: 2 }}
                value={ruleForm.riskWeight}
                onChange={e => setRuleForm(prev => ({ ...prev, riskWeight: Number(e.target.value) }))}
              />
              <TextField
                label="Koşul (JSON)"
                fullWidth
                multiline
                minRows={2}
                sx={{ mb: 2 }}
                value={ruleForm.conditions}
                onChange={e => setRuleForm(prev => ({ ...prev, conditions: e.target.value }))}
              />
              <TextField
                label="Açıklama"
                fullWidth
                multiline
                minRows={2}
                sx={{ mb: 2 }}
                value={ruleForm.description}
                onChange={e => setRuleForm(prev => ({ ...prev, description: e.target.value }))}
              />
              <Button type="submit" variant="contained" fullWidth>
                Kaydet
              </Button>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <WarningIcon color="warning" />
              <Typography variant="h6">İşlem riski kontrol et</Typography>
            </Box>
            {checkResult && <Alert severity="info" sx={{ mb: 2 }}>{checkResult}</Alert>}
            {error && !message && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            <Box component="form" onSubmit={handleCheck}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="Transaction Id"
                    type="number"
                    fullWidth
                    required
                    value={checkForm.transactionId || ''}
                    onChange={e => setCheckForm(prev => ({ ...prev, transactionId: Number(e.target.value) }))}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    label="User Id"
                    type="number"
                    fullWidth
                    required
                    value={checkForm.userId || ''}
                    onChange={e => setCheckForm(prev => ({ ...prev, userId: Number(e.target.value) }))}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    label="Tutar"
                    type="number"
                    fullWidth
                    required
                    value={checkForm.amount || ''}
                    onChange={e => setCheckForm(prev => ({ ...prev, amount: Number(e.target.value) }))}
                  />
                </Grid>
                <Grid item xs={12}>
                  <Button type="submit" variant="contained" fullWidth disabled={loading}>
                    {loading ? 'Kontrol ediliyor...' : 'Kontrol Et'}
                  </Button>
                </Grid>
              </Grid>
            </Box>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 2 }}>
              Bu çağrı fraud_rules tablosundaki aktif kuralları okur, risk skoru üretir ve gerekirse fraud_alerts tablosuna kayıt düşer.
            </Typography>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminFraudPage;
