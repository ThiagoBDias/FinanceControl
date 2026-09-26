# FinanceControl Frontend

Frontend React + TypeScript + Vite do FinanceControl.

## Executar
1. npm install
2. Copie .env.example para .env e ajuste VITE_API_URL se necessário.
3. npm run dev

O frontend consome os endpoints existentes do backend.

## Build e deploy
- npm run build gera os arquivos em dist/.
- Na Vercel, use frontend como Root Directory e configure VITE_API_URL para a origem publicada da API; o frontend acrescenta /api automaticamente quando necessário.
- vercel.json encaminha as rotas da aplicação para index.html, permitindo abrir e recarregar páginas internas diretamente.
- Para desenvolvimento local, configure .env a partir de .env.example; a API local usa http://localhost:8080.
