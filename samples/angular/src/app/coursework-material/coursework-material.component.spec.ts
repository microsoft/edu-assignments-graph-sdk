import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CourseMaterialComponent } from './coursework-material.component';

describe('CourseworkMaterialComponent', () => {
  let component: CourseMaterialComponent;
  let fixture: ComponentFixture<CourseMaterialComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CourseMaterialComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CourseMaterialComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
