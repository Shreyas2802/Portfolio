import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiAuthGet, apiAuthDelete, apiAuthPut } from '../../lib/api'

interface ContactMessage {
  id: number
  name: string
  email: string
  message: string
  submittedAtUtc: string
  isRead: boolean
}

interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

function useMessages(page: number) {
  return useQuery({
    queryKey: ['admin-messages', page],
    queryFn: () => apiAuthGet<PagedResult<ContactMessage>>(`/contact/messages?page=${page}&pageSize=20`),
  })
}

function useMarkRead() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiAuthPut(`/contact/messages/${id}/read`, {}),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-messages'] }),
  })
}

function useDeleteMessage() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiAuthDelete(`/contact/messages/${id}`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-messages'] })
      qc.invalidateQueries({ queryKey: ['admin-stats'] })
    },
  })
}

export function MessagesAdminPage() {
  const [page, setPage] = useState(1)
  const [expanded, setExpanded] = useState<number | null>(null)
  const [copied, setCopied] = useState<number | null>(null)
  const { data, isLoading } = useMessages(page)
  const markRead = useMarkRead()
  const deleteMsg = useDeleteMessage()

  function toggleExpand(msg: ContactMessage) {
    setExpanded((cur) => (cur === msg.id ? null : msg.id))
    if (!msg.isRead) markRead.mutate(msg.id)
  }

  async function copyEmail(msg: ContactMessage) {
    try {
      await navigator.clipboard.writeText(msg.email)
      setCopied(msg.id)
      setTimeout(() => setCopied(null), 2000)
    } catch {
      // fallback — select the text manually
    }
  }

  return (
    <section className="mx-auto max-w-[1600px] px-6 py-12 sm:px-10">
      <h1 className="text-3xl font-semibold tracking-tight text-[var(--color-ink)]">
        Contact Inbox
      </h1>
      {data && (
        <p className="mt-1 text-sm text-[var(--color-ink-muted)]">
          {data.totalCount} total
          {data.items.filter((m) => !m.isRead).length > 0 && (
            <> · <span className="text-[var(--color-gold)]">
              {data.items.filter((m) => !m.isRead).length} unread this page
            </span></>
          )}
        </p>
      )}

      {isLoading && (
        <div className="mt-8 space-y-2">
          {[1,2,3,4,5].map((i) => (
            <div key={i} className="h-16 animate-pulse rounded-xl bg-[var(--color-canvas-raised)]" />
          ))}
        </div>
      )}

      {data && data.items.length === 0 && (
        <p className="mt-10 text-sm text-[var(--color-ink-faint)]">No messages yet.</p>
      )}

      {data && data.items.length > 0 && (
        <div className="mt-8 space-y-2">
          {data.items.map((msg) => (
            <div
              key={msg.id}
              className={`rounded-xl border transition-colors ${
                !msg.isRead
                  ? 'border-[var(--color-gold)]/40 bg-[var(--color-canvas-raised)]'
                  : 'border-[var(--color-line)] bg-[var(--color-canvas-raised)]'
              }`}
            >
              {/* Row */}
              <button
                type="button"
                onClick={() => toggleExpand(msg)}
                className="flex w-full items-center gap-4 px-5 py-4 text-left"
              >
                <span
                  className={`h-2 w-2 shrink-0 rounded-full transition-colors ${
                    !msg.isRead ? 'bg-[var(--color-gold)]' : 'bg-transparent'
                  }`}
                  aria-hidden="true"
                />
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-baseline gap-x-3">
                    <span className="font-medium text-[var(--color-ink)]">{msg.name}</span>
                    <span className="text-xs text-[var(--color-ink-muted)]">{msg.email}</span>
                  </div>
                  <p className="mt-0.5 truncate text-sm text-[var(--color-ink-muted)]">
                    {msg.message}
                  </p>
                </div>
                <p className="shrink-0 text-xs text-[var(--color-ink-faint)]">
                  {formatDate(msg.submittedAtUtc)}
                </p>
                <span
                  className="shrink-0 text-xs text-[var(--color-ink-faint)] transition-transform duration-200"
                  style={{ transform: expanded === msg.id ? 'rotate(180deg)' : 'none' }}
                >
                  ▾
                </span>
              </button>

              {/* Expanded */}
              {expanded === msg.id && (
                <div className="border-t border-[var(--color-line)] px-5 py-5">
                  <p className="whitespace-pre-wrap text-sm leading-relaxed text-[var(--color-ink)]">
                    {msg.message}
                  </p>

                  <div className="mt-5 flex flex-wrap items-center gap-3">
                    {/* Primary: open mail client */}
                    <a
                      href={`mailto:${msg.email}?subject=Re: Your message`}
                      className="rounded-lg bg-[var(--color-gold)] px-4 py-2 text-sm font-semibold text-[var(--color-canvas)] transition-all hover:bg-[var(--color-gold-dim)]"
                    >
                      Reply in mail app
                    </a>

                    {/* Fallback: copy email address */}
                    <button
                      type="button"
                      onClick={() => copyEmail(msg)}
                      className="rounded-lg border border-[var(--color-line)] px-4 py-2 text-sm text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-line-strong)] hover:text-[var(--color-ink)]"
                    >
                      {copied === msg.id ? '✓ Copied' : 'Copy email'}
                    </button>

                    <button
                      type="button"
                      onClick={() => {
                        if (confirm(`Delete message from ${msg.name}?`)) deleteMsg.mutate(msg.id)
                      }}
                      className="ml-auto rounded-lg border border-[var(--color-line)] px-4 py-2 text-sm text-[var(--color-ink-muted)] transition-colors hover:border-red-900/50 hover:text-red-400"
                    >
                      Delete
                    </button>
                  </div>

                  {/* Note for Chrome/Brave users */}
                  <p className="mt-3 text-xs text-[var(--color-ink-faint)]">
                    "Reply in mail app" opens Outlook or your default mail client.
                    If nothing happens, use "Copy email" and paste into Gmail or your preferred app.
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-4">
          <button
            type="button"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="rounded-lg border border-[var(--color-line)] px-4 py-2 text-sm text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] disabled:opacity-40"
          >
            ← Previous
          </button>
          <p className="text-sm text-[var(--color-ink-muted)]">{data.page} / {data.totalPages}</p>
          <button
            type="button"
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page >= data.totalPages}
            className="rounded-lg border border-[var(--color-line)] px-4 py-2 text-sm text-[var(--color-ink-muted)] transition-colors hover:border-[var(--color-gold)] hover:text-[var(--color-gold)] disabled:opacity-40"
          >
            Next →
          </button>
        </div>
      )}
    </section>
  )
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric',
  })
}
