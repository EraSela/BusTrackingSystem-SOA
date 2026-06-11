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

            <input
              type="password"
              required
              value={form.password}
              onChange={(e) =>
                setForm({
                  ...form,
                  password: e.target.value
                })
              }
              className="w-full border border-zinc-300 rounded-xl px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-black"
              placeholder="••••••••"
            />
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