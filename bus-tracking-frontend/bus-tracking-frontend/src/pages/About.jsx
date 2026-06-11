import PageShell from '../components/PageShell'

export default function About() {
  return (
    <PageShell
      eyebrow="About Euro Linia"
      title="Modern intercity transportation between Strugë, Tetovë and Shkup."
      description="Euro Linia combines reliable bus transportation with live GPS tracking, digital reservations and real-time passenger updates."
    >
      <div className="grid gap-6 md:grid-cols-3">
        <div className="rounded-3xl bg-white p-8 shadow-sm ring-1 ring-zinc-200">
          <p className="text-4xl font-bold text-zinc-950">3</p>
          <p className="mt-3 text-sm text-zinc-600">
            Major destinations connecting Strugë, Tetovë and Shkup.
          </p>
        </div>

        <div className="rounded-3xl bg-white p-8 shadow-sm ring-1 ring-zinc-200">
          <p className="text-4xl font-bold text-zinc-950">24/7</p>
          <p className="mt-3 text-sm text-zinc-600">
            Live GPS monitoring of active buses and routes.
          </p>
        </div>

        <div className="rounded-3xl bg-white p-8 shadow-sm ring-1 ring-zinc-200">
          <p className="text-4xl font-bold text-zinc-950">ETA</p>
          <p className="mt-3 text-sm text-zinc-600">
            Estimated arrival times and pickup notifications.
          </p>
        </div>
      </div>

      <div className="mt-8 rounded-3xl bg-black text-white p-10">
        <h2 className="text-3xl font-bold">
          About the platform
        </h2>

        <p className="mt-4 text-zinc-300 max-w-3xl">
          Euro Linia is a smart transportation platform developed to
          improve passenger experience and operational efficiency.
          Passengers can reserve seats, follow buses in real time,
          receive notifications and view estimated arrival times from
          a single application.
        </p>
      </div>

      <div className="mt-8 rounded-3xl bg-white p-8 shadow-sm ring-1 ring-zinc-200">
        <h2 className="text-3xl font-bold text-zinc-950">
          Key features
        </h2>

        <div className="mt-6 grid gap-4 md:grid-cols-2">
          {[
            'Real-time bus tracking using GPS and SIM808 hardware.',
            'Digital seat reservations for scheduled departures.',
            'Pickup point selection during reservation.',
            'QR-code based ticket verification.',
            'Passenger notifications for delays and arrivals.',
            'Administrative trip and fleet management tools.'
          ].map((item) => (
            <div
              key={item}
              className="rounded-2xl bg-zinc-50 p-5 text-zinc-700 border border-zinc-200"
            >
              {item}
            </div>
          ))}
        </div>
      </div>
    </PageShell>
  )
}