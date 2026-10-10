import { Component, computed, input, output } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Listing } from '../models/listing';

@Component({
  selector: 'app-listing-card',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './listing-card.html',
  styleUrl: './listing-card.css',
})
export class ListingCard {
  listing = input.required<Listing>();
  active = input(false);
  selected = output<number>();

  isCheap = computed(() => this.listing().price < 40000);
  isNew = computed(() => Date.now() - new Date(this.listing().createdAt).getTime() < 7 * 24 * 3600 * 1000);
  roomsLabel = computed(() => {
    const r = this.listing().rooms;
    return r === 1 ? 'студия' : `${r} комн.`;
  });

  choose() {
    this.selected.emit(this.listing().id);
  }
}
