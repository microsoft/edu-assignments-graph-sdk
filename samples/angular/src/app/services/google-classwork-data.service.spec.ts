import { TestBed } from '@angular/core/testing';

import { GoogleClassworkDataService } from './google-classwork-data.service';

describe('GoogleClassworkDataService', () => {
  let service: GoogleClassworkDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GoogleClassworkDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
