FROM node:25-alpine AS build

WORKDIR /app

RUN apk add --no-cache librdkafka-dev python3 make g++

COPY package*.json ./

ENV CKJS_LINKING=dynamic
ENV BUILD_LIBRDKAFKA=0

RUN npm ci --only=production

COPY . .

FROM node:25-alpine AS production

WORKDIR /app

RUN apk add --no-cache librdkafka

COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/package*.json ./
COPY --from=build /app .

RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001

RUN chown -R nodejs:nodejs /app

USER nodejs

CMD ["node", "src/index.ts"]
