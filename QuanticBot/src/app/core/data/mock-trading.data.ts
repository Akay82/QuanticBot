import { Trade } from '../models/trading.models';

const ago = (days: number, hours: number): string => {
  const date = new Date();
  date.setDate(date.getDate() - days);
  date.setHours(hours, 30, 0, 0);
  return date.toISOString();
};

export const MOCK_TRADES: Trade[] = [
  { id: 'TRD-1028', symbol: 'NIFTY 50', side: 'BUY', quantity: 75, entryPrice: 24418.5, exitPrice: 24512.8, pnl: 7072.5, status: 'PROFIT', strategy: 'Momentum Alpha', executedAt: ago(0, 10) },
  { id: 'TRD-1027', symbol: 'BANKNIFTY', side: 'SELL', quantity: 30, entryPrice: 53218.4, exitPrice: 53085.2, pnl: 3996, status: 'PROFIT', strategy: 'Mean Reversion', executedAt: ago(0, 9) },
  { id: 'TRD-1026', symbol: 'RELIANCE', side: 'BUY', quantity: 120, entryPrice: 1421.6, pnl: 1848, status: 'OPEN', strategy: 'Breakout Pro', executedAt: ago(1, 14) },
  { id: 'TRD-1025', symbol: 'TCS', side: 'SELL', quantity: 45, entryPrice: 3518.2, exitPrice: 3548.6, pnl: -1368, status: 'LOSS', strategy: 'Momentum Alpha', executedAt: ago(2, 11) },
  { id: 'TRD-1024', symbol: 'HDFCBANK', side: 'BUY', quantity: 90, entryPrice: 1924.7, exitPrice: 1952.3, pnl: 2484, status: 'PROFIT', strategy: 'Breakout Pro', executedAt: ago(3, 13) },
  { id: 'TRD-1023', symbol: 'INFY', side: 'BUY', quantity: 140, entryPrice: 1620.2, exitPrice: 1606.7, pnl: -1890, status: 'LOSS', strategy: 'Mean Reversion', executedAt: ago(4, 10) },
  { id: 'TRD-1022', symbol: 'NIFTY 50', side: 'SELL', quantity: 100, entryPrice: 24396.2, exitPrice: 24242.6, pnl: 15360, status: 'PROFIT', strategy: 'Momentum Alpha', executedAt: ago(5, 15) },
  { id: 'TRD-1021', symbol: 'ICICIBANK', side: 'BUY', quantity: 160, entryPrice: 1422.5, exitPrice: 1439.1, pnl: 2656, status: 'PROFIT', strategy: 'Breakout Pro', executedAt: ago(6, 12) },
  { id: 'TRD-1020', symbol: 'BANKNIFTY', side: 'BUY', quantity: 45, entryPrice: 52812.4, exitPrice: 53024.5, pnl: 9544.5, status: 'PROFIT', strategy: 'Momentum Alpha', executedAt: ago(8, 11) },
  { id: 'TRD-1019', symbol: 'SBIN', side: 'SELL', quantity: 240, entryPrice: 812.8, exitPrice: 820.6, pnl: -1872, status: 'LOSS', strategy: 'Mean Reversion', executedAt: ago(11, 10) },
  { id: 'TRD-1018', symbol: 'RELIANCE', side: 'BUY', quantity: 130, entryPrice: 1394.2, exitPrice: 1432.5, pnl: 4979, status: 'PROFIT', strategy: 'Breakout Pro', executedAt: ago(15, 14) },
  { id: 'TRD-1017', symbol: 'TCS', side: 'BUY', quantity: 55, entryPrice: 3460.4, exitPrice: 3512.7, pnl: 2876.5, status: 'PROFIT', strategy: 'Momentum Alpha', executedAt: ago(19, 13) },
  { id: 'TRD-1016', symbol: 'NIFTY 50', side: 'SELL', quantity: 80, entryPrice: 24192.1, exitPrice: 24231.4, pnl: -3144, status: 'LOSS', strategy: 'Mean Reversion', executedAt: ago(23, 10) },
  { id: 'TRD-1015', symbol: 'INFY', side: 'BUY', quantity: 180, entryPrice: 1582.6, exitPrice: 1610.2, pnl: 4968, status: 'PROFIT', strategy: 'Breakout Pro', executedAt: ago(27, 12) }
];
