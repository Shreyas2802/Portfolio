import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import App from './App.tsx'

// One shared QueryClient for the whole app — this is the "server
// state" store the spec calls for instead of Redux. Component-local
// UI state (open/closed, form inputs) still uses plain useState;
// TanStack Query only owns data that comes from the API.
const queryClient = new QueryClient()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
)
