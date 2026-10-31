import type { Photo } from './types'

/**
 * Turns a Photo's raw EXIF fields into the short strings the
 * DataLabel component displays (e.g. "f/5.6", "1/320s", "ISO 400",
 * "210mm"). Missing fields are simply omitted rather than shown as
 * "N/A" - not every photo will have every field once film scans or
 * other sources are added later.
 */
export function formatExif(photo: Photo): string[] {
  const items: string[] = []
  if (photo.focalLengthMm) items.push(`${Math.round(photo.focalLengthMm)}mm`)
  if (photo.aperture) items.push(`f/${photo.aperture}`)
  if (photo.shutterSpeed) items.push(photo.shutterSpeed)
  if (photo.iso) items.push(`ISO ${photo.iso}`)
  return items
}

export function formatCamera(photo: Photo): string | null {
  if (!photo.cameraMake && !photo.cameraModel) return null
  return [photo.cameraMake, photo.cameraModel].filter(Boolean).join(' ')
}
