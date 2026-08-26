import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { motion } from 'motion/react'
import { Menu, Moon, ShoppingCart, Sun } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { useTheme } from '@/app/providers/theme-provider'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'
import { cn } from '@/lib/utils'

type NavItem = {
  to: string
  labelKey: 'nav.home' | 'nav.shop' | 'nav.about' | 'nav.contact'
}

const NAV_ITEMS: NavItem[] = [
  { to: '/', labelKey: 'nav.home' },
  { to: '/shop', labelKey: 'nav.shop' },
  { to: '/about', labelKey: 'nav.about' },
  { to: '/contact', labelKey: 'nav.contact' },
]

// Placeholder cart count until the cart feature module wires in real state.
const CART_ITEM_COUNT = 0

export function Navbar() {
  const { t } = useTranslation()
  const { theme, toggleTheme } = useTheme()
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <motion.header
      initial={{ y: -24, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.4, ease: 'easeOut' }}
      className="sticky top-0 z-50 w-full border-b border-border/60 bg-background/80 backdrop-blur supports-[backdrop-filter]:bg-background/60"
    >
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4 sm:px-6">
        <Link to="/" className="flex items-center gap-2 font-semibold tracking-tight">
          <span
            aria-hidden="true"
            className="inline-flex size-8 items-center justify-center rounded-full bg-primary text-primary-foreground"
          >
            N
          </span>
          <span className="text-lg">{t('brand.name')}</span>
        </Link>

        <nav aria-label="Primary" className="hidden md:block">
          <ul className="flex items-center gap-1">
            {NAV_ITEMS.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.to === '/'}
                  className={({ isActive }) =>
                    cn(
                      'rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:text-foreground',
                      isActive && 'text-foreground',
                    )
                  }
                >
                  {t(item.labelKey)}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="icon"
            aria-label={theme === 'light' ? 'Switch to dark mode' : 'Switch to light mode'}
            onClick={toggleTheme}
          >
            {theme === 'light' ? <Moon /> : <Sun />}
          </Button>

          <Button variant="ghost" size="icon" aria-label={t('nav.cart')} className="relative">
            <ShoppingCart />
            {CART_ITEM_COUNT > 0 && (
              <Badge className="absolute -right-1 -top-1 size-5 justify-center rounded-full p-0 text-[11px]">
                {CART_ITEM_COUNT}
              </Badge>
            )}
          </Button>

          <Button className="hidden sm:inline-flex" size="sm">
            {t('nav.signIn')}
          </Button>

          <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon" aria-label={t('nav.menu')} className="md:hidden">
                <Menu />
              </Button>
            </SheetTrigger>
            <SheetContent side="right" className="w-72">
              <SheetHeader>
                <SheetTitle>{t('brand.name')}</SheetTitle>
              </SheetHeader>
              <nav aria-label="Mobile" className="flex flex-col gap-1 px-4">
                {NAV_ITEMS.map((item) => (
                  <SheetClose asChild key={item.to}>
                    <NavLink
                      to={item.to}
                      end={item.to === '/'}
                      className={({ isActive }) =>
                        cn(
                          'rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground',
                          isActive && 'bg-accent text-accent-foreground',
                        )
                      }
                    >
                      {t(item.labelKey)}
                    </NavLink>
                  </SheetClose>
                ))}
                <SheetClose asChild>
                  <Button className="mt-2">{t('nav.signIn')}</Button>
                </SheetClose>
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </motion.header>
  )
}
