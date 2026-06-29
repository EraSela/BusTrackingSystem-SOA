import { useEffect, useState } from 'react'
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet'
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

const liveLocationIcon = L.divIcon({
  html: `
    <div class="live-location-marker" aria-hidden="true">
      <span class="live-location-pulse"></span>
      <span class="live-location-dot"></span>
    </div>
  `,
  className: '',
  iconSize: [44, 44],
  iconAnchor: [22, 22],
})

const formatNumber = (value, decimals = 1) => {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return 'N/A'
  return Number(value).toFixed(decimals)
}

const formatUpdatedAgo = (value) => {
  const updatedAt = new Date(value)
  if (Number.isNaN(updatedAt.getTime())) return 'N/A'

  const seconds = Math.max(0, Math.floor((Date.now() - updatedAt.getTime()) / 1000))
  if (seconds < 60) return `${seconds} sec ago`

  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes} min ago`

  const hours = Math.floor(minutes / 60)
  return `${hours} hr ago`
}

function MapFocus({ bus }) {
  const map = useMap()

  useEffect(() => {
    if (!bus?.latitude || !bus?.longitude) return

    map.flyTo([bus.latitude, bus.longitude], Math.max(map.getZoom(), 14), {
      animate: true,
      duration: 0.8,
    })
  }, [bus?.busId, bus?.latitude, bus?.longitude, map])

  return null
}

export default function Map() {
  const [buses, setBuses] = useState([])
  const [selectedBusId, setSelectedBusId] = useState(null)
  const selectedBus = buses.find(bus => bus.busId === selectedBusId)

  const fetchLive = async () => {
    try {
      const res = await API.get('/locations/live')
      setBuses(res.data)
    } catch (err) {
      console.error('Failed to fetch live locations', err)
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
                <button
                  type="button"
                  key={bus.busId}
                  onClick={() => setSelectedBusId(bus.busId)}
                  className={`w-full text-left border rounded-3xl p-6 transition focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    selectedBusId === bus.busId
                      ? 'bg-blue-50 border-blue-500 shadow-md'
                      : 'bg-zinc-50 border-zinc-200 hover:bg-white hover:shadow-md'
                  }`}
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
                        Updated {formatUpdatedAgo(bus.lastUpdated)}
                      </p>
                    </div>
                  </div>
                </button>
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
            <MapFocus bus={selectedBus} />

            {buses.map(bus => (
              <Marker
                key={bus.busId}
                position={[bus.latitude, bus.longitude]}
                icon={liveLocationIcon}
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
                      Updated: {formatUpdatedAgo(bus.lastUpdated)}
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
