import { Component, EventEmitter, Output } from '@angular/core';

export interface PriceRange {
  min: number | null;
  max: number | null;
}

@Component({
  selector: 'app-price-filter',
  templateUrl: './price-filter.html',
  standalone: false
})
export class PriceFilter {
  min: number | null = null;
  max: number | null = null;

  @Output() change = new EventEmitter<PriceRange>();

  apply(): void {
    this.change.emit({ min: this.normalize(this.min), max: this.normalize(this.max) });
  }

  reset(): void {
    this.min = null;
    this.max = null;
    this.change.emit({ min: null, max: null });
  }

  private normalize(value: number | null): number | null {
    if (value === null || value === undefined) return null;
    return Number.isFinite(value) && value >= 0 ? value : null;
  }
}
