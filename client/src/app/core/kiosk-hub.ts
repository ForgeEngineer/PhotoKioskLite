import { Injectable, signal } from '@angular/core';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class KioskHub {
  private readonly connection = new HubConnectionBuilder()
    .withUrl('/hubs/kiosk')
    .withAutomaticReconnect()
    .build();

  readonly connected = signal(false);

  constructor() {
    this.connection.onreconnecting(() => this.connected.set(false));
    this.connection.onreconnected(() => this.connected.set(true));
    this.connection.onclose(() => this.connected.set(false));
    void this.start();
  }

  on<T>(method: string, guard: (value: unknown) => value is T): Observable<T> {
    return new Observable<T>(subscriber => {
      const handler = (value: unknown) => {
        if (guard(value)) subscriber.next(value);
        else console.warn(`Invalid payload for ${method}`, value);
      };
      this.connection.on(method, handler);
      return () => this.connection.off(method, handler);   // teardown: no leaked handlers
    });
  }

  // withAutomaticReconnect only covers drops AFTER a successful start.
  // A kiosk may boot before the API is up, so retry the first connect ourselves.
  private async start(): Promise<void> {
    try {
      await this.connection.start();
      this.connected.set(true);
    } catch {
      setTimeout(() => void this.start(), 2000);
    }
  }
}