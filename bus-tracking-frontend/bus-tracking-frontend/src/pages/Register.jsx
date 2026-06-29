import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import API from '../services/api'
import Logo from '../components/Logo'

export default function Register() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    fullName: '',
    email: '',
    password: '',
    phoneNumber: ''
  })

  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      await API.post('/auth/register', {
        ...form,
        role: 0
      })

      navigate('/login')
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Registration failed'
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-zinc-950 via-zinc-900 to-black flex items-center justify-center px-4">
      <div className="bg-white rounded-3xl shadow-2xl p-8 w-full max-w-sm">
        <div className="mb-6">
          <Logo />

          <p className="text-center text-gray-500 mt-2">
            Create your passenger account
          </p>
        </div>

        {error && (
          <div className="bg-red-50 text-red-600 border border-red-200 rounded-xl px-4 py-3 mb-5 text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Full name
            </label>

            <input
              type="text"
              required
              value={form.fullName}
              onChange={(e) =>
                setForm({
                  ...form,
                  fullName: e.target.value
                })
              }
              className="w-full border border-zinc-300 rounded-xl px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-black"
              placeholder="Era Sela"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>

            <input
              type="email"
              required
              value={form.email}
              onChange={(e) =>
                setForm({
                  ...form,
                  email: e.target.value
                })
              }
              className="w-full border border-zinc-300 rounded-xl px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-black"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>

            <div className="relative">
            <input
              type={showPassword ? 'text' : 'password'}
              required
              value={form.password}
              onChange={(e) =>
                setForm({
                  ...form,
                  password: e.target.value
                })
              }
              className="w-full border border-zinc-300 rounded-xl px-4 py-2.5 pr-12 focus:outline-none focus:ring-2 focus:ring-black"
              placeholder="••••••••"
            />
              <button
                type="button"
                onClick={() => setShowPassword(value => !value)}
                className="absolute inset-y-0 right-3 flex items-center text-gray-500 hover:text-zinc-900"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
              >
                {showPassword ? <EyeOffIcon /> : <EyeIcon />}
              </button>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Phone number
            </label>

            <input
              type="text"
              value={form.phoneNumber}
              onChange={(e) =>
                setForm({
                  ...form,
                  phoneNumber: e.target.value
                })
              }
              className="w-full border border-zinc-300 rounded-xl px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-black"
              placeholder="070123456"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-black text-white py-2.5 rounded-xl font-semibold hover:bg-zinc-800 transition disabled:opacity-50"
          >
            {loading
              ? 'Creating account...'
              : 'Create account'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-500 mt-5">
          Already have an account?{' '}
          <Link
            to="/login"
            className="text-zinc-950 font-semibold underline underline-offset-4"
          >
            Log in
          </Link>
        </p>
      </div>
    </div>
  )
}

function EyeIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12Z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
      <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="2" />
    </svg>
  )
}

function EyeOffIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path d="M3 3l18 18" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <path d="M10.6 5.1A10.8 10.8 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-2.1 3.1M6.6 6.6C3.7 8.5 2 12 2 12s3.5 7 10 7a9.7 9.7 0 0 0 4.3-1" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M9.9 9.9A3 3 0 0 0 14.1 14.1" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}
