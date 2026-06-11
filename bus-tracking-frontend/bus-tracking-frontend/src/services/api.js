import axios from 'axios';
import { isTokenValid } from '../utils/auth';

const API = axios.create({
  baseURL: 'https://localhost:7113/api',
});

API.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');

  if (isTokenValid(token)) {
    config.headers.Authorization = `Bearer ${token}`;
  } else if (token) {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }

  return config;
});

API.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');

      if (window.location.pathname !== '/login') {
        window.location.assign('/login');
      }
    }

    return Promise.reject(error);
  }
);

export default API;
