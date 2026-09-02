# Build stage for the Svelte app
FROM node:24 AS svelte
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ENV VITE_BASE_URL=http://localhost:5200/
RUN npm run build

CMD ["npm", "run", "host"]
