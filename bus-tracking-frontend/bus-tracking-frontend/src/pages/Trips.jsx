import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import API from '../services/api'
import { getCurrentUser } from '../utils/auth'

const statusLabels = ['Scheduled', 'Delayed', 'In Progress', 'Completed', 'Cancelled']
const emptyForm = {
  busId: '',
  driverId: '',
  routeId: '',
  deviceId: 'SIM808_01',
  scheduledDeparture: ''
}

export default function Trips() {
  const isAdmin = getCurrentUser().role === 'Admin'
  const [trips, setTrips] = useState([])
  const [buses, setBuses] = useState([])
  const [drivers, setDrivers] = useState([])
  const [routes, setRoutes] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)
  const [driverAssignments, setDriverAssignments] = useState({})

  const loadTrips = async () => {
    const response = await API.get('/trips')
    setTrips(response.data)
  }

  useEffect(() => {
    const timer = setTimeout(async () => {
      try {
        const requests = [API.get('/trips'), API.get('/buses')]
        if (isAdmin) requests.push(API.get('/users'), API.get('/routes'))
        const responses = await Promise.all(requests)
        setTrips(responses[0].data)
        setBuses(responses[1].data)
        if (isAdmin) {
          setDrivers(responses[2].data.filter(user => user.role === 1 && user.isActive))
          setRoutes(responses[3].data)
        }
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to load trips')
      }
    }, 0)
    return () => clearTimeout(timer)
  }, [isAdmin])

  const selectedRoute = routes.find(route => route.id === Number(form.routeId))
  const expectedArrival = form.scheduledDeparture && selectedRoute
    ? new Date(new Date(form.scheduledDeparture).getTime() + selectedRoute.expectedDurationMinutes * 60000)
    : null

  const createTrip = async (event) => {
    event.preventDefault()
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await API.post('/trips', {
        busId: Number(form.busId),
        driverId: Number(form.driverId),
        routeId: Number(form.routeId),
        deviceId: form.deviceId.trim(),
        scheduledDeparture: new Date(form.scheduledDeparture).toISOString()
      })
      setForm(emptyForm)
      setSuccess('Trip scheduled successfully.')
      await loadTrips()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to schedule trip')
    } finally {
      setLoading(false)
    }
  }

  const updateStatus = async (trip, status) => {
    setError('')
    setSuccess('')
    try {
      const payload = { status }
      if (status === 2) payload.actualDeparture = new Date().toISOString()
      if (status === 3) payload.actualArrival = new Date().toISOString()
      await API.put(`/trips/${trip.id}/status`, payload)
      setSuccess(status === 2 ? 'Trip started.' : status === 3 ? 'Trip completed.' : 'Trip cancelled.')
      await loadTrips()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update trip')
    }
  }

  const assignDriver = async (trip) => {
    const driverId = Number(driverAssignments[trip.id])
    if (!driverId) {
      setError('Select a driver first.')
      return
    }

    setError('')
    setSuccess('')
    try {
      await API.put(`/trips/${trip.id}/driver`, { driverId })
      setDriverAssignments(current => ({ ...current, [trip.id]: '' }))
      setSuccess('Driver assigned successfully.')
      await loadTrips()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to assign driver')
    }
  }

  return (
    <div className="min-h-screen bg-white">
      <Navbar />
      <main className="mx-auto max-w-7xl px-6 py-10">
        <h1 className="text-5xl font-bold text-zinc-950">Trips</h1>
        <p className="mt-4 text-lg text-zinc-600">
          {isAdmin ? 'Schedule routes and assign a bus, driver and GPS device.' : 'Manage the trips assigned to you.'}
        </p>

        {(error || success) && (
          <div className={`mt-6 rounded-xl border px-4 py-3 ${error ? 'border-red-200 bg-red-50 text-red-700' : 'border-green-200 bg-green-50 text-green-700'}`}>
            {error || success}
          </div>
        )}

        {isAdmin && (
          <section className="mt-8 rounded-3xl border border-zinc-200 p-7">
            <h2 className="text-2xl font-bold">Schedule a trip</h2>
            <form onSubmit={createTrip} className="mt-6 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
              <Select label="Route" value={form.routeId} onChange={value => setForm({ ...form, routeId: value })}>
                <option value="">Select route</option>
                {routes.map(route => <option key={route.id} value={route.id}>{route.name} ({route.expectedDurationMinutes} min)</option>)}
              </Select>
              <Select label="Bus" value={form.busId} onChange={value => setForm({ ...form, busId: value })}>
                <option value="">Select bus</option>
                {buses.filter(bus => bus.isActive).map(bus => <option key={bus.id} value={bus.id}>{bus.name} · {bus.plateNumber}</option>)}
              </Select>
              <Select label="Driver" value={form.driverId} onChange={value => setForm({ ...form, driverId: value })}>
                <option value="">Select driver</option>
                {drivers.map(driver => <option key={driver.id} value={driver.id}>{driver.fullName}</option>)}
              </Select>
              <Field label="GPS device ID" value={form.deviceId} onChange={value => setForm({ ...form, deviceId: value })} />
              <Field label="Scheduled departure" type="datetime-local" value={form.scheduledDeparture} onChange={value => setForm({ ...form, scheduledDeparture: value })} />
              <div className="rounded-xl bg-zinc-50 px-4 py-3 text-sm">
                <p className="font-medium text-zinc-700">Expected arrival</p>
                <p className="mt-2">{expectedArrival ? expectedArrival.toLocaleString() : 'Calculated from route duration'}</p>
              </div>
              <button disabled={loading} className="rounded-xl bg-black px-5 py-3 font-semibold text-white md:col-span-2 xl:col-span-3">
                {loading ? 'Scheduling...' : 'Schedule trip'}
              </button>
            </form>
          </section>
        )}

        <section className="mt-10">
          <h2 className="text-3xl font-bold">{isAdmin ? 'All trips' : 'My trips'}</h2>
          <div className="mt-6 grid gap-5">
            {trips.map(trip => (
              <article key={trip.id} className="rounded-3xl border border-zinc-200 p-6">
                <div className="flex flex-col justify-between gap-6 lg:flex-row">
                  <div>
                    <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold">{statusLabels[trip.status] || 'Unknown'}</span>
                    <h3 className="mt-4 text-xl font-bold">{trip.routeName || 'Route not assigned'}</h3>
                    <p className="mt-2 text-zinc-600">{trip.busName} · Driver: {trip.driverName || 'Not assigned'}</p>
                    <p className="mt-1 text-sm">Device: {trip.deviceId || 'Not assigned'}</p>
                    <div className="mt-4 grid gap-4 text-sm md:grid-cols-2">
                      <p><strong>Departure:</strong> {new Date(trip.scheduledDeparture).toLocaleString()}</p>
                      <p><strong>Expected arrival:</strong> {new Date(trip.scheduledArrival).toLocaleString()}</p>
                    </div>
                  </div>
                  <div className="flex flex-wrap items-center gap-3 self-start lg:self-center">
                    {isAdmin && trip.status === 0 && !trip.driverId && (
                      <div className="flex flex-wrap items-center gap-2">
                        <select
                          value={driverAssignments[trip.id] || ''}
                          onChange={event => setDriverAssignments(current => ({
                            ...current,
                            [trip.id]: event.target.value
                          }))}
                          className="h-11 min-w-44 rounded-xl border border-zinc-300 bg-white px-4 text-sm font-medium text-zinc-800 outline-none transition focus:border-black focus:ring-2 focus:ring-zinc-200"
                        >
                          <option value="">Select driver</option>
                          {drivers.map(driver => (
                            <option key={driver.id} value={driver.id}>
                              {driver.fullName}
                            </option>
                          ))}
                        </select>
                        <button onClick={() => assignDriver(trip)} className="h-11 rounded-xl bg-black px-5 text-sm font-semibold text-white transition hover:bg-zinc-800">
                          Assign
                        </button>
                      </div>
                    )}
                    {trip.status === 0 && trip.driverId && <button onClick={() => updateStatus(trip, 2)} className="h-11 rounded-xl bg-black px-5 text-sm font-semibold text-white transition hover:bg-zinc-800">Start</button>}
                    {(trip.status === 1 || trip.status === 2) && <button onClick={() => updateStatus(trip, 3)} className="h-11 rounded-xl bg-black px-5 text-sm font-semibold text-white transition hover:bg-zinc-800">Complete</button>}
                    {(trip.status === 0 || trip.status === 1 || trip.status === 2) && <button onClick={() => updateStatus(trip, 4)} className="h-11 rounded-xl border border-red-200 bg-red-50 px-5 text-sm font-semibold text-red-700 transition hover:bg-red-100">Cancel</button>}
                  </div>
                </div>
              </article>
            ))}
            {trips.length === 0 && <p className="rounded-3xl border p-8 text-zinc-500">No trips found.</p>}
          </div>
        </section>
      </main>
    </div>
  )
}

function Field({ label, type = 'text', value, onChange }) {
  return (
    <label className="text-sm font-medium">
      {label}
      <input required type={type} value={value} onChange={event => onChange(event.target.value)} className="mt-2 w-full rounded-xl border px-4 py-3" />
    </label>
  )
}

function Select({ label, value, onChange, children }) {
  return (
    <label className="text-sm font-medium">
      {label}
      <select required value={value} onChange={event => onChange(event.target.value)} className="mt-2 w-full rounded-xl border px-4 py-3">
        {children}
      </select>
    </label>
  )
}
