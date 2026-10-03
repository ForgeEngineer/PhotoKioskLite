import { Component } from '@angular/core';
import { DeviceBanner } from './features/device/device-banner';
import { PhotoGrid } from './features/photos/photo-grid';
import { PrinterStatus } from './features/printer/printer-status';

@Component({
  selector: 'app-root',
  imports: [DeviceBanner, PhotoGrid, PrinterStatus],
  template: `
    <main>
      <app-device-banner />
      <app-printer-status />
      <app-photo-grid />
    </main>
  `,
  styles: `
    main { padding: 16px; display: grid; gap: 16px; }
  `,
})
export class App {}