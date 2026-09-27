# AI Ticket Triage

An AI-powered support ticket triage system. Customers submit tickets through a public form; an LLM automatically classifies each one by category, priority, and sentiment — so support agents see the fires already flagged, not a flat unsorted queue.

**Live demo:** [ticket-triage-gilt.vercel.app](https://ticket-triage-gilt.vercel.app)  
**Submit a ticket:** [ticket-triage-gilt.vercel.app/submit](https://ticket-triage-gilt.vercel.app/submit)

---

## What it does

1. A customer submits a ticket — name, email, subject, description. No login required.
2. The backend saves the ticket immediately, then calls Groq for AI classification.
3. Groq classifies each ticket by category, priority, sentiment, and a one-line reasoning explanation.
4. The customer receives an email confirmation with their ticket ID and submitted details.
Status update emails are sent when an agent marks a ticket as In Progress or Resolved.
5. The ticket appears in the agent dashboard, sorted by priority with AI classification visible.
6. AI failures do not prevent ticket creation — the system saves the ticket and marks it for manual review.

---

## Tech stack

**Backend** — [github.com/tasheera/TicketTriage.Api](https://github.com/tasheera/TicketTriage.Api)
- ASP.NET Core 8 Web API, Entity Framework Core
- PostgreSQL on Neon
- MailKit for transactional email (Gmail SMTP)
- Dockerized, deployed to Azure App Service using Azure Container Registry
- CI/CD via GitHub Actions — builds and pushes the Docker image to Azure Container Registry and deploys it to Azure App Service on pushes to `main`

**Frontend** — [github.com/tasheera/ticket-triage-frontend](https://github.com/tasheera/ticket-triage-frontend)
- Next.js 16, Tailwind CSS, ShadCN UI
- Deployed on Vercel

**AI**
- [Groq API](https://groq.com) — `openai/gpt-oss-20b`
- Structured Outputs in strict mode — schema-constrained JSON responses

---

## API endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/health` | Public | Health check |
| `POST` | `/api/tickets` | Public | Submit a ticket, triggers AI classification |
| `GET` | `/api/tickets` | Agent | Paginated ticket list with optional status, priority, and category filters |
| `GET` | `/api/tickets/{id}` | Agent | Get a single ticket |
| `PATCH` | `/api/tickets/{id}/status` | Agent | Update ticket status |
| `POST` | `/api/auth/login` | Public | Agent login, returns a JWT  |

---

## AI prompt design & testing

Classification was tested against edge cases including sarcasm, ambiguous categories, implied urgency, and informal/typo-heavy input. Testing revealed and helped correct cases where emotional tone inflated priority and explicit non-urgent requests were classified too highly. 

**Known limitation:** ambiguous tickets can occasionally fall between Medium and High priority.

---

## Architecture decisions

- **Reliable ticket creation** — tickets are persisted before AI classification, so a submission is not lost if the AI service fails.
- **Security & Cost Control** — JWT authentication uses httpOnly cookies, and the public submission endpoint is rate-limited (5 req/min) to prevent API abuse and Groq cost exhaustion.
- **Prompt injection guard** — customer input is wrapped in explicit XML delimiters in the system prompt, forcing the LLM to treat the subject and description strictly as plain data.
- **Email reliability** — email failures are isolated from ticket creation; an SMTP error is logged as a warning and never causes the submission to fail.
- **CI/CD pipeline** — GitHub Actions builds and deploys the Docker container to Azure App Service on every push to `main`, so deployments are automated rather than a manual step.
- **Consistent error handling** — every API error returns `ProblemDetails` JSON, so the frontend handles all error types with one code path.

---

## Local setup

### Backend
```bash
git clone https://github.com/tasheera/TicketTriage.Api
cd TicketTriage.Api

# Configure environment variables:
# database connection, GROQ_API_KEY, JWT settings,
# email SMTP config, seed agent credentials

dotnet ef database update
dotnet run
```
Hit `http://localhost:8080/health` to confirm it's running (the application uses port 8080 by default; set `PORT` to override it).

### Frontend
```bash
git clone https://github.com/tasheera/ticket-triage-frontend
cd ticket-triage-frontend

# .env.local
# NEXT_PUBLIC_API_URL=http://localhost:8080
# BACKEND_URL=http://localhost:8080


npm install
npm run dev
```

---

## Author

Abhishek Silva — [LinkedIn](https://www.linkedin.com/in/tasheerasilva) · [GitHub](https://github.com/tasheera) · [Portfolio](https://tasheeraabhishek.dev)