import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Device } from './device';
import { describeDevice } from './device-state';

@Component({
  selector: 'app-device-banner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="banner" [attr.data-status]="device.state().status">
      {{ label() }}
    </div>
  `,
  styles: `
    .banner { padding: 16px; border-radius: 8px; font-size: 1.1rem; }
    [data-status='none']      { background: #fff8e1; }
    [data-status='scanning']  { background: #e8f0fe; }
    [data-status='connected'] { background: #e6f4ea; }
    [data-status='error']     { background: #fce8e6; }
  `,
})
export class DeviceBanner {
  protected readonly device = inject(Device);
  protected readonly label = computed(() => describeDevice(this.device.state()));
}