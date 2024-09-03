import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { HeaderComponent } from './header/header.component';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'googleToTeamMigration';
  constructor(private router: Router) { }
}
