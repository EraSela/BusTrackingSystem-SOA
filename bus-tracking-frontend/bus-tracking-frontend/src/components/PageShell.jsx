import Navbar from './Navbar'

export default function PageShell({ eyebrow, title, description, children }) {
  return (
    <div className="min-h-screen bg-[#f6f6f3] text-black">
      <Navbar />
      <main className="mx-auto max-w-7xl px-6 py-8">
        {(eyebrow || title || description) && (
          <section className="mb-8 rounded-[2rem] bg-black p-8 text-white md:p-12">
            {eyebrow && <p className="mb-3 text-sm font-semibold uppercase tracking-[0.2em] text-zinc-400">{eyebrow}</p>}
            {title && <h1 className="max-w-3xl text-4xl font-bold tracking-tight md:text-6xl">{title}</h1>}
            {description && <p className="mt-5 max-w-2xl text-base leading-7 text-zinc-300 md:text-lg">{description}</p>}
          </section>
        )}
        {children}
      </main>
    </div>
  )
}
