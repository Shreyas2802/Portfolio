import { useCallback, useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import type { Photo } from '../../features/photography/types'
import { formatExif, formatCamera } from '../../features/photography/formatExif'
import { useState } from 'react'

interface LightboxProps {
  photos: Photo[]
  activeIndex: number
  onClose: () => void
  onNavigate: (index: number) => void
}

// All focusable element types inside the lightbox
const FOCUSABLE = [
  'a[href]',
  'button:not([disabled])',
  'textarea',
  'input',
  'select',
  '[tabindex]:not([tabindex="-1"])',
].join(',')

export function Lightbox({ photos, activeIndex, onClose, onNavigate }: LightboxProps) {
  const dialogRef = useRef<HTMLDivElement>(null)
  const closeButtonRef = useRef<HTMLButtonElement>(null)
  const previouslyFocused = useRef<HTMLElement | null>(null)
  const [isFilled, setIsFilled] = useState(false)
  const [lastIndex, setLastIndex] = useState(activeIndex)
  const [isBrowserFullscreen, setIsBrowserFullscreen] = useState(false)

  if (activeIndex !== lastIndex) {
    setLastIndex(activeIndex)
    setIsFilled(false)
  }

  // ── Focus trap ───────────────────────────────────────────────────
  const trapFocus = useCallback((e: KeyboardEvent) => {
    if (e.key !== 'Tab' || !dialogRef.current) return
    const focusable = Array.from(
      dialogRef.current.querySelectorAll<HTMLElement>(FOCUSABLE)
    ).filter((el) => !el.closest('[aria-hidden="true"]'))
    if (focusable.length === 0) return
    const first = focusable[0]
    const last  = focusable[focusable.length - 1]
    if (e.shiftKey) {
      if (document.activeElement === first) { e.preventDefault(); last.focus() }
    } else {
      if (document.activeElement === last) { e.preventDefault(); first.focus() }
    }
  }, [])

  // ── Keyboard navigation ──────────────────────────────────────────
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
      trapFocus(e)
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
  }, [activeIndex, photos.length, onClose, onNavigate, trapFocus])

  // ── Fullscreen detection ─────────────────────────────────────────
  useEffect(() => {
    function checkFullscreen() {
      setIsBrowserFullscreen(
        window.innerWidth === window.screen.width &&
        window.innerHeight === window.screen.height,
      )
    }
    checkFullscreen()
    window.addEventListener('resize', checkFullscreen)
    return () => window.removeEventListener('resize', checkFullscreen)
  }, [])

  const photo = photos[activeIndex]
  if (!photo) return null

  const exif   = formatExif(photo)
  const camera = formatCamera(photo)

  return createPortal(
    <div
      ref={dialogRef}
      role="dialog"
      aria-modal="true"
      aria-label={photo.title}
      className="fixed inset-0 z-50 bg-black"
      // Backdrop click closes — but NOT if click originated inside
      onClick={(e) => { if (e.target === e.currentTarget) onClose() }}
    >
      {/* Photo — click toggles fit/fill. stopPropagation prevents
          the backdrop handler from also firing. */}
      <button
        type="button"
        onClick={(e) => { e.stopPropagation(); setIsFilled((f) => !f) }}
        aria-label={isFilled ? 'Fit photo to screen' : 'Fill screen with photo'}
        className="absolute inset-0 cursor-zoom-in"
      >
        <img
          src={photo.largeUrl}
          alt={photo.caption || photo.title}
          className={`h-full w-full ${isFilled ? 'object-cover' : 'object-contain'}`}
        />
      </button>

      {/* Top bar */}
      <div
        className="absolute inset-x-0 top-0 flex items-center justify-between bg-gradient-to-b from-black/70 to-transparent px-6 py-4 sm:px-10"
        onClick={(e) => e.stopPropagation()}
      >
        <span className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-white/60">
          {activeIndex + 1} / {photos.length}
        </span>
        <button
          ref={closeButtonRef}
          type="button"
          onClick={onClose}
          className="rounded-full border border-white/20 px-4 py-2 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-white transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
        >
          Close (Esc)
        </button>
      </div>

      {/* Bottom bar */}
      {!isBrowserFullscreen && (
        <div
          className="absolute inset-x-0 bottom-0 flex flex-wrap items-end justify-between gap-4 bg-gradient-to-t from-black/70 to-transparent px-6 py-4 sm:px-10"
          onClick={(e) => e.stopPropagation()}
        >
          <div>
            <p className="font-medium text-white">{photo.title}</p>
            {photo.caption && (
              <p className="mt-0.5 text-sm text-white/70">{photo.caption}</p>
            )}
            {/* EXIF — explicit white/60 instead of data-label class
                which would apply ink-muted — fails contrast on black */}
            {(exif.length > 0 || camera) && (
              <div className="mt-2 flex flex-wrap items-center gap-4">
                {exif.length > 0 && (
                  <span className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-white/65">
                    {exif.join('  ·  ')}
                  </span>
                )}
                {camera && (
                  <span className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-white/65">
                    {camera}
                  </span>
                )}
              </div>
            )}
          </div>
          <div className="flex flex-col items-end gap-1.5">
            <p className="font-mono text-[0.5625rem] uppercase tracking-widest text-white/40">
              {isFilled ? 'Click to fit' : 'Click to fill'}
            </p>
            <a
              href={photo.largeUrl}
              target="_blank"
              rel="noreferrer"
              onClick={(e) => e.stopPropagation()}
              className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-white/65 transition-colors hover:text-[var(--color-gold)]"
            >
              Full resolution ↗
            </a>
          </div>
        </div>
      )}

      {/* Prev/Next — visible on ALL screen sizes (was hidden on mobile) */}
      {activeIndex > 0 && (
        <button
          type="button"
          aria-label="Previous photo"
          onClick={(e) => { e.stopPropagation(); onNavigate(activeIndex - 1) }}
          className="absolute left-2 top-1/2 -translate-y-1/2 rounded-full border border-white/20 bg-black/50 px-4 py-3 text-white transition-colors hover:border-[var(--color-gold)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white sm:left-4"
        >
          ←
        </button>
      )}
      {activeIndex < photos.length - 1 && (
        <button
          type="button"
          aria-label="Next photo"
          onClick={(e) => { e.stopPropagation(); onNavigate(activeIndex + 1) }}
          className="absolute right-2 top-1/2 -translate-y-1/2 rounded-full border border-white/20 bg-black/50 px-4 py-3 text-white transition-colors hover:border-[var(--color-gold)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white sm:right-4"
        >
          →
        </button>
      )}
    </div>,
    document.body,
  )
}
