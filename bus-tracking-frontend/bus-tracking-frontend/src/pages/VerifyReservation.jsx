import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import Navbar from '../components/Navbar'
import API from '../services/api'

export default function VerifyReservation() {
  const { qrCode } = useParams()
  const [reservation, setReservation] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(true)

  const fetchReservation = async () => {
    try {
      const res = await API.get(`/reservations/qr/${qrCode}`)
      setReservation(res.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid QR code')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    // Reload the reservation whenever the scanned QR code changes.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchReservation()
    // fetchReservation is scoped to the current QR code.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [qrCode])

  const verify = async () => {
    try {
      await API.post('/reservations/verify', { qrCode })
      setSuccess('Passenger checked in successfully.')
      fetchReservation()
    } catch (err) {
      setError(err.response?.data?.message || 'Verification failed')
    }
  }

  return (
    <div className="min-h-screen bg-white">
      <Navbar />

      <main className="px-6 md:px-10 py-10 max-w-3xl mx-auto">
        <h1 className="text-5xl font-bold text-zinc-950">
          Verify reservation
        </h1>

        <p className="text-gray-600 mt-3">
          Confirm passenger reservation before boarding.
        </p>

        {loading && <p className="mt-8 text-gray-500">Loading...</p>}

        {error && (
          <div className="mt-8 bg-red-50 text-red-600 border border-red-200 rounded-xl px-4 py-3">
            {error}
          </div>
        )}

        {success && (
          <div className="mt-8 bg-green-50 text-green-700 border border-green-200 rounded-xl px-4 py-3">
            {success}
          </div>
        )}

        {reservation && (
          <div className="mt-8 border border-zinc-200 rounded-3xl p-8">
            <h2 className="text-2xl font-bold text-zinc-950">
              {reservation.busName}
            </h2>

            <div className="mt-6 grid gap-4 text-sm">
              <p><strong>Passenger:</strong> {reservation.userFullName || 'N/A'}</p>
              <p><strong>Seat:</strong> {reservation.seatNumber}</p>
              <p><strong>Trip:</strong> {reservation.routeName}</p>
              <p><strong>Departure:</strong> {new Date(reservation.scheduledDeparture).toLocaleString()}</p>
              <p><strong>Pickup:</strong> {reservation.pickupPlaceName}</p>
              <p><strong>Status:</strong> {reservation.isVerified ? 'Checked In' : 'Reserved'}</p>
            </div>

            {!reservation.isVerified && (
              <button
                onClick={verify}
                className="mt-8 w-full bg-black text-white py-3 rounded-xl font-semibold hover:bg-zinc-800"
              >
                Verify passenger
              </button>
            )}
          </div>
        )}
      </main>
    </div>
  )
}
