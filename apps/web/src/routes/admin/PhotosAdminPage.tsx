import { useRef, useState, type FormEvent } from 'react'
import { DataLabel } from '../../components/ui/DataLabel'
import {
  useAdminAlbums,
  useAdminPhotos,
  useDeletePhoto,
  useUploadPhoto,
} from '../../features/admin/useAdminPhotos'
import { useTags } from '../../features/admin/useAdminTaxonomy'
import { ApiError } from '../../lib/api'

export function PhotosAdminPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-16 sm:px-10">
      <DataLabel items={['PHOTOS']} />
      <h1 className="mt-4 text-3xl font-semibold tracking-tight">Manage Photos</h1>

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
    setTagIds((current) => (current.includes(id) ? current.filter((t) => t !== id) : [...current, id]))
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
    <form
      onSubmit={handleSubmit}
      className="mt-8 flex flex-col gap-4 rounded-lg border border-[var(--color-line)] p-6 sm:flex-row sm:flex-wrap sm:items-end"
    >
      <div className="flex flex-col gap-2">
        <label htmlFor="photo-file-input" className="data-label">
          Photo file
        </label>
        <input
          id="photo-file-input"
          ref={fileInputRef}
          type="file"
          accept="image/*"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          className="text-sm text-[var(--color-ink-muted)]"
        />
      </div>
      <div className="flex flex-col gap-2">
        <label htmlFor="photo-title" className="data-label">
          Title
        </label>
        <input
          id="photo-title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
          className="rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
        />
      </div>
      <div className="flex flex-col gap-2">
        <label htmlFor="photo-caption" className="data-label">
          Caption
        </label>
        <input
          id="photo-caption"
          type="text"
          value={caption}
          onChange={(e) => setCaption(e.target.value)}
          className="rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
        />
      </div>
      <div className="flex flex-col gap-2">
        <label htmlFor="photo-album" className="data-label">
          Album
        </label>
        <select
          id="photo-album"
          value={albumId}
          onChange={(e) => setAlbumId(e.target.value ? Number(e.target.value) : '')}
          required
          className="rounded-md border border-[var(--color-line)] bg-[var(--color-canvas)] px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
        >
          <option value="">Select…</option>
          {albums?.map((album) => (
            <option key={album.id} value={album.id}>
              {album.title}
            </option>
          ))}
        </select>
      </div>

      {tags && tags.length > 0 && (
        <div className="flex flex-col gap-2">
          <span className="data-label">Tags</span>
          <div className="flex flex-wrap gap-2">
            {tags.map((tag) => (
              <label
                key={tag.id}
                className={`data-label cursor-pointer rounded-full border px-3 py-1 transition-colors ${
                  tagIds.includes(tag.id)
                    ? 'border-[var(--color-brass)] text-[var(--color-brass)]'
                    : 'border-[var(--color-line)] text-[var(--color-ink-muted)]'
                }`}
              >
                <input
                  type="checkbox"
                  checked={tagIds.includes(tag.id)}
                  onChange={() => toggleTag(tag.id)}
                  className="sr-only"
                />
                {tag.name}
              </label>
            ))}
          </div>
        </div>
      )}

      <button
        type="submit"
        disabled={uploadMutation.isPending || !file}
        className="rounded-full bg-[var(--color-ink)] px-6 py-2 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {uploadMutation.isPending ? 'Uploading…' : 'Upload'}
      </button>

      {uploadMutation.isError && (
        <p className="w-full text-sm text-red-400" role="alert">
          {uploadMutation.error instanceof ApiError
            ? uploadMutation.error.message
            : 'Upload failed.'}
        </p>
      )}
    </form>
  )
}

function PhotoList() {
  const { data: photos, isLoading } = useAdminPhotos()
  const deleteMutation = useDeletePhoto()

  if (isLoading) {
    return <p className="mt-8 text-[var(--color-ink-muted)]">Loading…</p>
  }

  if (!photos || photos.length === 0) {
    return <p className="mt-8 text-[var(--color-ink-muted)]">No photos yet.</p>
  }

  return (
    <div className="mt-8 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
      {photos.map((photo) => (
        <div key={photo.id} className="group relative overflow-hidden rounded-md border border-[var(--color-line)]">
          <img src={photo.thumbnailUrl} alt={photo.title} className="aspect-square w-full object-cover" />
          <div className="p-2">
            <p className="truncate text-sm text-[var(--color-ink)]">{photo.title}</p>
          </div>
          <button
            type="button"
            onClick={() => {
              if (confirm(`Delete "${photo.title}"? This cannot be undone.`)) {
                deleteMutation.mutate(photo.id)
              }
            }}
            className="absolute right-2 top-2 rounded-full bg-black/60 px-3 py-1 text-xs text-white opacity-0 transition-opacity hover:bg-red-600 group-hover:opacity-100"
          >
            Delete
          </button>
        </div>
      ))}
    </div>
  )
}
