import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamsclassesComponent } from './teamsclasses.component';

describe('TeamsclassesComponent', () => {
  let component: TeamsclassesComponent;
  let fixture: ComponentFixture<TeamsclassesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TeamsclassesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TeamsclassesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
