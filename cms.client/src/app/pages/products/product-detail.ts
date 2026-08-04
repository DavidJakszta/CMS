import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';
import { ProductResponse } from '../../models/product-response';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.html',
  standalone: false
})
export class ProductDetail implements OnInit {
  product: ProductResponse | null = null;
  loading = false;
  error = '';
  demoAction: 'message' | 'buy' | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) this.loadProduct(id);
    });
  }

  loadProduct(id: number): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.productService.getById(id).subscribe({
      next: data => {
        this.product = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Product not found.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get canModify(): boolean {
    if (!this.product) return false;
    return this.auth.isAdmin() || this.auth.getCurrentUserId() === this.product.ownerId;
  }

  get demoRequiresAuth(): boolean {
    return !this.auth.isAuthenticated();
  }

  runDemo(action: 'message' | 'buy'): void {
    this.demoAction = action;
    this.cdr.detectChanges();
  }

  closeDemo(): void {
    this.demoAction = null;
    this.cdr.detectChanges();
  }

  delete(): void {
    if (!this.product) return;
    if (!confirm(`Delete product "${this.product.name}"? This cannot be undone.`)) return;
    this.productService.delete(this.product.id).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => {
        this.error = 'Failed to delete product.';
        this.cdr.detectChanges();
      }
    });
  }
}
