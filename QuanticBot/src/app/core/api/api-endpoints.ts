export const API_BASE_URL = typeof window === 'undefined' ? 'http://localhost:5069/api' : '/api';

const resource = (path: string) => ({
  list: `${API_BASE_URL}/${path}`,
  details: (id: number) => `${API_BASE_URL}/${path}/${id}`
});

export const API_ENDPOINTS = {
  instruments: resource('instruments'),
  paperAccounts: resource('paper-accounts'),
  strategies: resource('strategies'),
  orders: {
    ...resource('orders'),
    cancel: (id: number) => `${API_BASE_URL}/orders/${id}/cancel`,
    execute: (id: number) => `${API_BASE_URL}/orders/${id}/execute`
  },
  positions: resource('positions'),
  priceHistory: resource('price-history'),
  signals: resource('signals'),
  trades: resource('trades'),
  watchlists: resource('watchlists'),
  marketData: {
    forexCharts: `${API_BASE_URL}/market-data/forex/charts`,
    forexChart: (instrumentId: number) => `${API_BASE_URL}/market-data/forex/charts/${instrumentId}`,
    refreshForex: `${API_BASE_URL}/market-data/forex/refresh`
  },
  tradingBots: {
    list: `${API_BASE_URL}/trading-bots`,
    details: (id: string) => `${API_BASE_URL}/trading-bots/${id}`,
    start: (id: string) => `${API_BASE_URL}/trading-bots/${id}/start`,
    stop: (id: string) => `${API_BASE_URL}/trading-bots/${id}/stop`
  }
} as const;
