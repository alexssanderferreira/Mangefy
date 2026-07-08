import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe, NgClass, TitleCasePipe, DecimalPipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { TenantService, TenantDto } from '../tenants/services/tenant.service';
import { PlansService, PlanDto } from '../plans/plans.service';
import { SubscriptionService } from '../subscriptions/subscription.service';
import { OwnerService } from '../owners/owner.service';
import {
  LucideAngularModule, X, Check, TriangleAlert, ChevronRight, RefreshCw,
  House, TrendingUp, ChartNoAxesColumn, EllipsisVertical
} from 'lucide-angular';

type Filter = 'all' | 'Active' | 'TrialPeriod' | 'Suspended' | 'Cancelled';

function startOf(y: number, m: number) { return new Date(y, m, 1).getTime(); }
function endOf(y: number, m: number)   { return new Date(y, m + 1, 0, 23, 59, 59, 999).getTime(); }

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, NgClass, TitleCasePipe, DecimalPipe, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Dashboard</h1>
          <p class="page-subtitle">Visão Geral Da Plataforma · {{ today | date:'EEEE, d MMM. yyyy' | titlecase }}</p>
        </div>
        <div class="header-actions">
          @if (activeFilter() !== 'all') {
            <button class="clear-filter" (click)="setFilter('all')">
              <lucide-icon [img]="X" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Limpar filtro
            </button>
          }
          @if (lastUpdated()) {
            <span class="last-updated">Atualizado às {{ lastUpdated() | date:'HH:mm' }}</span>
          }
          <button class="reload-btn" (click)="reload()" [disabled]="loading()" title="Recarregar">
            <lucide-icon [img]="RefreshCw" [size]="14" [strokeWidth]="2.5" [class.spinning]="loading()"></lucide-icon>
          </button>
        </div>
      </div>

      <!-- Atenção -->
      @if (!loading()) {
        <div class="attention-band">
          @if (actionItems().length === 0) {
            <div class="all-ok">
              <lucide-icon [img]="Check" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Tudo em ordem — nenhuma ação pendente
            </div>
          } @else {
            <span class="attention-label">
              <lucide-icon [img]="TriangleAlert" [size]="13" [strokeWidth]="2.5"></lucide-icon>
              Requer atenção
            </span>
            <div class="attention-chips">
              @for (item of actionItems(); track item.label) {
                <button class="att-chip" [style.--cc]="item.color" [style.--cbg]="item.bg"
                        (click)="router.navigate([item.route], { queryParams: item.params })">
                  <span class="att-count">{{ item.count }}</span>
                  <span class="att-label">{{ item.label }}</span>
                  <lucide-icon [img]="ChevronRight" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                </button>
              }
            </div>
          }
        </div>
      }

      <!-- Main two-column layout -->
      <div class="main-layout">

        <!-- LEFT -->
        <div class="main-col">

          <!-- KPI cards -->
          <p class="section-label">Indicadores Principais</p>
          <div class="kpi-grid">
            @for (c of kpiCards(); track c.key) {
              <div class="kpi-card" [class.kpi-active]="activeFilter() === c.key"
                   [class.kpi-dim]="activeFilter() !== 'all' && activeFilter() !== c.key"
                   [style.--accent]="c.color" (click)="setFilter(c.key)">
                <div class="kpi-header">
                  <span class="kpi-label">{{ c.label }}</span>
                  <div class="kpi-icon">
                    @switch (c.icon) {
                      @case ('home') {
                        <lucide-icon [img]="House" [size]="15" [strokeWidth]="2"></lucide-icon>
                      }
                      @case ('trending') {
                        <lucide-icon [img]="TrendingUp" [size]="15" [strokeWidth]="2"></lucide-icon>
                      }
                      @case ('chart') {
                        <lucide-icon [img]="ChartNoAxesColumn" [size]="15" [strokeWidth]="2"></lucide-icon>
                      }
                      @case ('alert') {
                        <lucide-icon [img]="TriangleAlert" [size]="15" [strokeWidth]="2"></lucide-icon>
                      }
                    }
                  </div>
                </div>
                <div class="kpi-value">
                  @if (loading()) { <span class="skel-val"></span> } @else { {{ c.value }} }
                </div>
                @if (!loading() && c.growth !== null) {
                  <div class="kpi-growth" [class.pos]="c.growth! >= 0" [class.neg]="c.growth! < 0">
                    {{ c.growth! >= 0 ? '▲' : '▼' }} {{ c.growth! | number:'1.1-1' }}% vs mês passado
                  </div>
                } @else if (!loading()) {
                  <div class="kpi-growth neutral">{{ c.sub }}</div>
                }
                <div class="kpi-bar"></div>
              </div>
            }
          </div>

          <!-- Billing -->
          <p class="section-label" style="margin-top:22px">Faturamento</p>
          <div class="billing-row">
            @for (c of billingCards(); track c.label) {
              <div class="billing-card" [style.--accent]="c.color">
                <div class="b-top">
                  <span class="b-label">{{ c.label }}</span>
                  @if (c.tag) {
                    <span class="tag" [style.background]="c.tagBg" [style.color]="c.tagColor">{{ c.tag }}</span>
                  }
                </div>
                <div class="b-value">
                  @if (loading()) { <span class="skel-val"></span> } @else { {{ c.value }} }
                </div>
                @if (!loading() && c.sub) {
                  <div class="b-sub">{{ c.sub }}</div>
                }
                @if (!loading() && c.sparkline.length > 1) {
                  <svg class="sparkline" viewBox="0 0 100 30" preserveAspectRatio="none">
                    <polyline [attr.points]="c.sparkline" fill="none" [attr.stroke]="c.sparkColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                }
              </div>
            }
          </div>

          <!-- Tenants recentes -->
          <div class="bottom-panel" style="margin-top:20px">
            <div class="panel-head">
              <div>
                <h2 class="panel-title">
                  @if (activeFilter() === 'all') { Estabelecimentos Recentes }
                  @else { Estabelecimentos · {{ filterLabel() }} }
                </h2>
                <p class="panel-sub">
                  @if (activeFilter() === 'all') { Últimos estabelecimentos criados }
                  @else { {{ filteredTenants().length }} resultado{{ filteredTenants().length !== 1 ? 's' : '' }} }
                </p>
              </div>
              <a routerLink="/admin/tenants" class="link-all">Ver todos →</a>
            </div>

            @if (loading()) {
              <div class="skel-rows">@for (i of [1,2,3,4]; track i) { <div class="skel-row"></div> }</div>
            } @else if (displayTenants().length) {
              <table class="mini-table">
                <thead><tr>
                  <th>Estabelecimento</th>
                  <th>Plano</th>
                  <th>Status</th>
                  <th>Criado em</th>
                  <th></th>
                </tr></thead>
                <tbody>
                  @for (t of displayTenants(); track t.id) {
                    <tr [routerLink]="['/admin/tenants', t.id]">
                      <td>
                        <div class="name-cell">
                          <div class="name-av">{{ t.name[0] }}</div>
                          <div>
                            <div class="fw6">{{ t.name }}</div>
                            <div class="sub">{{ t.email ?? t.slug }}</div>
                          </div>
                        </div>
                      </td>
                      <td class="mono">{{ planName(t.planId) }}</td>
                      <td><span class="badge" [ngClass]="statusClass(t.status)">{{ statusLabel(t.status) }}</span></td>
                      <td class="muted">{{ t.createdAt | date:'dd/MM/yyyy' }}</td>
                      <td class="action-col">
                        <button class="row-menu" (click)="$event.stopPropagation()">
                          <lucide-icon [img]="EllipsisVertical" [size]="14" [strokeWidth]="2"></lucide-icon>
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <div class="empty-state">
                <lucide-icon [img]="House" [size]="36" [strokeWidth]="1.5"></lucide-icon>
                <p>Nenhum estabelecimento encontrado</p>
              </div>
            }
          </div>

        </div>

        <!-- RIGHT sidebar -->
        <div class="sidebar-col">

          <!-- Distribuição de status -->
          <p class="section-label">Distribuição</p>
          <div class="sidebar-panel">
            @if (loading()) {
              <div class="donut-wrap"><div class="skel-circle"></div></div>
            } @else if (metrics()?.total) {
              <div class="donut-wrap">
                <svg class="donut-svg" viewBox="0 0 140 140">
                  @for (seg of donutSegments(); track seg.label) {
                    <circle class="donut-seg" cx="70" cy="70" r="50" fill="none"
                      [attr.stroke]="seg.color" stroke-width="18"
                      [attr.stroke-dasharray]="seg.dash + ' ' + seg.gap"
                      [attr.stroke-dashoffset]="seg.offset"/>
                  }
                  <text x="70" y="65" text-anchor="middle" class="donut-total">{{ metrics()!.total }}</text>
                  <text x="70" y="80" text-anchor="middle" class="donut-sub">estabelecimentos</text>
                </svg>
              </div>
              <div class="legend">
                @for (row of distRows(); track row.label) {
                  <div class="legend-row" (click)="setFilter(row.key)" [class.legend-active]="activeFilter() === row.key">
                    <span class="legend-dot" [style.background]="row.color"></span>
                    <span class="legend-name">{{ row.label }}</span>
                    <span class="legend-pct">{{ row.pct }}%</span>
                    <span class="legend-val">{{ row.count }}</span>
                  </div>
                }
              </div>
            } @else { <p class="no-data">Sem dados</p> }
          </div>

          <!-- Planos -->
          <p class="section-label" style="margin-top:18px">Assinantes por Plano</p>
          <div class="sidebar-panel">
            @if (loading()) {
              <div class="skel-rows">@for (i of [1,2,3]; track i) { <div class="skel-row" style="height:32px"></div> }</div>
            } @else if (planRows().length) {
              @for (row of planRows(); track row.planId) {
                <div class="plan-row">
                  <div class="plan-info">
                    <span class="plan-name">{{ row.name }}</span>
                    <span class="plan-count">{{ row.count }}</span>
                  </div>
                  <div class="plan-track">
                    <div class="plan-fill" [style.width.%]="row.pct"></div>
                  </div>
                </div>
              }
            } @else { <p class="no-data">Sem dados</p> }
          </div>

          <!-- Trials a vencer -->
          @if (metrics()?.trialsExpiringSoon?.length) {
            <p class="section-label" style="margin-top:18px">
              Trials a Vencer
              <span class="warn-badge">{{ metrics()!.trialsExpiringSoon.length }}</span>
            </p>
            <div class="sidebar-panel" style="padding:4px 0">
              @for (t of metrics()!.trialsExpiringSoon; track t.id) {
                <div class="trial-item" [routerLink]="['/admin/tenants', t.id]">
                  <div class="trial-av">{{ t.name[0] }}</div>
                  <div class="trial-info">
                    <span class="fw6">{{ t.name }}</span>
                    <span class="sub">vence {{ t.trialEndsAt | date:'dd/MM' }}</span>
                  </div>
                  <span class="days-badge" [ngClass]="daysClass(t.trialEndsAt!)">
                    {{ svc.daysUntil(t.trialEndsAt!) }}d
                  </span>
                </div>
              }
            </div>
          }

        </div>
      </div>

      @if (error()) {
        <div class="alert-error">{{ error() }}</div>
      }
    </div>
  `,
  styles: [`
    .page { padding: 24px 28px; }

    /* Header */
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }
    .header-actions { display: flex; align-items: center; gap: 8px; }
    .last-updated { font-size: 11px; color: #bbb; white-space: nowrap; }
    .clear-filter {
      display: flex; align-items: center; gap: 6px; padding: 6px 14px;
      border-radius: 8px; border: 1px solid #e8e8ec; background: #fff; color: #555;
      font-size: 12px; font-weight: 600; cursor: pointer; transition: all .15s;
      &:hover { background: #fef2f2; border-color: #fecaca; color: #b91c1c; }
    }
    .reload-btn {
      display: flex; align-items: center; justify-content: center;
      width: 32px; height: 32px; border-radius: 8px;
      border: 1px solid #e8e8ec; background: #fff; color: #888; cursor: pointer; transition: all .15s;
      &:hover { background: #f5f5f7; color: #333; }
      &:disabled { opacity: .4; cursor: default; }
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .spinning { animation: spin .8s linear infinite; }

    /* Attention band */
    .attention-band {
      display: flex; align-items: center; gap: 12px; flex-wrap: wrap;
      background: #fff; border: 1px solid #e8e8ec; border-radius: 10px;
      padding: 10px 16px; margin-bottom: 18px;
    }
    .all-ok {
      display: flex; align-items: center; gap: 7px;
      font-size: 12px; font-weight: 600; color: #16a34a;
    }
    .attention-label {
      display: flex; align-items: center; gap: 6px;
      font-size: 11px; font-weight: 700; color: #b45309; white-space: nowrap;
    }
    .attention-chips { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }
    .att-chip {
      display: inline-flex; align-items: center; gap: 5px;
      padding: 4px 10px 4px 8px; border-radius: 99px; border: none; cursor: pointer;
      background: var(--cbg); color: var(--cc); font-size: 11px; font-weight: 700;
      transition: filter .15s;
      &:hover { filter: brightness(.94); }
    }
    .att-count {
      display: inline-flex; align-items: center; justify-content: center;
      width: 18px; height: 18px; border-radius: 50%;
      background: var(--cc); color: #fff; font-size: 10px; font-weight: 800;
    }
    .att-label { font-weight: 600; }

    /* Section label */
    .section-label {
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .07em; color: #bbb; margin-bottom: 10px;
      display: flex; align-items: center; gap: 8px;
    }
    .warn-badge { font-size: 10px; font-weight: 700; padding: 1px 6px; border-radius: 99px; background: #fef3c7; color: #b45309; }

    /* Layout */
    .main-layout { display: grid; grid-template-columns: 1fr 272px; gap: 20px; align-items: start; }
    .main-col    { display: flex; flex-direction: column; min-width: 0; }
    .sidebar-col { display: flex; flex-direction: column; min-width: 0; }

    /* KPI Cards */
    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }

    /* ── Tablet (≤1024px) ── */
    @media (max-width: 1024px) {
      .main-layout  { grid-template-columns: 1fr; }
      .kpi-grid     { grid-template-columns: repeat(2, 1fr); }
      .billing-row  { grid-template-columns: repeat(3, 1fr); }
      .sidebar-col  { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; align-items: start; }
      /* section labels inside sidebar span both cols */
      .sidebar-col > p { grid-column: 1 / -1; margin-top: 0 !important; }
    }

    /* ── Mobile (≤640px) ── */
    @media (max-width: 640px) {
      .page         { padding: 12px; }
      .page-header  { margin-bottom: 14px; }
      .page-title   { font-size: 18px; }
      .page-subtitle{ font-size: 11px; }
      .main-layout  { gap: 12px; }

      /* KPIs: 2 colunas compactas */
      .kpi-grid     { grid-template-columns: repeat(2, 1fr); gap: 8px; }
      .kpi-card     { padding: 12px 12px 0; border-radius: 10px; }
      .kpi-value    { font-size: 26px; margin-bottom: 6px; }
      .kpi-growth   { font-size: 9px; padding-bottom: 10px; }
      .kpi-icon     { width: 24px; height: 24px; border-radius: 6px; }

      /* Billing: 1 coluna */
      .billing-row  { grid-template-columns: 1fr; gap: 8px; }
      .billing-card { padding: 10px 14px; }
      .b-value      { font-size: 18px; }

      /* Sidebar: coluna única */
      .sidebar-col  { display: flex; flex-direction: column; }
      .sidebar-panel{ padding: 12px; }

      /* Donut menor */
      .donut-svg { width: 90px; height: 90px; }

      /* Tabela recentes: esconde slug e plano, apenas nome/status/data */
      .bottom-panel { padding: 14px; border-radius: 10px; }
      .mini-table { display: block; overflow-x: auto; }
      .mini-table th:nth-child(2),
      .mini-table td:nth-child(2) { display: none; }
      .mini-table th:nth-child(4),
      .mini-table td:nth-child(4) { display: none; }
      .name-av { width: 24px; height: 24px; font-size: 10px; }
      .fw6  { font-size: 11px; }
      .sub  { display: none; }

      /* Trials sidebar */
      .trial-item { padding: 8px 10px; }
      .trial-av   { width: 22px; height: 22px; font-size: 9px; }

      /* Section labels */
      .section-label { font-size: 9px; }
    }
    .kpi-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 14px;
      padding: 18px 18px 0; overflow: hidden; cursor: pointer; transition: all .18s;
      &:hover { box-shadow: 0 4px 20px rgba(0,0,0,.08); transform: translateY(-1px); border-color: var(--accent); }
      &.kpi-active {
        border-color: var(--accent);
        box-shadow: 0 0 0 3px color-mix(in srgb, var(--accent) 15%, transparent);
        background: color-mix(in srgb, var(--accent) 4%, #fff);
      }
      &.kpi-dim { opacity: .35; transform: none; }
    }
    .kpi-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
    .kpi-label  { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .05em; color: #999; }
    .kpi-icon {
      width: 30px; height: 30px; border-radius: 8px; flex-shrink: 0;
      background: color-mix(in srgb, var(--accent) 12%, transparent); color: var(--accent);
      display: flex; align-items: center; justify-content: center;
    }
    .kpi-value  { font-size: 36px; font-weight: 800; color: #111; line-height: 1; margin-bottom: 8px; }
    .kpi-growth {
      font-size: 10px; font-weight: 600; padding-bottom: 14px;
      &.pos     { color: #16a34a; }
      &.neg     { color: #dc2626; }
      &.neutral { color: #aaa; }
    }
    .kpi-bar { height: 3px; margin: 0 -18px; background: color-mix(in srgb, var(--accent) 18%, transparent); }
    .kpi-active .kpi-bar { background: var(--accent); }

    /* Billing */
    .billing-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
    .billing-card {
      background: #fff; border: 1px solid #e8e8ec; border-left: 3px solid var(--accent);
      border-radius: 10px; padding: 14px 16px; transition: box-shadow .15s;
      &:hover { box-shadow: 0 2px 14px rgba(0,0,0,.07); }
    }
    .b-top   { display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px; }
    .b-label { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .05em; color: #aaa; }
    .b-value { font-size: 22px; font-weight: 800; color: #111; margin-bottom: 4px; }
    .b-sub   { font-size: 10px; color: #aaa; }
    .tag { font-size: 10px; font-weight: 700; padding: 2px 7px; border-radius: 99px; white-space: nowrap; }
    .sparkline { width: 100%; height: 30px; display: block; margin-top: 8px; opacity: .7; }

    /* Sidebar panels */
    .sidebar-panel { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; padding: 14px; }
    .donut-wrap  { display: flex; justify-content: center; margin-bottom: 14px; }
    .donut-svg   { width: 120px; height: 120px; }
    .donut-seg   { transition: stroke-dasharray .5s ease, stroke-dashoffset .5s ease; }
    .donut-total { font-size: 20px; font-weight: 800; fill: #111; font-family: inherit; }
    .donut-sub   { font-size: 10px; fill: #aaa; font-family: inherit; }
    .skel-circle { width: 90px; height: 90px; border-radius: 50%; background: #f0f0f3; animation: pulse 1.4s infinite; margin: 10px auto; }

    .legend     { display: flex; flex-direction: column; gap: 5px; }
    .legend-row {
      display: flex; align-items: center; gap: 7px; cursor: pointer;
      padding: 4px 5px; border-radius: 6px; transition: background .1s;
      &:hover         { background: #f9f9fb; }
      &.legend-active { background: #f2f2f5; }
    }
    .legend-dot  { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
    .legend-name { font-size: 11px; color: #555; flex: 1; }
    .legend-pct  { font-size: 11px; color: #aaa; }
    .legend-val  { font-size: 11px; font-weight: 700; color: #222; width: 22px; text-align: right; }

    /* Plan rows */
    .plan-row  { margin-bottom: 10px; &:last-child { margin-bottom: 0; } }
    .plan-info { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 4px; }
    .plan-name { font-size: 12px; font-weight: 600; color: #333; }
    .plan-count{ font-size: 11px; font-weight: 700; color: #777; }
    .plan-track{ height: 6px; background: #f0f0f3; border-radius: 99px; overflow: hidden; }
    .plan-fill { height: 100%; background: var(--color-brand); border-radius: 99px; transition: width .5s ease; }

    /* Trials */
    .trial-item {
      display: flex; align-items: center; gap: 9px; padding: 9px 12px;
      cursor: pointer; transition: background .1s;
      &:hover { background: #f9f9fb; }
    }
    .trial-av {
      width: 26px; height: 26px; border-radius: 7px; flex-shrink: 0;
      background: var(--color-brand); color: #fff; font-size: 10px; font-weight: 800;
      display: flex; align-items: center; justify-content: center;
    }
    .trial-info { flex: 1; display: flex; flex-direction: column; min-width: 0; }
    .days-badge {
      font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 99px; white-space: nowrap;
      &.urgent  { background: #fee2e2; color: #b91c1c; }
      &.warning { background: #fef3c7; color: #b45309; }
      &.ok      { background: #f0fdf4; color: #15803d; }
    }

    /* Bottom panel */
    .bottom-panel { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; padding: 18px 20px; }
    .panel-head  { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 14px; }
    .panel-title { font-size: 14px; font-weight: 700; color: #111; }
    .panel-sub   { font-size: 11px; color: #aaa; margin-top: 2px; }
    .link-all    { font-size: 12px; color: var(--color-brand); white-space: nowrap; }

    /* Table */
    .mini-table { width: 100%; border-collapse: collapse; }
    .mini-table th {
      padding: 7px 10px; text-align: left;
      font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .05em;
      color: #bbb; border-bottom: 1px solid #f0f0f3; background: #fafafa;
    }
    .mini-table td { padding: 10px 10px; font-size: 12px; color: #333; border-bottom: 1px solid #f7f7f9; }
    .mini-table tbody tr {
      cursor: pointer; transition: background .1s;
      &:hover td      { background: #f9f9fb; }
      &:last-child td { border-bottom: none; }
    }
    .name-cell { display: flex; align-items: center; gap: 9px; }
    .name-av {
      width: 28px; height: 28px; border-radius: 7px; flex-shrink: 0;
      background: var(--color-brand); color: #fff; font-size: 11px; font-weight: 800;
      display: flex; align-items: center; justify-content: center;
    }

    .fw6  { font-weight: 600; font-size: 12px; color: #111; }
    .sub  { font-size: 11px; color: #aaa; margin-top: 1px; }
    .mono { font-size: 11px; color: #888; }
    .muted{ color: #aaa; font-size: 11px; }

    .action-col { width: 32px; text-align: center; }
    .row-menu {
      display: inline-flex; align-items: center; justify-content: center;
      width: 26px; height: 26px; border-radius: 6px;
      border: none; background: none; color: #bbb; cursor: pointer;
      transition: background .1s, color .1s;
      &:hover { background: #f0f0f3; color: #555; }
    }

    /* Skeleton */
    .skel-val  { display: block; width: 60px; height: 28px; background: #f0f0f3; border-radius: 6px; animation: pulse 1.4s infinite; }
    .skel-rows { display: flex; flex-direction: column; gap: 8px; }
    .skel-row  { height: 40px; background: #f5f5f7; border-radius: 8px; animation: pulse 1.4s infinite; }
    @keyframes pulse { 0%,100% { opacity: 1; } 50% { opacity: .5; } }

    .empty-state { display: flex; flex-direction: column; align-items: center; padding: 36px 0; gap: 10px; color: #ddd; p { font-size: 13px; color: #bbb; } }
    .no-data     { font-size: 12px; color: #ccc; text-align: center; padding: 8px 0; }
  `]
})
export class DashboardComponent implements OnInit {
  svc            = inject(TenantService);
  plansSvc       = inject(PlansService);
  subscriptionSvc = inject(SubscriptionService);
  ownerSvc       = inject(OwnerService);
  router         = inject(Router);

  readonly X = X;
  readonly Check = Check;
  readonly TriangleAlert = TriangleAlert;
  readonly ChevronRight = ChevronRight;
  readonly RefreshCw = RefreshCw;
  readonly House = House;
  readonly TrendingUp = TrendingUp;
  readonly ChartNoAxesColumn = ChartNoAxesColumn;
  readonly EllipsisVertical = EllipsisVertical;

  today   = new Date();
  loading = signal(true);
  error   = signal('');
  tenants = signal<TenantDto[]>([]);
  plans   = signal<PlanDto[]>([]);
  overdueCount       = signal(0);
  pendingOwnersCount = signal(0);
  lastUpdated        = signal<Date | null>(null);

  activeFilter = signal<Filter>('all');

  // ─── Action items ──────────────────────────────────────────────────────────

  actionItems = computed(() => {
    const m        = this.metrics();
    const trials   = m?.trialsExpiringSoon?.length ?? 0;
    const overdue  = this.overdueCount();
    const suspended = m?.suspended ?? 0;
    const pending  = this.pendingOwnersCount();
    const items: { count: number; label: string; route: string; params: Record<string, string>; color: string; bg: string }[] = [];
    if (trials   > 0) items.push({ count: trials,   label: `trial${trials   !== 1 ? 's' : ''} vencendo`,      route: '/admin/tenants', params: { status: 'TrialPeriod' },          color: '#d97706', bg: '#fef3c7' });
    if (overdue  > 0) items.push({ count: overdue,  label: `inadimplente${overdue  !== 1 ? 's' : ''}`,        route: '/admin/overdue', params: {},                                  color: '#dc2626', bg: '#fee2e2' });
    if (suspended > 0) items.push({ count: suspended, label: `suspenso${suspended !== 1 ? 's' : ''}`,         route: '/admin/tenants', params: { status: 'Suspended' },             color: '#ea580c', bg: '#fff7ed' });
    if (pending  > 0) items.push({ count: pending,  label: `aguardando ativação`,                             route: '/admin/owners',  params: { status: 'PendingActivation' },     color: '#2563eb', bg: '#dbeafe' });
    return items;
  });

  // ─── Derived metrics ───────────────────────────────────────────────────────

  metrics = computed(() =>
    this.tenants().length ? this.svc.computeMetrics(this.tenants()) : null
  );

  filteredTenants = computed(() => {
    const f = this.activeFilter();
    return f === 'all' ? this.tenants() : this.tenants().filter(t => t.status === f);
  });

  displayTenants = computed(() => {
    const f = this.activeFilter();
    return f === 'all' ? (this.metrics()?.recent ?? []) : this.filteredTenants().slice(0, 10);
  });

  filterLabel = computed(() => ({
    Active: 'Ativos', TrialPeriod: 'Em Trial',
    Suspended: 'Suspensos', Cancelled: 'Cancelados', all: ''
  }[this.activeFilter()] ?? ''));

  // ─── Growth (this month vs last month) ─────────────────────────────────────

  private growthData = computed(() => {
    const ts = this.tenants();
    const now = new Date();
    const y = now.getFullYear(), m = now.getMonth();
    const thisMonthTs  = ts.filter(t => { const d = new Date(t.createdAt).getTime(); return d >= startOf(y, m) && d <= endOf(y, m); });
    const lastMonthTs  = ts.filter(t => { const d = new Date(t.createdAt).getTime(); return d >= startOf(y, m - 1) && d <= endOf(y, m - 1); });
    const dayOfMonth   = now.getDate();
    const daysInMonth  = new Date(y, m + 1, 0).getDate();
    const projection   = Math.round((thisMonthTs.length / dayOfMonth) * daysInMonth);
    const growthPct    = lastMonthTs.length === 0 ? null : ((thisMonthTs.length - lastMonthTs.length) / lastMonthTs.length) * 100;
    return { thisMonth: thisMonthTs.length, lastMonth: lastMonthTs.length, projection, growthPct };
  });

  // ─── KPI cards ─────────────────────────────────────────────────────────────

  kpiCards = computed(() => {
    const m = this.metrics();
    const g = this.growthData();
    const total = m?.total ?? 0;
    const churnPct = total > 0 ? ((m?.cancelled ?? 0) / total * 100) : 0;
    const convPct  = (m?.active ?? 0) + (m?.cancelled ?? 0) > 0
      ? ((m?.active ?? 0) / ((m?.active ?? 0) + (m?.cancelled ?? 0)) * 100)
      : null;

    return [
      {
        key: 'Active' as Filter, label: 'Estab. Ativos', color: '#16a34a', icon: 'home',
        value: m?.active ?? '—', growth: null,
        sub: `${m?.trial ?? 0} em trial`,
      },
      {
        key: 'all' as Filter, label: 'Novos Este Mês', color: '#2563eb', icon: 'trending',
        value: g.thisMonth, growth: g.growthPct,
        sub: `Projeção: ${g.projection} até fim do mês`,
      },
      {
        key: 'TrialPeriod' as Filter, label: 'Taxa de Conversão', color: '#7c3aed', icon: 'chart',
        value: convPct !== null ? (convPct.toFixed(0) + '%') : '—', growth: null,
        sub: `${m?.active ?? 0} ativos de ${(m?.active ?? 0) + (m?.cancelled ?? 0)} convertidos`,
      },
      {
        key: 'Suspended' as Filter, label: 'Churn Rate', color: '#dc2626', icon: 'alert',
        value: churnPct.toFixed(1) + '%', growth: null,
        sub: `${m?.suspended ?? 0} suspensos · ${m?.cancelled ?? 0} cancelados`,
      },
    ];
  });

  // ─── Sparkline data (last 6 months new tenants) ────────────────────────────

  private monthlyNewTenants = computed(() => {
    const ts = this.tenants();
    const now = new Date();
    return Array.from({ length: 6 }, (_, i) => {
      const m = now.getMonth() - (5 - i);
      const y = now.getFullYear() + Math.floor(m / 12);
      const mo = ((m % 12) + 12) % 12;
      return ts.filter(t => {
        const d = new Date(t.createdAt);
        return d.getFullYear() === y && d.getMonth() === mo;
      }).length;
    });
  });

  private toSparkline(values: number[]): string {
    if (values.length < 2) return '';
    const max = Math.max(...values, 1);
    const min = Math.min(...values);
    const range = max - min || 1;
    return values.map((v, i) => {
      const x = (i / (values.length - 1)) * 100;
      const y = 28 - ((v - min) / range) * 26;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    }).join(' ');
  }

  // ─── Billing cards ─────────────────────────────────────────────────────────

  private mrr = computed(() => {
    return this.tenants()
      .filter(t => t.status === 'Active')
      .reduce((acc, t) => {
        const plan = this.plans().find(p => p.id === t.planId);
        return acc + (plan?.monthlyPrice ?? 0);
      }, 0);
  });

  billingCards = computed(() => {
    const mrr  = this.mrr();
    const arr  = mrr * 12;
    const risk = this.tenants()
      .filter(t => t.status === 'Suspended')
      .reduce((acc, t) => { const p = this.plans().find(p => p.id === t.planId); return acc + (p?.monthlyPrice ?? 0); }, 0);
    const fmt  = (v: number) => v === 0 ? '—' : 'R$ ' + v.toLocaleString('pt-BR');
    const g    = this.growthData();

    const spark = this.monthlyNewTenants();
    const sparkLine = this.toSparkline(spark);
    const riskSpark = this.toSparkline(spark.map(v => Math.max(0, v - 1)));

    return [
      { label: 'MRR Estimado', value: fmt(mrr), color: '#059669',
        tag: 'Mensal', tagBg: '#d1fae5', tagColor: '#065f46',
        sub: g.growthPct !== null ? `${g.growthPct >= 0 ? '+' : ''}${g.growthPct.toFixed(1)}% vs mês passado` : null,
        sparkline: sparkLine, sparkColor: '#059669' },
      { label: 'ARR Estimado', value: fmt(arr), color: '#0284c7',
        tag: 'Anual', tagBg: '#e0f2fe', tagColor: '#0369a1',
        sub: `Projeção MRR: ${fmt(Math.round((mrr / (new Date().getDate())) * new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0).getDate()))}`,
        sparkline: sparkLine, sparkColor: '#0284c7' },
      { label: 'Receita em Risco', value: fmt(risk), color: '#dc2626',
        tag: risk > 0 ? `${this.metrics()?.suspended ?? 0} susp.` : null, tagBg: '#fee2e2', tagColor: '#b91c1c',
        sub: risk > 0 ? 'Estabelecimentos suspensos sem pagamento' : 'Sem receita em risco',
        sparkline: riskSpark, sparkColor: '#dc2626' },
    ];
  });

  // ─── Distribution ──────────────────────────────────────────────────────────

  distRows = computed(() => {
    const m = this.metrics();
    if (!m?.total) return [];
    const pct = (v: number) => Math.round((v / m.total) * 100);
    return [
      { label: 'Ativos',     color: '#16a34a', pct: pct(m.active),    count: m.active,    key: 'Active'      as Filter },
      { label: 'Trial',      color: '#d97706', pct: pct(m.trial),     count: m.trial,     key: 'TrialPeriod' as Filter },
      { label: 'Suspensos',  color: '#dc2626', pct: pct(m.suspended), count: m.suspended, key: 'Suspended'   as Filter },
      { label: 'Cancelados', color: '#a1a1aa', pct: pct(m.cancelled), count: m.cancelled, key: 'Cancelled'   as Filter },
    ];
  });

  donutSegments = computed(() => {
    const m = this.metrics();
    if (!m?.total) return [];
    const circ = 2 * Math.PI * 50;
    let offset = circ * 0.25;
    return this.distRows().map(r => {
      const dash = (r.pct / 100) * circ;
      const gap  = circ - dash;
      const seg  = { label: r.label, color: r.color, dash, gap, offset };
      offset -= dash;
      return seg;
    });
  });

  // ─── Plan distribution ──────────────────────────────────────────────────────

  planRows = computed(() => {
    const ts = this.tenants();
    const ps = this.plans();
    if (!ts.length || !ps.length) return [];
    const counts = new Map<string, number>();
    ts.forEach(t => counts.set(t.planId, (counts.get(t.planId) ?? 0) + 1));
    const max = Math.max(...counts.values());
    return [...counts.entries()]
      .map(([planId, count]) => ({
        planId,
        name: ps.find(p => p.id === planId)?.name ?? 'Desconhecido',
        count,
        pct: Math.round((count / max) * 100)
      }))
      .sort((a, b) => b.count - a.count);
  });

  // ─── Helpers ───────────────────────────────────────────────────────────────

  planName(planId: string) {
    return this.plans().find(p => p.id === planId)?.name ?? '—';
  }

  setFilter(f: Filter) {
    this.activeFilter.set(this.activeFilter() === f ? 'all' : f);
  }

  statusLabel(s: string) {
    return ({ Active: 'Ativo', TrialPeriod: 'Trial', Suspended: 'Suspenso', Cancelled: 'Cancelado' } as any)[s] ?? s;
  }
  statusClass(s: string) {
    return ({ Active: 'badge-success', TrialPeriod: 'badge-info', Suspended: 'badge-warning', Cancelled: 'badge-neutral' } as any)[s] ?? 'badge-neutral';
  }
  daysClass(date: string) {
    const d = this.svc.daysUntil(date);
    return d <= 3 ? 'urgent' : d <= 7 ? 'warning' : 'ok';
  }

  reload() { this.load(); }

  ngOnInit() { this.load(); }

  private load() {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      tenants: this.svc.getPaged(1, 500),
      plans:   this.plansSvc.getAll(),
      overdue: this.subscriptionSvc.getOverdue(),
      owners:  this.ownerSvc.getAll(1, 500),
    }).subscribe({
      next: ({ tenants, plans, overdue, owners }) => {
        const items = Array.isArray(tenants) ? tenants : (tenants as any).items ?? [];
        const ownerItems: any[] = Array.isArray(owners) ? owners : (owners as any).items ?? [];
        this.tenants.set(items);
        this.plans.set(plans);
        this.overdueCount.set(overdue.length);
        this.pendingOwnersCount.set(ownerItems.filter((o: any) => o.status === 'PendingActivation').length);
        this.lastUpdated.set(new Date());
        this.loading.set(false);
      },
      error: () => { this.error.set('Não foi possível carregar os dados.'); this.loading.set(false); }
    });
  }
}
