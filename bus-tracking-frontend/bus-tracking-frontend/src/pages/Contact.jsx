import PageShell from '../components/PageShell'

export default function Contact() {
  return (
    <PageShell
      eyebrow="Contact Euro Linia"
      title="Support for reservations, routes and live tracking."
      description="Get help with seat reservations, pickup locations, schedules or trip updates for the Strugë, Tetovë and Shkup route."
    >
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="rounded-3xl bg-black p-8 text-white lg:col-span-1">
          <h2 className="text-2xl font-bold">Euro Linia Support</h2>

          <p className="mt-4 text-sm text-zinc-300">
            Passenger support for route information, reservations and live bus updates.
          </p>

          <div className="mt-8 space-y-5 text-sm">
            <div>
              <p className="text-zinc-400">Route</p>
              <p className="font-semibold">Strugë — Tetovë — Shkup</p>
            </div>

            <div>
              <p className="text-zinc-400">Phone</p>
              <p className="font-semibold">+389 XX XXX XXX</p>
            </div>

            <div>
              <p className="text-zinc-400">Email</p>
              <p className="font-semibold">support@eurolinia.mk</p>
            </div>

            <div>
              <p className="text-zinc-400">Working hours</p>
              <p className="font-semibold">08:00 — 20:00</p>
            </div>
          </div>
        </div>

        <form className="rounded-3xl bg-white p-8 shadow-sm ring-1 ring-zinc-200 lg:col-span-2">
          <h2 className="text-2xl font-bold text-zinc-950">Send a message</h2>

          <p className="mt-2 text-sm text-zinc-600">
            This form can later be connected to your backend contact endpoint.
          </p>

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <input
              className="w-full rounded-2xl border border-zinc-300 px-4 py-3 outline-none focus:border-black"
              placeholder="Your name"
            />

            <input
              className="w-full rounded-2xl border border-zinc-300 px-4 py-3 outline-none focus:border-black"
              placeholder="Email or phone"
            />

            <select className="w-full rounded-2xl border border-zinc-300 px-4 py-3 outline-none focus:border-black md:col-span-2">
              <option>Reservation help</option>
              <option>Route information</option>
              <option>Pickup location</option>
              <option>Live tracking issue</option>
              <option>Other</option>
            </select>

            <textarea
              className="min-h-32 w-full rounded-2xl border border-zinc-300 px-4 py-3 outline-none focus:border-black md:col-span-2"
              placeholder="How can we help?"
            />

            <button
              type="button"
              className="w-full rounded-full bg-black px-5 py-3 font-semibold text-white transition hover:bg-zinc-800 md:col-span-2"
            >
              Send message
            </button>
          </div>
        </form>
      </div>

      <div className="mt-8 grid gap-6 md:grid-cols-3">
        <div className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-zinc-200">
          <p className="text-sm text-zinc-500">Main route</p>
          <p className="mt-2 text-xl font-bold text-zinc-950">
            Strugë to Shkup
          </p>
          <p className="mt-2 text-sm text-zinc-600">
            Departures through Tetovë with pickup points along the route.
          </p>
        </div>

        <div className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-zinc-200">
          <p className="text-sm text-zinc-500">Reservations</p>
          <p className="mt-2 text-xl font-bold text-zinc-950">
            Digital seat booking
          </p>
          <p className="mt-2 text-sm text-zinc-600">
            Passengers can reserve seats and use QR codes for check-in.
          </p>
        </div>

        <div className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-zinc-200">
          <p className="text-sm text-zinc-500">Tracking</p>
          <p className="mt-2 text-xl font-bold text-zinc-950">
            Live GPS updates
          </p>
          <p className="mt-2 text-sm text-zinc-600">
            Active buses appear on the live map when GPS data is received.
          </p>
        </div>
      </div>
    </PageShell>
  )
}