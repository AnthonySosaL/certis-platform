import { motion } from 'motion/react'

import { Button } from '@/components/ui/button'

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
          Fuel your training. Track your progress.
        </h1>
        <p className="max-w-xl text-lg text-muted-foreground">
          NutriBoost is a work-in-progress storefront for fitness nutrition products.
          This page is a scaffold — catalog, cart, and checkout land next.
        </p>
        <Button size="lg">Shop the collection</Button>
      </motion.section>
    </main>
  )
}
