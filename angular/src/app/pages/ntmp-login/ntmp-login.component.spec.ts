import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NTMPLoginComponent } from './ntmp-login.component';

describe('NTMPLoginComponent', () => {
  let component: NTMPLoginComponent;
  let fixture: ComponentFixture<NTMPLoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NTMPLoginComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NTMPLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
