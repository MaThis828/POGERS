import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
  });

  it('zeigt die Personenliste', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController).expectOne(r => r.url === '/api/personen').flush([]);
    fixture.detectChanges();

    const ueberschrift = (fixture.nativeElement as HTMLElement).querySelector('h1')?.textContent;
    expect(ueberschrift).toContain('Abgeordnete der 21. Wahlperiode');
  });
});
