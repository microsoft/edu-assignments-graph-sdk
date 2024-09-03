import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MigratedDataComponent } from './migrated-data.component';

describe('MigratedDataComponent', () => {
  let component: MigratedDataComponent;
  let fixture: ComponentFixture<MigratedDataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MigratedDataComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MigratedDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
