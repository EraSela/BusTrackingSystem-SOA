import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import API from '../services/api'

const notificationTypeLabel = {
  0: 'Delay',
  1: 'Pickup alert',
  2: 'Pickup warning'
}

export default function Notifications() {
  const [notifications, setNotifications] = useState([])
  const [error, setError] = useState('')

  const fetchNotifications = async () => {
    try {
      const res = await API.get('/notifications/my')
      setNotifications(res.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load notifications')
    }
  }

  useEffect(() => {
    // Initial API loading intentionally updates component state.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchNotifications()
  }, [])

  const markAsRead = async (id) => {
    try {
      await API.put(`/notifications/${id}/read`)
      fetchNotifications()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to mark notification as read')
    }
  }

  const unreadCount = notifications.filter(n => !n.isRead).length

  return (
    <div className="min-h-screen bg-white">
      <Navbar />

      <main className="px-6 md:px-10 py-10 max-w-5xl mx-auto">
        <div className="mb-10 flex flex-col md:flex-row md:items-end md:justify-between gap-4">
          <div>
            <h1 className="text-5xl md:text-6xl font-bold tracking-tight text-zinc-950">
              Notifications
            </h1>

            <p className="text-lg text-gray-600 mt-4 max-w-2xl">
              Stay updated about delays, pickup alerts and trip changes.
            </p>
          </div>

          <div className="bg-zinc-100 px-4 py-2 rounded-full text-sm text-zinc-700 w-fit">
            {unreadCount} unread
          </div>
        </div>

        {error && (
          <div className="bg-red-50 text-red-600 border border-red-200 rounded-xl px-4 py-3 mb-6 text-sm">
            {error}
          </div>
        )}

        {notifications.length === 0 ? (
          <div className="border border-zinc-200 rounded-3xl p-10 text-gray-500">
            No notifications yet.
          </div>
        ) : (
          <div className="space-y-4">
            {notifications.map(n => (
              <div
                key={n.id}
                className={`rounded-3xl border p-6 transition ${
                  n.isRead
                    ? 'bg-white border-zinc-200'
                    : 'bg-zinc-50 border-zinc-300'
                }`}
              >
                <div className="flex flex-col md:flex-row md:items-start md:justify-between gap-5">
                  <div>
                    <div className="flex items-center gap-3 mb-3">
                      <span className="text-xs font-semibold uppercase tracking-wide text-gray-500">
                        {notificationTypeLabel[n.type] || 'Notification'}
                      </span>

                      {!n.isRead && (
                        <span className="bg-black text-white text-xs px-3 py-1 rounded-full">
                          New
                        </span>
                      )}
                    </div>

                    <p className="text-lg font-semibold text-zinc-950">
                      {n.message}
                    </p>

                    <p className="mt-3 text-sm text-gray-500">
                      {new Date(n.createdAt).toLocaleString()}
                    </p>
                  </div>

                  {!n.isRead && (
                    <button
                      onClick={() => markAsRead(n.id)}
                      className="rounded-full bg-black px-5 py-2 text-sm font-semibold text-white hover:bg-zinc-800 transition"
                    >
                      Mark as read
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
