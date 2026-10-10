import { Component, computed, inject, signal } from '@angular/core';
import { Listing, ListingFilter as FilterValue } from './models/listing';
import { ListingsApi } from './listings-api';
import { ListingFilter, DistrictOption } from './listing-filter/listing-filter';
import { ListingList } from './listing-list/listing-list';
import { ListingDetails } from './listing-details/listing-details';

@Component({
  selector: 'app-root',
  imports: [ListingFilter, ListingList, ListingDetails],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private api = inject(ListingsApi);

  listings = signal<Listing[]>([]);
  districts = signal<DistrictOption[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  filter = signal<FilterValue>({ districtId: null, maxPrice: null });
  selectedId = signal<number | null>(null);

  filtered = computed(() => {
    const { districtId, maxPrice } = this.filter();
    return this.listings().filter(l =>
      (districtId === null || l.districtId === districtId) &&
      (maxPrice === null || l.price <= maxPrice));
  });

  selectedListing = computed(() =>
    this.listings().find(l => l.id === this.selectedId()) ?? null);

  constructor() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set(null);

    this.api.getListings().subscribe({
      next: data => { this.listings.set(data); this.loading.set(false); },
      error: err => { this.error.set('Сервер недоступен: ' + err.message); this.loading.set(false); },
    });

    this.api.getDistricts().subscribe({
      next: data => this.districts.set(data),
      error: () => { /* ошибку покажет загрузка списка */ },
    });
  }

  onFilter(value: FilterValue) {
    this.filter.set(value);
    this.selectedId.set(null);        // выбранное могло исчезнуть из списка
  }

  onSelect(id: number) {
    this.selectedId.update(current => current === id ? null : id);   // повторный клик снимает выбор
  }
}
