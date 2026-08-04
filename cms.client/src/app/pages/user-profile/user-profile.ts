import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../../services/user.service';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';
import { UserResponse } from '../../models/user-response';
import { ProductResponse } from '../../models/product-response';
import { getInitials } from '../../utils/text.util';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.html',
  standalone: false
})
export class UserProfile implements OnInit {
  user: UserResponse | null = null;
  loading = false;
  error = '';
  userProducts: ProductResponse[] = [];
  productsLoading = false;
  showAllProducts = false;

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private productService: ProductService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) this.loadUser(id);
    });
  }

  get visibleProducts(): ProductResponse[] {
    return this.showAllProducts ? this.userProducts : this.userProducts.slice(0, 3);
  }

  avatarFor(name: string): string {
    return getInitials(name);
  }

  loadUser(id: number): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.userService.getById(id).subscribe({
      next: u => {
        this.user = u;
        this.loading = false;
        this.cdr.detectChanges();
        this.loadProducts(id);
      },
      error: () => {
        this.error = 'User not found.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadProducts(ownerId: number): void {
    this.productsLoading = true;
    this.cdr.detectChanges();
    this.productService.getAll().subscribe({
      next: products => {
        this.userProducts = products.filter(p => p.ownerId === ownerId);
        this.productsLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.productsLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get isSelf(): boolean {
    return this.user !== null && this.auth.getCurrentUserId() === this.user.id;
  }
}
