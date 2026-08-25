import { lazy, Suspense } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { RootLayout } from './components/RootLayout'
import { HomePage } from './routes/HomePage'
import { WorkPage } from './routes/WorkPage'
import { ProjectsPage } from './routes/ProjectsPage'
import { ProjectDetailPage } from './routes/ProjectDetailPage'
import { AboutPage } from './routes/AboutPage'
import { ContactPage } from './routes/ContactPage'
import { ProtectedRoute } from './components/admin/ProtectedRoute'
import { AuthProvider } from './features/auth/AuthContext'

// Everything under /admin is code-split into its own chunk (Phase 8 -
// "bundle analysis"). A public visitor browsing the portfolio never
// downloads any of this JS; it only loads the moment someone actually
// navigates to /admin/*. The public site and the admin panel are used
// by entirely different people (visitors vs. the site owner), so
// there's no reason the former should pay the download cost for the
// latter.
const LoginPage = lazy(() => import('./routes/admin/LoginPage').then((m) => ({ default: m.LoginPage })))
const DashboardPage = lazy(() => import('./routes/admin/DashboardPage').then((m) => ({ default: m.DashboardPage })))
const PhotosAdminPage = lazy(() => import('./routes/admin/PhotosAdminPage').then((m) => ({ default: m.PhotosAdminPage })))
const AlbumsAdminPage = lazy(() => import('./routes/admin/AlbumsAdminPage').then((m) => ({ default: m.AlbumsAdminPage })))
const TaxonomyAdminPage = lazy(() => import('./routes/admin/TaxonomyAdminPage').then((m) => ({ default: m.TaxonomyAdminPage })))
const ProjectsAdminPage = lazy(() => import('./routes/admin/ProjectsAdminPage').then((m) => ({ default: m.ProjectsAdminPage })))
const AdminLayout = lazy(() => import('./components/admin/AdminLayout').then((m) => ({ default: m.AdminLayout })))

function AdminFallback() {
  return <div className="flex min-h-screen items-center justify-center text-[var(--color-ink-muted)]">Loading…</div>
}

const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'work', element: <WorkPage /> },
      { path: 'projects', element: <ProjectsPage /> },
      { path: 'projects/:slug', element: <ProjectDetailPage /> },
      { path: 'about', element: <AboutPage /> },
      { path: 'contact', element: <ContactPage /> },
    ],
  },
  {
    path: '/admin/login',
    element: (
      <Suspense fallback={<AdminFallback />}>
        <LoginPage />
      </Suspense>
    ),
  },
  {
    path: '/admin',
    element: <ProtectedRoute />,
    children: [
      {
        element: (
          <Suspense fallback={<AdminFallback />}>
            <AdminLayout />
          </Suspense>
        ),
        children: [
          { index: true, element: <Suspense fallback={<AdminFallback />}><DashboardPage /></Suspense> },
          { path: 'photos', element: <Suspense fallback={<AdminFallback />}><PhotosAdminPage /></Suspense> },
          { path: 'albums', element: <Suspense fallback={<AdminFallback />}><AlbumsAdminPage /></Suspense> },
          { path: 'taxonomy', element: <Suspense fallback={<AdminFallback />}><TaxonomyAdminPage /></Suspense> },
          { path: 'projects', element: <Suspense fallback={<AdminFallback />}><ProjectsAdminPage /></Suspense> },
        ],
      },
    ],
  },
])

function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  )
}

export default App
