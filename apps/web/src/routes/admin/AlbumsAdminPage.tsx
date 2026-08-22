import { useState, type FormEvent } from 'react'
import { DataLabel } from '../../components/ui/DataLabel'
import { useCategories } from '../../features/admin/useAdminTaxonomy'
import {
  useAdminAlbumsList,
  useCreateAlbum,
  useUpdateAlbum,
  useDeleteAlbum,
  type AlbumAdmin,
  type AlbumFormInput,
} from '../../features/admin/useAdminAlbums'

const EMPTY_FORM: AlbumFormInput = {
  title: '',
  description: '',
  categoryId: null,
  isPublished: true,
}

export function AlbumsAdminPage() {
  const { data: albums, isLoading } = useAdminAlbumsList()
  const { data: categories } = useCategories()
  const createMutation = useCreateAlbum()
  const updateMutation = useUpdateAlbum()
  const deleteMutation = useDeleteAlbum()

  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState<AlbumFormInput>(EMPTY_FORM)

  function startCreate() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function startEdit(album: AlbumAdmin) {
    setEditingId(album.id)
    setForm({
      title: album.title,
      description: album.description,
      categoryId: album.categoryId,
      isPublished: album.isPublished,
    })
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (editingId === null) {
      createMutation.mutate(form, { onSuccess: () => setForm(EMPTY_FORM) })
    } else {
      updateMutation.mutate(
        { id: editingId, input: form },
        { onSuccess: () => { setEditingId(null); setForm(EMPTY_FORM) } },
      )
    }
  }

  const isSaving = createMutation.isPending || updateMutation.isPending

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-16 sm:px-10">
      <DataLabel items={['ALBUMS']} />
      <h1 className="mt-4 text-3xl font-semibold tracking-tight">Manage Albums</h1>

      <form
        onSubmit={handleSubmit}
        className="mt-8 flex flex-col gap-4 rounded-lg border border-[var(--color-line)] p-6"
      >
        <p className="data-label">{editingId === null ? 'New album' : 'Editing album'}</p>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-2">
            <label htmlFor="album-title" className="data-label">
              Title
            </label>
            <input
              id="album-title"
              type="text"
              required
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
            />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="album-category" className="data-label">
              Category
            </label>
            <select
              id="album-category"
              value={form.categoryId ?? ''}
              onChange={(e) =>
                setForm((f) => ({ ...f, categoryId: e.target.value ? Number(e.target.value) : null }))
              }
              className="rounded-md border border-[var(--color-line)] bg-[var(--color-canvas)] px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
            >
              <option value="">None</option>
              {categories?.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label htmlFor="album-description" className="data-label">
            Description
          </label>
          <textarea
            id="album-description"
            rows={3}
            value={form.description}
            onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
            className="rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
          />
        </div>

        <label className="flex items-center gap-2 text-sm text-[var(--color-ink-muted)]">
          <input
            type="checkbox"
            checked={form.isPublished}
            onChange={(e) => setForm((f) => ({ ...f, isPublished: e.target.checked }))}
          />
          Published (visible on the public site)
        </label>

        <div className="flex gap-3">
          <button
            type="submit"
            disabled={isSaving}
            className="rounded-full bg-[var(--color-ink)] px-6 py-2 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
          >
            {isSaving ? 'Saving…' : editingId === null ? 'Create album' : 'Save changes'}
          </button>
          {editingId !== null && (
            <button
              type="button"
              onClick={startCreate}
              className="data-label rounded-full border border-[var(--color-line)] px-6 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
            >
              Cancel
            </button>
          )}
        </div>
      </form>

      {isLoading && <p className="mt-8 text-[var(--color-ink-muted)]">Loading…</p>}

      {albums && (
        <div className="mt-8 flex flex-col gap-2">
          {albums.map((album) => (
            <div
              key={album.id}
              className="flex flex-wrap items-center justify-between gap-4 rounded-lg border border-[var(--color-line)] p-4"
            >
              <div>
                <p className="text-[var(--color-ink)]">
                  {album.title}{' '}
                  {!album.isPublished && (
                    <span className="data-label text-[var(--color-ink-muted)]">(unpublished)</span>
                  )}
                </p>
                <p className="data-label mt-1 text-[var(--color-ink-muted)]">
                  {album.photoCount} photo{album.photoCount === 1 ? '' : 's'}
                  {album.categoryName ? ` · ${album.categoryName}` : ''}
                </p>
              </div>
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => startEdit(album)}
                  className="data-label rounded-full border border-[var(--color-line)] px-4 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]"
                >
                  Edit
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (
                      confirm(
                        `Delete "${album.title}" and all ${album.photoCount} photo(s) in it? This cannot be undone.`,
                      )
                    ) {
                      deleteMutation.mutate(album.id)
                    }
                  }}
                  className="data-label rounded-full border border-[var(--color-line)] px-4 py-2 hover:border-red-500 hover:text-red-500"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  )
}
