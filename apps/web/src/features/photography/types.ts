export interface Photo {
  id: number
  title: string
  slug: string
  caption: string
  thumbnailUrl: string
  mediumUrl: string
  largeUrl: string
  width: number
  height: number
  takenAtUtc: string | null
  cameraMake: string | null
  cameraModel: string | null
  lensModel: string | null
  aperture: number | null
  shutterSpeed: string | null
  iso: number | null
  focalLengthMm: number | null
  tagNames: string[]
  isHeroPhoto: boolean
  /** Comma-separated hex colours e.g. "#1a2b3c,#d4a040,#8c7060" */
  accentPalette: string | null
}

export interface AlbumSummary {
  id: number
  title: string
  slug: string
  description: string
  categoryName: string | null
  coverThumbnailUrl: string | null
  photoCount: number
}

export interface AlbumDetail {
  id: number
  title: string
  slug: string
  description: string
  categoryName: string | null
  photoCount: number
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
