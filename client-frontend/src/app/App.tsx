import { Route, Routes } from 'react-router-dom'

import { ThemeProvider } from '@/app/providers/theme-provider'
import { Footer } from '@/components/layout/Footer'
import { Navbar } from '@/components/layout/Navbar'
import { Home } from '@/pages/Home'

function Placeholder({ title }: { title: string }) {
  return (
    <main className="mx-auto max-w-6xl px-4 py-16 sm:px-6">
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="mt-2 text-muted-foreground">Coming soon.</p>
    </main>
  )
}

export function App() {
  return (
    <ThemeProvider>
      <div className="flex min-h-svh flex-col">
        <Navbar />
        <div className="flex-1">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/shop" element={<Placeholder title="Shop" />} />
            <Route path="/about" element={<Placeholder title="About" />} />
            <Route path="/contact" element={<Placeholder title="Contact" />} />
          </Routes>
        </div>
        <Footer />
      </div>
    </ThemeProvider>
  )
}
