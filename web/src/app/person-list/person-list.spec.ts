import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PersonList } from './person-list';

describe('PersonList', () => {
  let fixture: ComponentFixture<PersonList>;
  let httpMock: HttpTestingController;

  const scholz = { id: 1, vorname: 'Olaf', nachname: 'Scholz', titel: null, fraktion: 'SPD' };
  const merz = { id: 2, vorname: 'Friedrich', nachname: 'Merz', titel: 'Dr.', fraktion: 'CDU/CSU' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonList],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    fixture = TestBed.createComponent(PersonList);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('fragt die Personen der 21. Wahlperiode an', () => {
    fixture.detectChanges();

    const anfrage = httpMock.expectOne(r => r.url === '/api/personen');
    expect(anfrage.request.params.get('wahlperiode')).toBe('21');
    anfrage.flush([scholz]);

    expect(fixture.componentInstance.personen()).toEqual([scholz]);
  });

  it('leitet die Fraktionen sortiert aus den Daten ab', () => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url === '/api/personen').flush([scholz, merz, { ...scholz, id: 3 }]);

    expect(fixture.componentInstance.fraktionen()).toEqual(['CDU/CSU', 'SPD']);
  });

  it('filtert nach der gewaehlten Fraktion', () => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url === '/api/personen').flush([scholz, merz]);

    fixture.componentInstance.gewaehlteFraktion.set('SPD');

    expect(fixture.componentInstance.sichtbarePersonen()).toEqual([scholz]);
  });

  it('zeigt eine Meldung, wenn die Api nicht erreichbar ist', () => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url === '/api/personen').error(new ProgressEvent('error'));
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).querySelector('.fehler')?.textContent;
    expect(text).toContain('Port 5080');
  });
});
