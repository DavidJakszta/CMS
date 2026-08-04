import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { Navbar } from './components/navbar/navbar';
import { Pagination } from './components/pagination/pagination';
import { Modal } from './components/modal/modal';
import { PriceFilter } from './components/price-filter/price-filter';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Profile } from './pages/profile/profile';
import { Users } from './pages/users/users';
import { Admin } from './pages/admin/admin';
import { Home } from './pages/home/home';
import { ProductList } from './pages/products/product-list';
import { ProductDetail } from './pages/products/product-detail';
import { ProductForm } from './pages/products/product-form';
import { UserProfile } from './pages/user-profile/user-profile';

@NgModule({
  declarations: [
    App,
    Navbar,
    Pagination,
    Modal,
    PriceFilter,
    Login,
    Register,
    Profile,
    Users,
    Admin,
    Home,
    ProductList,
    ProductDetail,
    ProductForm,
    UserProfile
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
