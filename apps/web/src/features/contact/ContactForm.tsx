import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { z } from 'zod'
import { apiPost } from '../../lib/api'

const contactSchema = z.object({
  name:    z.string().trim().min(1, 'Name is required').max(200),
  email:   z.string().trim().email('Enter a valid email address').max(320),
  message: z.string().trim().min(1, 'Message is required').max(4000),
})

type ContactFormValues = z.infer<typeof contactSchema>

interface FieldErrors {
  name?: string
  email?: string
  message?: string
}

const INPUT_CLASS =
  'rounded-md border border-[var(--color-line)] bg-transparent px-4 py-3 ' +
  'text-[var(--color-ink)] transition-colors ' +
  // Replace outline:none with a visible focus style that passes WCAG 2.4.11
  'focus:outline-none focus:border-[var(--color-gold)] focus:ring-2 ' +
  'focus:ring-[var(--color-gold)]/30 focus:ring-offset-0'

export function ContactForm() {
  const successRef = useRef<HTMLParagraphElement>(null)
  const [values, setValues] = useState<ContactFormValues & { website: string }>({
    name: '', email: '', message: '', website: '',
  })
  const [errors, setErrors] = useState<FieldErrors>({})

  const mutation = useMutation({
    mutationFn: (payload: ContactFormValues & { website: string }) =>
      apiPost<typeof payload, { status: string }>('/contact', payload),
  })

  // Move focus to success message when form submits successfully
  // so screen reader users hear the confirmation without hunting for it
  useEffect(() => {
    if (mutation.isSuccess) successRef.current?.focus()
  }, [mutation.isSuccess])

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
      <p
        ref={successRef}
        tabIndex={-1}
        role="status"
        className="rounded-lg border border-[var(--color-line)] bg-[var(--color-canvas-raised)] px-6 py-8 text-[var(--color-ink)] focus:outline-none"
      >
        Thanks — your message has been sent. I'll get back to you soon.
      </p>
    )
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="flex max-w-lg flex-col gap-6">
      {/* Honeypot — visually and semantically hidden */}
      <div
        aria-hidden="true"
        style={{ position: 'absolute', left: '-9999px', width: '1px', height: '1px', overflow: 'hidden' }}
      >
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

      {/* Name */}
      <div className="flex flex-col gap-2">
        <label htmlFor="name" className="text-sm font-medium text-[var(--color-ink)]">
          Name
        </label>
        <input
          id="name"
          type="text"
          autoComplete="name"
          value={values.name}
          onChange={(e) => setValues((v) => ({ ...v, name: e.target.value }))}
          className={INPUT_CLASS}
          aria-invalid={Boolean(errors.name)}
          aria-describedby={errors.name ? 'name-error' : undefined}
        />
        {errors.name && (
          <p id="name-error" role="alert" className="text-sm text-red-400">{errors.name}</p>
        )}
      </div>

      {/* Email */}
      <div className="flex flex-col gap-2">
        <label htmlFor="email" className="text-sm font-medium text-[var(--color-ink)]">
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          value={values.email}
          onChange={(e) => setValues((v) => ({ ...v, email: e.target.value }))}
          className={INPUT_CLASS}
          aria-invalid={Boolean(errors.email)}
          aria-describedby={errors.email ? 'email-error' : undefined}
        />
        {errors.email && (
          <p id="email-error" role="alert" className="text-sm text-red-400">{errors.email}</p>
        )}
      </div>

      {/* Message */}
      <div className="flex flex-col gap-2">
        <label htmlFor="message" className="text-sm font-medium text-[var(--color-ink)]">
          Message
        </label>
        <textarea
          id="message"
          rows={6}
          value={values.message}
          onChange={(e) => setValues((v) => ({ ...v, message: e.target.value }))}
          className={`${INPUT_CLASS} resize-y`}
          aria-invalid={Boolean(errors.message)}
          aria-describedby={errors.message ? 'message-error' : undefined}
        />
        {errors.message && (
          <p id="message-error" role="alert" className="text-sm text-red-400">{errors.message}</p>
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
        className="self-start rounded-full bg-[var(--color-ink)] px-6 py-3 text-sm font-semibold text-[var(--color-canvas)] transition-opacity hover:opacity-85 disabled:opacity-50 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-gold)]"
      >
        {mutation.isPending ? 'Sending…' : 'Send message'}
      </button>
    </form>
  )
}
