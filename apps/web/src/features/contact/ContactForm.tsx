import { useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { z } from 'zod'
import { apiPost } from '../../lib/api'

const contactSchema = z.object({
  name: z.string().trim().min(1, 'Name is required').max(200),
  email: z.string().trim().email('Enter a valid email address').max(320),
  message: z.string().trim().min(1, 'Message is required').max(4000),
})

type ContactFormValues = z.infer<typeof contactSchema>

interface FieldErrors {
  name?: string
  email?: string
  message?: string
}

/**
 * Contact form. Validates client-side with Zod for fast feedback, but
 * the API re-validates everything server-side (DataAnnotations on
 * SubmitContactMessageRequest) since the client is never trusted
 * (spec principle #2) - this validation is purely a UX nicety.
 *
 * Includes a "website" honeypot field, visually hidden and skipped by
 * screen readers via aria-hidden + tabIndex=-1, that real users never
 * fill. The backend silently drops any submission where it's non-empty.
 */
export function ContactForm() {
  const [values, setValues] = useState<ContactFormValues & { website: string }>({
    name: '',
    email: '',
    message: '',
    website: '',
  })
  const [errors, setErrors] = useState<FieldErrors>({})

  const mutation = useMutation({
    mutationFn: (payload: ContactFormValues & { website: string }) =>
      apiPost<typeof payload, { status: string }>('/contact', payload),
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const result = contactSchema.safeParse(values)
    if (!result.success) {
      const fieldErrors: FieldErrors = {}
      for (const issue of result.error.issues) {
        const key = issue.path[0] as keyof FieldErrors
        fieldErrors[key] = issue.message
      }
      setErrors(fieldErrors)
      return
    }
    setErrors({})
    mutation.mutate(values)
  }

  if (mutation.isSuccess) {
    return (
      <p className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas-raised)] px-6 py-8 text-[var(--color-ink)]">
        Thanks — your message has been sent. I'll get back to you soon.
      </p>
    )
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="flex max-w-lg flex-col gap-6">
      {/* Honeypot - hidden from real users, invisible to screen readers */}
      <div aria-hidden="true" className="absolute -left-[9999px]" tabIndex={-1}>
        <label htmlFor="website">Website</label>
        <input
          id="website"
          type="text"
          tabIndex={-1}
          autoComplete="off"
          value={values.website}
          onChange={(e) => setValues((v) => ({ ...v, website: e.target.value }))}
        />
      </div>

      <div className="flex flex-col gap-2">
        <label htmlFor="name" className="data-label">
          Name
        </label>
        <input
          id="name"
          type="text"
          value={values.name}
          onChange={(e) => setValues((v) => ({ ...v, name: e.target.value }))}
          className="rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
          aria-invalid={Boolean(errors.name)}
          aria-describedby={errors.name ? 'name-error' : undefined}
        />
        {errors.name && (
          <p id="name-error" className="text-sm text-red-400">
            {errors.name}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <label htmlFor="email" className="data-label">
          Email
        </label>
        <input
          id="email"
          type="email"
          value={values.email}
          onChange={(e) => setValues((v) => ({ ...v, email: e.target.value }))}
          className="rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
          aria-invalid={Boolean(errors.email)}
          aria-describedby={errors.email ? 'email-error' : undefined}
        />
        {errors.email && (
          <p id="email-error" className="text-sm text-red-400">
            {errors.email}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <label htmlFor="message" className="data-label">
          Message
        </label>
        <textarea
          id="message"
          rows={6}
          value={values.message}
          onChange={(e) => setValues((v) => ({ ...v, message: e.target.value }))}
          className="rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 text-[var(--color-ink)] outline-none focus:border-[var(--color-brass)]"
          aria-invalid={Boolean(errors.message)}
          aria-describedby={errors.message ? 'message-error' : undefined}
        />
        {errors.message && (
          <p id="message-error" className="text-sm text-red-400">
            {errors.message}
          </p>
        )}
      </div>

      {mutation.isError && (
        <p className="text-sm text-red-400" role="alert">
          Something went wrong sending your message. Please try again, or email directly.
        </p>
      )}

      <button
        type="submit"
        disabled={mutation.isPending}
        className="rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-medium text-[var(--color-canvas)] transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {mutation.isPending ? 'Sending…' : 'Send message'}
      </button>
    </form>
  )
}
