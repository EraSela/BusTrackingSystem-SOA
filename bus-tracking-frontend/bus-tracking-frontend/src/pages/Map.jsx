import { useEffect, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import Navbar from '../components/Navbar'
import API from '../services/api'

delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
})

const butterflyIcon = L.divIcon({
  html: `
    <div class="live-butterfly-marker" aria-hidden="true">
      <svg viewBox="0 0 64 64" role="img" focusable="false">
        <path class="butterfly-wing butterfly-wing-left" d="M30 31C22 11 8 8 4 18c-4 11 6 24 23 24 2-3 3-7 3-11Z" />
        <path class="butterfly-wing butterfly-wing-right" d="M34 31c8-20 22-23 26-13 4 11-6 24-23 24-2-3-3-7-3-11Z" />
        <path class="butterfly-wing butterfly-wing-left lower" d="M29 38C19 38 11 44 13 53c2 8 15 6 19-8-1-3-2-5-3-7Z" />
        <path class="butterfly-wing butterfly-wing-right lower" d="M35 38c10 0 18 6 16 15-2 8-15 6-19-8 1-3 2-5 3-7Z" />
        <path class="butterfly-body" d="M32 26c3 0 5 5 5 12s-2 14-5 14-5-7-5-14 2-12 5-12Z" />
        <path class="butterfly-antenna" d="M30 27c-2-5-5-8-10-9M34 27c2-5 5-8 10-9" />
      </svg>
    </div>
  `,
  className: '',
  iconSize: [54, 54],
  iconAnchor: [27, 31],
})

const formatNumber = (value, decimals = 1) => {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return 'N/A'
  return Number(value).toFixed(decimals)
}

export default function Map() {
  const [buses, setBuses] = useState([])
  const [loading, setLoading] = useState(true)

  const fetchLive = async () => {
    try {
      const res = await API.get('/locations/live')
      setBuses(res.data)
    } catch (err) {
      console.error('Failed to fetch live locations', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    // Initial load and polling intentionally update component state.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchLive()
    const interval = setInterval(fetchLive, 5000)
    return () => clearInterval(interval)
  }, [])

  return (
    <div className="min-h-screen bg-white">
      <Navbar />

      <main className="px-6 md:px-10 py-10 max-w-7xl mx-auto">
        <div className="mb-10">
          <div>
            <h1 className="text-5xl md:text-6xl font-bold tracking-tight text-zinc-950">
              Live Bus Map
            </h1>

            <p className="text-lg text-gray-600 mt-4 max-w-2xl">
              Track Euro Linia buses in real time across the Strugë, Tetovë and Shkup route.
            </p>
          </div>

        </div>

        <section className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
          <div className="lg:col-span-1 bg-black text-white rounded-3xl p-7">
            <p className="text-sm text-zinc-400 mb-3">
              Active buses
            </p>

            <h2 className="text-5xl font-bold">
              {buses.length}
            </h2>

            <p className="text-zinc-300 mt-4">
              {buses.length === 1
                ? 'One Euro Linia bus is currently transmitting GPS data.'
                : 'Euro Linia buses currently transmitting GPS data.'}
            </p>
          </div>

          <div className="lg:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-4">
            {buses.length === 0 ? (
              <div className="md:col-span-2 bg-zinc-50 border border-zinc-200 rounded-3xl p-8 text-gray-500">
                No active buses right now. Once the tracker sends GPS data, the bus will appear here.
              </div>
            ) : (
              buses.map(bus => (
                <div
                  key={bus.busId}
                  className="bg-zinc-50 border border-zinc-200 rounded-3xl p-6 hover:bg-white hover:shadow-md transition"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="font-semibold text-zinc-950 text-lg">
                        {bus.busName}
                      </p>

                      <p className="text-sm text-gray-500 mt-2">
                        {bus.routeName || 'Active trip'}
                        {bus.scheduledDeparture
                          ? ` - departs ${new Date(bus.scheduledDeparture).toLocaleTimeString([], {
                              hour: '2-digit',
                              minute: '2-digit'
                            })}`
                          : ''}
                      </p>
                    </div>

                    <span className="bg-green-100 text-green-700 text-xs font-semibold px-3 py-1 rounded-full">
                      Live
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-4 mt-5 text-sm">
                    <div>
                      <p className="text-gray-500">Speed</p>
                      <p className="font-semibold text-zinc-900">
                        {bus.speed !== null && bus.speed !== undefined ? `${formatNumber(bus.speed)} km/h` : 'N/A'}
                      </p>
                    </div>

                    <div>
                      <p className="text-gray-500">Last update</p>
                      <p className="font-semibold text-zinc-900">
                        {new Date(bus.lastUpdated).toLocaleTimeString()}
                      </p>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </section>

        <section className="rounded-3xl overflow-hidden border border-zinc-200 shadow-sm">
          <MapContainer
            center={[41.6086, 20.6736]}
            zoom={10}
            style={{ height: '560px', width: '100%' }}
          >
            <TileLayer
              attribution="&copy; OpenStreetMap contributors"
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            {buses.map(bus => (
              <Marker
                key={bus.busId}
                position={[bus.latitude, bus.longitude]}
                icon={butterflyIcon}
              >
                <Popup>
                  <div className="text-sm min-w-48">
                    <p className="font-bold text-zinc-950 mb-2">
                      {bus.busName}
                    </p>

                    <p>
                      Speed: {bus.speed !== null && bus.speed !== undefined ? `${formatNumber(bus.speed)} km/h` : 'N/A'}
                    </p>

                    <p>
                      Heading: {bus.heading !== null && bus.heading !== undefined ? `${bus.heading}°` : 'N/A'}
                    </p>

                    <p>
                      Updated: {new Date(bus.lastUpdated).toLocaleTimeString()}
                    </p>
                  </div>
                </Popup>
              </Marker>
            ))}
          </MapContainer>
        </section>
      </main>
    </div>
  )
}
