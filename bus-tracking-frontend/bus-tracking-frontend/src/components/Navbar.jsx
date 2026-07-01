import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getCurrentUser, isAdmin } from '../utils/auth'
import Logo from './Logo'

export default function Navbar() {
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)
  const user = getCurrentUser() || {}
  const admin = isAdmin()
  const driver = user.role === 'Driver'

  const logout = () => {
    if (!window.confirm('Are you sure you want to log out?')) return

    localStorage.removeItem('token')
    localStorage.removeItem('user')
    navigate('/login')
  }

  const closeMenu = () => setMenuOpen(false)

  const links = [
    { to: '/', label: 'Live Map', show: true },
    { to: '/reservations', label: 'Reservations', show: !driver },
    { to: '/trips', label: 'Trips', show: admin || driver },
    { to: '/admin/buses', label: 'Buses', show: admin },
    { to: '/admin/users', label: 'Users', show: admin },
    { to: '/notifications', label: 'Notifications', show: !driver },
    { to: '/about', label: 'About', show: true },
    { to: '/contact', label: 'Contact', show: true }
  ].filter(link => link.show)

  return (
    <nav className="bg-black text-white shadow-sm">
      <div className="flex items-center justify-between px-5 py-4 md:px-10 md:py-5">
        <Link to="/" className="hover:opacity-90 transition" onClick={closeMenu}>
          <Logo compact />
        </Link>

        <div className="hidden md:flex items-center gap-8 text-sm font-medium">
          {links.map(link => (
            <Link key={link.to} to={link.to} className="hover:text-zinc-300 transition">
              {link.label}
            </Link>
          ))}
        </div>

        <div className="flex items-center gap-3 md:gap-4">
          <span className="hidden lg:block text-sm text-zinc-300">
            {user?.fullName || 'User'}
            {user?.role ? ` - ${user.role}` : ''}
          </span>

          <button
            type="button"
            onClick={() => setMenuOpen(open => !open)}
            aria-label="Toggle navigation menu"
            aria-expanded={menuOpen}
            className="inline-flex h-10 w-10 items-center justify-center rounded-lg border border-white/20 text-white transition hover:bg-white/10 md:hidden"
          >
            <span className="sr-only">Menu</span>
            <span className="flex flex-col gap-1.5">
              <span className={`block h-0.5 w-5 bg-current transition ${menuOpen ? 'translate-y-2 rotate-45' : ''}`} />
              <span className={`block h-0.5 w-5 bg-current transition ${menuOpen ? 'opacity-0' : ''}`} />
              <span className={`block h-0.5 w-5 bg-current transition ${menuOpen ? '-translate-y-2 -rotate-45' : ''}`} />
            </span>
          </button>

          <button
            onClick={logout}
            className="hidden bg-white text-black px-5 py-2 rounded-full text-sm font-semibold hover:bg-zinc-200 transition sm:inline-flex"
          >
            Log out
          </button>
        </div>
      </div>

      {menuOpen && (
        <div className="border-t border-white/10 px-5 pb-5 md:hidden">
          <div className="py-4 text-sm text-zinc-300">
            {user?.fullName || 'User'}
            {user?.role ? ` - ${user.role}` : ''}
          </div>

          <div className="grid gap-1 text-base font-medium">
            {links.map(link => (
              <Link
                key={link.to}
                to={link.to}
                onClick={closeMenu}
                className="rounded-lg px-3 py-3 transition hover:bg-white/10"
              >
                {link.label}
              </Link>
            ))}
          </div>

          <button
            onClick={logout}
            className="mt-4 w-full rounded-lg bg-white px-5 py-3 text-sm font-semibold text-black transition hover:bg-zinc-200"
          >
            Log out
          </button>
        </div>
      )}
    </nav>
  )
}
