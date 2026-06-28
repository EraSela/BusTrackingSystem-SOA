import { useCallback, useEffect, useState } from 'react'
import { QRCodeCanvas } from 'qrcode.react'
import Navbar from '../components/Navbar'
import API from '../services/api'
import { getCurrentUser } from '../utils/auth'

const pickupPlaces = [
  { name: 'Struga Bus Station', lat: 41.17799, lng: 20.67784, order: 0 },
  { name: 'Kicevo Bus Station', lat: 41.5122, lng: 20.9582, order: 1 },
  { name: 'Gostivar Bus Station', lat: 41.8006, lng: 20.9142, order: 2 },
  { name: 'SEEU Tetovo Campus', lat: 41.9957, lng: 20.9615, order: 3 },
  { name: 'Tetovo Bus Station', lat: 42.0069, lng: 20.9715, order: 4 },
  { name: 'Skopje Transport Center', lat: 41.9925, lng: 21.4314, order: 5 }
]

const tripStatusLabels = ['Scheduled', 'Delayed', 'In progress', 'Completed', 'Cancelled']

const emptyForm = {
  tripId: '',
  scheduleId: '',
  travelDate: '',
  seatNumber: '',
  pickupPlaceName: '',
  pickupLatitude: '',
  pickupLongitude: ''
}

const todayValue = () => {
  const now = new Date()
  const offset = now.getTimezoneOffset() * 60000
  return new Date(now.getTime() - offset).toISOString().slice(0, 10)
}

export default function Reservations() {
  const isPassenger = getCurrentUser().role === 'Passenger'
  const [timetable, setTimetable] = useState([])
  const [activeTrips, setActiveTrips] = useState([])
  const [liveLocations, setLiveLocations] = useState([])
  const [reservations, setReservations] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [etas, setEtas] = useState({})
  const [confirmed, setConfirmed] = useState(null)
  const [selectedQr, setSelectedQr] = useState(null)
  const [message, setMessage] = useState({ error: '', success: '' })
  const [loading, setLoading] = useState(false)

  const loadData = useCallback(async () => {
    try {
      const requests = [
        API.get('/reservations/timetable'),
        API.get(isPassenger ? '/reservations/my' : '/reservations')
      ]
      if (isPassenger) requests.push(API.get('/trips'), API.get('/locations/live'))

      const [tripResponse, reservationResponse, activeTripResponse, liveLocationResponse] = await Promise.all(requests)
      setTimetable(tripResponse.data)
      setReservations(reservationResponse.data)
      if (isPassenger) {
        setActiveTrips(activeTripResponse.data.filter(trip => trip.status === 1 || trip.status === 2))
        setLiveLocations(liveLocationResponse.data)
      }
    } catch (err) {
      setMessage({ error: err.response?.data?.message || 'Failed to load reservations', success: '' })
    }
  }, [isPassenger])

  useEffect(() => {
    const timer = setTimeout(loadData, 0)
    return () => clearTimeout(timer)
  }, [loadData])

  const selectedSchedule = timetable.find(item => item.id === form.scheduleId)
  const selectedActiveTrip = activeTrips.find(item => item.id === Number(form.tripId))
  const selectedTripValue = form.tripId
    ? `trip:${form.tripId}`
    : form.scheduleId
      ? `schedule:${form.scheduleId}`
      : ''
  const selectedDay = form.travelDate
    ? new Date(`${form.travelDate}T12:00:00`).getDay()
    : null
  const availableSchedules = selectedDay === null
    ? []
    : timetable.filter(item =>
        item.availableDays.includes(selectedDay) &&
        new Date(`${form.travelDate}T${item.departureTime}:00`) > new Date()
      )
  const expectedArrival = selectedActiveTrip
    ? new Date(selectedActiveTrip.scheduledArrival)
    : selectedSchedule && form.travelDate
      ? new Date(
          new Date(`${form.travelDate}T${selectedSchedule.departureTime}:00`).getTime() +
          selectedSchedule.expectedDurationMinutes * 60000
        )
      : null
  const selectedSeatLimit = selectedActiveTrip?.totalSeats || selectedSchedule?.totalSeats || 100
  const selectedSeatsBooked = selectedActiveTrip?.reservationCount ?? 0
  const selectedSeatsAvailable = selectedActiveTrip
    ? Math.max(selectedActiveTrip.totalSeats - selectedSeatsBooked, 0)
    : null
  const selectedLiveLocation = selectedActiveTrip
    ? liveLocations.find(location => location.busId === selectedActiveTrip.busId)
    : null
  const availablePickupPlaces = selectedActiveTrip
    ? getBookablePickupPlaces(selectedActiveTrip, selectedLiveLocation)
    : pickupPlaces

  const selectTrip = (value) => {
    if (value.startsWith('trip:')) {
      setForm({
        ...form,
        tripId: value.replace('trip:', ''),
        scheduleId: '',
        seatNumber: '',
        pickupPlaceName: '',
        pickupLatitude: '',
        pickupLongitude: ''
      })
      return
    }

    if (value.startsWith('schedule:')) {
      setForm({
        ...form,
        tripId: '',
        scheduleId: value.replace('schedule:', ''),
        seatNumber: '',
        pickupPlaceName: '',
        pickupLatitude: '',
        pickupLongitude: ''
      })
      return
    }

    setForm({
      ...form,
      tripId: '',
      scheduleId: '',
      seatNumber: '',
      pickupPlaceName: '',
      pickupLatitude: '',
      pickupLongitude: ''
    })
  }

  const selectPickup = (event) => {
    const pickup = availablePickupPlaces.find(item => item.name === event.target.value)
    setForm({
      ...form,
      pickupPlaceName: pickup?.name || '',
      pickupLatitude: pickup?.lat || '',
      pickupLongitude: pickup?.lng || ''
    })
  }

  const submit = async (event) => {
    event.preventDefault()
    setLoading(true)
    setMessage({ error: '', success: '' })
    try {
      const response = await API.post('/reservations', {
        tripId: form.tripId ? Number(form.tripId) : null,
        scheduleId: form.tripId ? null : form.scheduleId,
        travelDate: form.tripId ? null : form.travelDate,
        seatNumber: Number(form.seatNumber),
        pickupPlaceName: form.pickupPlaceName,
        pickupLatitude: Number(form.pickupLatitude),
        pickupLongitude: Number(form.pickupLongitude)
      })
      setConfirmed(response.data)
      setForm(emptyForm)
      setMessage({ error: '', success: 'Reservation created successfully.' })
      await loadData()
    } catch (err) {
      setMessage({ error: err.response?.data?.message || 'Failed to create reservation', success: '' })
    } finally {
      setLoading(false)
    }
  }

  const remove = async (id) => {
    if (!window.confirm('Delete this reservation?')) return
    try {
      await API.delete(`/reservations/${id}`)
      await loadData()
    } catch (err) {
      setMessage({ error: err.response?.data?.message || 'Failed to delete reservation', success: '' })
    }
  }

  const getEta = async (id) => {
    try {
      const response = await API.get(`/locations/eta/reservation/${id}`)
      setEtas(current => ({ ...current, [id]: response.data }))
    } catch (err) {
      setMessage({ error: err.response?.data?.message || 'ETA is available after the trip starts', success: '' })
    }
  }

  return (
    <div className="min-h-screen bg-white">
      <Navbar />
      <main className="mx-auto max-w-7xl px-6 py-10">
        <h1 className="text-5xl font-bold text-zinc-950">Reservations</h1>
        <p className="mt-4 text-lg text-zinc-600">Choose a future departure or a trip already in progress, then pick your seat and pickup point.</p>

        {(message.error || message.success) && (
          <div className={`mt-6 rounded-xl border px-4 py-3 ${message.error ? 'border-red-200 bg-red-50 text-red-700' : 'border-green-200 bg-green-50 text-green-700'}`}>
            {message.error || message.success}
          </div>
        )}

        {isPassenger && <section className="mt-8 rounded-3xl border border-zinc-200 p-7">
          <h2 className="text-2xl font-bold">Book a trip</h2>
          {activeTrips.length > 0 && (
            <div className="mt-6">
              <div className="flex flex-col gap-1 md:flex-row md:items-end md:justify-between">
                <h3 className="text-lg font-bold">Trips in progress</h3>
                <p className="text-sm text-zinc-500">Choose one if you want to join a bus already on the route.</p>
              </div>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                {activeTrips.map(trip => {
                  const seatsAvailable = Math.max(trip.totalSeats - (trip.reservationCount ?? 0), 0)
                  const isSelected = Number(form.tripId) === trip.id

                  return (
                    <button
                      key={trip.id}
                      type="button"
                      onClick={() => selectTrip(`trip:${trip.id}`)}
                      className={`rounded-2xl border p-4 text-left transition ${isSelected ? 'border-black bg-zinc-50' : 'border-zinc-200 hover:border-zinc-400'}`}
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-semibold text-zinc-950">{trip.routeName || 'Active trip'}</p>
                          <p className="mt-1 text-sm text-zinc-500">
                            {trip.busName} · {tripStatusLabels[trip.status] || 'Active'}
                          </p>
                        </div>
                        <span className="rounded-full bg-green-100 px-3 py-1 text-xs font-semibold text-green-700">
                          {seatsAvailable} seats left
                        </span>
                      </div>
                      <div className="mt-3 grid gap-2 text-sm text-zinc-600 sm:grid-cols-2">
                        <p>Departed {new Date(trip.scheduledDeparture).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                        <p>Arrives {new Date(trip.scheduledArrival).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                      </div>
                    </button>
                  )
                })}
              </div>
            </div>
          )}
          <form onSubmit={submit} className="mt-6 grid gap-5 md:grid-cols-2">
            <label className="text-sm font-medium">
              Travel date for advance booking
              <input
                type="date"
                min={todayValue()}
                value={form.travelDate}
                onChange={event => setForm({
                  ...form,
                  travelDate: event.target.value,
                  tripId: '',
                  scheduleId: '',
                  seatNumber: ''
                })}
                className="mt-2 w-full rounded-xl border px-4 py-3"
              />
            </label>

            <label className="text-sm font-medium">
              Trip
              <select required value={selectedTripValue} onChange={event => selectTrip(event.target.value)} className="mt-2 w-full rounded-xl border px-4 py-3">
                <option value="">Select a trip</option>
                {activeTrips.length > 0 && (
                  <optgroup label="In progress now">
                    {activeTrips.map(trip => (
                      <option key={trip.id} value={`trip:${trip.id}`}>
                        {trip.routeName || 'Active trip'} - {new Date(trip.scheduledDeparture).toLocaleTimeString([], {
                          hour: '2-digit',
                          minute: '2-digit'
                        })} - {Math.max(trip.totalSeats - (trip.reservationCount ?? 0), 0)} seats left
                      </option>
                    ))}
                  </optgroup>
                )}
                {availableSchedules.length > 0 && (
                  <optgroup label="Future departures">
                    {availableSchedules.map(item => (
                      <option key={item.id} value={`schedule:${item.id}`}>
                        {item.routeName} - {item.departureTime}
                      </option>
                    ))}
                  </optgroup>
                )}
                {!form.travelDate && activeTrips.length === 0 && (
                  <option disabled value="">Select a travel date to see future departures</option>
                )}
                {form.travelDate && availableSchedules.length === 0 && activeTrips.length === 0 && (
                  <option disabled value="">No bookable trips for this date</option>
                )}
              </select>
            </label>

            <label className="text-sm font-medium">
              Seat number
              <input required type="number" min="1" max={selectedSeatLimit} disabled={!selectedSchedule && !selectedActiveTrip} value={form.seatNumber} onChange={event => setForm({ ...form, seatNumber: event.target.value })} className="mt-2 w-full rounded-xl border px-4 py-3" />
              {selectedActiveTrip && (
                <span className="mt-2 block text-xs text-zinc-500">
                  {selectedSeatsAvailable} of {selectedActiveTrip.totalSeats} seats available.
                </span>
              )}
            </label>

            <label className="text-sm font-medium">
              Pickup place
              <select required value={form.pickupPlaceName} onChange={selectPickup} className="mt-2 w-full rounded-xl border px-4 py-3">
                <option value="">Select pickup place</option>
                {availablePickupPlaces.map(place => <option key={place.name} value={place.name}>{place.name}</option>)}
              </select>
              {selectedActiveTrip && availablePickupPlaces.length < pickupPlaces.length && (
                <span className="mt-2 block text-xs text-zinc-500">
                  Pickup places already behind the active bus are hidden.
                </span>
              )}
            </label>

            <div className="rounded-xl bg-zinc-50 px-4 py-3 text-sm">
              {selectedActiveTrip ? (
                <>
                  <p className="font-semibold">{selectedActiveTrip.routeName || 'Trip in progress'}</p>
                  <p className="text-zinc-600">
                    In progress now - expected arrival: {expectedArrival?.toLocaleString()}
                  </p>
                  <p className="mt-1 text-zinc-600">
                    {selectedSeatsBooked} / {selectedActiveTrip.totalSeats} seats booked.
                  </p>
                </>
              ) : selectedSchedule ? (
                <>
                  <p className="font-semibold">{selectedSchedule.routeName}</p>
                  <p className="text-zinc-600">
                    Expected arrival: {expectedArrival?.toLocaleString()}
                  </p>
                </>
              ) : 'Select an active trip or choose a date for future departures.'}
            </div>

            <button disabled={loading} className="rounded-xl bg-black px-5 py-3 font-semibold text-white md:col-span-2">
              {loading ? 'Booking...' : 'Book seat'}
            </button>
          </form>
        </section>}

        <section className="mt-10">
          <h2 className="text-3xl font-bold">{isPassenger ? 'My reservations' : 'All reservations'}</h2>
          <div className="mt-6 grid gap-5">
            {reservations.map(reservation => (
              <article key={reservation.id} className="flex flex-col justify-between gap-6 rounded-3xl border p-6 md:flex-row">
                <div>
                  <h3 className="text-xl font-bold">{reservation.routeName}</h3>
                  <p className="mt-2 text-zinc-600">{reservation.busName}</p>
                  <p className="mt-1">Departure: {new Date(reservation.scheduledDeparture).toLocaleString()}</p>
                  <p>Seat {reservation.seatNumber} · {reservation.pickupPlaceName}</p>
                  <div className="mt-4 flex gap-4">
                    <button onClick={() => getEta(reservation.id)} className="font-semibold underline">
                      {etas[reservation.id] ? `${etas[reservation.id].estimatedMinutes} min away` : 'Check ETA'}
                    </button>
                    <button onClick={() => remove(reservation.id)} className="font-semibold text-red-600">Delete</button>
                  </div>
                </div>
                {reservation.qrCode && (
                  <button
                    type="button"
                    onClick={() => setSelectedQr(reservation)}
                    className="self-start rounded-2xl border border-zinc-200 p-2 text-center transition hover:border-black focus:outline-none focus:ring-2 focus:ring-black md:self-center"
                    aria-label="Open reservation QR code"
                  >
                    <QRCodeCanvas value={`${window.location.origin}/verify/${reservation.qrCode}`} size={112} level="H" includeMargin />
                    <span className="mt-2 block text-xs font-semibold text-zinc-600">Tap to enlarge</span>
                  </button>
                )}
              </article>
            ))}
            {reservations.length === 0 && <p className="rounded-3xl border p-8 text-zinc-500">No reservations yet.</p>}
          </div>
        </section>
      </main>

      {selectedQr && (
        <QrModal
          reservation={selectedQr}
          onClose={() => setSelectedQr(null)}
        />
      )}

      {confirmed && (
        <QrModal
          reservation={confirmed}
          title="Reservation confirmed"
          onClose={() => setConfirmed(null)}
        />
      )}
    </div>
  )
}

function QrModal({ reservation, title = 'Reservation QR code', onClose }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-4">
      <div className="w-full max-w-lg rounded-3xl bg-white p-8 text-center">
        <h2 className="text-2xl font-bold">{title}</h2>
        <p className="mt-2 font-semibold">{reservation.routeName}</p>
        <p className="mt-1 text-sm text-zinc-600">
          Seat {reservation.seatNumber} · {reservation.pickupPlaceName}
        </p>
        <div className="mt-6 inline-block rounded-3xl border border-zinc-200 bg-white p-4 shadow-sm">
          <QRCodeCanvas value={`${window.location.origin}/verify/${reservation.qrCode}`} size={280} level="H" includeMargin />
        </div>
        <p className="mt-4 text-sm text-zinc-500">Show this screen to the driver for check-in.</p>
        <button onClick={onClose} className="mt-6 w-full rounded-xl bg-black py-3 font-semibold text-white">Done</button>
      </div>
    </div>
  )
}

function getBookablePickupPlaces(trip, liveLocation) {
  if (!liveLocation) return pickupPlaces

  const nearest = pickupPlaces.reduce((closest, place) => {
    const distance = distanceKm(
      liveLocation.latitude,
      liveLocation.longitude,
      place.lat,
      place.lng
    )

    return distance < closest.distance ? { place, distance } : closest
  }, { place: pickupPlaces[0], distance: Infinity }).place

  if (trip.origin === 'Skopje' || trip.routeName?.startsWith('Skopje')) {
    return pickupPlaces.filter(place => place.order <= nearest.order)
  }

  if (trip.origin === 'Tetovo' || trip.routeName?.startsWith('Tetovo')) {
    return pickupPlaces.filter(place => place.order <= nearest.order && place.order <= 4)
  }

  return pickupPlaces.filter(place => place.order >= nearest.order)
}

function distanceKm(lat1, lon1, lat2, lon2) {
  const toRadians = value => value * Math.PI / 180
  const earthRadiusKm = 6371
  const dLat = toRadians(lat2 - lat1)
  const dLon = toRadians(lon2 - lon1)
  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(toRadians(lat1)) * Math.cos(toRadians(lat2)) *
    Math.sin(dLon / 2) * Math.sin(dLon / 2)

  return earthRadiusKm * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}
