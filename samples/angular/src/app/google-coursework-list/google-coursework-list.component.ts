import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { GoogleClassworkDataService } from '../services/google-classwork-data.service';

@Component({
  selector: 'app-google-coursework-list',
  templateUrl: './google-coursework-list.component.html',
  styleUrls: ['./google-coursework-list.component.scss']
})
export class GoogleCourseworkListComponent implements OnInit {
  @Input() courseId: string = '';
  @Input() selectedMaterials: any[] = [];
  courseWorkList: any[] = [];
  selectedCheckboxes: string[] = [];
  loading: boolean = false;
  error: string = '';
  allCheckboxesSelected = false;

  constructor(
    private courseworkMaterialService: GoogleClassworkDataService,
    private route: ActivatedRoute,
    private router: Router,
    private msalService: MsalService, private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.courseId = this.route.snapshot.paramMap.get('id')!;
    this.courseworkMaterialService.fetchCourseWorkList(this.courseId).subscribe(
      (response: any) => {
        this.courseWorkList = response.courseWork;
        this.loading = false;
      },
      (error: any) => {
        this.error = error;
        this.loading = false;
      }
    );
  }

  submitSelections(): void {
    const selectedItems = this.courseWorkList.filter(item =>
      this.selectedCheckboxes.includes(item.id)
    );
    console.log('Course Works from the List selected successfully!');
    sessionStorage.setItem('selectedCourseWorkList', JSON.stringify(selectedItems));

    this.router.navigate(['teamsclasses'])
  }

  selectAll(event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedCheckboxes = this.courseWorkList.map(item => item.id);
    } else {
      this.selectedCheckboxes = [];
    }
  }

  toggleCheckbox(id: string): void {
    if (this.selectedCheckboxes.includes(id)) {
      this.selectedCheckboxes = this.selectedCheckboxes.filter(item => item !== id);
    } else {
      this.selectedCheckboxes.push(id);
    }
    this.updateSelectAllCheckbox();
  }

  toggleSelectAll(): void {
    this.allCheckboxesSelected = !this.allCheckboxesSelected;
    if (this.allCheckboxesSelected) {
      this.selectedCheckboxes = this.courseWorkList.map(item => item.id);
    } else {
      this.selectedCheckboxes = [];
    }
  }

  updateSelectAllCheckbox(): void {
    this.allCheckboxesSelected = this.selectedCheckboxes.length === this.courseWorkList.length;
  }

  isAllSelected(){
    return this.courseWorkList.every(course => this.isTitleSelected(course.id));
  }

  isTitleSelected(materialId: string): boolean {
    return this.selectedCheckboxes.includes(materialId);
  }
}
