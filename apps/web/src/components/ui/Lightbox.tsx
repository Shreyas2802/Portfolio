import { useEffect, useRef, useState } from 'react'
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
 * Accessible modal lightbox. Default view fits the photo entirely on
 * screen (object-contain within the available space) - this works
 * identically for portrait and landscape, which is why there's no
 * orientation switching needed. Clicking the photo toggles to a
 * full-resolution zoomed view that scrolls, for inspecting detail on
 * the high-resolution originals - the standard pattern used by
 * Google Photos/Unsplash/Behance, rather than always forcing a
 * scrollbar (which undercuts the clean presentation of the fit view).
 *
 * Traps initial focus, closes on Escape, navigates with arrow keys,
 * restores focus to whatever triggered it on close. Rendered via a
 * portal so it always sits above the rest of the page.
 */
export function Lightbox({ photos, activeIndex, onClose, onNavigate }: LightboxProps) {
  const closeButtonRef = useRef<HTMLButtonElement>(null)
  const previouslyFocused = useRef<HTMLElement | null>(null)
  const [isZoomed, setIsZoomed] = useState(false)
  const [lastIndex, setLastIndex] = useState(activeIndex)
  const photo = photos[activeIndex]

  // Reset zoom whenever the photo changes, so navigating never carries
  // a scrolled/zoomed state over to the next image. Adjusting state
  // during render (rather than in an effect) avoids an extra render
  // pass - see https://react.dev/learn/you-might-not-need-an-effect
  if (activeIndex !== lastIndex) {
    setLastIndex(activeIndex)
    setIsZoomed(false)
  }

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

      {/*
        min-h-0 is load-bearing here: without it, this flex-1 area's
        default min-height would grow to fit the image's intrinsic
        size instead of staying capped at the space actually available,
        which is what let tall portrait photos blow past the viewport
        with nothing to scroll. With it, the fit view has a real fixed
        height to contain against, and the zoomed view has a real fixed
        height to scroll within.
      */}
      <div
        className={`flex min-h-0 flex-1 items-center justify-center px-4 pb-4 ${isZoomed ? 'overflow-auto' : 'overflow-hidden'}`}
        onClick={(e) => e.stopPropagation()}
      >
        <button
          type="button"
          onClick={() => setIsZoomed((z) => !z)}
          aria-label={isZoomed ? 'Zoom out' : 'Zoom in to full resolution'}
          className={isZoomed ? 'cursor-zoom-out m-auto' : 'cursor-zoom-in'}
        >
          <img
            src={photo.largeUrl}
            alt={photo.caption || photo.title}
            className={isZoomed ? 'max-w-none' : 'max-h-full max-w-full object-contain'}
          />
        </button>
      </div>

      <div
        className="flex flex-col gap-2 border-t border-[var(--color-line)] px-6 py-4 sm:px-10"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-[var(--color-ink)]">{photo.title}</p>
            {photo.caption && <p className="text-sm text-[var(--color-ink-muted)]">{photo.caption}</p>}
          </div>
          <p className="data-label whitespace-nowrap text-[var(--color-ink-muted)]">
            {isZoomed ? 'Click photo to fit screen' : 'Click photo to zoom'}
          </p>
        </div>
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
