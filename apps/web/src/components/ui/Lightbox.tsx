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
 * Accessible modal lightbox: traps focus, closes on Escape, navigates
 * with arrow keys, restores focus to whatever triggered it on close.
 * Rendered via a portal so it always sits above the rest of the page
 * regardless of where it's mounted in the tree.
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
      className="fixed inset-0 z-50 flex flex-col bg-[var(--color-canvas)]/98 backdrop-blur"
      onClick={onClose}
    >
      <div className="flex items-center justify-between px-6 py-4 sm:px-10">
        <DataLabel items={[`${activeIndex + 1} / ${photos.length}`]} />
        <button
          ref={closeButtonRef}
          type="button"
          onClick={onClose}
          className="data-label rounded-full border border-[var(--color-line)] px-4 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
        >
          Close (Esc)
        </button>
      </div>

      <div
        className="flex flex-1 justify-center overflow-auto px-4 pb-4"
        onClick={(e) => e.stopPropagation()}
      >
        <img
          src={photo.largeUrl}
          alt={photo.caption || photo.title}
          className="m-auto max-w-full object-contain"
        />
      </div>

      <div
        className="flex flex-col gap-2 border-t border-[var(--color-line)] px-6 py-4 sm:px-10"
        onClick={(e) => e.stopPropagation()}
      >
        <p className="text-[var(--color-ink)]">{photo.title}</p>
        {photo.caption && <p className="text-sm text-[var(--color-ink-muted)]">{photo.caption}</p>}
        <div className="flex flex-wrap items-center gap-4">
          {exif.length > 0 && <DataLabel items={exif} />}
          {camera && <p className="data-label text-[var(--color-ink-muted)]">{camera}</p>}
        </div>
      </div>

      {activeIndex > 0 && (
        <button
          type="button"
          aria-label="Previous photo"
          onClick={(e) => {
            e.stopPropagation()
            onNavigate(activeIndex - 1)
          }}
          className="absolute left-2 top-1/2 hidden -translate-y-1/2 rounded-full border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-3 text-[var(--color-ink)] hover:border-[var(--color-brass)] sm:block"
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
          className="absolute right-2 top-1/2 hidden -translate-y-1/2 rounded-full border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-3 text-[var(--color-ink)] hover:border-[var(--color-brass)] sm:block"
        >
          →
        </button>
      )}
    </div>,
    document.body,
  )
}
