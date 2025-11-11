import { TestBed } from '@angular/core/testing';

import { FocusSessionService } from './focus-session.service';

describe('FocusSessionService', () => {
  let service: FocusSessionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FocusSessionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
