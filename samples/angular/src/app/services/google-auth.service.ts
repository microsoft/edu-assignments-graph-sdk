import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { AuthConfig, OAuthService } from "angular-oauth2-oidc";
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class GoogleAuthService {
  private popupWindow: Window | null = null;

  constructor(private oAuthService: OAuthService, private router: Router) {
    this.initConfiguration();
  }

  initConfiguration() {
    const authConfig: AuthConfig = {
      issuer: 'https://accounts.google.com',
      strictDiscoveryDocumentValidation: false,
      clientId: environment.googleClientId,
      redirectUri: window.location.origin,
      scope: 'https://www.googleapis.com/auth/classroom.courses.readonly https://www.googleapis.com/auth/classroom.coursework.students.readonly https://www.googleapis.com/auth/classroom.courseworkmaterials.readonly https://www.googleapis.com/auth/drive.readonly',
    };

    this.oAuthService.configure(authConfig);
    this.oAuthService.setupAutomaticSilentRefresh();
    this.oAuthService.loadDiscoveryDocumentAndTryLogin();
  }

  login() {
    const redirectUri = window.location.origin;
    const authUrl = `${this.oAuthService.issuer}/o/oauth2/v2/auth?response_type=token&client_id=${this.oAuthService.clientId}&redirect_uri=${redirectUri}&scope=${this.oAuthService.scope}`;
    if (sessionStorage.getItem('googleAccessToken') === null) {
      const width = 500;
      const height = 600;
      const left = (screen.width / 2) - (width / 2);
      const top = (screen.height / 2) - (height / 2);

      this.popupWindow = window.open(authUrl, 'Google Login', `width=${width},height=${height},left=${left},top=${top}`);

      if (this.popupWindow) {
        const interval = setInterval(() => {
          try {
            const popupUrl = new URL(this.popupWindow!.location.href);
            if (popupUrl.origin === window.location.origin) {
              const accessToken = this.getAccessTokenFromUrl(popupUrl);
              if (accessToken) {
                sessionStorage.setItem('googleAccessToken', accessToken);
                clearInterval(interval);
                this.popupWindow!.close();
              }
            }
          } catch (error) {
            console.error('Error:', error);
          }
        }, 500);
      }
    }

    if (sessionStorage.getItem('googleAccessToken') !== null) {
      this.router.navigate(['courses']);
    }
  }

  private getAccessTokenFromUrl(url: URL): string | null {
    const params = new URLSearchParams(url.hash.substring(1));
    return params.get('access_token');
  }

  getAccessToken() {
    console.log('Access Token:', this.oAuthService.getAccessToken());
    sessionStorage.setItem('googleAccessToken', this.oAuthService.getAccessToken());
  }
}
