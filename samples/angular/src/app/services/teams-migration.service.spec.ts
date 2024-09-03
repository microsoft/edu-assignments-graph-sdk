import { TestBed } from '@angular/core/testing';

import { TeamsMigrationService } from './teams-migration.service';

describe('TeamsMigrationService', () => {
  let service: TeamsMigrationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TeamsMigrationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
