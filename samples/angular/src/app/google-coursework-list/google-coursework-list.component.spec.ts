import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoogleCourseworkListComponent } from './google-coursework-list.component';

describe('GoogleCourseworkListComponent', () => {
  let component: GoogleCourseworkListComponent;
  let fixture: ComponentFixture<GoogleCourseworkListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GoogleCourseworkListComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GoogleCourseworkListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
