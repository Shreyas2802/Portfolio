import { useRef, useState, type FormEvent } from 'react'
import {
  useAdminAlbums,
  useAdminPhotos,
  useDeletePhoto,
  useSetHeroPhoto,
  useUploadPhoto,
} from '../../features/admin/useAdminPhotos'
import { useTags } from '../../features/admin/useAdminTaxonomy'
import { ApiError } from '../../lib/api'

export function PhotosAdminPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-12 sm:px-10">
      <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
        Photos
      </p>
      <h1 className="mt-3 text-3xl font-semibold tracking-tight text-[var(--color-ink)]">
        Manage Photos
      </h1>
      <UploadForm />
      <PhotoList />
    </section>
  )
}

function UploadForm() {
  const { data: albums } = useAdminAlbums()
  const { data: tags } = useTags()
  const uploadMutation = useUploadPhoto()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [file, setFile] = useState<File | null>(null)
  const [title, setTitle] = useState('')
  const [caption, setCaption] = useState('')
  const [albumId, setAlbumId] = useState<number | ''>('')
  const [tagIds, setTagIds] = useState<number[]>([])

  function toggleTag(id: number) {
    setTagIds((cur) => (cur.includes(id) ? cur.filter((t) => t !== id) : [...cur, id]))
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!file || !albumId) return
    uploadMutation.mutate(
      { file, title, caption, albumId, tagIds },
      {
        onSuccess: () => {
          setFile(null)
          setTitle('')
          setCaption('')
          setTagIds([])
          if (fileInputRef.current) fileInputRef.current.value = ''
        },
      },
    )
  }

  return (
    <div className="mt-8 rounded-xl border border-[var(--color-line)] bg-[var(--color-canvas-raised)] p-6">
      <p className="text-sm font-medium text-[var(--color-ink)]">Upload a photo</p>
      <p className="mt-0.5 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-faint)]">
        JPEG, PNG or WebP · max 100 MB
      </p>

      <form onSubmit={handleSubmit} className="mt-6 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        <div className="flex flex-col gap-1.5 lg:col-span-full">
          <label htmlFor="photo-file-input" className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
            Photo file
          </label>
          <input
            id="photo-file-input"
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink-muted)] outline-none transition-colors file:mr-4 file:rounded-md file:border-0 file:bg-[var(--color-canvas-hover)] file:px-3 file:py-1 file:text-xs file:font-medium file:text-[var(--color-ink)] focus-within:border-[var(--color-gold)]"
          />
          {file && (
            <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-gold)]">
              {file.name} · {(file.size / 1024 / 1024).toFixed(1)} MB
            </p>
          )}
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="photo-title" className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
            Title <span className="text-[var(--color-gold)]">*</span>
          </label>
          <input
            id="photo-title"
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            placeholder="e.g. Kasba Ganapati"
            className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink)] outline-none transition-colors placeholder:text-[var(--color-ink-faint)] focus:border-[var(--color-gold)]"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="photo-caption" className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
            Caption
          </label>
          <input
            id="photo-caption"
            type="text"
            value={caption}
            onChange={(e) => setCaption(e.target.value)}
            placeholder="Optional short description"
            className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink)] outline-none transition-colors placeholder:text-[var(--color-ink-faint)] focus:border-[var(--color-gold)]"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="photo-album" className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
            Album <span className="text-[var(--color-gold)]">*</span>
          </label>
          <select
            id="photo-album"
            value={albumId}
            onChange={(e) => setAlbumId(e.target.value ? Number(e.target.value) : '')}
            required
            className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas)] px-4 py-2.5 text-sm text-[var(--color-ink)] outline-none transition-colors focus:border-[var(--color-gold)]"
          >
            <option value="">Select album…</option>
            {albums?.map((album) => (
              <option key={album.id} value={album.id}>{album.title}</option>
            ))}
          </select>
        </div>

        {tags && tags.length > 0 && (
          <div className="flex flex-col gap-1.5 lg:col-span-full">
            <span className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">Tags</span>
            <div className="flex flex-wrap gap-2">
              {tags.map((tag) => (
                <label
                  key={tag.id}
                  className={`cursor-pointer rounded-full border px-3 py-1 font-mono text-[0.6875rem] uppercase tracking-[0.08em] transition-colors ${
                    tagIds.includes(tag.id)
                      ? 'border-[var(--color-gold)] bg-[var(--color-gold)]/10 text-[var(--color-gold)]'
                      : 'border-[var(--color-line)] text-[var(--color-ink-muted)] hover:border-[var(--color-line-strong)]'
                  }`}
                >
                  <input type="checkbox" checked={tagIds.includes(tag.id)} onChange={() => toggleTag(tag.id)} className="sr-only" />
                  {tag.name}
                </label>
              ))}
            </div>
          </div>
        )}

        <div className="flex items-end gap-4 lg:col-span-full">
          <button
            type="submit"
            disabled={uploadMutation.isPending || !file || !title || !albumId}
            className="rounded-lg bg-[var(--color-gold)] px-6 py-2.5 text-sm font-semibold text-[var(--color-canvas)] transition-all hover:bg-[var(--color-gold-dim)] disabled:cursor-not-allowed disabled:opacity-50"
          >
            {uploadMutation.isPending ? 'Uploading…' : 'Upload photo'}
          </button>
          {uploadMutation.isSuccess && (
            <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-green-400">Uploaded.</p>
          )}
        </div>

        {uploadMutation.isError && (
          <p className="rounded-lg border border-red-900/40 bg-red-950/20 px-4 py-2.5 text-sm text-red-400 lg:col-span-full" role="alert">
            {uploadMutation.error instanceof ApiError ? uploadMutation.error.message : 'Upload failed.'}
          </p>
        )}
      </form>
    </div>
  )
}

function PhotoList() {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useAdminPhotos(page)
  const deleteMutation = useDeletePhoto()
  const heroMutation = useSetHeroPhoto()

  if (isLoading) {
    return (
      <div className="mt-8 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {[1,2,3,4,5,6,7,8].map((i) => (
          <div key={i} className="aspect-square animate-pulse rounded-xl bg-[var(--color-canvas-raised)]" />
        ))}
      </div>
    )
  }

  if (!data || data.items.length === 0) {
    return (
      <p className="mt-10 font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-faint)]">
        No photos yet — upload one above.
      </p>
    )
  }

  return (
    <>
      <div className="mt-8 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {data.items.map((photo) => (
          <div
            key={photo.id}
            className={`group relative overflow-hidden rounded-xl border bg-[var(--color-canvas-raised)] transition-all ${
              photo.isHeroPhoto
                ? 'border-[var(--color-gold)] ring-1 ring-[var(--color-gold)]/30'
                : 'border-[var(--color-line)] hover:border-[var(--color-gold)]/40'
            }`}
          >
            <img
              src={photo.thumbnailUrl}
              alt={photo.title}
              className="aspect-square w-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
            />

            {/* Hero badge */}
            {photo.isHeroPhoto && (
              <div className="absolute left-2 top-2 rounded-md bg-[var(--color-gold)] px-2 py-0.5 font-mono text-[0.5625rem] uppercase tracking-widest text-[var(--color-canvas)]">
                Hero
              </div>
            )}

            {/* Action buttons — appear on hover */}
            <div className="absolute right-2 top-2 flex flex-col gap-1 opacity-0 transition-opacity group-hover:opacity-100">
              {!photo.isHeroPhoto && (
                <button
                  type="button"
                  disabled={heroMutation.isPending}
                  onClick={() => heroMutation.mutate(photo)}
                  className="rounded-md bg-[var(--color-gold)] px-2.5 py-1 text-xs font-semibold text-[var(--color-canvas)] hover:bg-[var(--color-gold-dim)] disabled:opacity-50"
                  title="Set as homepage hero"
                >
                  Set Hero
                </button>
              )}
              <button
                type="button"
                onClick={() => {
                  if (confirm(`Delete "${photo.title}"? This cannot be undone.`)) {
                    deleteMutation.mutate(photo.id)
                  }
                }}
                className="rounded-md bg-black/70 px-2.5 py-1 text-xs font-medium text-white hover:bg-red-600"
              >
                Delete
              </button>
            </div>

            <div className="border-t border-[var(--color-line)] px-3 py-2">
              <p className="truncate text-xs font-medium text-[var(--color-ink)]">{photo.title}</p>
            </div>
          </div>
        ))}
      </div>

      {data.totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-4">
          <button
            type="button"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="rounded-lg border border-[var(--color-line)] px-4 py-2 font-mono text-[0.6875rem] uppercase tracking-[0.08em] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] disabled:opacity-40"
          >
            ← Previous
          </button>
          <p className="font-mono text-[0.6875rem] uppercase tracking-[0.08em] text-[var(--color-ink-muted)]">
            {data.page} / {data.totalPages}
          </p>
          <button
            type="button"
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page >= data.totalPages}
            className="rounded-lg border border-[var(--color-line)] px-4 py-2 font-mono text-[0.6875rem] uppercase tracking-[0.08em] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] disabled:opacity-40"
          >
            Next →
          </button>
        </div>
      )}
    </>
  )
}
