export interface Photo {
  readonly id: number;
  readonly thumbUrl: string;
}

export function isPhoto(value: unknown): value is Photo {
  if (typeof value !== 'object' || value === null) return false;
  const v = value as Record<string, unknown>;
  return typeof v['id'] === 'number' && typeof v['thumbUrl'] === 'string';
}

export function isPhotoArray(value: unknown): value is Photo[] {
  return Array.isArray(value) && value.every(isPhoto);
}