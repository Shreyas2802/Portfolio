import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { RootLayout } from './components/RootLayout'
import { HomePage } from './routes/HomePage'
import { WorkPage } from './routes/WorkPage'
import { ProjectsPage } from './routes/ProjectsPage'
import { ProjectDetailPage } from './routes/ProjectDetailPage'
import { AboutPage } from './routes/AboutPage'
import { ContactPage } from './routes/ContactPage'

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
])

function App() {
  return <RouterProvider router={router} />
}

export default App
