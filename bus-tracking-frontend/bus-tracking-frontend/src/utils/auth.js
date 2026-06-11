export function decodeJwt(token) {
  try {
    const payload = token.split('.')[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(normalized)
        .split('')
        .map((char) => `%${(`00${char.charCodeAt(0).toString(16)}`).slice(-2)}`)
        .join('')
    )
    return JSON.parse(json)
  } catch {
    return {}
  }
}

export function buildUserFromToken(token) {
  const decoded = decodeJwt(token)

  return {
    id: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decoded.nameid || decoded.sub || '',
    fullName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || decoded.unique_name || decoded.name || 'User',
    email: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || decoded.email || '',
    role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || ''
  }
}

export function isTokenValid(token) {
  if (!token) return false

  const decoded = decodeJwt(token)
  const expiresAt = Number(decoded.exp)

  return Number.isFinite(expiresAt) && expiresAt * 1000 > Date.now()
}

export function getCurrentUser() {
  try {
    return JSON.parse(localStorage.getItem('user') || '{}')
  } catch {
    return {}
  }
}

export function isAdmin() {
  return getCurrentUser().role === 'Admin'
}
