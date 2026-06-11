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

const busIcon = L.divIcon({
  html: `
    <div style="
      width: 38px;
      height: 38px;
      border-radius: 9999px;
      background: #000;
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 17px;
      font-weight: 700;
      box-shadow: 0 8px 20px rgba(0,0,0,0.25);
      border: 3px solid white;
    ">
      BUS
    </div>
  `,
  className: '',
  iconSize: [38, 38],
  iconAnchor: [19, 19],
})

export default function Map() {
  const [buses, setBuses] = useState([])
  const [lastUpdated, setLastUpdated] = useState(null)
  const [loading, setLoading] = useState(true)

  const fetchLive = async () => {
    try {
      const res = await API.get('/locations/live')
      setBuses(res.data)
      setLastUpdated(new Date().toLocaleTimeString())
    } catch (err) {
      console.error('Failed to fetch live locations', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchLive()
    const interval = setInterval(fetchLive, 5000)
    return () => clearInterval(interval)
  }, [])

  return (
    <div className="min-h-screen bg-white">
      <Navbar />

      <main className="px-6 md:px-10 py-10 max-w-7xl mx-auto">
        <div className="mb-10 flex flex-col lg:flex-row lg:items-end lg:justify-between gap-6">
          <div>
            <h1 className="text-5xl md:text-6xl font-bold tracking-tight text-zinc-950">
              Live Bus Map
            </h1>

            <p className="text-lg text-gray-600 mt-4 max-w-2xl">
              Track Euro Linia buses in real time across the Strugë, Tetovë and Shkup route.
            </p>
          </div>

          <div className="bg-zinc-100 rounded-full px-5 py-3 text-sm text-zinc-700 flex items-center gap-3 w-fit">
            <span className="inline-block w-2.5 h-2.5 bg-green-500 rounded-full animate-pulse" />
            {loading
              ? 'Loading live data...'
              : lastUpdated
                ? `Updated ${lastUpdated}`
                : 'Waiting for GPS data'}
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
                        Departure {bus.departureTime?.substring(0, 5) || 'N/A'}
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
                        {bus.speed !== null && bus.speed !== undefined ? `${bus.speed} km/h` : 'N/A'}
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
                icon={busIcon}
              >
                <Popup>
                  <div className="text-sm min-w-48">
                    <p className="font-bold text-zinc-950 mb-2">
                      {bus.busName}
                    </p>

                    <p>
                      Speed: {bus.speed !== null && bus.speed !== undefined ? `${bus.speed} km/h` : 'N/A'}
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