import { AuthService } from '@abp/ng.core';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated
  }

  constructor(private authService: AuthService, private router: Router) {
    setTimeout(() => {
      if(this.authService.isAuthenticated){
        this.router.navigate(['/syria-stats']);
        console.log('authenticated');
      }else{
        console.log('not authenticated');
      }
    }, 1000);
    }
  

  login() {
    this.authService.navigateToLogin();
  }
}
