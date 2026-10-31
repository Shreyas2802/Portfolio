import { useState, type FormEvent } from 'react'
import { DataLabel } from '../../components/ui/DataLabel'
import {
  useAdminProjects,
  useCreateProject,
  useUpdateProject,
  useDeleteProject,
  type ProjectAdmin,
  type ProjectFormInput,
} from '../../features/admin/useAdminProjects'

const EMPTY_FORM: ProjectFormInput = {
  title: '',
  tagline: '',
  role: '',
  status: '',
  overview: '',
  contributionsText: '',
  technologies: '',
  githubUrl: '',
  liveUrl: '',
  isFeatured: false,
  isPublished: true,
}

function toFormInput(project: ProjectAdmin): ProjectFormInput {
  return {
    title: project.title,
    tagline: project.tagline,
    role: project.role,
    status: project.status,
    overview: project.overview,
    contributionsText: project.contributions.join('\n'),
    technologies: project.technologies.join(', '),
    githubUrl: project.githubUrl ?? '',
    liveUrl: project.liveUrl ?? '',
    isFeatured: project.isFeatured,
    isPublished: project.isPublished,
  }
}

export function ProjectsAdminPage() {
  const { data: projects, isLoading } = useAdminProjects()
  const createMutation = useCreateProject()
  const updateMutation = useUpdateProject()
  const deleteMutation = useDeleteProject()

  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState<ProjectFormInput>(EMPTY_FORM)

  function startCreate() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  function startEdit(project: ProjectAdmin) {
    setEditingId(project.id)
    setForm(toFormInput(project))
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
  const inputClass =
    'rounded-md border border-[var(--color-line)] bg-transparent px-3 py-2 text-sm text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]'

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-16 sm:px-10">
      <DataLabel items={['PROJECTS']} />
      <h1 className="mt-4 text-3xl font-semibold tracking-tight">Manage Projects</h1>

      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-4 rounded-lg border border-[var(--color-line)] p-6">
        <p className="data-label">{editingId === null ? 'New project' : 'Editing project'}</p>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-title" className="data-label">Title</label>
            <input id="proj-title" type="text" required value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))} className={inputClass} />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-tagline" className="data-label">Tagline</label>
            <input id="proj-tagline" type="text" value={form.tagline}
              onChange={(e) => setForm((f) => ({ ...f, tagline: e.target.value }))} className={inputClass} />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-role" className="data-label">Role</label>
            <input id="proj-role" type="text" value={form.role}
              onChange={(e) => setForm((f) => ({ ...f, role: e.target.value }))} className={inputClass} />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-status" className="data-label">Status</label>
            <input id="proj-status" type="text" value={form.status}
              onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))} className={inputClass} />
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label htmlFor="proj-overview" className="data-label">Overview</label>
          <textarea id="proj-overview" rows={3} value={form.overview}
            onChange={(e) => setForm((f) => ({ ...f, overview: e.target.value }))} className={inputClass} />
        </div>

        <div className="flex flex-col gap-2">
          <label htmlFor="proj-contributions" className="data-label">Key contributions (one per line)</label>
          <textarea id="proj-contributions" rows={4} value={form.contributionsText}
            onChange={(e) => setForm((f) => ({ ...f, contributionsText: e.target.value }))} className={inputClass} />
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-tech" className="data-label">Technologies (comma-separated)</label>
            <input id="proj-tech" type="text" value={form.technologies}
              onChange={(e) => setForm((f) => ({ ...f, technologies: e.target.value }))} className={inputClass} />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-github" className="data-label">GitHub URL</label>
            <input id="proj-github" type="url" value={form.githubUrl}
              onChange={(e) => setForm((f) => ({ ...f, githubUrl: e.target.value }))} className={inputClass} />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="proj-live" className="data-label">Live URL</label>
            <input id="proj-live" type="url" value={form.liveUrl}
              onChange={(e) => setForm((f) => ({ ...f, liveUrl: e.target.value }))} className={inputClass} />
          </div>
        </div>

        <div className="flex flex-wrap gap-6">
          <label className="flex items-center gap-2 text-sm text-[var(--color-ink-muted)]">
            <input type="checkbox" checked={form.isFeatured}
              onChange={(e) => setForm((f) => ({ ...f, isFeatured: e.target.checked }))} />
            Featured (shown as a large case-study card)
          </label>
          <label className="flex items-center gap-2 text-sm text-[var(--color-ink-muted)]">
            <input type="checkbox" checked={form.isPublished}
              onChange={(e) => setForm((f) => ({ ...f, isPublished: e.target.checked }))} />
            Published (visible on the public site)
          </label>
        </div>

        <div className="flex gap-3">
          <button type="submit" disabled={isSaving}
            className="rounded-full bg-[var(--color-ink)] px-6 py-2 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50">
            {isSaving ? 'Saving…' : editingId === null ? 'Create project' : 'Save changes'}
          </button>
          {editingId !== null && (
            <button type="button" onClick={startCreate}
              className="data-label rounded-full border border-[var(--color-line)] px-6 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]">
              Cancel
            </button>
          )}
        </div>
      </form>

      {isLoading && <p className="mt-8 text-[var(--color-ink-muted)]">Loading…</p>}

      {projects && (
        <div className="mt-8 flex flex-col gap-2">
          {projects.map((project) => (
            <div key={project.id} className="flex flex-wrap items-center justify-between gap-4 rounded-lg border border-[var(--color-line)] p-4">
              <div>
                <p className="text-[var(--color-ink)]">
                  {project.title}{' '}
                  {project.isFeatured && <span className="data-label text-[var(--color-brass)]">(featured)</span>}{' '}
                  {!project.isPublished && <span className="data-label text-[var(--color-ink-muted)]">(unpublished)</span>}
                </p>
                <p className="data-label mt-1 text-[var(--color-ink-muted)]">{project.tagline}</p>
              </div>
              <div className="flex gap-2">
                <button type="button" onClick={() => startEdit(project)}
                  className="data-label rounded-full border border-[var(--color-line)] px-4 py-2 hover:border-[var(--color-brass)] hover:text-[var(--color-brass)]">
                  Edit
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (confirm(`Delete "${project.title}"? This cannot be undone.`)) {
                      deleteMutation.mutate(project.id)
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
