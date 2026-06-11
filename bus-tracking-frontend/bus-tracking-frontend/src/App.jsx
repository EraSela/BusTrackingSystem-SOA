import { Routes, Route, Navigate } from 'react-router-dom'
import Login from './pages/Login'
import Register from './pages/Register'
import Map from './pages/Map'
import Reservations from './pages/Reservations'
import Trips from './pages/Trips'
import About from './pages/About'
import Contact from './pages/Contact'
import Notifications from './pages/Notifications'
import VerifyReservation from './pages/VerifyReservation'
import AdminBuses from './pages/AdminBuses'
import AdminUsers from './pages/AdminUsers'
import { getCurrentUser, isTokenValid } from './utils/auth'

const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('token')

  if (!isTokenValid(token)) {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    return <Navigate to="/login" replace />
  }

  return children
}

const AdminRoute = ({ children }) => {
  const token = localStorage.getItem('token')

  if (!isTokenValid(token)) {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    return <Navigate to="/login" replace />
  }

  return getCurrentUser().role === 'Admin'
    ? children
    : <Navigate to="/" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/" element={<PrivateRoute><Map /></PrivateRoute>} />
      <Route path="/reservations" element={<PrivateRoute><Reservations /></PrivateRoute>} />
      <Route path="/trips" element={<PrivateRoute><Trips /></PrivateRoute>} />
      <Route path="/admin/buses" element={<AdminRoute><AdminBuses /></AdminRoute>} />
      <Route path="/admin/users" element={<AdminRoute><AdminUsers /></AdminRoute>} />
      <Route path="/notifications" element={<PrivateRoute><Notifications /></PrivateRoute>} />
      <Route path="/about" element={<PrivateRoute><About /></PrivateRoute>} />
      <Route path="/contact" element={<PrivateRoute><Contact /></PrivateRoute>} />
      <Route path="*" element={<Navigate to="/" />} />
      <Route path="/verify/:qrCode" element={<PrivateRoute><VerifyReservation /></PrivateRoute>}/>
    </Routes>
  )
}
