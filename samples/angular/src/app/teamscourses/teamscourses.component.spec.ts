import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamscoursesComponent } from './teamscourses.component';

describe('TeamscoursesComponent', () => {
  let component: TeamscoursesComponent;
  let fixture: ComponentFixture<TeamscoursesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TeamscoursesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TeamscoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
