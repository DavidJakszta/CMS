import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';
import { ProductResponse } from '../../models/product-response';
import { PriceRange } from '../../components/price-filter/price-filter';

type SortOption = 'random' | 'price-asc' | 'price-desc' | 'name-asc' | 'name-desc';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.html',
  standalone: false
})
export class ProductList implements OnInit {
  products: ProductResponse[] = [];
  filteredProducts: ProductResponse[] = [];
  search = '';
  priceRange: PriceRange = { min: null, max: null };
  sortOption: SortOption = 'random';
  loading = false;
  error = '';
  pageSize = 10;
  currentPage = 1;

  constructor(
    private productService: ProductService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  get pagedProducts(): ProductResponse[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredProducts.slice(start, start + this.pageSize);
  }

  loadProducts(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.productService.getAll().subscribe({
      next: data => {
        this.products = data;
        this.shuffleProducts();
        this.applyFilter();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load products.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  shuffleProducts(): void {
    const arr = this.products;
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
  }

  applyFilter(): void {
    const q = this.search.trim().toLowerCase();
    const min = this.priceRange.min ?? 0;
    const max = this.priceRange.max ?? Infinity;
    this.filteredProducts = this.products.filter(p => {
      const matchesSearch =
        !q ||
        p.name.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q);
      return matchesSearch && p.price >= min && p.price <= max;
    });
    if (this.sortOption !== 'random') {
      this.filteredProducts = this.sortProducts(this.filteredProducts);
    }
    this.currentPage = 1;
  }

  sortProducts(list: ProductResponse[]): ProductResponse[] {
    const sorted = [...list];
    switch (this.sortOption) {
      case 'price-asc': sorted.sort((a, b) => a.price - b.price); break;
      case 'price-desc': sorted.sort((a, b) => b.price - a.price); break;
      case 'name-asc': sorted.sort((a, b) => a.name.localeCompare(b.name)); break;
      case 'name-desc': sorted.sort((a, b) => b.name.localeCompare(a.name)); break;
      default: break;
    }
    return sorted;
  }

  onSortChange(option: SortOption): void {
    this.sortOption = option;
    if (option === 'random') {
      this.shuffleProducts();
    }
    this.applyFilter();
    this.cdr.detectChanges();
  }

  onSearchChange(): void {
    this.applyFilter();
    this.cdr.detectChanges();
  }

  onPriceChange(range: PriceRange): void {
    this.priceRange = range;
    this.applyFilter();
    this.cdr.detectChanges();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.cdr.detectChanges();
  }
}
