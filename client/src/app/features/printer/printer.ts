import { Injectable, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { KioskHub } from '../../core/kiosk-hub';
import { PrinterState, isPrinterState } from './printer-state';

@Injectable({ providedIn: 'root' })
export class Printer {
  private readonly hub = inject(KioskHub);

  private readonly last = toSignal(
    this.hub.on('PrinterStateChanged', isPrinterState),
    { initialValue: null },
  );

  readonly state = computed<PrinterState>(() => {
    const last = this.last();
    return this.hub.connected() && last ? last : { status: 'offline' };
  });
}