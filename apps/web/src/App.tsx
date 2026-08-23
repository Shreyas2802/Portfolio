import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { RootLayout } from './components/RootLayout'
import { HomePage } from './routes/HomePage'
import { WorkPage } from './routes/WorkPage'
import { ProjectsPage } from './routes/ProjectsPage'
import { ProjectDetailPage } from './routes/ProjectDetailPage'
import { AboutPage } from './routes/AboutPage'
import { ContactPage } from './routes/ContactPage'
import { LoginPage } from './routes/admin/LoginPage'
import { DashboardPage } from './routes/admin/DashboardPage'
import { PhotosAdminPage } from './routes/admin/PhotosAdminPage'
import { AlbumsAdminPage } from './routes/admin/AlbumsAdminPage'
import { TaxonomyAdminPage } from './routes/admin/TaxonomyAdminPage'
import { ProjectsAdminPage } from './routes/admin/ProjectsAdminPage'
import { ProtectedRoute } from './components/admin/ProtectedRoute'
import { AdminLayout } from './components/admin/AdminLayout'
import { AuthProvider } from './features/auth/AuthContext'

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
  { path: '/admin/login', element: <LoginPage /> },
  {
    path: '/admin',
    element: <ProtectedRoute />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'photos', element: <PhotosAdminPage /> },
          { path: 'albums', element: <AlbumsAdminPage /> },
          { path: 'taxonomy', element: <TaxonomyAdminPage /> },
          { path: 'projects', element: <ProjectsAdminPage /> },
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
