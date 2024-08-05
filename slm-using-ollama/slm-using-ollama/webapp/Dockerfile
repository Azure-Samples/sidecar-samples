FROM mcr.microsoft.com/appsvc/node:20-lts_20240522.2.tuxprod

WORKDIR /app

COPY . .

RUN npm install

ENTRYPOINT [ "node", "app.js" ]