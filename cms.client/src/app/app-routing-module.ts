import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Profile } from './pages/profile/profile';
import { Users } from './pages/users/users';
import { UserProfile } from './pages/user-profile/user-profile';
import { ProductList } from './pages/products/product-list';
import { ProductDetail } from './pages/products/product-detail';
import { ProductForm } from './pages/products/product-form';
import { Admin } from './pages/admin/admin';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile, canActivate: [AuthGuard] },
  { path: 'users', component: Users },
  { path: 'users/:id', component: UserProfile },
  { path: 'products', component: ProductList },
  { path: 'products/new', component: ProductForm, canActivate: [AuthGuard] },
  { path: 'products/:id', component: ProductDetail },
  { path: 'products/:id/edit', component: ProductForm, canActivate: [AuthGuard] },
  { path: 'admin', component: Admin, canActivate: [AuthGuard, AdminGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
