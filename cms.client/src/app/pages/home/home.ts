import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  standalone: false
})
export class Home {
  constructor(public auth: AuthService) {}
}
