import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GoogleAuthService } from '../services/google-auth.service';

@Component({
  selector: 'app-google-login',
  templateUrl: './google-login.component.html',
  styleUrls: ['./google-login.component.scss']
})
export class GoogleLoginComponent {
  enableSignIn: boolean = false;

  constructor(private router: Router, private googleAuthService: GoogleAuthService) {
    this.googleAuthService.login();
  }
  signIn() {
    this.googleAuthService.login();
    this.enableSignIn = true;
    if(sessionStorage.getItem('googleAccessToken') !== null) {
      this.router.navigate(['courses']);
    }
  }
}