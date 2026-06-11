import { Link, useNavigate } from 'react-router-dom'
import { getCurrentUser, isAdmin } from '../utils/auth'
import Logo from './Logo'

export default function Navbar() {
  const navigate = useNavigate()
  const user = getCurrentUser() || {}
  const admin = isAdmin()
  const driver = user.role === 'Driver'

  const logout = () => {
    if (!window.confirm('Are you sure you want to log out?')) return

    localStorage.removeItem('token')
    localStorage.removeItem('user')
    navigate('/login')
  }

  return (
    <nav className="bg-black text-white px-10 py-5 flex items-center justify-between shadow-sm">
      <Link to="/" className="hover:opacity-90 transition">
        <Logo compact />
      </Link>

      <div className="hidden md:flex items-center gap-8 text-sm font-medium">
        <Link to="/" className="hover:text-zinc-300 transition">
          Live Map
        </Link>

        {!driver && (
          <Link to="/reservations" className="hover:text-zinc-300 transition">
            Reservations
          </Link>
        )}

        {(admin || driver) && (
          <Link to="/trips" className="hover:text-zinc-300 transition">
            Trips
          </Link>
        )}

        {admin && (
          <>
            <Link to="/admin/buses" className="hover:text-zinc-300 transition">
              Buses
            </Link>
            <Link to="/admin/users" className="hover:text-zinc-300 transition">
              Users
            </Link>
          </>
        )}

        {!driver && (
          <Link to="/notifications" className="hover:text-zinc-300 transition">
            Notifications
          </Link>
        )}

        <Link to="/about" className="hover:text-zinc-300 transition">
          About
        </Link>

        <Link to="/contact" className="hover:text-zinc-300 transition">
          Contact
        </Link>
      </div>

      <div className="flex items-center gap-4">
        <span className="hidden lg:block text-sm text-zinc-300">
          {user?.fullName || 'User'}
          {user?.role ? ` · ${user.role}` : ''}
        </span>

        <button
          onClick={logout}
          className="bg-white text-black px-5 py-2 rounded-full text-sm font-semibold hover:bg-zinc-200 transition"
        >
          Log out
        </button>
      </div>
    </nav>
  )
}
