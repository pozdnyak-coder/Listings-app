// Точная копия ListingResponse из API (camelCase, createdAt — ISO-строка)
export interface Listing {
  id: number;
  title: string;
  price: number;
  rooms: number;
  districtId: number;
  districtName: string;
  address?: string | null;
  createdAt: string;
}

export interface ListingFilter {
  districtId: number | null;   // null = все районы
  maxPrice: number | null;     // null = без ограничения
}
