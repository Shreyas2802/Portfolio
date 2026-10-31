interface DataLabelProps {
  /** e.g. "f/2.8", "35mm", "1/500s", "ISO 200" */
  items: string[]
}

/**
 * The site's signature visual device: technical camera-setting labels
 * rendered like a viewfinder HUD. This is intentional, not decorative —
 * it's the one place the "photographer" and "developer" halves of the
 * brief overlap visually, instead of living in separate sections.
 * Real data will come from Photo.CameraMake/Aperture/etc (see the
 * spec's domain model) once the API serves real photos in Phase 4.
 */
export function DataLabel({ items }: DataLabelProps) {
  // aria-hidden because these are currently placeholder/decorative
  // values. Once Phase 4 wires up real Photo.Aperture/FocalLength/etc,
  // revisit this — real EXIF data is meaningful content and should be
  // readable by screen readers, not hidden.
  return (
    <div className="data-label flex flex-wrap gap-x-3 gap-y-1" aria-hidden="true">
      {items.map((item, i) => (
        <span key={item} className="flex items-center gap-3">
          {item}
          {i < items.length - 1 && <span className="text-[var(--color-brass-dim)]">/</span>}
        </span>
      ))}
    </div>
  )
}
