declare var google: any;
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { SocialUser } from '@abacritt/angularx-social-login';
import { MsalService } from '@azure/msal-angular';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})

export class LoginComponent {
  user: SocialUser | null = null;
  loggedIn: boolean = false;
  loading = true;
  activeAccount: any;

  constructor(
    private router: Router,
    private msalService: MsalService, 
    private http: HttpClient
  ) { }

  async authenticate(): Promise<void> {
    await this.msalService.instance.handleRedirectPromise().then(() => {
      if (this.msalService.instance.getAllAccounts().length === 0) {
        this.login();
        this.activeAccount = this.msalService.instance.getActiveAccount();
      } else {
        this.fetchClasses();
        this.activeAccount = this.msalService.instance.getActiveAccount();
      }
    });

    if (this.msalService.instance.getAllAccounts().length > 0) {
      this.fetchClasses();
    }

    if(this.activeAccount) {
      this.router.navigate(['google-login']);
    }
  }

  login(): void {
    this.msalService.loginPopup({
      scopes: ['openid', 'profile', 'user.read']
    }).subscribe((response) => {
      console.log('Login successful:', response);
      this.msalService.instance.setActiveAccount(response.account);
      this.fetchClasses();
    }, (error) => {
      console.error('Login error', error);
      this.loading = true;
    });
  }

  fetchClasses(): void {
    const accounts = this.msalService.instance.getAllAccounts();
    if (accounts.length === 0) {
      // No user logged in, initiate login process
      this.login();
      return;
    } else{
      this.msalService.instance.setActiveAccount(accounts[0]);
    }
  
    // User is logged in, acquire token silently
    this.msalService.acquireTokenSilent({
      scopes: ['user.read']
    }).subscribe({
      next: (response) => {
        this.fetchGraphData(response.accessToken);
      },
      error: (error) => {
        console.error('Token acquisition error:', error);
        // Handle token acquisition errors
        if (error.errorMessage.indexOf('interaction_required') !== -1) {
          this.msalService.acquireTokenPopup({
            scopes: ['user.read']
          }).subscribe((response) => {
            console.log('Token acquired with popup:', response);
            this.fetchGraphData(response.accessToken);
          }, (popupError) => {
            console.error('Popup error:', popupError);
            alert('Error occured during teams login');
            this.loading = false;
          });
        }
      }
    });
  }
  
  private fetchGraphData(accessToken: string): void {
    this.http.get('https://graph.microsoft.com/v1.0/me', {
      headers: new HttpHeaders({
        Authorization: `Bearer ${accessToken}`
      })
    }).subscribe(data => {
      console.log('Graph API response:', data);
      // Navigate to the required page after fetching data
      this.loading = false;
      this.router.navigate(['/google-login']);
      
    }, (error) => {
      console.error('Error fetching data from Graph API:', error);
      this.loading = false;
    });
  }
}
