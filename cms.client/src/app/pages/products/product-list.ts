import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';
import { ProductResponse } from '../../models/product-response';
import { PriceRange } from '../../components/price-filter/price-filter';

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

  applyFilter(): void {
    const q = this.search.trim().toLowerCase();
    const { min, max } = this.priceRange;
    this.filteredProducts = this.products.filter(p => {
      const matchesSearch =
        !q ||
        p.name.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q);
      const matchesMin = min === null || p.price >= min;
      const matchesMax = max === null || p.price <= max;
      return matchesSearch && matchesMin && matchesMax;
    });
    this.currentPage = 1;
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
