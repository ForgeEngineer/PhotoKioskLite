import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Printer } from './printer';
import { describe } from './printer-state';

@Component({
  selector: 'app-printer-status',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let s = printer.state();
    <div class="status" [attr.data-status]="s.status">
      @if (s.status === 'printing') {
        <progress max="100" [value]="s.progress"></progress>
      }
      <span>{{ label() }}</span>
    </div>
  `,
  styles: `
    .status { display: flex; gap: 12px; align-items: center; padding: 12px; border-radius: 8px; }
    [data-status='offline']  { background: #eeeeee; }
    [data-status='ready']    { background: #e6f4ea; }
    [data-status='printing'] { background: #e8f0fe; }
    [data-status='error']    { background: #fce8e6; }
  `,
})
export class PrinterStatus {
  protected readonly printer = inject(Printer);
  protected readonly label = computed(() => describe(this.printer.state()));
}