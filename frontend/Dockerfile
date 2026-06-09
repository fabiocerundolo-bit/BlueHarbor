# ─── Stage 1: Build React/Vite ────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Installa dipendenze prima di copiare il sorgente (cache layer)
COPY package*.json ./
RUN npm install

# Copia il resto e builda
COPY . .
RUN npm run build

# ─── Stage 2: Serve con Nginx ─────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Copia il bundle produzione
COPY --from=build /app/dist /usr/share/nginx/html

# Copia la configurazione Nginx (proxy /api → backend)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
