import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ScoreService } from './score-service';

describe('ScoreService', () => {
  let service: ScoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(ScoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
