import { Injectable, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { KioskHub } from '../../core/kiosk-hub';
import { DeviceState, isDeviceState } from './device-state';

@Injectable({ providedIn: 'root' })
export class Device {
  private readonly hub = inject(KioskHub);

  private readonly last = toSignal(
    this.hub.on('DeviceStateChanged', isDeviceState),
    { initialValue: null },
  );

  readonly state = computed<DeviceState>(() => this.last() ?? { status: 'none' });
}