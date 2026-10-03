import { assertNever } from '../../core/assert-never';

export type DeviceState =
  | { readonly status: 'none' }
  | { readonly status: 'scanning'; readonly label: string; readonly found: number }
  | { readonly status: 'connected'; readonly label: string; readonly photoCount: number }
  | { readonly status: 'error'; readonly message: string };

export function describeDevice(state: DeviceState): string {
  switch (state.status) {
    case 'none':      return 'Insert a USB drive to get started';
    case 'scanning':  return `Reading ${state.label}… ${state.found.toLocaleString()} photos found`;
    case 'connected': return `${state.label} · ${state.photoCount.toLocaleString()} photos`;
    case 'error':     return state.message;
    default:          return assertNever(state);
  }
}

export function isDeviceState(value: unknown): value is DeviceState {
  if (typeof value !== 'object' || value === null) return false;
  const status = (value as Record<string, unknown>)['status'];
  return status === 'none' || status === 'scanning' || status === 'connected' || status === 'error';
}