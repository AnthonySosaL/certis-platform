import { motion } from 'motion/react'

import { Button } from '@/components/ui/button'

// Placeholder copy — the real homepage content depends on the platform's
// actual feature scope (still being defined). See docs/PENDING_IDEAS.md.
export function Home() {
  return (
    <main className="mx-auto max-w-6xl px-4 py-16 sm:px-6">
      <motion.section
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: 'easeOut' }}
        className="flex flex-col items-start gap-6"
      >
        <h1 className="text-4xl font-semibold tracking-tight sm:text-5xl">
          Practice. Track. Reach C1.
        </h1>
        <p className="max-w-xl text-lg text-muted-foreground">
          This is a scaffold — the navbar, theming, and layout are ready.
          Real content (exercises, progress tracking, evaluation) lands once
          the feature scope is defined.
        </p>
        <Button size="lg">Get started</Button>
      </motion.section>
    </main>
  )
}
