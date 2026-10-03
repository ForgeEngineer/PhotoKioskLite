import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { PhotoStore } from './photo-store';
import { Photo } from './photo';

const COLUMNS = 5;

@Component({
  selector: 'app-photo-grid',
  imports: [ScrollingModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header>{{ store.selectedCount() }} selected</header>

    <cdk-virtual-scroll-viewport itemSize="210" class="viewport">
      <div *cdkVirtualFor="let row of rows(); trackBy: trackRow" class="row">
        @for (photo of row; track photo.id) {
          <img [src]="photo.thumbUrl" width="200" height="200"
               loading="lazy" decoding="async"
               [class.selected]="store.isSelected(photo.id)"
               (click)="store.toggle(photo.id)" />
        }
      </div>
    </cdk-virtual-scroll-viewport>
  `,
  styles: `
    .viewport { height: 80vh; }
    .row { display: flex; gap: 10px; height: 210px; }
    img { border-radius: 8px; cursor: pointer; object-fit: cover; }
    .selected { outline: 4px solid #2f7cf6; }
  `,
})
export class PhotoGrid {
  protected readonly store = inject(PhotoStore);

  protected readonly rows = computed(() => {
    const photos = this.store.photos();
    const rows: Photo[][] = [];
    for (let i = 0; i < photos.length; i += COLUMNS) {
      rows.push(photos.slice(i, i + COLUMNS));
    }
    return rows;
  });

  protected trackRow = (_: number, row: Photo[]) => row[0].id;

  constructor() {
    this.store.load();
  }
}