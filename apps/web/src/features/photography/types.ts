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
  photos: Photo[]
}
