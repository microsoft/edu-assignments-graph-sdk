import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from "./header/header.component";
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LoginComponent } from './login/login.component';
import { CoursesComponent } from './courses/courses.component';
import { GoogleLoginProvider, SocialAuthServiceConfig, SocialLoginModule } from '@abacritt/angularx-social-login';
import { environment } from '../environments/environment.dev';
import { FormsModule } from '@angular/forms';
import { CourseMaterialComponent } from './coursework-material/coursework-material.component';
import { LoaderComponent } from './loader/loader.component';
import { IPublicClientApplication, InteractionType, LogLevel, PublicClientApplication } from '@azure/msal-browser';
import { MsalGuard, MsalGuardConfiguration, MsalInterceptor, MsalInterceptorConfiguration, MsalModule, MsalRedirectComponent, MsalService } from '@azure/msal-angular';
import { GoogleCourseworkListComponent } from './google-coursework-list/google-coursework-list.component';
import { TeamsclassesComponent } from './teamsclasses/teamsclasses.component';
import { GoogleLoginComponent } from './google-login/google-login.component';
import { MigratedDataComponent } from './migrated-data/migrated-data.component';
import { OAuthModule, OAuthService } from 'angular-oauth2-oidc';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: environment.microsoftClientId,
      authority: 'https://login.microsoftonline.com/' + environment.microsoftTenantId,
      redirectUri: 'http://localhost:80/google-login', 
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: isIE, // Set to true if using IE 11
    },
    system: {
      loggerOptions: {
        loggerCallback(logLevel: LogLevel, message: string) {
          console.log(message);
        },
        piiLoggingEnabled: false,
      }
    }
  });
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Popup,
    authRequest: {
      scopes: ['user.read', 'openid', 'profile', 'https://graph.microsoft.com/Education.Classes.Read']
      
    }
  };
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
  protectedResourceMap.set('https://graph.microsoft.com/v1.0/me', ['user.read']);

  return {
    interactionType: InteractionType.Popup,
    protectedResourceMap
  };
}

@NgModule({
  declarations: [
    AppComponent,
    CoursesComponent,
    CourseMaterialComponent,
    LoaderComponent,
    GoogleCourseworkListComponent,
    TeamsclassesComponent,
    GoogleLoginComponent,
    MigratedDataComponent
  ],
  providers: [
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              environment.googleClientId
            )
          }
        ],
        onError: (err) => {
          console.error(err);
        }
      } as SocialAuthServiceConfig,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (msalService: MsalService) => {
        return () => msalService.instance.initialize();
      },
      deps: [MsalService],
      multi: true
    },
    MsalGuard,
    MsalService,
    OAuthService
  ],
  bootstrap: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    LoginComponent,
    HeaderComponent,
    SocialLoginModule,
    BrowserModule,
    HttpClientModule,
    MsalModule.forRoot(
      MSALInstanceFactory(),
      MSALGuardConfigFactory(),
      MSALInterceptorConfigFactory()
    ),
    OAuthModule.forRoot()
  ]
})
export class AppModule { }
