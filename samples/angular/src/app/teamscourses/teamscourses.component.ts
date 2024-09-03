import { HttpHeaders, HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { AuthenticationResult, EventMessage, EventType, InteractionStatus, AccountInfo } from '@azure/msal-browser';
import { defer, filter, from } from 'rxjs';

interface GraphClassesResponse {
  value: { id: string, displayName: string }[];
}

@Component({
  selector: 'app-teamscourses',
  templateUrl: './teamscourses.component.html',
  styleUrls: ['./teamscourses.component.scss']
})
export class TeamscoursesComponent implements OnInit {

  loginDisplay = false;
  activeAccount: AccountInfo | null = null;
  classes: { id: string, displayName: string }[] = [];
  selectedClass: string = '';

  constructor(private authService: MsalService,
              private msalBroadcastService: MsalBroadcastService,
              private http: HttpClient) { }

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.instance.initialize();
      this.activeAccount = this.authService.instance.getActiveAccount();
      if (!this.activeAccount && this.authService.instance.getAllAccounts().length > 0) {
        this.activeAccount = this.authService.instance.getAllAccounts()[0];
        this.authService.instance.setActiveAccount(this.activeAccount);
      }

      this.msalBroadcastService.msalSubject$
        .pipe(
          filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS)
        )
        .subscribe((result: EventMessage) => {
          console.log('Login success:', result);
          this.activeAccount = this.authService.instance.getAllAccounts()[0];
          this.authService.instance.setActiveAccount(this.activeAccount);
          this.setLoginDisplay();
          this.fetchClasses();
        });

      this.msalBroadcastService.inProgress$
        .pipe(
          filter((status: InteractionStatus) => status === InteractionStatus.None)
        )
        .subscribe(() => {
          this.setLoginDisplay();
        });

      this.setLoginDisplay();
      this.fetchClasses();
    } catch (error) {
      console.error('MSAL initialization error:', error);
    }
  }

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
    if (this.loginDisplay) {
      this.fetchClasses();
    }
  }

  fetchClasses(): void {
    if (!this.activeAccount) {
      console.log('No active account found.');
      return;
    }

    this.authService.acquireTokenSilent({
      account: this.activeAccount,
      scopes: ['User.Read', 'EduRoster.ReadBasic']
    }).subscribe({
      next: (result: AuthenticationResult) => {
        const headers = new HttpHeaders({
          'Authorization': `Bearer ${result.accessToken}`
        });

        this.http.get<GraphClassesResponse>('https://graph.microsoft.com/v1.0/education/classes', { headers })
          .subscribe(response => {
            console.log('Classes:', response);
            this.classes = response.value;
          });
      },
      error: (error) => {
        if (error.errorCode === 'interaction_in_progress') {
          console.log('Interaction in progress, cannot start a new one.');
          return;
        }
        console.error('Error acquiring token silently:', error);
        this.authService.acquireTokenRedirect({
          scopes: ['User.Read', 'EduRoster.ReadBasic']
        });
      }
    });
  }

  onClassSelected(event: any): void {
    const selectedClass = {
      id: event.target.value,
      displayName: this.classes.find(c => c.id === event.target.value)?.displayName
    };
    console.log('Selected class:', selectedClass.id, selectedClass.displayName);
    // You can perform any actions when a class is selected, such as storing it or using it further
  }

  onSelect(): void {
    // Action to perform when the select button is clicked
    // For example, navigate to a different component or perform an operation based on selected class
    console.log('Select button clicked');
    console.log('Selected class:', this.selectedClass);
  }
}
