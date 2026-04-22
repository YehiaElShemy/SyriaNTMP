import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DetailedStatisticsComponent } from './detailed-statistics/detailed-statistics.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@abp/ng.core';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { TranslationService } from '../../shared/services/translation.service';
import { TranslateModule } from '@ngx-translate/core';
import { effect } from '@angular/core';

@Component({
  selector: 'app-syria-stats-system',
  standalone: true,
  imports: [CommonModule, DetailedStatisticsComponent, DashboardComponent, SelectButtonModule, FormsModule, MenuModule, TranslateModule],
  templateUrl: './syria-stats-system.component.html',
  styleUrl: './syria-stats-system.component.scss'
})
export class SyriaStatsSystemComponent implements OnInit {
  viewOptions: any[] = [];
  selectedView: string = 'Detailed Statistics';
  langItems: MenuItem[] | undefined;

  constructor (private authService: AuthService, public translationService: TranslationService){
    effect(() => {
      const currentLang = this.translationService.currentLanguage();
      this.langItems = [
        { 
          label: 'العربية', 
          command: () => this.translationService.setLanguage('ar'),
          icon: currentLang === 'ar' ? 'pi pi-check' : undefined
        },
        { 
          label: 'English', 
          command: () => this.translationService.setLanguage('en'),
          icon: currentLang === 'en' ? 'pi pi-check' : undefined
        }
      ];
    });
  }

  ngOnInit() {
    this.viewOptions = [
      { label: 'Detailed Statistics', value: 'Detailed Statistics' },
      { label: 'Dashboard', value: 'Dashboard' }
    ];
  }

  logout() {
    this.authService.logout();
    this.authService.navigateToLogin();
  }
}
