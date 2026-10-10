import { Injectable, inject, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Listing } from './models/listing';

export interface District { id: number; name: string; listingsCount: number; }

@Injectable({ providedIn: 'root' })
export class ListingsApi {
  private http = inject(HttpClient);

  // При ng serve API живёт на :5000. На сервере фронтенд и API — на одном адресе.
  private base = isDevMode() ? 'http://localhost:5000/api' : '/api';

  getListings() {
    return this.http.get<Listing[]>(`${this.base}/listings`);
  }

  getDistricts() {
    return this.http.get<District[]>(`${this.base}/districts`);
  }
}
