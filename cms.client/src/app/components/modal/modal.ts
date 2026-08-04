import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
  standalone: false
})
export class Modal {
  @Input() title = '';
  @Input() showClose = true;
  @Output() close = new EventEmitter<void>();

  constructor(private cdr: ChangeDetectorRef) {}

  closeModal(): void {
    this.close.emit();
    this.cdr.detectChanges();
  }
}
