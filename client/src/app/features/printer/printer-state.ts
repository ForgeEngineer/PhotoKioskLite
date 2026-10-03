import { assertNever } from '../../core/assert-never';

export type PrinterState =
  | { readonly status: 'offline' }
  | { readonly status: 'ready' }
  | { readonly status: 'printing'; readonly jobId: string; readonly progress: number }
  | { readonly status: 'error'; readonly code: string; readonly message: string };

export function describe(state: PrinterState): string {
  switch (state.status) {
    case 'offline':  return 'Printer offline';
    case 'ready':    return 'Ready to print';
    case 'printing': return `Printing… ${state.progress}%`;
    case 'error':    return `${state.code}: ${state.message}`;
    default:         return assertNever(state);
  }
}

export function isPrinterState(value: unknown): value is PrinterState {
  if (typeof value !== 'object' || value === null) return false;
  const status = (value as Record<string, unknown>)['status'];
  return status === 'ready' || status === 'printing' || status === 'error';
}