import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SyriaStatsSystemComponent } from '../pages/syria-NTMP/syria-stats-system.component';
import { authGuard } from '@abp/ng.core';
import { HomeComponent } from './home.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent 
  },
  {
    path: 'syria-stats',
    component: SyriaStatsSystemComponent,
    canActivate: [authGuard] 
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}