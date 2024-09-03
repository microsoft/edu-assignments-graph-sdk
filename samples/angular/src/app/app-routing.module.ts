import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { CoursesComponent } from './courses/courses.component';
import { CourseMaterialComponent } from './coursework-material/coursework-material.component';
import { TeamscoursesComponent } from './teamscourses/teamscourses.component';
import { GoogleCourseworkListComponent } from './google-coursework-list/google-coursework-list.component';
import { TeamsclassesComponent } from './teamsclasses/teamsclasses.component';
import { GoogleLoginComponent } from './google-login/google-login.component';
import { MigratedDataComponent } from './migrated-data/migrated-data.component';

const routes: Routes = [
  { path: '', component: LoginComponent},
  { path: 'login', component: LoginComponent},
  { path: 'google-login', component: GoogleLoginComponent},
  { path: 'courses', component: CoursesComponent},
  { path: 'coursework-material/:id', component: CourseMaterialComponent},
  { path: 'teamscourses', component: TeamscoursesComponent},
  { path: 'google-coursework-list/:id', component: GoogleCourseworkListComponent},
  { path: 'teamsclasses', component: TeamsclassesComponent},
  { path: 'migrated-data', component: MigratedDataComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
