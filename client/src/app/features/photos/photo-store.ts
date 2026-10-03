import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { KioskHub } from '../../core/kiosk-hub';
import { Device } from '../device/device';
import { Photo, isPhotoArray } from './photo';

@Injectable({ providedIn: 'root' })
export class PhotoStore {
  private readonly http = inject(HttpClient);
  private readonly hub = inject(KioskHub);
  private readonly device = inject(Device);

  private readonly _photos = signal<Photo[]>([]);
  private readonly _selected = signal<ReadonlySet<number>>(new Set());

  readonly photos = this._photos.asReadonly();
  readonly selectedCount = computed(() => this._selected().size);

  // Only changes when the status string changes, not on every "found" count update
  private readonly deviceStatus = computed(() => this.device.state().status);

  constructor() {
    // Fast path: batches stream in while the device is being scanned
    this.hub.on('PhotosAdded', isPhotoArray)
      .pipe(takeUntilDestroyed())
      .subscribe(batch => this.append(batch));

    // Correct path: when the scan settles, re-sync from the server
    effect(() => {
      const status = this.deviceStatus();
      untracked(() => {
        if (status === 'none') this.clear();
        if (status === 'connected') this.load();
      });
    });
  }

  isSelected(id: number): boolean {
    return this._selected().has(id);
  }

  load(): void {
    this.http.get<Photo[]>('/api/photos')
      .subscribe(photos => this._photos.set(photos));
  }

  toggle(id: number): void {
    this._selected.update(current => {
      const next = new Set(current);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  private append(batch: readonly Photo[]): void {
    this._photos.update(current => {
      const known = new Set(current.map(p => p.id));
      const fresh = batch.filter(p => !known.has(p.id));
      return fresh.length ? [...current, ...fresh] : current;
    });
  }

  // Device removed: wipe everything, including the selection
  private clear(): void {
    this._photos.set([]);
    this._selected.set(new Set());
  }
}