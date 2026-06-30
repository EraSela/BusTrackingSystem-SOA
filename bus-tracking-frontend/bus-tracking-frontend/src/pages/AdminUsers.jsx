import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import API from '../services/api'
import { getCurrentUser } from '../utils/auth'

const roles = ['Passenger', 'Driver', 'Admin']
const emptyForm = {
  fullName: '',
  email: '',
  password: '',
  phoneNumber: '',
  role: 0,
  isActive: true
}

export default function AdminUsers() {
  const currentUser = getCurrentUser()
  const [users, setUsers] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const loadUsers = async () => {
    try {
      const response = await API.get('/users')
      setUsers(response.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load users')
    }
  }

  useEffect(() => {
    const timer = setTimeout(loadUsers, 0)
    return () => clearTimeout(timer)
  }, [])

  const resetForm = () => {
    setForm(emptyForm)
    setEditingId(null)
  }

  const submit = async (event) => {
    event.preventDefault()
    setLoading(true)
    setError('')
    setSuccess('')

    try {
      if (editingId) {
        await API.put(`/users/${editingId}`, {
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          phoneNumber: form.phoneNumber.trim(),
          role: Number(form.role),
          isActive: form.isActive
        })
        setSuccess('User updated successfully.')
      } else {
        await API.post('/users', {
          ...form,
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          phoneNumber: form.phoneNumber.trim(),
          role: Number(form.role)
        })
        setSuccess('User created successfully.')
      }

      resetForm()
      await loadUsers()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save user')
    } finally {
      setLoading(false)
    }
  }

  const editUser = (user) => {
    setEditingId(user.id)
    setForm({
      fullName: user.fullName,
      email: user.email,
      password: '',
      phoneNumber: user.phoneNumber || '',
      role: user.role,
      isActive: user.isActive
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const deleteUser = async (user) => {
    if (String(user.id) === String(currentUser.id)) {
      setError('You cannot delete the account currently signed in.')
      return
    }

    if (!window.confirm(`Delete ${user.fullName}?`)) return

    try {
      await API.delete(`/users/${user.id}`)
      setSuccess('User deleted successfully.')
      setError('')
      await loadUsers()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete user')
    }
  }

  return (
    <div className="min-h-screen bg-zinc-50">
      <Navbar />
      <main className="mx-auto max-w-7xl px-6 py-10">
        <header className="mb-10">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-zinc-500">Admin</p>
          <h1 className="mt-3 text-5xl font-bold tracking-tight text-zinc-950">User management</h1>
          <p className="mt-4 max-w-2xl text-lg text-zinc-600">
            Create accounts, assign roles, disable access and manage registered users.
          </p>
        </header>

        {(error || success) && (
          <div className={`mb-6 rounded-2xl border px-5 py-4 text-sm ${
            error ? 'border-red-200 bg-red-50 text-red-700' : 'border-green-200 bg-green-50 text-green-700'
          }`}>
            {error || success}
          </div>
        )}

        <section className="mb-10 rounded-3xl border border-zinc-200 bg-white p-7">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-2xl font-bold">{editingId ? 'Edit user' : 'Add user'}</h2>
            {editingId && <button onClick={resetForm} className="text-sm font-semibold underline">Cancel editing</button>}
          </div>

	          <form onSubmit={submit} className="max-w-md space-y-4">
	            <Field label="Full name" value={form.fullName} onChange={value => setForm({ ...form, fullName: value })} />
	            <Field label="Email" type="email" value={form.email} onChange={value => setForm({ ...form, email: value })} />
	            {!editingId && (
	              <Field label="Password" type="password" minLength="6" value={form.password} onChange={value => setForm({ ...form, password: value })} />
	            )}
	            <Field label="Phone number" required={false} value={form.phoneNumber} onChange={value => setForm({ ...form, phoneNumber: value })} />
	
	            <label className="block text-sm font-medium text-zinc-700">
	              Role
	              <select
	                value={form.role}
	                onChange={event => setForm({ ...form, role: event.target.value })}
	                className="mt-2 w-full rounded-xl border border-zinc-300 bg-white px-4 py-3"
	              >
	                {roles.map((role, index) => <option key={role} value={index}>{role}</option>)}
	              </select>
	            </label>
	
	            <label className="flex items-center gap-3 rounded-xl border border-zinc-200 px-4 py-3 text-sm font-medium">
	              <input
	                type="checkbox"
	                checked={form.isActive}
	                onChange={event => setForm({ ...form, isActive: event.target.checked })}
	              />
	              Account active
	            </label>
	
	            <div className="flex justify-end">
	              <button
	                disabled={loading}
	                className="rounded-lg bg-black px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-zinc-800 disabled:opacity-50"
	              >
	                {loading ? 'Saving...' : editingId ? 'Save changes' : 'Create user'}
	              </button>
	            </div>
	          </form>
        </section>

        <section className="overflow-hidden rounded-3xl border border-zinc-200 bg-white">
          <div className="flex items-center justify-between border-b border-zinc-200 p-6">
            <h2 className="text-2xl font-bold">Accounts</h2>
            <span className="rounded-full bg-zinc-100 px-4 py-2 text-sm">{users.length} users</span>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[850px] text-left text-sm">
              <thead className="bg-zinc-50 text-zinc-500">
                <tr>
                  {['Name', 'Email', 'Phone', 'Role', 'Status', 'Created', 'Actions'].map(label => (
                    <th key={label} className="px-6 py-4 font-semibold">{label}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {users.map(user => (
                  <tr key={user.id} className="border-t border-zinc-100">
                    <td className="px-6 py-4 font-semibold">{user.fullName}</td>
                    <td className="px-6 py-4">{user.email}</td>
                    <td className="px-6 py-4">{user.phoneNumber || 'Not provided'}</td>
                    <td className="px-6 py-4">{roles[user.role] || 'Unknown'}</td>
                    <td className="px-6 py-4">
                      <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                        user.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                      }`}>
                        {user.isActive ? 'Active' : 'Disabled'}
                      </span>
                    </td>
                    <td className="px-6 py-4">{new Date(user.createdAt).toLocaleDateString()}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-3">
                        <button onClick={() => editUser(user)} className="font-semibold underline">Edit</button>
                        <button onClick={() => deleteUser(user)} className="font-semibold text-red-600">Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>
  )
}

function Field({ label, type = 'text', value, onChange, required = true, ...props }) {
  return (
    <label className="text-sm font-medium text-zinc-700">
      {label}
      <input
        type={type}
        required={required}
        value={value}
        onChange={event => onChange(event.target.value)}
        className="mt-2 w-full rounded-xl border border-zinc-300 px-4 py-3"
        {...props}
      />
    </label>
  )
}
