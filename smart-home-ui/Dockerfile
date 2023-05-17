# Build stage for the Svelte app
FROM node:14 AS svelte
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
ENV VITE_BASE_URL=http://localhost:5200/
RUN npm run build

CMD ["npm", "run", "host"]
