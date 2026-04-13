import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SyriaStatsSystemComponent } from '../pages/syria-NTMP/syria-stats-system.component';

const routes: Routes = [{ path: '', component: SyriaStatsSystemComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}
