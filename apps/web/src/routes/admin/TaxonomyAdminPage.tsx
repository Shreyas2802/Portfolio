import { useState, type FormEvent } from 'react'
import { DataLabel } from '../../components/ui/DataLabel'
import {
  useCategories,
  useCreateCategory,
  useDeleteCategory,
  useCreateTag,
  useDeleteTag,
  useTags,
} from '../../features/admin/useAdminTaxonomy'

export function TaxonomyAdminPage() {
  return (
    <section className="mx-auto max-w-[1600px] px-6 py-16 sm:px-10">
      <DataLabel items={['CATEGORIES & TAGS']} />
      <h1 className="mt-4 text-3xl font-semibold tracking-tight">Categories & Tags</h1>

      <div className="mt-8 grid gap-8 lg:grid-cols-2">
        <CategoriesPanel />
        <TagsPanel />
      </div>
    </section>
  )
}

function CategoriesPanel() {
  const { data: categories, isLoading } = useCategories()
  const createMutation = useCreateCategory()
  const deleteMutation = useDeleteCategory()
  const [name, setName] = useState('')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!name.trim()) return
    createMutation.mutate(name.trim(), { onSuccess: () => setName('') })
  }

  return (
    <div className="rounded-lg border border-[var(--color-line)] p-6">
      <p className="data-label">Categories</p>
      <form onSubmit={handleSubmit} className="mt-4 flex gap-2">
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="New category name"
          className="flex-1 rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
        />
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-full bg-[var(--color-ink)] px-4 py-2 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
        >
          Add
        </button>
      </form>

      {isLoading && <p className="mt-4 text-sm text-[var(--color-ink-muted)]">Loading…</p>}

      <ul className="mt-4 flex flex-col gap-2">
        {categories?.map((cat) => (
          <li key={cat.id} className="flex items-center justify-between text-sm">
            <span className="text-[var(--color-ink)]">
              {cat.name}{' '}
              <span className="text-[var(--color-ink-muted)]">
                ({cat.albumCount} album{cat.albumCount === 1 ? '' : 's'})
              </span>
            </span>
            <button
              type="button"
              onClick={() => {
                if (confirm(`Delete category "${cat.name}"? Albums using it become uncategorized.`)) {
                  deleteMutation.mutate(cat.id)
                }
              }}
              className="data-label text-[var(--color-ink-muted)] hover:text-red-500"
            >
              Delete
            </button>
          </li>
        ))}
        {categories?.length === 0 && (
          <li className="text-sm text-[var(--color-ink-muted)]">No categories yet.</li>
        )}
      </ul>
    </div>
  )
}

function TagsPanel() {
  const { data: tags, isLoading } = useTags()
  const createMutation = useCreateTag()
  const deleteMutation = useDeleteTag()
  const [name, setName] = useState('')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!name.trim()) return
    createMutation.mutate(name.trim(), { onSuccess: () => setName('') })
  }

  return (
    <div className="rounded-lg border border-[var(--color-line)] p-6">
      <p className="data-label">Tags</p>
      <form onSubmit={handleSubmit} className="mt-4 flex gap-2">
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="New tag name"
          className="flex-1 rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
        />
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-full bg-[var(--color-ink)] px-4 py-2 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
        >
          Add
        </button>
      </form>

      {isLoading && <p className="mt-4 text-sm text-[var(--color-ink-muted)]">Loading…</p>}

      <ul className="mt-4 flex flex-col gap-2">
        {tags?.map((tag) => (
          <li key={tag.id} className="flex items-center justify-between text-sm">
            <span className="text-[var(--color-ink)]">
              {tag.name}{' '}
              <span className="text-[var(--color-ink-muted)]">
                ({tag.photoCount} photo{tag.photoCount === 1 ? '' : 's'})
              </span>
            </span>
            <button
              type="button"
              onClick={() => {
                if (confirm(`Delete tag "${tag.name}"?`)) {
                  deleteMutation.mutate(tag.id)
                }
              }}
              className="data-label text-[var(--color-ink-muted)] hover:text-red-500"
            >
              Delete
            </button>
          </li>
        ))}
        {tags?.length === 0 && <li className="text-sm text-[var(--color-ink-muted)]">No tags yet.</li>}
      </ul>
    </div>
  )
}
