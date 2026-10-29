/** Entspricht PersonListItem im Backend, Eigenschaften in camelCase. */
export interface Person {
  id: number;
  vorname: string;
  nachname: string;
  titel: string | null;
  fraktion: string;
}
