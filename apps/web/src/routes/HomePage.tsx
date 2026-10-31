import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useHeroPhoto, useFeaturedPhotos } from '../features/photography/usePhotography'

export function HomePage() {
  const { data: hero } = useHeroPhoto()
  const { data: featured, isLoading: featuredLoading } = useFeaturedPhotos(6)
  const [photoLoaded, setPhotoLoaded] = useState(false)

  // Apply accent palette as CSS vars
  useEffect(() => {
    if (!hero?.accentPalette) return
    const colours = hero.accentPalette.split(',').map((c) => c.trim()).filter(Boolean)
    if (!colours[0]) return
    const root = document.documentElement
    root.style.setProperty('--color-gold',      colours[0])
    root.style.setProperty('--color-brass',     colours[0])
    root.style.setProperty('--color-gold-dim',  darkenHex(colours[0], 0.82))
    root.style.setProperty('--color-brass-dim', darkenHex(colours[0], 0.82))
    const rgb = hexToRgb(colours[0])
    if (rgb) root.style.setProperty('--color-gold-rgb', `${rgb.r},${rgb.g},${rgb.b}`)
    return () => {
      const r = document.documentElement
      r.style.removeProperty('--color-gold')
      r.style.removeProperty('--color-gold-dim')
      r.style.removeProperty('--color-brass')
      r.style.removeProperty('--color-brass-dim')
      r.style.removeProperty('--color-gold-rgb')
    }
  }, [hero?.accentPalette])

  const heroBgUrl  = hero?.largeUrl ?? hero?.mediumUrl ?? null
  const textOnHero = photoLoaded

  return (
    <>
      {/* ── Hero ───────────────────────────────────────────────────── */}
      <section aria-label="Introduction" className="hero-section flex min-h-[100svh] flex-col justify-end">

        {heroBgUrl && (
          <img
            src={heroBgUrl}
            alt=""
            aria-hidden="true"
            fetchPriority="high"
            width={hero?.width ?? 2400}
            height={hero?.height ?? 1600}
            onLoad={() => setPhotoLoaded(true)}
            className="absolute inset-0 h-full w-full object-cover object-center"
            style={{
              opacity: photoLoaded ? 1 : 0,
              transition: 'opacity 1.4s ease',
              animation: photoLoaded ? 'heroZoom 18s ease-out forwards' : 'none',
            }}
          />
        )}

        {photoLoaded && <div className="hero-overlay" aria-hidden="true" />}

        <div className="hero-content relative z-10 mx-auto w-full max-w-[1600px] px-6 pb-16 pt-40 sm:px-10 sm:pb-24">
          <div
            className="flex flex-wrap items-center gap-x-3 gap-y-1 font-mono text-[0.625rem] uppercase tracking-[0.12em]"
            style={{ color: textOnHero ? 'rgba(255,255,255,0.55)' : 'var(--color-ink-muted)' }}
          >
            <span>Photographer</span>
            <span style={{ color: textOnHero ? 'rgba(255,255,255,0.25)' : 'var(--color-line-strong)' }}>/</span>
            <span>Software Developer</span>
            <span style={{ color: textOnHero ? 'rgba(255,255,255,0.25)' : 'var(--color-line-strong)' }}>/</span>
            <span>Pune, India</span>
          </div>

          <h1
            className="mt-4 max-w-4xl text-[clamp(2.5rem,7vw,5.5rem)] font-semibold leading-[1.04] tracking-[-0.035em]"
            style={{ color: textOnHero ? '#ffffff' : 'var(--color-ink)' }}
          >
            I build software.
            <br />
            I frame moments.
          </h1>

          <p
            className="mt-5 max-w-md text-[1rem] leading-relaxed"
            style={{ color: textOnHero ? 'rgba(255,255,255,0.68)' : 'var(--color-ink-muted)' }}
          >
            A full-stack developer and photographer — in one place.
          </p>

          <div className="mt-9 flex flex-wrap gap-3">
            <Link
              to="/work"
              className="inline-flex items-center rounded-full px-6 py-3 text-sm font-semibold transition-opacity hover:opacity-90 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
              style={
                textOnHero
                  ? { background: '#ffffff', color: '#0c0b09' }
                  : { background: 'var(--color-gold)', color: 'var(--color-canvas)' }
              }
            >
              View Photography
            </Link>
            <Link
              to="/projects"
              className="inline-flex items-center rounded-full px-6 py-3 text-sm font-medium transition-all focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
              style={
                textOnHero
                  ? { border: '1px solid rgba(255,255,255,0.3)', background: 'rgba(255,255,255,0.1)', color: '#ffffff', backdropFilter: 'blur(8px)' }
                  : { border: '1px solid var(--color-line-strong)', background: 'transparent', color: 'var(--color-ink-muted)' }
              }
            >
              View Projects
            </Link>
          </div>

          {hero && photoLoaded && (
            <p
              className="mt-8 font-mono text-[0.5625rem] uppercase tracking-[0.14em]"
              style={{ color: 'rgba(255,255,255,0.3)' }}
            >
              {hero.title}
            </p>
          )}
        </div>

        <div className="absolute bottom-7 left-1/2 z-10 -translate-x-1/2" aria-hidden="true">
          <div
            className="flex h-7 w-[16px] items-start justify-center rounded-full border p-[3px]"
            style={{ borderColor: textOnHero ? 'rgba(255,255,255,0.22)' : 'var(--color-line-strong)' }}
          >
            <div
              className="h-1.5 w-[2px] animate-bounce rounded-full"
              style={{ background: textOnHero ? 'rgba(255,255,255,0.55)' : 'var(--color-gold)' }}
            />
          </div>
        </div>
      </section>

      {/* ── Featured Work ────────────────────────────────────────────── */}
      <section aria-label="Featured photographs" className="border-t border-[var(--color-line)] px-6 py-20 sm:px-10">
        <div className="mx-auto max-w-[1600px]">
          <div className="flex items-end justify-between">
            <div>
              <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
                Featured Work
              </p>
              <h2 className="mt-3 text-2xl font-semibold tracking-tight text-[var(--color-ink)]">
                Selected photographs
              </h2>
            </div>
            <Link
              to="/work"
              className="hidden font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-gold)] transition-opacity hover:opacity-70 sm:block"
            >
              View all →
            </Link>
          </div>

          {/* Loading skeletons — mixed heights to hint at masonry */}
          {featuredLoading && (
            <div className="mt-8 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
              {[120, 180, 150, 200, 130, 170].map((h, i) => (
                <div
                  key={i}
                  className="w-full animate-pulse rounded-sm bg-[var(--color-canvas-raised)]"
                  style={{ height: h }}
                />
              ))}
            </div>
          )}

          {/*
            MASONRY grid — same as WorkPage.
            Each photo renders at its natural aspect ratio (width/height from DB).
            No forced aspect-[4/5], no hollow space below landscape photos.
            columns-* (CSS columns) handles the masonry automatically.
          */}
          {featured && featured.length > 0 && (
            <div className="mt-8 columns-1 gap-1 sm:columns-2 lg:columns-3 [&>*]:mb-1">
              {featured.map((photo, i) => (
                <Link
                  key={photo.id}
                  to="/work"
                  className="photo-card group relative block overflow-hidden rounded-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-gold)]"
                >
                  <img
                    src={photo.thumbnailUrl}
                    srcSet={`${photo.thumbnailUrl} 480w, ${photo.mediumUrl} 1200w`}
                    sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                    alt={photo.caption || photo.title}
                    loading={i < 2 ? 'eager' : 'lazy'}
                    decoding="async"
                    /* Natural dimensions — browser reserves correct space,
                       no forced aspect ratio, no hollow gaps */
                    width={photo.width}
                    height={photo.height}
                    className="w-full object-cover transition-transform duration-500 group-hover:scale-[1.02]"
                    style={{ aspectRatio: `${photo.width} / ${photo.height}` }}
                  />
                  {/* Hover overlay — always black, never canvas colour */}
                  <div className="absolute inset-0 flex items-end bg-gradient-to-t from-black/75 via-black/5 to-transparent p-4 opacity-0 transition-opacity duration-300 group-hover:opacity-100">
                    <div>
                      <p className="text-sm font-semibold text-white">{photo.title}</p>
                      {photo.caption && (
                        <p className="mt-0.5 font-mono text-[0.5625rem] uppercase tracking-widest text-white/60">
                          {photo.caption}
                        </p>
                      )}
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          )}

          {featured && featured.length === 0 && !featuredLoading && (
            <p className="mt-8 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-faint)]">
              No published photos yet.
            </p>
          )}

          {featured && featured.length > 0 && (
            <div className="mt-8 flex justify-center sm:hidden">
              <Link
                to="/work"
                className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-gold)] hover:opacity-70"
              >
                View all photography →
              </Link>
            </div>
          )}
        </div>
      </section>

      {/* ── Stats strip ─────────────────────────────────────────────── */}
      <section aria-label="About" className="section-alt border-t border-[var(--color-line)] px-6 py-14 sm:px-10">
        <div className="mx-auto grid max-w-[1600px] grid-cols-2 gap-10 sm:grid-cols-4">
          {[
            { value: 'Sony A6000', label: 'Primary camera' },
            { value: 'Pune, India', label: 'Based in'       },
            { value: '.NET + React', label: 'Built with'    },
            { value: 'Open source', label: 'Philosophy'     },
          ].map((s) => (
            <div key={s.label}>
              <p className="text-lg font-semibold tracking-tight text-[var(--color-ink)]">{s.value}</p>
              <p className="mt-1 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-faint)]">
                {s.label}
              </p>
            </div>
          ))}
        </div>
      </section>
    </>
  )
}

function hexToRgb(hex: string) {
  const m = hex.replace('#', '').match(/.{2}/g)
  if (!m || m.length < 3) return null
  return { r: parseInt(m[0], 16), g: parseInt(m[1], 16), b: parseInt(m[2], 16) }
}

function darkenHex(hex: string, factor: number): string {
  const rgb = hexToRgb(hex)
  if (!rgb) return hex
  const c = (v: number) =>
    Math.max(0, Math.min(255, Math.round(v))).toString(16).padStart(2, '0')
  return `#${c(rgb.r * factor)}${c(rgb.g * factor)}${c(rgb.b * factor)}`
}
