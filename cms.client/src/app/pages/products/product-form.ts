import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductRequest } from '../../models/product-request';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.html',
  standalone: false
})
export class ProductForm implements OnInit {
  model: ProductRequest = { name: '', price: 0, description: '', pictureUrl: '' };
  editingId: number | null = null;
  loading = false;
  saving = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.editingId = Number(idParam);
        this.loadExisting(this.editingId);
      }
    });
  }

  get isEditing(): boolean {
    return this.editingId !== null;
  }

  loadExisting(id: number): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.productService.getById(id).subscribe({
      next: product => {
        this.model = {
          name: product.name,
          price: product.price,
          description: product.description,
          pictureUrl: product.pictureUrl ?? ''
        };
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

  submit(): void {
    this.saving = true;
    this.error = '';
    this.cdr.detectChanges();
    const request: ProductRequest = {
      name: this.model.name,
      price: this.model.price,
      description: this.model.description,
      pictureUrl: this.model.pictureUrl || undefined
    };
    const target = this.isEditing
      ? this.productService.update(this.editingId!, request)
      : this.productService.create(request);

    target.subscribe({
      next: product => {
        this.saving = false;
        this.cdr.detectChanges();
        this.router.navigate(['/products', product.id]);
      },
      error: err => {
        this.saving = false;
        this.error = err.error?.error ?? 'Failed to save product.';
        this.cdr.detectChanges();
      }
    });
  }
}
