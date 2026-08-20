import { useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import type { Photo } from '../../features/photography/types'
import { formatExif, formatCamera } from '../../features/photography/formatExif'
import { DataLabel } from './DataLabel'

interface LightboxProps {
  photos: Photo[]
  activeIndex: number
  onClose: () => void
  onNavigate: (index: number) => void
}

/**
 * Full-bleed modal lightbox. The photo fills the entire viewport
 * (object-contain against 100% width/height) - this is what makes
 * portrait and landscape both look immersive without any orientation
 * switching: a portrait photo naturally uses full height, a landscape
 * one naturally uses full width, edge to edge either way.
 *
 * All chrome (close button, counter, caption, EXIF) is an absolutely
 * positioned overlay with a gradient scrim, not separate header/footer
 * rows - rows would each claim a fixed slice of vertical space and
 * shrink the image itself, which is what made the previous version
 * feel cramped next to full-bleed references.
 *
 * Traps initial focus, closes on Escape, navigates with arrow keys,
 * restores focus to whatever triggered it on close. Rendered via a
 * portal so it always sits above the rest of the page.
 */
export function Lightbox({ photos, activeIndex, onClose, onNavigate }: LightboxProps) {
  const closeButtonRef = useRef<HTMLButtonElement>(null)
  const previouslyFocused = useRef<HTMLElement | null>(null)
  const photo = photos[activeIndex]

  useEffect(() => {
    previouslyFocused.current = document.activeElement as HTMLElement
    closeButtonRef.current?.focus()
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = ''
      previouslyFocused.current?.focus()
    }
  }, [])

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') {
        onClose()
      } else if (e.key === 'ArrowRight' && activeIndex < photos.length - 1) {
        onNavigate(activeIndex + 1)
      } else if (e.key === 'ArrowLeft' && activeIndex > 0) {
        onNavigate(activeIndex - 1)
      }
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [activeIndex, photos.length, onClose, onNavigate])

  if (!photo) return null

  const exif = formatExif(photo)
  const camera = formatCamera(photo)

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label={photo.title}
      className="fixed inset-0 z-50 bg-black"
      onClick={onClose}
    >
      {/* The photo itself - full-bleed, edge to edge on whichever
          dimension it's constrained by. */}
      <img
        src={photo.largeUrl}
        alt={photo.caption || photo.title}
        className="absolute inset-0 h-full w-full object-contain"
      />

      {/* Top scrim + controls */}
      <div
        className="absolute inset-x-0 top-0 flex items-center justify-between bg-gradient-to-b from-black/70 to-transparent px-6 py-4 sm:px-10"
        onClick={(e) => e.stopPropagation()}
      >
        <DataLabel items={[`${activeIndex + 1} / ${photos.length}`]} />
        <button
          ref={closeButtonRef}
          type="button"
          onClick={onClose}
          className="data-label rounded-full border border-white/20 px-4 py-2 text-white hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
        >
          Close (Esc)
        </button>
      </div>

      {/* Bottom scrim + caption/EXIF */}
      <div
        className="absolute inset-x-0 bottom-0 flex flex-wrap items-end justify-between gap-4 bg-gradient-to-t from-black/70 to-transparent px-6 py-4 sm:px-10"
        onClick={(e) => e.stopPropagation()}
      >
        <div>
          <p className="text-white">{photo.title}</p>
          {photo.caption && <p className="text-sm text-white/70">{photo.caption}</p>}
          <div className="mt-2 flex flex-wrap items-center gap-4">
            {exif.length > 0 && <DataLabel items={exif} />}
            {camera && <p className="data-label text-white/60">{camera}</p>}
          </div>
        </div>
        <a
          href={photo.largeUrl}
          target="_blank"
          rel="noreferrer"
          className="data-label whitespace-nowrap text-white/70 hover:text-[var(--color-brass)]"
        >
          View full resolution ↗
        </a>
      </div>

      {activeIndex > 0 && (
        <button
          type="button"
          aria-label="Previous photo"
          onClick={(e) => {
            e.stopPropagation()
            onNavigate(activeIndex - 1)
          }}
          className="absolute left-2 top-1/2 hidden -translate-y-1/2 rounded-full border border-white/20 bg-black/40 px-4 py-3 text-white hover:border-[var(--color-brass)] sm:block"
        >
          ←
        </button>
      )}
      {activeIndex < photos.length - 1 && (
        <button
          type="button"
          aria-label="Next photo"
          onClick={(e) => {
            e.stopPropagation()
            onNavigate(activeIndex + 1)
          }}
          className="absolute right-2 top-1/2 hidden -translate-y-1/2 rounded-full border border-white/20 bg-black/40 px-4 py-3 text-white hover:border-[var(--color-brass)] sm:block"
        >
          →
        </button>
      )}
    </div>,
    document.body,
  )
}
