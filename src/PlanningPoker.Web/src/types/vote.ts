export interface Vote {
  userId: string
  userName: string
  value: number | null
  submittedAt: Date
}

export interface VotingResults {
  lowFibonacciAverage: number
  highFibonacciAverage: number
  actualAverage: number
  distribution: Record<number, number>
  minVote: number
  maxVote: number
  totalVotes: number
}

export interface VotingRound {
  id: string
  status: 'InProgress' | 'Revealed' | 'Reset'
  startedAt: Date
  revealedAt: Date | null
  votes: Vote[]
  results: VotingResults | null
}
