import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Person } from './person';

@Injectable({ providedIn: 'root' })
export class PersonService {
  private readonly http = inject(HttpClient);

  getPersonen(wahlperiode: number): Observable<Person[]> {
    const params = new HttpParams().set('wahlperiode', wahlperiode);
    return this.http.get<Person[]>('/api/personen', { params });
  }
}
