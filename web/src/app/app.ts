import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Listing { id: number; title: string; price: number; area: number; }

@Component({
  selector: 'app-root',
  template: `
    <h1>Объявления</h1>

    @if (error()) {
      <p style="color:red">Ошибка: {{ error() }}</p>
    }

    @for (l of listings(); track l.id) {
      <div style="border:1px solid #ccc; padding:12px; margin:8px 0; border-radius:8px">
        <b>{{ l.title }}</b><br>
        {{ l.area }} м² — {{ l.price }} $
      </div>
    } @empty {
      <p>Загрузка…</p>
    }
  `,
})
export class App {
  private http = inject(HttpClient);
  listings = signal<Listing[]>([]);
  error = signal<string | null>(null);

  constructor() {
    this.http.get<Listing[]>('http://localhost:5000/api/listings')   // ← свой порт!
        .subscribe({
          next: data => this.listings.set(data),
          error: e => this.error.set(e.message),
        });
  }
}
