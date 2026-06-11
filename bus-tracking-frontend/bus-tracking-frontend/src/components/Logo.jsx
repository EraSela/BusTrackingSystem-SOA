import blackLogo from '../assets/eurolinia-logo-black.png'
import whiteLogo from '../assets/eurolinia-logo-white.png'

export default function Logo({ compact = false }) {
  if (compact) {
    return (
      <div className="flex items-center gap-3">
        <img
          src={whiteLogo}
          alt="Euro Linia"
          className="w-8 h-8 object-contain"
        />

        <span className="text-white font-semibold text-xl tracking-tight">
          Euro Linia
        </span>
      </div>
    )
  }

  return (
    <div className="text-center">
     <img
        src={blackLogo}
        alt="Euro Linia"
        className="w-14 h-14 object-contain mx-auto"
    />

    <h1 className="text-3xl font-bold text-zinc-950 mt-3">
      EURO LINIA
    </h1>

    </div>
  )
}