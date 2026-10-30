/** Server tomonda ishlatiladigan sozlamalar. Brauzerga chiqmaydi. */
export const serverEnv = {
  apiUrl: process.env.API_URL ?? "http://localhost:5000",
} as const;
