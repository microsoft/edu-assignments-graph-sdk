import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GoogleClassworkDataService } from '../services/google-classwork-data.service';

@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html',
  styleUrls: ['./courses.component.scss']
})
export class CoursesComponent implements OnInit {
  selectedCourse: any;
  courses: any[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(private courseService: GoogleClassworkDataService, 
      private router: Router) { }

  ngOnInit(): void {
    this.fetchCourses();
  }

  fetchCourses(): void {
    this.loading = true; // Set loading to true before fetching courses
    this.courseService.fetchCourses()
      .subscribe(
        (response: any) => {
          this.courses = response.courses;
          this.loading = false; // Set loading to false after courses are fetched
          if (this.courses.length > 0) {
            this.selectedCourse = this.courses[0]; // Select the first course by default
          }
        },
        (error: any) => {
          this.error = error;
          this.loading = false; // Ensure loading is set to false in case of error
          console.error('Error fetching courses:', error);
        }
      );
  }

  onCourseChange(event: any): void {
    const selectedId = event.target.value;
    const selectedCourse = this.courses.find(course => course.id === selectedId);
    this.selectedCourse = selectedCourse;
  }

  onCourseSelect(): void {
    console.log('Course selected Successfully');
    this.router.navigate(['/coursework-material', this.selectedCourse?.id]);
  }
}
