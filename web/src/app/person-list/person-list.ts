import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Person } from '../person';
import { PersonService } from '../person.service';

@Component({
  selector: 'app-person-list',
  imports: [FormsModule],
  templateUrl: './person-list.html',
  styleUrl: './person-list.css'
})
export class PersonList implements OnInit {
  private readonly service = inject(PersonService);

  readonly wahlperiode = 21;
  readonly personen = signal<Person[]>([]);
  readonly fehler = signal<string | null>(null);
  readonly laedt = signal(false);
  readonly gewaehlteFraktion = signal('');

  // Fraktionen aus den geladenen Daten statt fester Liste
  readonly fraktionen = computed(() => [...new Set(this.personen().map(p => p.fraktion))].sort());

  // Gefiltert wird im Browser, die Liste ist mit rund 600 Personen klein genug
  readonly sichtbarePersonen = computed(() => {
    const gewaehlt = this.gewaehlteFraktion();
    return gewaehlt === '' ? this.personen() : this.personen().filter(p => p.fraktion === gewaehlt);
  });

  ngOnInit(): void {
    this.laedt.set(true);

    this.service.getPersonen(this.wahlperiode).subscribe({
      next: personen => {
        this.personen.set(personen);
        this.laedt.set(false);
      },
      error: () => {
        this.fehler.set('Die Personen konnten nicht geladen werden. Laeuft die Api auf Port 5080?');
        this.laedt.set(false);
      }
    });
  }
}
