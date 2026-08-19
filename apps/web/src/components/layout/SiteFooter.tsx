export function SiteFooter() {
  return (
    <footer className="border-t border-[var(--color-line)]">
      <div className="mx-auto flex max-w-[1600px] flex-col gap-2 px-6 py-10 sm:flex-row sm:items-center sm:justify-between sm:px-10">
        <p className="data-label">© {new Date().getFullYear()} Shreyas</p>
        <p className="data-label">Pune, India</p>
      </div>
    </footer>
  )
}
