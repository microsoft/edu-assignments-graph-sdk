import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { GoogleClassworkDataService } from '../services/google-classwork-data.service';

@Component({
  selector: 'app-coursework-material',
  templateUrl: './coursework-material.component.html',
  styleUrls: ['./coursework-material.component.scss'],
})
export class CourseMaterialComponent implements OnInit {
  @Input() courseId: string = '';
  courseMaterials: any[] = [];
  error: string = '';
  selectedMaterials: any[] = [];
  loading: boolean = false;
  loginDisplay = false;
  allCheckboxesSelected = false;

  constructor(
    private courseMaterialService: GoogleClassworkDataService, 
    private route: ActivatedRoute, 
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loading = true;
    this.courseId = this.route.snapshot.paramMap.get('id')!;
    this.courseMaterialService.fetchCourseWorkMaterials(this.courseId).subscribe(
      (response: any) => {
        this.courseMaterials = response.courseWorkMaterial;
        this.loading = false;
      },
      (error: any) => {
        this.error = error;
        this.loading = false;
      }
    );
  }

  toggleSelectAll(): void {
    this.allCheckboxesSelected = !this.allCheckboxesSelected;
    if (this.allCheckboxesSelected) {
      this.selectedMaterials = [...this.courseMaterials];
    } else {
      this.selectedMaterials = [];
    }
  }

  toggleCheckbox(material: any): void {
    const index = this.selectedMaterials.findIndex((item) => item.id === material.id);
    if (index !== -1) {
      this.selectedMaterials.splice(index, 1);
    } else {
      this.selectedMaterials.push(material);
    }
    this.updateSelectAllCheckbox();
  }

  updateSelectAllCheckbox(): void {
    this.allCheckboxesSelected = this.selectedMaterials.length === this.courseMaterials.length;
  }

  isAllSelected(): boolean {
    return this.courseMaterials.every((course) => this.isMaterialSelected(course.id));
  }

  isMaterialSelected(materialId: string): boolean {
    return this.selectedMaterials.some((material) => material.id === materialId);
  }

  submitSelections(): void {
    console.log('Course Work Material selected successfully!');
    sessionStorage.setItem('selectedCourseMaterials', JSON.stringify(this.selectedMaterials));
    this.router.navigate(['/google-coursework-list', this.courseId]);
  }
}
