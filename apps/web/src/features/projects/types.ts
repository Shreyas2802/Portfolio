export interface Project {
  id: number
  title: string
  slug: string
  tagline: string
  role: string
  status: string
  overview: string
  contributions: string[]
  technologies: string[]
  githubUrl: string | null
  liveUrl: string | null
  coverImageUrl: string | null
  isFeatured: boolean
}
