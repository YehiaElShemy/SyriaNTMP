import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DetailedStatisticsComponent } from './detailed-statistics/detailed-statistics.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-syria-stats-system',
  standalone: true,
  imports: [CommonModule, DetailedStatisticsComponent, DashboardComponent, SelectButtonModule, FormsModule],
  templateUrl: './syria-stats-system.component.html',
  styleUrl: './syria-stats-system.component.scss'
})
export class SyriaStatsSystemComponent implements OnInit {
  viewOptions: any[] = [];
  selectedView: string = 'Detailed Statistics';

  ngOnInit() {
    this.viewOptions = [
      { label: 'Detailed Statistics', value: 'Detailed Statistics' },
      { label: 'Dashboard', value: 'Dashboard' }
    ];
  }
}
