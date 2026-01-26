import type { User } from './user'
import type { VotingRound } from './vote'

export interface Session {
  sessionCode: string
  ownerId: string
  ownerName: string
  users: User[]
  currentRound: VotingRound | null
  createdAt: Date
}
