import { useState } from 'react'
import { askJobAssistant } from '../services/jobAssistantService'
import styles from './JobAssistantCard.module.css'

const prompts = [
  { mode: 'explain', label: 'Explain this job' },
  { mode: 'daily_work', label: 'What will I be doing?' },
  { mode: 'interview_questions', label: 'Give me 10 interview questions' },
]

function createId(prefix) {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`
}

export default function JobAssistantCard({ jobId }) {
  const [messages, setMessages] = useState([
    {
      id: createId('assistant'),
      role: 'assistant',
      response: {
        intro: 'I can break this role down in simple words, tell you what the day-to-day work may look like, or prepare 10 likely interview questions for this specific job.',
        highlights: [],
        interviewQuestions: [],
        usedAi: false,
        note: 'Based on the current job post.',
      },
    },
  ])
  const [loadingMode, setLoadingMode] = useState('')

  async function handlePrompt(prompt) {
    if (loadingMode) {
      return
    }

    const pendingId = createId('pending')
    setLoadingMode(prompt.mode)
    setMessages((current) => [
      ...current,
      { id: createId('user'), role: 'user', text: prompt.label },
      { id: pendingId, role: 'assistant', loading: true },
    ])

    try {
      const response = await askJobAssistant(jobId, prompt.mode)
      setMessages((current) =>
        current.map((message) =>
          message.id === pendingId
            ? { id: pendingId, role: 'assistant', response }
            : message,
        ),
      )
    } catch {
      setMessages((current) =>
        current.map((message) =>
          message.id === pendingId
            ? {
                id: pendingId,
                role: 'assistant',
                response: {
                  intro: 'I could not prepare that answer right now. Please try again in a moment.',
                  highlights: [],
                  interviewQuestions: [],
                  usedAi: false,
                  note: '',
                },
              }
            : message,
        ),
      )
    } finally {
      setLoadingMode('')
    }
  }

  return (
    <section className={styles.card}>
      <header className={styles.header}>
        <div>
          <span className={styles.kicker}>AI Job Assistant</span>
          <h2>Ask about this role</h2>
          <p>Simple explanations and candidate prep help, without changing the existing match analysis.</p>
        </div>
        <div className={styles.badge}>Job-specific</div>
      </header>

      <div className={styles.actions}>
        {prompts.map((prompt) => (
          <button
            key={prompt.mode}
            type="button"
            disabled={Boolean(loadingMode)}
            onClick={() => handlePrompt(prompt)}
          >
            {loadingMode === prompt.mode ? 'Thinking...' : prompt.label}
          </button>
        ))}
      </div>

      <div className={styles.chat}>
        {messages.map((message) => (
          <article
            key={message.id}
            className={`${styles.message} ${message.role === 'user' ? styles.user : styles.assistant}`}
          >
            {message.role === 'user' ? (
              <p>{message.text}</p>
            ) : message.loading ? (
              <p>Preparing an answer from this job post...</p>
            ) : (
              <>
                <p>{message.response.intro}</p>
                {message.response.mode !== 'interview_questions' && message.response.highlights.length > 0 && (
                  <ul>
                    {message.response.highlights.map((item) => (
                      <li key={item}>{item}</li>
                    ))}
                  </ul>
                )}
                {message.response.mode === 'interview_questions' && message.response.interviewQuestions.length > 0 && (
                  <ol>
                    {message.response.interviewQuestions.map((item) => (
                      <li key={item}>{item}</li>
                    ))}
                  </ol>
                )}
                {message.response.note && <small>{message.response.note}</small>}
              </>
            )}
          </article>
        ))}
      </div>
    </section>
  )
}
