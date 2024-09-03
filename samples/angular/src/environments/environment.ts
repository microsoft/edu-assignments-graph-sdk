// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --configuration=dev` replaces `environment.ts` with `environment.dev.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  envName: 'dev',
  production: false,
  apiUrl: 'https://localhost/login/',
  apiHost: 'https://localhost/login/',
  googleClientId: 'YOUR_GOOGLE_ID',
  googleClientSecret: 'YOUR_GOOGLE_SECRET',
  googleApiUrl: 'https://www.googleapis.com',
  googleAuthEndpoint: 'https://accounts.google.com/o/oauth2/v2/auth',
  baseGoogleApiUrl: 'https://classroom.googleapis.com/v1',
  microsoftTenantId: "YOUR_TENANT_ID",
  microsoftClientId: "YOUR_APPLICATION_ID",
  microsoftSecret: "YOUR_Azure_SECRET",
  teamsApiUrl: 'https://graph.microsoft.com/v1.0',
  cacheLocation: "sessionStorage",
};

/*
 * In development mode, for easier debugging, you can ignore zone related error
 * stack frames such as `zone.run`/`zoneDelegate.invokeTask` by importing the
 * below file. Don't forget to comment it out in production mode
 * because it will have a performance impact when errors are thrown
 */
  // import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
