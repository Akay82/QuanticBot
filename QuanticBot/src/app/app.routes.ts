import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/app-shell/app-shell.component').then((m) => m.AppShellComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'bots',
        loadComponent: () =>
          import('./features/bots/bots.component').then((m) => m.BotsComponent)
      },
      {
        path: 'paper-trading',
        loadComponent: () =>
          import('./features/paper-trading/paper-trading.component').then((m) => m.PaperTradingComponent)
      },
      {
        path: 'market-data',
        loadComponent: () =>
          import('./features/market-data/market-data.component').then((m) => m.MarketDataComponent)
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/reports/reports.component').then((m) => m.ReportsComponent)
      },
      {
        path: 'trades',
        loadComponent: () =>
          import('./features/trades/trades.component').then((m) => m.TradesComponent)
      },
      { path: '**', redirectTo: '' }
    ]
  }
];
