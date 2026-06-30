import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import API from '../services/api'

const emptyForm = {
  name: '',
  plateNumber: '',
  totalSeats: 40,
  isActive: true
}

export default function AdminBuses() {
  const [buses, setBuses] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const loadData = async () => {
    try {
      const busResponse = await API.get('/buses')
      setBuses(busResponse.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load bus management data')
    }
  }

  useEffect(() => {
    const timer = setTimeout(loadData, 0)
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

    const payload = {
      name: form.name.trim(),
      plateNumber: form.plateNumber.trim(),
      totalSeats: Number(form.totalSeats),
      ...(editingId ? {
        isActive: form.isActive
      } : {})
    }

    try {
      if (editingId) {
        await API.put(`/buses/${editingId}`, payload)
        setSuccess('Bus updated successfully.')
      } else {
        await API.post('/buses', payload)
        setSuccess('Bus created successfully.')
      }

      resetForm()
      await loadData()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save bus')
    } finally {
      setLoading(false)
    }
  }

  const editBus = (bus) => {
    setEditingId(bus.id)
    setForm({
      name: bus.name,
      plateNumber: bus.plateNumber,
      totalSeats: bus.totalSeats,
      isActive: bus.isActive
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const deleteBus = async (bus) => {
    if (!window.confirm(`Delete ${bus.name}?`)) return

    setError('')
    setSuccess('')

    try {
      await API.delete(`/buses/${bus.id}`)
      setSuccess('Bus deleted successfully.')
      await loadData()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete bus')
    }
  }

  return (
    <div className="min-h-screen bg-zinc-50">
      <Navbar />
      <main className="mx-auto max-w-7xl px-6 py-10">
        <header className="mb-10">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-zinc-500">Admin</p>
          <h1 className="mt-3 text-5xl font-bold tracking-tight text-zinc-950">Bus management</h1>
          <p className="mt-4 max-w-2xl text-lg text-zinc-600">
            Create vehicles and control their seating capacity and availability.
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
          <div className="mb-6 flex items-center justify-between gap-4">
            <h2 className="text-2xl font-bold">{editingId ? 'Edit bus' : 'Add bus'}</h2>
            {editingId && (
              <button type="button" onClick={resetForm} className="text-sm font-semibold underline">
                Cancel editing
              </button>
            )}
          </div>

          <form onSubmit={submit}>
            <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-4">
              <Field label="Bus name" value={form.name} onChange={value => setForm({ ...form, name: value })} />
              <Field label="Plate number" value={form.plateNumber} onChange={value => setForm({ ...form, plateNumber: value })} />
              <Field label="Total seats" type="number" min="1" max="100" value={form.totalSeats} onChange={value => setForm({ ...form, totalSeats: value })} />

              {editingId && (
                <label className="flex items-center gap-3 rounded-xl border border-zinc-200 px-4 py-3 text-sm font-medium">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={event => setForm({ ...form, isActive: event.target.checked })}
                  />
                  Available for bookings
                </label>
              )}
            </div>

            <div className="mt-5 flex justify-end">
              <button
                disabled={loading}
                className="rounded-lg bg-black px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-zinc-800 disabled:opacity-50"
              >
                {loading ? 'Saving...' : editingId ? 'Save changes' : 'Create bus'}
              </button>
            </div>
          </form>
        </section>

        <section className="overflow-hidden rounded-3xl border border-zinc-200 bg-white">
          <div className="flex items-center justify-between border-b border-zinc-200 p-6">
            <h2 className="text-2xl font-bold">Fleet</h2>
            <span className="rounded-full bg-zinc-100 px-4 py-2 text-sm">{buses.length} buses</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] text-left text-sm">
              <thead className="bg-zinc-50 text-zinc-500">
                <tr>
                  {['Bus', 'Plate', 'Seats', 'Status', 'Actions'].map(label => (
                    <th key={label} className="px-6 py-4 font-semibold">{label}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {buses.map(bus => (
                  <tr key={bus.id} className="border-t border-zinc-100">
                    <td className="px-6 py-4 font-semibold text-zinc-950">{bus.name}</td>
                    <td className="px-6 py-4">{bus.plateNumber}</td>
                    <td className="px-6 py-4">{bus.totalSeats}</td>
                    <td className="px-6 py-4">
                      <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                        bus.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                      }`}>
                        {bus.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex gap-3">
                        <button onClick={() => editBus(bus)} className="font-semibold underline">Edit</button>
                        <button onClick={() => deleteBus(bus)} className="font-semibold text-red-600">Delete</button>
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

function Field({ label, type = 'text', value, onChange, ...props }) {
  return (
    <label className="text-sm font-medium text-zinc-700">
      {label}
      <input
        required
        type={type}
        value={value}
        onChange={event => onChange(event.target.value)}
        className="mt-2 w-full rounded-xl border border-zinc-300 px-4 py-3"
        {...props}
      />
    </label>
  )
}
